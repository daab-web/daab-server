using Daab.Modules.Auth.Options;
using Daab.Modules.Auth.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Daab.Modules.Auth;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public void AddAuthModule(IConfiguration config)
        {
            var connectionString =
                config.GetConnectionString("auth-module")
                ?? throw new ArgumentException("Auth module connection string cannot be null");

            services.Configure<JwtOptions>(config.GetRequiredSection(nameof(JwtOptions)));

            services.AddDbContext<AuthDbContext>(optionsBuilder =>
                optionsBuilder.UseSqlite(connectionString)
            );

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly)
            );
        }
    }

    extension(IHost host)
    {
        public IHost UseAuthModule()
        {
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            context.Database.Migrate();

            return host;
        }
    }
}
