using Daab.Modules.Scientists.Persistence;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Scientists.Features.GetAllCountries;

public sealed class GetAllCountriesQueryHandler(ScientistsContext context)
    : IRequestHandler<GetAllCountriesQuery, GetAllCountriesResponse>
{
    public Task<GetAllCountriesResponse> Handle(
        GetAllCountriesQuery _,
        CancellationToken cancellationToken
    )
    {
        var countries = context.Scientists.Select(s => s.Countries).Flatten().Distinct().ToArray();

        return Task.FromResult(new GetAllCountriesResponse(countries));
    }
}
