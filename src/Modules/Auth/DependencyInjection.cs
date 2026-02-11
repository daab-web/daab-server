using System.Text;
using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Options;
using Daab.Modules.Auth.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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

            var jwtOptions = config.GetRequiredSection(nameof(JwtOptions)).Get<JwtOptions>();
            ArgumentNullException.ThrowIfNull(jwtOptions);

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.JwtSecret)
                        ),
                    };
                });
            services.AddAuthorization();

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
}