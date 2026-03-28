using System.Threading.Channels;
using Daab.Modules.Scientists.Messages;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using Daab.SharedKernel.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;

namespace Daab.Modules.Scientists;

public class ProfilePictureUploadWorker(
    [FromKeyedServices(ChannelKeys.ProfilePictureUpload)]
        Channel<ProfilePictureUploadMessage> channel,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<MinioOptions> minioOptions,
    ILogger<ProfilePictureUploadWorker> logger
) : BackgroundService
{
    private readonly MinioOptions _options = minioOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
                var context = scope.ServiceProvider.GetRequiredService<ScientistsDbContext>();

                using var image = Image.Load(message.ImageData);
                await using var outputStream = new MemoryStream();

                await image.SaveAsWebpAsync(outputStream, stoppingToken);

                var name = $"profile-pictures/{message.ScientistId}.webp";

                await blobStorage.UploadAsync("scientists", name, outputStream, stoppingToken);

                var scientist = await context.Scientists.FindAsync(
                    [message.ScientistId],
                    stoppingToken
                );

                if (scientist is null)
                {
                    logger.LogWarning(
                        "Scientist with ID {ScientistId} not found for profile picture upload",
                        message.ScientistId
                    );
                    continue;
                }

                scientist.PhotoUrl = $"{_options.Endpoint}/scientists/{name}";
                await context.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Gracefully handle cancellation
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing profile picture upload");
            }
        }
    }
}
