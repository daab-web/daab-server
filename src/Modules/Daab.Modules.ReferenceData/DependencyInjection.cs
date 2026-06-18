using Daab.Modules.ReferenceData.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Daab.Modules.ReferenceData;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddReferenceDataModule(IConfiguration config)
        {
            var connectionString =
                config.GetConnectionString("reference-data-module")
                ?? throw new ArgumentException(
                    "Reference data module connection string cannot be null"
                );

            services.AddDbContextPool<ReferenceDataDbContext>(optionsBuilder =>
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
        public IHost UseReferenceDataModule()
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ReferenceDataDbContext>();

            context.Database.Migrate();

            return app;
        }
    }
}
