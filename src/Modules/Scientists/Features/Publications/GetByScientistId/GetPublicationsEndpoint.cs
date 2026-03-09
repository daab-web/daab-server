using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Publications.GetByScientistId;

public class GetPublicationsEndpoint(IMediator mediator)
    : EndpointWithoutRequest<GetPublicationsResponse>
{
    public override void Configure()
    {
        Get("/scientists/{id}/publications");

        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");

        if (string.IsNullOrWhiteSpace(id))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var response = await mediator.Send(new GetPublicationsQuery(id), ct);

        await Send.OkAsync(response, ct);
    }
}
