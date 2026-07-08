using Daab.Modules.Translation.Lexical;

namespace Daab.Modules.Translation.Tests;

public sealed class ExtractTests
{
    [Fact]
    public void News_ProducesOneBlockPerNonEmptyParagraph()
    {
        var extract = LexicalWalker.Extract(Fixtures.Load(Fixtures.NewsAz)).ExpectSucc();
        Assert.Equal(6, extract.Blocks.Count);
    }

    [Fact]
    public void News_LinkBlock_TagsAnchorAndStashesUrl()
    {
        var extract = LexicalWalker.Extract(Fixtures.Load(Fixtures.NewsAz)).ExpectSucc();
        var linkBlock = Assert.Single(
            extract.Blocks,
            b => b.SideMap.Values.Any(t => t.Kind == InlineKind.Link)
        );

        Assert.Equal("<0>Xəbərə keçid</0>", linkBlock.Source);
        var tag = linkBlock.SideMap[0];
        Assert.Equal(InlineKind.Link, tag.Kind);
        Assert.Equal(
            "https://beu.edu.az/az/media/news/bmu-il-dunya-azrbaycanl-alimlr-birliyi-uzvlrinin-mkdaslg-muzakir-olunub",
            tag.Link!.Url
        );
    }

    [Fact]
    public void ScientistAz_NestedList_ProducesDeepNodePath()
    {
        var extract = LexicalWalker.Extract(Fixtures.Load(Fixtures.ScientistAz)).ExpectSucc();
        Assert.Contains(extract.Blocks, block => block.NodePath.Count >= 4);
    }

    [Fact]
    public void ScientistAz_SkipsEmptySpacers_NoBlockIsWhitespace()
    {
        var extract = LexicalWalker.Extract(Fixtures.Load(Fixtures.ScientistAz)).ExpectSucc();
        Assert.All(extract.Blocks, block => Assert.False(string.IsNullOrWhiteSpace(block.Source)));
    }

    [Fact]
    public void ScientistAz_BoldHeading_TaggedAsFormatRun()
    {
        var extract = LexicalWalker.Extract(Fixtures.Load(Fixtures.ScientistAz)).ExpectSucc();
        var first = extract.Blocks[0];

        Assert.Equal("<0>Qeyri-xətti Analiz və Riyazi Fizikaya Töhfələri</0>", first.Source);
        Assert.Equal(InlineKind.FormatRun, first.SideMap[0].Kind);
        Assert.Equal(1, first.SideMap[0].Format);
    }

    [Fact]
    public void InvalidJson_ReturnsFailure() =>
        _ = LexicalWalker.Extract("{ not json").ExpectFail();
}
