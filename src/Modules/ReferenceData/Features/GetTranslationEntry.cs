using Daab.Modules.ReferenceData.Database;
using Daab.Modules.ReferenceData.Domain;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.ReferenceData.Features;

public sealed record GetTranslationEntryResponse(string NameEn, TranslationEntry[] Translations);

public class GetTranslationEntryEndpoint(ReferenceDataDbContext ctx)
    : EndpointWithoutRequest<GetTranslationEntryResponse>
{
    public override void Configure()
    {
        Get("/translations/entry");

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var ns = Query<string>("namespace");
        var nameEn = Query<string>("nameEn");

        if (string.IsNullOrWhiteSpace(ns) || string.IsNullOrWhiteSpace(nameEn))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var key = TranslationKey.From(nameEn);
        var rows = await ctx
            .Translations.Where(t => t.Namespace == ns && t.Key == key)
            .AsNoTracking()
            .ToListAsync(ct);

        if (rows.Count == 0)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var englishValue = rows.SingleOrDefault(t => t.Locale == "en")?.Value;
        var translations = rows.Where(t => t.Locale != "en")
            .Select(t => new TranslationEntry(t.Locale, t.Value))
            .ToArray();

        await Send.OkAsync(
            new GetTranslationEntryResponse(
                string.IsNullOrWhiteSpace(englishValue) ? nameEn : englishValue,
                translations
            ),
            ct
        );
    }
}
