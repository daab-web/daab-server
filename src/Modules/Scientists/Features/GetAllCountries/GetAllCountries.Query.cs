using MediatR;

namespace Daab.Modules.Scientists.Features.GetAllCountries;

public sealed record GetAllCountriesQuery : IRequest<GetAllCountriesResponse> { }
