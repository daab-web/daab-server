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

        var result = await ctx
            .Scientists.Select(s => new
            {
                s.Id,
                s.Slug,
                s.Status,
                TranslatedLocales = s
                    .Translations.Where(t => _localeOptions.SupportedLocales.Contains(t.Locale))
                    .Select(t => t.Locale)
                    .ToList(),
            })
            .ToListAsync(ct);

        var breakdown = result
            .Select(s => new UntranslatedBreakdown
            {
                ScientistId = s.Id,
                Slug = s.Slug,
                Status = s.Status.ToString(),
                MissingLocales = _localeOptions
                    .SupportedLocales.Except(s.TranslatedLocales)
                    .ToList(),
            })
            .Where(x => x.MissingLocales.Count > 0)
            .ToList();

        await Send.OkAsync(breakdown, ct);
    }

    public record UntranslatedBreakdown
    {
        public required string ScientistId { get; init; }
        public required string Slug { get; init; }
        public required string Status { get; init; }
        public List<string> MissingLocales { get; init; } = [];
    }
}
