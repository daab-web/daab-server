using LanguageExt;

namespace Daab.Modules.Blog.Common;

/// <summary>
/// Machine translation of plain text (post titles and summaries, comments). It only suggests: the
/// author reviews every suggestion before it is saved. Implemented by the host, which bridges to the
/// translation provider; see <c>Daab.Web/Blog</c>.
/// </summary>
public interface ITextTranslator
{
    /// <summary>Translates each text, returning the results in the same order.</summary>
    Task<Fin<IReadOnlyList<string>>> TranslateAsync(
        IReadOnlyList<string> texts,
        string sourceLocale,
        string targetLocale,
        CancellationToken cancellationToken
    );
}
