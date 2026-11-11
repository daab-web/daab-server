using Daab.SharedKernel.Middlewares;

namespace Daab.Modules.Blog.Features.Translations.SuggestTranslation;

/// <param name="Texts">Plain texts to translate, e.g. a post's title and summary or a comment.</param>
public sealed record SuggestTranslationRequest(
    string SourceLocale,
    string TargetLocale,
    List<string> Texts
) : ILocalizedCollection
{
    public IEnumerable<string> GetLocales() => [SourceLocale, TargetLocale];
}
