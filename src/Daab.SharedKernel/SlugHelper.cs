using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Daab.SharedKernel;

public static partial class SlugHelper
{
    private static readonly Dictionary<char, string> CharMap = new()
    {
        { 'ə', "e" },
        { 'Ə', "E" },
        { 'ı', "i" },
        { 'İ', "I" },
        { 'ğ', "g" },
        { 'Ğ', "G" },
        { 'ş', "s" },
        { 'Ş', "S" },
        { 'ç', "c" },
        { 'Ç', "C" },
        { 'ö', "o" },
        { 'Ö', "O" },
        { 'ü', "u" },
        { 'Ü', "U" },
    };

    public static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        var slug = title.ToLowerInvariant();
        slug = Transliterate(slug);
        slug = LettersRegex().Replace(slug, "-");
        slug = SymbolsRegex().Replace(slug, "-");
        slug = slug.Trim('-');

        return slug;
    }

    private static string Transliterate(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (char c in text)
        {
            if (CharMap.TryGetValue(c, out var mapped))
                sb.Append(mapped);
            else
                sb.Append(c);
        }

        var decomposed = sb.ToString().Normalize(NormalizationForm.FormD);
        sb.Clear();

        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString();
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex LettersRegex();

    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex SymbolsRegex();
}
