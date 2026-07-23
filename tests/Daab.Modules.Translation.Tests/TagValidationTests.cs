using Daab.Modules.Translation.Lexical;

namespace Daab.Modules.Translation.Tests;

public sealed class TagValidationTests
{
    [Fact]
    public void IdenticalMultiset_Passes() =>
        Assert.True(LexicalWalker.ValidateTags("a <0>b</0> c", "x <0>y</0> z").Ok);

    [Fact]
    public void ReorderedButBalanced_Passes() =>
        Assert.True(LexicalWalker.ValidateTags("<0>a</0> <1>b</1>", "<1>b</1> <0>a</0>").Ok);

    [Fact]
    public void MissingCloseTag_Fails() =>
        Assert.False(LexicalWalker.ValidateTags("<0>a</0>", "<0>a").Ok);

    [Fact]
    public void InventedId_Fails() =>
        Assert.False(LexicalWalker.ValidateTags("<0>a</0>", "<0>a</0> <1>b</1>").Ok);

    [Fact]
    public void DroppedId_Fails() =>
        Assert.False(LexicalWalker.ValidateTags("<0>a</0> <1>b</1>", "<0>a</0>").Ok);

    [Fact]
    public void DuplicatedId_Fails() =>
        Assert.False(LexicalWalker.ValidateTags("<0>a</0>", "<0>a</0><0>b</0>").Ok);

    [Fact]
    public void LineBreakPreserved_Passes() =>
        Assert.True(LexicalWalker.ValidateTags("a<br0/>b", "c<br0/>d").Ok);

    [Fact]
    public void LineBreakDropped_Fails() =>
        Assert.False(LexicalWalker.ValidateTags("a<br0/>b", "cd").Ok);
}
