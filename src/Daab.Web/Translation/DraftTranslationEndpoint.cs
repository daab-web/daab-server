using Daab.Modules.Translation.Models;
using Daab.Modules.Translation.Service;
using Daab.SharedKernel.Extensions;
using FastEndpoints;

namespace Daab.Web.Translation;

/// <summary>Body for a draft-translation request. ContentType binds by its numeric value.</summary>
public sealed record DraftTranslationRequest(
    string EditorStateJson,
    ContentType ContentType,
    string SourceLocale,
    string TargetLocale,
    int MaxExamples = 5
);

/// <summary>Translates one Lexical document, returning the draft and its production accounting.</summary>
public sealed class DraftTranslationEndpoint(ITranslationService service)
    : Endpoint<DraftTranslationRequest, TranslationDraft>
{
    public override void Configure()
    {
        Post("/translations/draft");

        // TODO: restrict to editors once auth is wired for this surface.
        AllowAnonymous();
    }

    public override async Task HandleAsync(DraftTranslationRequest req, CancellationToken ct)
    {
        var result = await service.TranslateDocumentAsync(
            new TranslationRequest(
                req.EditorStateJson,
                req.ContentType,
                req.SourceLocale,
                req.TargetLocale,
                req.MaxExamples
            ),
            ct
        );

        await result.Match(
            draft => Send.OkAsync(draft, ct),
            error => error.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
