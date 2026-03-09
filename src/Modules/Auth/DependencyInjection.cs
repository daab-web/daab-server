using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Options;
using Daab.Modules.Auth.Persistence;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Daab.Modules.Auth;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAuthModule(IConfiguration config)
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

            // services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            var jwtOptions = config.GetRequiredSection(nameof(JwtOptions)).Get<JwtOptions>();
            ArgumentNullException.ThrowIfNull(jwtOptions);

            services
                .AddAuthenticationJwtBearer(
                    s => s.SigningKey = jwtOptions.JwtSecret,
                    JwtBearerOptions
                )
                .AddAuthorization();

            return services;
        }
    }

    extension(WebApplication host)
    {
        public WebApplication UseAuthModule()
        {
            host.UseAuthentication();
            host.UseAuthorization();

            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            context.Database.Migrate();
            SeedAuthDatabase(context);

            return host;
        }
    }

    private static void SeedAuthDatabase(AuthDbContext context)
    {
        var roles = context.Set<Role>();
        var users = context.Set<User>();

        if (roles.SingleOrDefault(r => r.Name == "admin") is null)
        {
            roles.Add(new Role("admin"));
            context.SaveChanges();
        }

        if (users.SingleOrDefault(u => u.Username == "admin") is null)
        {
            var adminRole = roles.Single(r => r.Name == "admin");
            var admin = new User("admin", "admin");
            admin.Roles.Add(adminRole);

            users.Add(admin);
        }

        context.SaveChanges();
    }

    private static readonly Action<JwtBearerOptions> JwtBearerOptions = options =>
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("daab.accessToken", out var accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
        };
}
