using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel.Entities;
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

        var result = await ctx
            .News.Where(n => n.Status != EntityStatus.Published)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Status,
                TranslatedLocales = n
                    .Translations.Where(t => _localeOptions.SupportedLocales.Contains(t.Locale))
                    .Select(t => t.Locale)
                    .ToList(),
            })
            .ToListAsync(ct);

        var breakdown = result
            .Select(n => new UntranslatedBreakdown
            {
                NewsId = n.Id,
                Title = n.Title,
                Status = n.Status.ToString(),
                MissingLocales = _localeOptions
                    .SupportedLocales.Except(n.TranslatedLocales)
                    .ToList(),
            })
            .ToList();

        await Send.OkAsync(breakdown, ct);
    }

    public record UntranslatedBreakdown
    {
        public required string NewsId { get; init; }
        public string? Title { get; init; }
        public string? Status { get; init; }
        public List<string> MissingLocales { get; init; } = [];
    }
}
