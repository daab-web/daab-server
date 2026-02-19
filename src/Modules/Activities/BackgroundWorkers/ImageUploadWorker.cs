using System.Threading.Channels;
using Daab.Modules.Activities.Common;
using Daab.Modules.Activities.Configuration;
using Daab.Modules.Activities.Messages;
using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;

namespace Daab.Modules.Activities.BackgroundWorkers;

public sealed class ImageUploadWorker(
    [FromKeyedServices(ChannelKeys.ThumbnailUpload)] Channel<ThumbnailUploadMessage> channel,
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

                var name = $"news/{message.NewsId}.webp";

                await blobStorage.UploadAsync("activities", name, outputStream, stoppingToken);

                var news = await context.News.FindAsync([message.NewsId], stoppingToken);

                if (news is null)
                {
                    logger.LogWarning(
                        "News with ID {NewsId} not found for thumbnail upload",
                        message.NewsId
                    );
                    continue;
                }

                news.Thumbnail = $"{_options.Endpoint}/activities/{name}";
                await context.SaveChangesAsync(stoppingToken);
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
}
