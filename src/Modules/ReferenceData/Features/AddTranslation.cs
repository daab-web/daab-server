using Daab.Modules.ReferenceData.Database;
using Daab.Modules.ReferenceData.Domain;
using Daab.SharedKernel.Middlewares;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.ReferenceData.Features;

public sealed record TranslationEntry(string Locale, string Name);

public sealed record AddTranslationRequest(
    string Namespace,
    string NameEn,
    TranslationEntry[] Translations
) : ILocalizedCollection
{
    public IEnumerable<string> GetLocales()
    {
        return Translations.Select(t => t.Locale);
    }
}

public class AddTranslationEndpoint(ReferenceDataDbContext ctx) : Endpoint<AddTranslationRequest>
{
    public override void Configure()
    {
        Post("/translations");

        PreProcessor<LocaleCollectionPreProcessor>();

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddTranslationRequest req, CancellationToken ct)
    {
        var key = TranslationKey.From(req.NameEn);

        // English name is always the base entry; per-locale overrides follow.
        var values = new Dictionary<string, string> { ["en"] = req.NameEn };
        foreach (var t in req.Translations)
        {
            values[t.Locale] = t.Name;
        }

        var existing = await ctx
            .Translations.Where(t => t.Namespace == req.Namespace && t.Key == key)
            .ToListAsync(ct);

        foreach (var (locale, value) in values)
        {
            var row = existing.SingleOrDefault(t => t.Locale == locale);
            if (row is null)
            {
                await ctx.Translations.AddAsync(
                    new Translation
                    {
                        Locale = locale,
                        Namespace = req.Namespace,
                        Key = key,
                        Value = value,
                    },
                    ct
                );
            }
            else
            {
                row.Value = value;
            }
        }

        await ctx.SaveChangesAsync(ct);
    }
}
