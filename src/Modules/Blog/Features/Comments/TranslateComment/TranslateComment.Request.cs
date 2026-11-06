using Daab.SharedKernel.Middlewares;

namespace Daab.Modules.Blog.Features.Comments.TranslateComment;

/// <param name="Locale">The locale to translate into (the reader's app language).</param>
public sealed record TranslateCommentRequest(string Id, string Locale) : ILocalizedCollection
{
    public IEnumerable<string> GetLocales() => [Locale];
}
