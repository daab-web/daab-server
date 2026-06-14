using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.DeleteThumbnail;

public sealed class DeleteThumbnailCommandHandler(
    ActivitiesDbContext context,
    IBlobStorage blobStorage
) : IRequestHandler<DeleteThumbnailCommand, Fin<bool>>
{
    public async Task<Fin<bool>> Handle(
        DeleteThumbnailCommand request,
        CancellationToken cancellationToken
    )
    {
        var news = await context.News.FindAsync([request.NewsId], cancellationToken);

        if (news is null)
        {
            return Error.New(
                StatusCodes.Status404NotFound,
                $"News with an Id of {request.NewsId} not found"
            );
        }

        await blobStorage.DeleteAsync(
            "activities",
            $"news/{request.NewsId}/thumbnails/thumbnail.webp",
            cancellationToken
        );

        news.Thumbnail = null;
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
