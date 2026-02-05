using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Scientists.Features.Applications.Approve;

public sealed class ApproveApplicetionEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Put("/applications/{applicationId}/approve");

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

        var result = await mediator.Send(new ApproveApplicationCommand(applicationId), ct);

        await result.Match(
            scientist =>
                Send.CreatedAtAsync(
                    endpointName: nameof(ApproveApplicetionEndpoint),
                    routeValues: applicationId,
                    responseBody: scientist,
                    generateAbsoluteUrl: true,
                    cancellation: ct
                ),
            error => Send.ResultAsync(TypedResults.Problem(error.ToProblemDetails(HttpContext)))
        );
    }
}
