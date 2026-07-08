using Daab.Modules.Translation.Memory;

namespace Daab.Modules.Translation.Tests.Memory;

public sealed class SourceKeyTests
{
    [Fact]
    public void Normalize_CollapsesWhitespaceAndTrims() =>
        Assert.Equal("a b c", SourceKey.Normalize("  a\t\n b   c  "));

    [Fact]
    public void Hash_IsStableAcrossWhitespace() =>
        Assert.Equal(SourceKey.Hash("hello world"), SourceKey.Hash("hello   world"));

    [Fact]
    public void Hash_IsStableAcrossUnicodeForm()
    {
        // Code points keep the source pure ASCII: composed ring-A (U+00C5) vs "A" + combining ring (U+030A).
        string composed = ((char)0x00C5).ToString();
        string decomposed = "A" + (char)0x030A;
        Assert.Equal(SourceKey.Hash(composed), SourceKey.Hash(decomposed));
    }

    [Fact]
    public void Hash_DoesNotCaseFold() =>
        Assert.NotEqual(SourceKey.Hash("hello"), SourceKey.Hash("Hello"));

    [Fact]
    public void Plain_StripsTagsAndUnescapes() =>
        Assert.Equal("The quick <fox>", SourceKey.Plain("The <0>quick</0> &lt;fox&gt;"));
}
