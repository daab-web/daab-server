using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.DeleteProfilePicture;

public class DeleteProfilePictureEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/scientists/{scientistId}/profile-picture");

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var scientistId = Route<string>("scientistId");

        if (string.IsNullOrEmpty(scientistId))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var res = await mediator.Send(new DeleteProfilePictureCommand(scientistId), ct);

        await res.Match(
            entity => Send.OkAsync(entity, cancellation: ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
