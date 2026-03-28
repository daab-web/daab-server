using System.Threading.Channels;
using Amazon.S3;
using Daab.Modules.Activities.BackgroundWorkers;
using Daab.Modules.Activities.Messages;
using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp.Web.Caching.AWS;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Providers.AWS;
using MinioOptions = Daab.SharedKernel.MinioOptions;

namespace Daab.Modules.Activities;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddActivitiesModule(IConfiguration config)
        {
            var connectionString =
                config.GetConnectionString("activities-module")
                ?? throw new ArgumentException(
                    "Activities module connection string cannot be null"
                );

            services.Configure<MinioOptions>(config.GetRequiredSection("Minio"));

            services.AddKeyedSingleton(
                ChannelKeys.ThumbnailUpload,
                (_, _) => Channel.CreateUnbounded<UploadMessage>()
            );
            services.AddHostedService<ImageUploadWorker>();

            services.AddDbContextPool<ActivitiesDbContext>(optionsBuilder =>
                optionsBuilder.UseSqlite(connectionString)
            );

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly)
            );

            return services;
        }
    }

    extension(WebApplication app)
    {
        public IHost UseActivitiesModule()
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ActivitiesDbContext>();

            context.Database.Migrate();

            return app;
        }
    }

    extension(IImageSharpBuilder builder)
    {
        public IImageSharpBuilder AddActivitiesModuleImages(IConfiguration config)
        {
            var minioOptions = config.GetRequiredSection("Minio").Get<MinioOptions>();
            ArgumentNullException.ThrowIfNull(minioOptions);

            builder
                .Configure<AWSS3StorageImageProviderOptions>(options =>
                {
                    options.S3Buckets.Add(
                        new AWSS3BucketClientOptions
                        {
                            Endpoint = minioOptions.Endpoint,
                            BucketName = "activities",
                            AccessKey = minioOptions.AccessKey,
                            AccessSecret = minioOptions.SecretKey,
                        }
                    );
                })
                .AddProvider<AWSS3StorageImageProvider>()
                .Configure<AWSS3StorageCacheOptions>(options =>
                {
                    options.Endpoint = minioOptions.Endpoint;
                    options.BucketName = "activities";
                    options.AccessKey = minioOptions.AccessKey;
                    options.AccessSecret = minioOptions.SecretKey;
                    AWSS3StorageCache.CreateIfNotExists(options, S3CannedACL.Private);
                })
                .SetCache<AWSS3StorageCache>();

            return builder;
        }
    }
}
