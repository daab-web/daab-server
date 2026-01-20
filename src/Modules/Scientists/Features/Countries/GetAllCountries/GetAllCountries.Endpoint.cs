using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.GetAllCountries;

public class GetAllCountriesEndpoint(IMediator mediator)
    : EndpointWithoutRequest<GetAllCountriesResponse>
{
    public override void Configure()
    {
        Get("/countries");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = await mediator.Send(new GetAllCountriesQuery());

        await Send.OkAsync(response, ct);
    }
}
