using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel.Entities;
using Daab.SharedKernel.Options;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Daab.Modules.Scientists.Features.Directors.GetUntranslated;

public sealed class UntranslatedBreakdown
{
    public required string DirectorId { get; init; }
    public required string Slug { get; init; }
    public required string Status { get; init; }
    public List<string> MissingLocales { get; init; } = [];
}

public class GetUntranslated(ScientistsDbContext ctx, IOptionsMonitor<LocaleOptions> opts)
    : EndpointWithoutRequest<IEnumerable<UntranslatedBreakdown>>
{
    private readonly LocaleOptions _localeOptions = opts.CurrentValue;

    public override void Configure()
    {
        Get("/directors/untranslated");

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var directors = await ctx
            .Directors.Include(d => d.Scientist)
            .Where(d => d.Status != EntityStatus.Published)
            .AsNoTracking()
            .ToListAsync(ct);

        var result = directors.Select(d => new UntranslatedBreakdown
        {
            DirectorId = d.Id,
            Slug = d.Scientist.Slug,
            Status = d.Status.ToString(),
            MissingLocales = _localeOptions
                .SupportedLocales.Except(d.RoleTranslations.Keys)
                .ToList(),
        });

        await Send.OkAsync(result, ct);
    }
}
