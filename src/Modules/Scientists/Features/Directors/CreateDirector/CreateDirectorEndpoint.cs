using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Directors.CreateDirector;

public class CreateDirectorEndpoint(IMediator mediator)
    : Endpoint<CreateDirectorRequest, CreateDirectorResponse>
{
    public override void Configure()
    {
        Post("/directors");

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateDirectorRequest req, CancellationToken ct)
    {
        var res = await mediator.Send(new CreateDirectorCommand(req), ct);

        await res.Match(
            entity =>
                Send.CreatedAtAsync(
                    endpointName: nameof(CreateDirectorEndpoint),
                    routeValues: null,
                    responseBody: entity,
                    generateAbsoluteUrl: true,
                    cancellation: ct
                ),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
