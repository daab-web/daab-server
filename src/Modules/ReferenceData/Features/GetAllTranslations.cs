using Daab.Modules.ReferenceData.Database;
using Daab.SharedKernel.Entities;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.ReferenceData.Features;

public class GetAllTranslations(ReferenceDataDbContext ctx, ILocaleResolver localeResolver)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/translations");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var locale = localeResolver.Resolve();

        var translations = await ctx
            .Translations.Where(t => t.Locale == locale)
            .AsNoTracking()
            .ToListAsync(ct);

        var response = translations
            .GroupBy(t => t.Namespace)
            .ToDictionary(
                nsGroup => nsGroup.Key,
                nsGroup => nsGroup.ToDictionary(t => t.Key, t => t.Value)
            );

        await Send.OkAsync(response, ct);
    }
}
