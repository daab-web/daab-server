using MediatR;

namespace Daab.Modules.Scientists.Features.Countries.GetAllCountries;

public sealed record GetAllCountriesQuery : IRequest<GetAllCountriesResponse> { }
