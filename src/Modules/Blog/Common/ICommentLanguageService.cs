using LanguageExt;

namespace Daab.Modules.Blog.Common;

/// <summary>
/// Language detection and machine translation for comments. Implemented by the host, which
/// bridges to the translation provider; see <c>Daab.Web/Blog</c>.
/// </summary>
public interface ICommentLanguageService
{
    /// <summary>Returns the ISO 639-1 code (e.g. <c>en</c>, <c>az</c>) of the text's language.</summary>
    Task<Fin<string>> DetectLanguageAsync(string text, CancellationToken cancellationToken);

    /// <summary>Translates plain text into <paramref name="targetLocale"/>.</summary>
    Task<Fin<string>> TranslateAsync(
        string text,
        string targetLocale,
        CancellationToken cancellationToken
    );
}
