using Daab.Modules.ReferenceData.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.ReferenceData.Features;

public sealed record TranslationName(string Key, string Name);

public class GetTranslationNamesEndpoint(ReferenceDataDbContext ctx)
    : EndpointWithoutRequest<TranslationName[]>
{
    public override void Configure()
    {
        Get("/translations/names");

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var ns = Query<string>("namespace");
        var locale = Query<string>("locale", isRequired: false) ?? "en";

        if (string.IsNullOrWhiteSpace(ns))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var names = await ctx
            .Translations.Where(t => t.Namespace == ns && t.Locale == locale)
            .OrderBy(t => t.Value)
            .Select(t => new TranslationName(t.Key, t.Value))
            .AsNoTracking()
            .ToArrayAsync(ct);

        await Send.OkAsync(names, ct);
    }
}
