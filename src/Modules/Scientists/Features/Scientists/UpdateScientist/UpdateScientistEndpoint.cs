using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Scientists.Features.Scientists.UpdateScientist;

public class UpdateScientistEndpoint(IMediator mediator)
    : Endpoint<UpdateScientistRequest, UpdateScientistResponse>
{
    public override void Configure()
    {
        Put("/scientists/{id}");
        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateScientistRequest req, CancellationToken ct)
    {
        var id = Route<string>("id");
        if (string.IsNullOrEmpty(id))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var res = await mediator.Send(new UpdateScientistCommand(id, req), ct);

        await res.Match(
            entity => Send.OkAsync(entity, cancellation: ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
