using Daab.Modules.ReferenceData.Database;
using Daab.Modules.ReferenceData.Domain;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.ReferenceData.Features;

public sealed record DeleteTranslationRequest(string Namespace, string NameEn);

public class DeleteTranslationEndpoint(ReferenceDataDbContext ctx)
    : Endpoint<DeleteTranslationRequest>
{
    public override void Configure()
    {
        Delete("/translations");

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteTranslationRequest req, CancellationToken ct)
    {
        var key = TranslationKey.From(req.NameEn);

        await ctx
            .Translations.Where(t => t.Namespace == req.Namespace && t.Key == key)
            .ExecuteDeleteAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
