using System.Text;
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

            return host;
        }
    }
}
