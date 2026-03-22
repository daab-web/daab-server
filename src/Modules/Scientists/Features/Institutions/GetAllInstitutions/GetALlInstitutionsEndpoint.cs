using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Institutions.GetAllInstitutions;

public class GetALlInstitutionsEndpoint(IMediator mediator)
    : EndpointWithoutRequest<GetAllInstitutionsResponse>
{
    public override void Configure()
    {
        Get("/institutions");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = await mediator.Send(new GetAllInstitutionsQuery(), ct);

        await Send.OkAsync(response, ct);
    }
}
