using Daab.Modules.Blog.Common;

namespace Daab.Modules.Blog.Tests;

public sealed class LexicalTextTests
{
    [Fact]
    public void Extract_JoinsTextAcrossBlocks()
    {
        const string state = """
            {"root":{"type":"root","children":[
              {"type":"heading","children":[{"type":"text","text":"Title"}]},
              {"type":"paragraph","children":[
                {"type":"text","text":"Hello "},
                {"type":"link","children":[{"type":"text","text":"world"}]}
              ]}
            ]}}
            """;

        var text = LexicalText.Extract(state);

        Assert.Contains("Title", text, StringComparison.Ordinal);
        Assert.Contains("Hello world", text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("not json", false)]
    [InlineData("[]", false)]
    [InlineData("""{"foo":1}""", false)]
    [InlineData("""{"root":{"children":[]}}""", true)]
    public void IsValid_RequiresALexicalRoot(string state, bool expected) =>
        Assert.Equal(expected, LexicalText.IsValid(state));
}
