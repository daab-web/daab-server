using Daab.Modules.Blog.Common;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Blog.Features.Translations.SuggestTranslation;

/// <summary>
/// AI translation suggestion for text the user is writing. Nothing is stored: the user approves
/// the suggestion or replaces it with their own translation before saving.
/// </summary>
public sealed class SuggestTranslationCommandHandler(ITextTranslator translator)
    : IRequestHandler<SuggestTranslationCommand, Fin<SuggestTranslationResponse>>
{
    public async Task<Fin<SuggestTranslationResponse>> Handle(
        SuggestTranslationCommand request,
        CancellationToken cancellationToken
    )
    {
        var nonEmpty = request
            .Texts.Select((text, index) => (text, index))
            .Where(x => !string.IsNullOrWhiteSpace(x.text))
            .ToList();

        var results = new string[request.Texts.Count];
        if (nonEmpty.Count == 0)
        {
            return new SuggestTranslationResponse(results.Select(_ => string.Empty).ToList());
        }

        var translated = await translator.TranslateAsync(
            nonEmpty.ConvertAll(x => x.text.Trim()),
            request.SourceLocale,
            request.TargetLocale,
            cancellationToken
        );

        if (translated.IsFail)
        {
            return Error.New(
                StatusCodes.Status503ServiceUnavailable,
                "Translation is unavailable right now. Please try again later"
            );
        }

        var list = translated.Match(t => t, _ => []);
        for (var i = 0; i < nonEmpty.Count; i++)
        {
            results[nonEmpty[i].index] = i < list.Count ? list[i] : string.Empty;
        }

        return new SuggestTranslationResponse(results.Select(r => r ?? string.Empty).ToList());
    }
}
