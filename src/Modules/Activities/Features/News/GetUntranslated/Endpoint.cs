using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel.Options;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Daab.Modules.Activities.Features.News.GetUntranslated;

public class Endpoint(ActivitiesDbContext ctx, IOptionsMonitor<LocaleOptions> opts)
    : EndpointWithoutRequest
{
    private readonly LocaleOptions _localeOptions = opts.CurrentValue;

    public override void Configure()
    {
        Get("/news/untranslated");
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
            .NewsTranslations.Where(t => _localeOptions.SupportedLocales.Contains(t.Locale))
            .GroupBy(t => t.NewsId)
            .Select(g => new
            {
                NewsId = g.Key,
                TranslatedLocales = g.Select(t => t.Locale).ToList(),
            })
            .ToListAsync(ct);

        var newsIdsWithAnyTranslation = newsWithTranslations.Select(x => x.NewsId).ToHashSet();

        var untranslatedNewsIds = await ctx
            .News.Where(n => !newsIdsWithAnyTranslation.Contains(n.Id))
            .Select(n => new { n.Id, n.Title })
            .ToListAsync(ct);

        var result = newsWithTranslations
            .Select(x => new UntranslatedBreakdown
            {
                NewsId = x.NewsId,
                MissingLocales = _localeOptions
                    .SupportedLocales.Except(x.TranslatedLocales)
                    .ToList(),
            })
            .Where(x => x.MissingLocales.Count > 0) // skip fully translated
            .Concat(
                untranslatedNewsIds.Select(obj => new UntranslatedBreakdown
                {
                    NewsId = obj.Id,
                    Title = obj.Title,
                    MissingLocales = _localeOptions.SupportedLocales.ToList(), // all locales missing
                })
            )
            .ToList();

        await Send.OkAsync(result, ct);
    }

    public record UntranslatedBreakdown
    {
        public required string NewsId { get; init; }
        public string? Title { get; init; }
        public List<string> MissingLocales { get; init; } = [];
    }
}
