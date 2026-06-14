using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.Attachments.DeleteAttachment;

public sealed class DeleteAttachmentCommandHandler(
    ActivitiesDbContext context,
    IBlobStorage blobStorage
) : IRequestHandler<DeleteAttachmentCommand, Fin<bool>>
{
    public async Task<Fin<bool>> Handle(
        DeleteAttachmentCommand request,
        CancellationToken cancellationToken
    )
    {
        var attachment = await context.Attachments.FindAsync(
            [request.AttachmentId],
            cancellationToken
        );

        if (attachment is null || attachment.ParentObjectId != request.ArticleId)
        {
            return Error.New(
                StatusCodes.Status404NotFound,
                $"Attachment with an Id of {request.AttachmentId} not found"
            );
        }

        await blobStorage.DeleteAsync(
            "activities",
            $"news/{request.ArticleId}/attachments/{request.AttachmentId}.webp",
            cancellationToken
        );

        context.Attachments.Remove(attachment);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
