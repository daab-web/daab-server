using System.Threading.Channels;
using Daab.Modules.Activities.Common;
using Daab.Modules.Activities.Messages;
using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Daab.Modules.Activities.Features.News.UpdateNews;

public sealed class UpdateNewsCommandHandler(
    ActivitiesDbContext context,
    IBlobStorage blobStorage,
    [FromKeyedServices(ChannelKeys.ThumbnailUpload)]
        Channel<ThumbnailUploadMessage> thumbnailUploadChannel
) : IRequestHandler<UpdateNewsCommand, Fin<UpdateNewsResponse>>
{
    public async Task<Fin<UpdateNewsResponse>> Handle(
        UpdateNewsCommand request,
        CancellationToken cancellationToken
    )
    {
        var news = await context.News.FindAsync([request.Id], cancellationToken);
        if (news is null)
        {
            return Error.New($"News with an Id of {request.Id} not found");
        }

        news.EditorState = request.EditorState;
        news.Category = request.Category;
        news.Title = request.Title;
        news.Excerpt = request.Excerpt;
        news.Tags = request.Tags;

        // remove old thumbnail and create a new one
        if (
            request.Thumbnail is not null
            && await blobStorage.ExistsAsync(
                "activities",
                $"news/{news.Id}.webp",
                cancellationToken
            )
        )
        {
            await using var stream = new MemoryStream();
            await request.Thumbnail.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            var res = await blobStorage.UploadAsync(
                "activities",
                $"news/{news.Id}.webp",
                stream,
                cancellationToken
            );
            if (res is < 200 or > 299)
            {
                await blobStorage.DeleteAsync(
                    "activities",
                    $"news/{news.Id}.webp",
                    cancellationToken
                );
                var message = new ThumbnailUploadMessage(news.Id, stream.ToArray());
                await thumbnailUploadChannel.Writer.WriteAsync(message, cancellationToken);
            }
        }

        return new UpdateNewsResponse(news.Id);
    }
}
