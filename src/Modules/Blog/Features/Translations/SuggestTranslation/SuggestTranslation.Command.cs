using LanguageExt;
using MediatR;

namespace Daab.Modules.Blog.Features.Translations.SuggestTranslation;

public sealed record SuggestTranslationCommand(
    string SourceLocale,
    string TargetLocale,
    IReadOnlyList<string> Texts
) : IRequest<Fin<SuggestTranslationResponse>>;

/// <param name="Translations">One suggestion per input text, in the same order.</param>
public sealed record SuggestTranslationResponse(IReadOnlyList<string> Translations);
