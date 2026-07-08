using Daab.Modules.Translation.Lexical;
using static Daab.Modules.Translation.Tests.LexicalDocs;

namespace Daab.Modules.Translation.Tests;

public sealed class FailureTests
{
    [Fact]
    public void Reinsert_UnbalancedTranslationTags_Fails()
    {
        var json = Doc(Paragraph(Text("hello", 1)));
        var result = LexicalWalker.Reinsert(json, new Dictionary<int, string> { [0] = "<0>salam" });
        _ = result.ExpectFail();
    }

    [Fact]
    public void Reinsert_EmptyTranslation_Fails()
    {
        var json = Doc(Paragraph(Text("hello")));
        var result = LexicalWalker.Reinsert(json, new Dictionary<int, string> { [0] = "   " });
        _ = result.ExpectFail();
    }

    [Fact]
    public void Reinsert_InventedTagId_Fails()
    {
        var json = Doc(Paragraph(Text("hello", 1)));
        var result = LexicalWalker.Reinsert(json, new Dictionary<int, string> { [0] = "<9>x</9>" });
        _ = result.ExpectFail();
    }
}
