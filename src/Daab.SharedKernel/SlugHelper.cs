using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Daab.SharedKernel;

public static partial class SlugHelper
{
    public static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        // Lowercase
        var slug = title.ToLowerInvariant();

        // Replace accented characters with ASCII equivalents
        slug = RemoveDiacritics(slug);

        // Replace anything that's not a letter, digit, or space with a hyphen
        slug = LettersRegex().Replace(slug, "-");

        // Replace multiple spaces/hyphens with a single hyphen
        slug = SymbolsRegex().Replace(slug, "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        return slug;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var chars = normalized.Where(c =>
            CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark
        );
        return new string([.. chars]).Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex LettersRegex();

    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex SymbolsRegex();
}
