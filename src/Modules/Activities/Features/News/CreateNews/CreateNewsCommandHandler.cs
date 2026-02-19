using System.Text.Json;
using System.Threading.Channels;
using Daab.Modules.Activities.BackgroundWorkers;
using Daab.Modules.Activities.Common;
using Daab.Modules.Activities.Messages;
using Daab.Modules.Activities.Persistence;
using FastEndpoints;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Daab.Modules.Activities.Features.News.CreateNews;

public sealed class CreateNewsCommandHandler(
    ActivitiesDbContext context,
    [FromKeyedServices(ChannelKeys.ThumbnailUpload)]
        Channel<ThumbnailUploadMessage> thumbnailUploadChannel
) : IRequestHandler<CreateNewsCommand, Fin<CreateNewsResponse>>
{
    public async Task<Fin<CreateNewsResponse>> Handle(
        CreateNewsCommand request,
        CancellationToken cancellationToken
    )
    {
        var news = new Models.News
        {
            Title = request.Title,
            EditorState = JsonSerializer.Serialize(request.EditorState),
            Slug = request.Slug,
            PublishedDate = DateTimeOffset.UtcNow,
            AuthorId = request.AuthorId,
            AuthorName = request.AuthorName,
            Category = request.Category,
            Tags = request.Tags,
            Excerpt = request.Excerpt,
        };

        var entityEntry = await context.News.AddAsync(news, cancellationToken);
        var statesWritten = await context.SaveChangesAsync(cancellationToken);

        if (statesWritten <= 0)
        {
            return Error.New("Unable to save news... Please try again");
        }

        if (request.Thumbnail is not null)
        {
            await using var stream = new MemoryStream();
            await request.Thumbnail.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            var message = new ThumbnailUploadMessage(entityEntry.Entity.Id, stream.ToArray());

            await thumbnailUploadChannel.Writer.WriteAsync(message, cancellationToken);
        }

        return new CreateNewsResponse(entityEntry.Entity.Id);
    }
}
