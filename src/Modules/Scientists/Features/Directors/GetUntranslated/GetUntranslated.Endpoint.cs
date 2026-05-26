using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel.Options;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Daab.Modules.Scientists.Features.Directors.GetUntranslated;

public class GetUntranslated(ScientistsDbContext ctx, IOptionsMonitor<LocaleOptions> opts)
    : EndpointWithoutRequest
{
    private readonly LocaleOptions _localeOpotions = opts.CurrentValue;

    public override void Configure()
    {
        Get("/directors/untranslated");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // var result = await ctx
        //     .Scientists.Select(s => new UntranslatedBreakdown
        //     {
        //         ScientistId = s.Id,
        //         MissingLocales = _localeOpotions.SupportedLocales
        //             .Where(locale => !s.Translations.Any(t => t.Locale == locale))
        //             .ToList(),
        //     })
        //     .Where(x => x.MissingLocales.Count > 0) // only incomplete ones
        //     .ToListAsync(ct);

        var newsWithTranslations = await ctx
            .ScientistTranslations.Where(t => _localeOpotions.SupportedLocales.Contains(t.Locale))
            .GroupBy(t => t.ScientistId)
            .Select(g => new
            {
                NewsId = g.Key,
                TranslatedLocales = g.Select(t => t.Locale).ToList(),
            })
            .ToListAsync(ct);

        // 2. Also get news IDs with NO translations at all (they won't appear in the join above)
        var newsIdsWithAnyTranslation = newsWithTranslations.Select(x => x.NewsId).ToHashSet();

        var untranslatedNewsIds = await ctx
            .Scientists.Where(n => !newsIdsWithAnyTranslation.Contains(n.Id))
            .Select(n => n.Id)
            .ToListAsync(ct);

        // 3. Compute missing locales in memory — no APPLY needed
        var result = newsWithTranslations
            .Select(x => new UntranslatedBreakdown
            {
                ScientistId = x.NewsId,
                MissingLocales = _localeOpotions
                    .SupportedLocales.Except(x.TranslatedLocales)
                    .ToList(),
            })
            .Where(x => x.MissingLocales.Count > 0) // skip fully translated
            .Concat(
                untranslatedNewsIds.Select(id => new UntranslatedBreakdown
                {
                    ScientistId = id,
                    MissingLocales = _localeOpotions.SupportedLocales.ToList(), // all locales missing
                })
            )
            .ToList();

        await Send.OkAsync(result, ct);
    }

    public record UntranslatedBreakdown
    {
        public required string ScientistId { get; init; }
        public List<string> MissingLocales { get; init; } = [];
    }
}
