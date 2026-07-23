using Daab.Modules.Translation.Models;
using Daab.Modules.Translation.Service;
using Daab.SharedKernel.Extensions;
using FastEndpoints;

namespace Daab.Web.Translation;

/// <summary>Body for an approval: the original document and its approved translation.</summary>
public sealed record ApproveTranslationRequest(
    string SourceEditorStateJson,
    string TargetEditorStateJson,
    ContentType ContentType,
    string SourceLocale,
    string TargetLocale
);

/// <summary>
/// Folds an approved translation back into the memory, closing the learning loop. Returns how many
/// block pairs were newly stored versus already remembered.
/// </summary>
public sealed class ApproveTranslationEndpoint(ITranslationService service)
    : Endpoint<ApproveTranslationRequest, ApprovalResult>
{
    public override void Configure()
    {
        Post("/translations/approve");

        // TODO: restrict to editors once auth is wired for this surface.
        AllowAnonymous();
    }

    public override async Task HandleAsync(ApproveTranslationRequest req, CancellationToken ct)
    {
        var result = await service.ApproveDocumentAsync(
            new ApprovalRequest(
                req.SourceEditorStateJson,
                req.TargetEditorStateJson,
                req.ContentType,
                req.SourceLocale,
                req.TargetLocale
            ),
            ct
        );

        await result.Match(
            approval => Send.OkAsync(approval, ct),
            error => error.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
