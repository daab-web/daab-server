using Daab.Modules.Scientists.Features.Applications.Apply;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Scientists.Features.Apply;

public sealed class ApplyEndpoint(IMediator mediator) : Endpoint<ApplyRequest, ApplyResponse>
{
    public override void Configure()
    {
        Post("/applications");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ApplyRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new ApplyCommand(req), ct);

        await result.Match(
            async application =>
                await Send.CreatedAtAsync(
                    endpointName: nameof(ApplyEndpoint),
                    routeValues: null,
                    responseBody: application,
                    generateAbsoluteUrl: true,
                    cancellation: ct
                ),
            async err =>
                await Send.ResultAsync(TypedResults.Problem(err.ToProblemDetails(HttpContext)))
        );
    }
}
