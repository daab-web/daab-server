using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Publications.UpdatePublications;

public class UpdatePublicationEndpoint(IMediator mediator)
    : Endpoint<UpdatePublicationRequest, UpdatePublicationResponse>
{
    public override void Configure()
    {
        Put("/publications/{id}");
        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdatePublicationRequest req, CancellationToken ct)
    {
        var id = Route<string>("id");
        if (string.IsNullOrEmpty(id))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var res = await mediator.Send(new UpdatePublicationCommand(req with { Id = id }), ct);

        await res.Match(
            entity => Send.OkAsync(entity, cancellation: ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
