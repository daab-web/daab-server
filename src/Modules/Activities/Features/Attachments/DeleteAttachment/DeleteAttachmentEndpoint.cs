using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Activities.Features.Attachments.DeleteAttachment;

public class DeleteAttachmentEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/news/{articleId}/attachments/{attachmentId}");

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var articleId = Route<string>("articleId");
        var attachmentId = Route<string>("attachmentId");

        if (string.IsNullOrEmpty(articleId) || string.IsNullOrEmpty(attachmentId))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var res = await mediator.Send(new DeleteAttachmentCommand(articleId, attachmentId), ct);

        await res.Match(
            entity => Send.OkAsync(entity, cancellation: ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
