using Daab.Modules.Scientists.Features.Applications.Approve;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Applications.Reject;

public sealed class RejectApplicationEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/applications/{applicationId}/reject");

        // TODO: this should be available for admins only
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var applicationId = Route<string>("applicationId");

        if (applicationId is null)
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await mediator.Send(new RejectApplicationCommand(applicationId), ct);
        await Send.OkAsync(true, ct);
    }
}
