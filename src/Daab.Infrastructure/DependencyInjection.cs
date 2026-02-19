using Amazon.Runtime;
using Amazon.S3;
using Daab.SharedKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Daab.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            services.AddSingleton<IAmazonS3>(sp =>
            {
                var options =
                    configuration.GetRequiredSection("Minio").Get<MinioOptions>()
                    ?? throw new InvalidOperationException("Minio configuration is missing.");

                var config = new AmazonS3Config
                {
                    ServiceURL = options.Endpoint,
                    ForcePathStyle = true,
                };

                var credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);

                return new AmazonS3Client(credentials, config);
            });

            services.AddScoped<IBlobStorage, MinioStorageService>();

            return services;
        }
    }
}
