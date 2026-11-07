using Daab.SharedKernel.Options;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using static LanguageExt.Prelude;

namespace Daab.Modules.Blog.Common;

/// <summary>
/// Posts and comments are stored in every supported language at once: the author writes in one
/// and approves or writes the translations before saving.
/// </summary>
public sealed class BlogLocales(IOptions<LocaleOptions> options)
{
    public IReadOnlyList<string> Supported => options.Value.SupportedLocales;

    /// <summary>Checks that <paramref name="provided"/> covers exactly the supported locales.</summary>
    public Fin<Unit> RequireAll(string sourceLocale, IEnumerable<string> provided)
    {
        var locales = provided.ToHashSet(StringComparer.Ordinal);

        if (!Supported.Contains(sourceLocale) || !locales.Contains(sourceLocale))
        {
            return FinFail<Unit>(
                Error.New(
                    StatusCodes.Status400BadRequest,
                    "The original language must be one of the provided languages"
                )
            );
        }

        var missing = Supported.Where(l => !locales.Contains(l)).ToList();
        if (missing.Count > 0)
        {
            return FinFail<Unit>(
                Error.New(
                    StatusCodes.Status400BadRequest,
                    $"Provide the text in every language. Missing: {string.Join(", ", missing)}"
                )
            );
        }

        var unsupported = locales.Where(l => !Supported.Contains(l)).ToList();
        return unsupported.Count > 0
            ? FinFail<Unit>(
                Error.New(
                    StatusCodes.Status400BadRequest,
                    $"Unsupported languages: {string.Join(", ", unsupported)}"
                )
            )
            : FinSucc(Unit.Default);
    }
}
