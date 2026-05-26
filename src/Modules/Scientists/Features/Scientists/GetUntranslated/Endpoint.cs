using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel.Options;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Daab.Modules.Scientists.Features.Scientists.GetUntranslated;

public class GetUntranslatedEndpoint(ScientistsDbContext ctx, IOptionsMonitor<LocaleOptions> opts)
    : EndpointWithoutRequest
{
    private readonly LocaleOptions _localeOptions = opts.CurrentValue;

    public override void Configure()
    {
        Get("/scientists/untranslated");
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
            .ScientistTranslations.Where(t => _localeOptions.SupportedLocales.Contains(t.Locale))
            .GroupBy(t => t.ScientistId)
            .Select(g => new
            {
                NewsId = g.Key,
                TranslatedLocales = g.Select(t => t.Locale).ToList(),
            })
            .ToListAsync(ct);

        var newsIdsWithAnyTranslation = newsWithTranslations.Select(x => x.NewsId).ToHashSet();

        var untranslatedNewsIds = await ctx
            .Scientists.Where(n => !newsIdsWithAnyTranslation.Contains(n.Id))
            .Select(n => n.Id)
            .ToListAsync(ct);

        var result = newsWithTranslations
            .Select(x => new UntranslatedBreakdown
            {
                ScientistId = x.NewsId,
                MissingLocales = _localeOptions
                    .SupportedLocales.Except(x.TranslatedLocales)
                    .ToList(),
            })
            .Where(x => x.MissingLocales.Count > 0) // skip fully translated
            .Concat(
                untranslatedNewsIds.Select(id => new UntranslatedBreakdown
                {
                    ScientistId = id,
                    MissingLocales = _localeOptions.SupportedLocales.ToList(), // all locales missing
                })
            )
            .ToList();

        await Send.OkAsync(result, ct);
    }

    public record UntranslatedBreakdown
    {
        public required string ScientistId { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public List<string> MissingLocales { get; init; } = [];
    }
}
