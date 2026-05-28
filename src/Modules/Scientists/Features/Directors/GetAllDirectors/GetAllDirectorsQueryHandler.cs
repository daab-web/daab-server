using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Directors.GetAllDirectors;

public class GetAllDirectorsQueryHandler(ScientistsDbContext context)
    : IRequestHandler<GetAllDirectorsQuery, IEnumerable<DirectorResponse>>
{
    public async Task<IEnumerable<DirectorResponse>> Handle(
        GetAllDirectorsQuery request,
        CancellationToken cancellationToken
    )
    {
        var directors = await context
            .Directors.AsNoTracking()
            .Where(d => d.Status == EntityStatus.Published)
            .Include(d => d.Scientist)
                .ThenInclude(s => s.Translations.Where(t => t.Locale == request.Locale))
            .ToListAsync(cancellationToken);

        var response = directors.Select(s => MapToResponse(s, request.Locale));

        return response;
    }

    private static DirectorResponse MapToResponse(Director d, string locale)
    {
        var t = d.Scientist.Translations.FirstOrDefault();
        ArgumentNullException.ThrowIfNull(t);
        var role =
            d.RoleTranslations.GetValueOrDefault(locale)
            ?? d.RoleTranslations.GetValueOrDefault("en")
            ?? throw new InvalidOperationException($"No role translation for d {d.Id}");

        return new DirectorResponse(
            d.Id,
            d.ScientistId,
            d.Scientist.PhotoUrl,
            t.FirstName!,
            t.LastName!,
            role,
            d.Scientist.AcademicTitle,
            [.. d.Scientist.Countries]
        );
    }
}
