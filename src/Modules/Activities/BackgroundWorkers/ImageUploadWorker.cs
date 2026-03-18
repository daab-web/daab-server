using System.Threading.Channels;
using Daab.Modules.Activities.Configuration;
using Daab.Modules.Activities.Messages;
using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;

namespace Daab.Modules.Activities.BackgroundWorkers;

public sealed class ImageUploadWorker(
    [FromKeyedServices(ChannelKeys.ThumbnailUpload)] Channel<UploadMessage> channel,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<MinioOptions> minioOptions,
    ILogger<ImageUploadWorker> logger
) : BackgroundService
{
    private readonly MinioOptions _options = minioOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: Handle OperationCanceledException
        while (await channel.Reader.WaitToReadAsync(stoppingToken))
        {
            try
            {
                var message = await channel.Reader.ReadAsync(stoppingToken);

                if (message is null)
                {
                    continue;
                }

                using var scope = serviceScopeFactory.CreateScope();
                var blobStorage = scope.ServiceProvider.GetRequiredService<IBlobStorage>();
                var context = scope.ServiceProvider.GetRequiredService<ActivitiesDbContext>();

                using var image = Image.Load(message.ImageData);
                await using var outputStream = new MemoryStream();

                await image.SaveAsWebpAsync(outputStream, stoppingToken);

                var name = GetName(message);

                await blobStorage.UploadAsync("activities", name, outputStream, stoppingToken);

                await HandleAfterwork(message, name, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Gracefully handle cancellation
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing thumbnail upload");
            }
        }
    }

    private static string GetName(UploadMessage message)
    {
        return message.Type switch
        {
            MessageType.Thumbnail => $"news/{message.ActivityId}/thumbnails/thumbnail.webp",
            MessageType.Attachment => $"news/{message.ActivityId}/attachments/{message.Name}.webp",
            _ => throw new InvalidOperationException("Unsupported message type"),
        };
    }

    private async Task HandleAfterwork(
        UploadMessage message,
        string path,
        CancellationToken stoppingToken
    )
    {
        using var scope = serviceScopeFactory.CreateScope();
        await using var ctx = scope.ServiceProvider.GetRequiredService<ActivitiesDbContext>();

        switch (message.Type)
        {
            case MessageType.Attachment:
                var attachment = await ctx.Attachments.SingleOrDefaultAsync(
                    n => n.Id == message.Name,
                    stoppingToken
                );

                if (attachment is null)
                {
                    logger.LogWarning(
                        "Attachment related to activity with ID {activityId} not found",
                        message.ActivityId
                    );

                    return;
                }

                attachment.FileUrl = $"{_options.Endpoint}/activities/{path}";

                break;

            case MessageType.Thumbnail:
                var news = await ctx.News.FindAsync([message.ActivityId], stoppingToken);

                if (news is null)
                {
                    logger.LogWarning("News with ID {NewsId} not found", message.ActivityId);

                    return;
                }

                news.Thumbnail = $"{_options.Endpoint}/activities/{path}";
                await ctx.SaveChangesAsync(stoppingToken);
                break;

            default:
                throw new InvalidOperationException("Unsupported message type");
        }

        await ctx.SaveChangesAsync(stoppingToken);
    }
}
