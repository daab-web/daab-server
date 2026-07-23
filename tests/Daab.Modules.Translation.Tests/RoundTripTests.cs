using Daab.Modules.Translation.Lexical;

namespace Daab.Modules.Translation.Tests;

/// <summary>
/// The Step 1 gate: extracting every block and reinserting each block's own source must reproduce
/// the original tree. Runs against the four real documents plus negative controls.
/// </summary>
public sealed class RoundTripTests
{
    [Fact]
    public void NoOp_ScientistAz_IsSemanticallyIdentical() =>
        AssertNoOpRoundTrip(Fixtures.Load(Fixtures.ScientistAz));

    [Fact]
    public void NoOp_ScientistEn_IsSemanticallyIdentical() =>
        AssertNoOpRoundTrip(Fixtures.Load(Fixtures.ScientistEn));

    [Fact]
    public void NoOp_NewsAz_IsSemanticallyIdentical() =>
        AssertNoOpRoundTrip(Fixtures.Load(Fixtures.NewsAz));

    [Fact]
    public void NoOp_NewsEn_IsSemanticallyIdentical() =>
        AssertNoOpRoundTrip(Fixtures.Load(Fixtures.NewsEn));

    [Fact]
    public void Reinsert_NoTranslations_LeavesTreeIdentical()
    {
        var json = Fixtures.Load(Fixtures.NewsAz);
        var result = LexicalWalker.Reinsert(json, new Dictionary<int, string>()).ExpectSucc();
        Assert.True(LexicalWalker.TreesEquivalent(json, result));
    }

    [Fact]
    public void TreesEquivalent_DistinguishesDifferentDocuments()
    {
        var az = Fixtures.Load(Fixtures.NewsAz);
        var en = Fixtures.Load(Fixtures.NewsEn);
        Assert.True(LexicalWalker.TreesEquivalent(az, az));
        Assert.False(LexicalWalker.TreesEquivalent(az, en));
    }

    static void AssertNoOpRoundTrip(string json)
    {
        var extract = LexicalWalker.Extract(json).ExpectSucc();
        Assert.NotEmpty(extract.Blocks);
        var translations = extract.Blocks.ToDictionary(block => block.Index, block => block.Source);
        var result = LexicalWalker.Reinsert(json, translations).ExpectSucc();
        Assert.True(
            LexicalWalker.TreesEquivalent(json, result),
            "Round-trip produced a structurally different tree."
        );
    }
}
