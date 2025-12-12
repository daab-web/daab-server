using Daab.Modules.Scientists.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Daab.Modules.Scientists;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddScientistsModule(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("default");
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.AddDbContext<ScientistsContext>(options =>
                options.UseSqlite(connectionString)
            );

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly)
            );

            return services;
        }
    }

    extension(IHost host)
    {
        public IHost InitializeScientistsModule()
        {
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ScientistsContext>();

            context.Database.Migrate();

            return host;
        }
    }
}

