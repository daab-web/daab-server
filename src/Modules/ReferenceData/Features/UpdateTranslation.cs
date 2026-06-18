using Daab.Modules.ReferenceData.Database;
using Daab.Modules.ReferenceData.Domain;
using Daab.SharedKernel.Middlewares;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.ReferenceData.Features;

public sealed record UpdateTranslationRequest(
    string Namespace,
    string NameEn,
    string? PreviousNameEn,
    TranslationEntry[] Translations
) : ILocalizedCollection
{
    public IEnumerable<string> GetLocales()
    {
        return Translations.Select(t => t.Locale);
    }
}

public class UpdateTranslationEndpoint(ReferenceDataDbContext ctx)
    : Endpoint<UpdateTranslationRequest>
{
    public override void Configure()
    {
        Put("/translations");

        PreProcessor<LocaleCollectionPreProcessor>();

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateTranslationRequest req, CancellationToken ct)
    {
        var oldKey = TranslationKey.From(
            string.IsNullOrWhiteSpace(req.PreviousNameEn) ? req.NameEn : req.PreviousNameEn
        );
        var newKey = TranslationKey.From(req.NameEn);

        // Drop the old entry (and the new key, to avoid stale rows on rename) then re-insert.
        await ctx
            .Translations.Where(t =>
                t.Namespace == req.Namespace && (t.Key == oldKey || t.Key == newKey)
            )
            .ExecuteDeleteAsync(ct);

        var values = new Dictionary<string, string> { ["en"] = req.NameEn };
        foreach (var t in req.Translations)
        {
            values[t.Locale] = t.Name;
        }

        await ctx.Translations.AddRangeAsync(
            values.Select(kv => new Translation
            {
                Locale = kv.Key,
                Namespace = req.Namespace,
                Key = newKey,
                Value = kv.Value,
            }),
            ct
        );

        await ctx.SaveChangesAsync(ct);
    }
}
