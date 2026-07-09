using Daab.Modules.Translation.Glossary;
using GlossarySet = Daab.Modules.Translation.Glossary.Glossary;

namespace Daab.Modules.Translation.Tests.Glossary;

public sealed class GlossaryProjectionTests
{
    static GlossaryRow Row(string ns, string key, string locale, string value) =>
        new(ns, key, locale, value);

    [Fact]
    public void Project_PairsSourceAndTargetByNamespaceAndKey()
    {
        GlossaryRow[] rows =
        [
            Row("institutions", "BakuStateUniversity", "en", "Baku State University"),
            Row("institutions", "BakuStateUniversity", "az", "Bakı Dövlət Universiteti"),
        ];

        var glossary = GlossaryProjection.Project(rows, "en", "az");

        var term = Assert.Single(glossary.Terms);
        Assert.Equal("Baku State University", term.Source);
        Assert.Equal("Bakı Dövlət Universiteti", term.Target);
        Assert.Equal("institutions", term.Namespace);
    }

    [Fact]
    public void Project_SkipsWhenTargetMissing()
    {
        GlossaryRow[] rows = [Row("areas", "Genetics", "en", "Genetics")];

        Assert.True(GlossaryProjection.Project(rows, "en", "az").IsEmpty);
    }

    [Fact]
    public void Project_SkipsWhenSourceMissing()
    {
        GlossaryRow[] rows = [Row("areas", "Genetics", "az", "Genetika")];

        Assert.True(GlossaryProjection.Project(rows, "en", "az").IsEmpty);
    }

    [Fact]
    public void Project_SkipsWhenTargetValueBlank()
    {
        GlossaryRow[] rows =
        [
            Row("areas", "Genetics", "en", "Genetics"),
            Row("areas", "Genetics", "az", "   "),
        ];

        Assert.True(GlossaryProjection.Project(rows, "en", "az").IsEmpty);
    }

    [Fact]
    public void Project_SkipsIdenticalSourceAndTarget()
    {
        // Proper nouns identical across locales carry no forcing signal.
        GlossaryRow[] rows = [Row("areas", "DNA", "en", "DNA"), Row("areas", "DNA", "az", "DNA")];

        Assert.True(GlossaryProjection.Project(rows, "en", "az").IsEmpty);
    }

    [Fact]
    public void Project_DedupsIdenticalPairsAcrossKeys()
    {
        GlossaryRow[] rows =
        [
            Row("institutions", "Ans", "en", "ANAS"),
            Row("institutions", "Ans", "az", "AMEA"),
            Row("institutions", "AzNas", "en", "ANAS"),
            Row("institutions", "AzNas", "az", "AMEA"),
        ];

        var term = Assert.Single(GlossaryProjection.Project(rows, "en", "az").Terms);
        Assert.Equal("ANAS", term.Source);
        Assert.Equal("AMEA", term.Target);
    }

    [Fact]
    public void Project_DifferentNamespacesSameKey_KeptSeparate()
    {
        GlossaryRow[] rows =
        [
            Row("areas", "Physics", "en", "Physics"),
            Row("areas", "Physics", "az", "Fizika"),
            Row("institutions", "Physics", "en", "Physics Institute"),
            Row("institutions", "Physics", "az", "Fizika İnstitutu"),
        ];

        var glossary = GlossaryProjection.Project(rows, "en", "az");

        Assert.Equal(2, glossary.Terms.Count);
    }

    [Fact]
    public void Project_OrdersDeterministicallyForCacheStability()
    {
        GlossaryRow[] rows =
        [
            Row("n", "b", "en", "Beta"),
            Row("n", "b", "az", "Beta-az"),
            Row("n", "a", "en", "Alpha"),
            Row("n", "a", "az", "Alpha-az"),
        ];

        var forward = GlossaryProjection.Project(rows, "en", "az");
        var reversedInput = GlossaryProjection.Project([.. rows.Reverse()], "en", "az");

        Assert.Equal(["Alpha", "Beta"], forward.Terms.Select(t => t.Source));
        // Input order must not affect output order (byte-stable cache prefix).
        Assert.Equal(forward.Render(), reversedInput.Render());
    }

    [Fact]
    public void Project_IgnoresUnrelatedLocales()
    {
        GlossaryRow[] rows =
        [
            Row("areas", "Genetics", "en", "Genetics"),
            Row("areas", "Genetics", "az", "Genetika"),
            Row("areas", "Genetics", "ru", "Генетика"),
        ];

        var term = Assert.Single(GlossaryProjection.Project(rows, "en", "az").Terms);
        Assert.Equal("Genetika", term.Target);
    }

    [Fact]
    public void Project_EmptyRows_ReturnsEmptyGlossary() =>
        Assert.True(GlossaryProjection.Project([], "en", "az").IsEmpty);

    [Fact]
    public void Render_ProducesQuotedArrowLines()
    {
        GlossaryRow[] rows =
        [
            Row("areas", "Genetics", "en", "Genetics"),
            Row("areas", "Genetics", "az", "Genetika"),
        ];

        var rendered = GlossaryProjection.Project(rows, "en", "az").Render();

        Assert.Equal("\"Genetics\" → \"Genetika\"", rendered);
    }

    [Fact]
    public void Render_EmptyGlossary_IsEmptyString() =>
        Assert.Equal(string.Empty, GlossarySet.Empty.Render());
}
