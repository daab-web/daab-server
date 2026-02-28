using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using FluentValidation;
using MediatR;
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
            var connectionString = config.GetConnectionString("scientists-module");
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.AddDbContext<ScientistsDbContext>(options =>
                options.UseSqlite(connectionString)
            );

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly)
            );

            // services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            // services.AddFluentValidationAutoValidation();

            return services;
        }
    }

    extension(IHost host)
    {
        public IHost UseScientistsModule()
        {
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ScientistsDbContext>();

            context.Database.Migrate();

            return host;
        }
    }
}
