using System.Threading.Channels;
using Amazon.S3;
using Daab.Modules.Scientists.Messages;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using Daab.SharedKernel.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Caching.AWS;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Providers.AWS;

namespace Daab.Modules.Scientists;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddScientistsModule(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("scientists-module");
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.AddDbContext<ScientistsDbContext>(options =>
                options.UseSqlite(connectionString)
            );

            services.AddKeyedSingleton(
                ChannelKeys.ProfilePictureUpload,
                (_, _) => Channel.CreateUnbounded<ProfilePictureUploadMessage>()
            );

            services.AddHostedService<ProfilePictureUploadWorker>();

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly)
            );

            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication UseScientistsModule()
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ScientistsDbContext>();

            context.Database.Migrate();

            return app;
        }
    }

    extension(IImageSharpBuilder builder)
    {
        public IImageSharpBuilder AddScientistsModuleImages(IConfiguration config)
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
                            BucketName = "scientists",
                            AccessKey = minioOptions.AccessKey,
                            AccessSecret = minioOptions.SecretKey,
                        }
                    );
                })
                .AddProvider<AWSS3StorageImageProvider>()
                .Configure<AWSS3StorageCacheOptions>(options =>
                {
                    options.Endpoint = minioOptions.Endpoint;
                    options.BucketName = "scientists";
                    options.AccessKey = minioOptions.AccessKey;
                    options.AccessSecret = minioOptions.SecretKey;
                    AWSS3StorageCache.CreateIfNotExists(options, S3CannedACL.Private);
                })
                .SetCache<AWSS3StorageCache>();

            return builder;
        }
    }
}
