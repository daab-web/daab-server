using Daab.Modules.Translation.Glossary;
using Daab.Modules.Translation.Lexical;
using Daab.Modules.Translation.Memory;
using Daab.Modules.Translation.Models;
using Daab.Modules.Translation.Service;
using GlossarySet = Daab.Modules.Translation.Glossary.Glossary;

namespace Daab.Modules.Translation.Tests.Service;

public sealed class PromptBuilderTests
{
    static LexicalBlock Block(int index, string source) =>
        new(index, BlockType.Paragraph, [index], source, new Dictionary<int, InlineTag>());

    static GlossarySet Glossary() =>
        GlossaryProjection.Project(
            [
                new GlossaryRow("areas", "Genetics", "en", "Genetics"),
                new GlossaryRow("areas", "Genetics", "az", "Genetika"),
            ],
            "en",
            "az"
        );

    [Fact]
    public void Build_SystemInstruction_CarriesRulesAndGlossary()
    {
        var (system, _) = PromptBuilder.Build(
            ContentType.Scientist,
            "en",
            "az",
            Glossary(),
            [],
            [Block(0, "Hello")]
        );

        Assert.Contains("Azerbaijani", system, StringComparison.Ordinal);
        Assert.Contains("JSON array", system, StringComparison.Ordinal);
        Assert.Contains("tag", system, StringComparison.OrdinalIgnoreCase);
        // Glossary term is rendered into the cacheable prefix.
        Assert.Contains("Genetika", system, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_NoGlossary_ShowsNone()
    {
        var (system, _) = PromptBuilder.Build(
            ContentType.Article,
            "en",
            "az",
            GlossarySet.Empty,
            [],
            [Block(0, "Hello")]
        );

        Assert.Contains("(none)", system, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_User_NumbersTasksAndRequestsExactCount()
    {
        var (_, user) = PromptBuilder.Build(
            ContentType.Scientist,
            "en",
            "az",
            GlossarySet.Empty,
            [],
            [Block(0, "Alpha"), Block(1, "Beta")]
        );

        Assert.Contains("these 2 items", user, StringComparison.Ordinal);
        Assert.Contains("exactly 2", user, StringComparison.Ordinal);
        Assert.Contains("1. Alpha", user, StringComparison.Ordinal);
        Assert.Contains("2. Beta", user, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_User_IncludesExamplesWhenPresent()
    {
        var examples = new[]
        {
            new TranslationExample("<0>Cell</0>", "<0>Hüceyrə</0>", 0.9, ExampleTier.Fuzzy),
        };

        var (_, user) = PromptBuilder.Build(
            ContentType.Scientist,
            "en",
            "az",
            GlossarySet.Empty,
            examples,
            [Block(0, "Hello")]
        );

        Assert.Contains("<0>Cell</0>", user, StringComparison.Ordinal);
        Assert.Contains("<0>Hüceyrə</0>", user, StringComparison.Ordinal);
    }
}
