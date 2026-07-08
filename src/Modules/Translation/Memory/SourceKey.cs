using System.Security.Cryptography;
using System.Text;
using Daab.Modules.Translation.Lexical;

namespace Daab.Modules.Translation.Memory;

/// <summary>
/// Derives the exact-match hash and the embedder input from a tagged block string. Normalization
/// (NFC + trim + whitespace-collapse, no case-folding, tags kept) defines the edit-cache guarantee:
/// an approved target is re-served only for the same unit up to insignificant whitespace/form noise.
/// </summary>
internal static class SourceKey
{
    public static string Normalize(string text) =>
        string.Join(
            ' ',
            text.Normalize(NormalizationForm.FormC)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
        );

    public static string Hash(string taggedSource) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(Normalize(taggedSource))));

    /// <summary>The plain, tag-stripped, unescaped text handed to the embedder (tags are noise for cosine).</summary>
    public static string Plain(string taggedSource)
    {
        var builder = new StringBuilder(taggedSource.Length);
        int i = 0;
        while (i < taggedSource.Length)
        {
            if (taggedSource[i] == '<')
            {
                i = TagCodec.MatchTag(taggedSource, i).End;
            }
            else
            {
                builder.Append(taggedSource[i]);
                i++;
            }
        }
        return Normalize(TagCodec.Unescape(builder.ToString()));
    }
}
