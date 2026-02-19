using System.Threading.Channels;
using Daab.Modules.Activities.BackgroundWorkers;
using Daab.Modules.Activities.Common;
using Daab.Modules.Activities.Configuration;
using Daab.Modules.Activities.Messages;
using Daab.Modules.Activities.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                (_, _) => Channel.CreateUnbounded<ThumbnailUploadMessage>()
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

    extension(IHost host)
    {
        public IHost UseActivitiesModule()
        {
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ActivitiesDbContext>();

            context.Database.Migrate();

            return host;
        }
    }
}
