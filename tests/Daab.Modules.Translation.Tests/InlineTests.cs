using Daab.Modules.Translation.Lexical;
using static Daab.Modules.Translation.Tests.LexicalDocs;

namespace Daab.Modules.Translation.Tests;

public sealed class InlineTests
{
    [Fact]
    public void Escaping_LiteralAngleBrackets_AreNotTreatedAsTags()
    {
        var json = Doc(Paragraph(Text("for x<0 & y>1")));
        var block = Assert.Single(LexicalWalker.Extract(json).ExpectSucc().Blocks);

        Assert.Equal("for x&lt;0 &amp; y&gt;1", block.Source);
        Assert.Empty(block.SideMap);
        AssertNoOp(json, block);
    }

    [Fact]
    public void Escaping_TextThatLooksLikeATag_RoundTripsAsText()
    {
        var json = Doc(Paragraph(Text("<0>hi</0>")));
        var block = Assert.Single(LexicalWalker.Extract(json).ExpectSucc().Blocks);

        Assert.Equal("&lt;0&gt;hi&lt;/0&gt;", block.Source);
        Assert.Empty(block.SideMap);
        AssertNoOp(json, block);
    }

    [Fact]
    public void Plain_ExtractThenReinsert_RoundTripsSpecialCharacters()
    {
        const string title = "R&D <x> & more";
        Assert.Equal(title, LexicalWalker.ReinsertPlain(LexicalWalker.ExtractPlain(title)));
    }

    [Fact]
    public void Mixed_BoldRunMidParagraph_ProducesTaggedRun()
    {
        var json = Doc(Paragraph(Text("The "), Text("quick", 1), Text(" brown fox")));
        var block = Assert.Single(LexicalWalker.Extract(json).ExpectSucc().Blocks);

        Assert.Equal("The <0>quick</0> brown fox", block.Source);
        Assert.Equal(1, block.SideMap[0].Format);
        AssertNoOp(json, block);
    }

    [Fact]
    public void Mixed_TagIds_RestartPerBlock()
    {
        var json = Doc(
            Paragraph(Text("first "), Text("bold", 1)),
            Paragraph(Text("second "), Text("bold", 1))
        );
        var blocks = LexicalWalker.Extract(json).ExpectSucc().Blocks;

        Assert.Equal(2, blocks.Count);
        Assert.True(blocks[0].SideMap.ContainsKey(0));
        Assert.True(blocks[1].SideMap.ContainsKey(0));
    }

    [Fact]
    public void Mixed_TranslatedRun_ReappliesFormatBitmask()
    {
        var json = Doc(Paragraph(Text("The "), Text("quick", 1), Text(" brown fox")));
        var result = LexicalWalker
            .Reinsert(json, new Dictionary<int, string> { [0] = "Sürətli <0>qonur</0> tülkü" })
            .ExpectSucc();

        var children = Children(result, 0);
        Assert.Equal(3, children.Count);
        Assert.Equal("Sürətli ", (string)children[0]!["text"]!);
        Assert.Equal(0, (int)children[0]!["format"]!);
        Assert.Equal("qonur", (string)children[1]!["text"]!);
        Assert.Equal(1, (int)children[1]!["format"]!);
        Assert.Equal(" tülkü", (string)children[2]!["text"]!);
    }

    [Fact]
    public void Link_TranslatedAnchor_PreservesUrl()
    {
        var json = Doc(Paragraph(Link("read more", "https://example.org/a")));
        var result = LexicalWalker
            .Reinsert(json, new Dictionary<int, string> { [0] = "<0>daha ətraflı</0>" })
            .ExpectSucc();

        var link = Children(result, 0)[0]!;
        Assert.Equal("link", (string)link["type"]!);
        Assert.Equal("https://example.org/a", (string)link["url"]!);
        Assert.Equal("daha ətraflı", (string)link["children"]![0]!["text"]!);
    }

    [Fact]
    public void EmptySpacerParagraph_IsSkipped_ButPreservedOnReinsert()
    {
        var json = Doc(Paragraph(Text("above")), Paragraph(), Paragraph(Text("below")));
        var extract = LexicalWalker.Extract(json).ExpectSucc();
        Assert.Equal(2, extract.Blocks.Count);

        var translations = extract.Blocks.ToDictionary(b => b.Index, b => b.Source);
        var result = LexicalWalker.Reinsert(json, translations).ExpectSucc();
        Assert.True(LexicalWalker.TreesEquivalent(json, result));
    }

    static void AssertNoOp(string json, LexicalBlock block)
    {
        var result = LexicalWalker
            .Reinsert(json, new Dictionary<int, string> { [block.Index] = block.Source })
            .ExpectSucc();
        Assert.True(LexicalWalker.TreesEquivalent(json, result));
    }
}
