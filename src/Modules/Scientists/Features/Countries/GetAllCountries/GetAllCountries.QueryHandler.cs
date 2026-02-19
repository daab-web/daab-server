using Daab.Modules.Scientists.Persistence;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Countries.GetAllCountries;

public sealed class GetAllCountriesQueryHandler(ScientistsDbContext context)
    : IRequestHandler<GetAllCountriesQuery, GetAllCountriesResponse>
{
    public Task<GetAllCountriesResponse> Handle(
        GetAllCountriesQuery _,
        CancellationToken cancellationToken
    )
    {
        var countries = context
            .Scientists.AsNoTracking()
            .Select(s => s.Countries)
            .Flatten()
            .Order()
            .Distinct()
            .ToArray();

        return Task.FromResult(new GetAllCountriesResponse(countries));
    }
}
