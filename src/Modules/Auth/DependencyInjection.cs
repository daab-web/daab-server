using System.Text;
using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Options;
using Daab.Modules.Auth.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.JsonWebTokens;
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

            services.AddScoped<ITokenService, TokenService>();
            services.TryAddScoped<IAccountLinker, NullAccountLinker>();
            services.AddScoped<IInvitationService, InvitationService>();

            var authentication = services
                .AddAuthentication("cookie")
                .AddCookie("cookie", options => options.LoginPath = "/auth/login")
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        NameClaimType = JwtRegisteredClaimNames.Name,
                        RoleClaimType = AuthClaims.Roles,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.JwtSecret)
                        ),
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (
                                context.Request.Cookies.TryGetValue(
                                    "daab.accessToken",
                                    out var accessToken
                                )
                            )
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                    };
                });

            AddGoogleSignIn(services, authentication, config);

            services.AddAuthorization();

            return services;
        }
    }

    /// <summary>
    /// Registers Google sign-in when a client id and secret are configured. Google signs into the
    /// short-lived <c>external</c> cookie; the callback endpoint then maps the Google identity to
    /// an app account and starts the regular <c>cookie</c> session.
    /// </summary>
    private static void AddGoogleSignIn(
        IServiceCollection services,
        AuthenticationBuilder authentication,
        IConfiguration config
    )
    {
        var section = config.GetSection(GoogleOptions.SectionName);
        services.Configure<GoogleOptions>(section);

        var google = section.Get<GoogleOptions>() ?? new GoogleOptions();
        if (!google.IsConfigured)
        {
            return;
        }

        authentication
            .AddCookie(
                CookieSession.ExternalScheme,
                options =>
                {
                    options.Cookie.Name = "daab.external";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                }
            )
            .AddGoogle(
                ExternalLogin.Google,
                options =>
                {
                    options.ClientId = google.ClientId!;
                    options.ClientSecret = google.ClientSecret!;
                    options.SignInScheme = CookieSession.ExternalScheme;
                    options.ClaimActions.MapJsonKey("picture", "picture");
                    options.ClaimActions.MapJsonKey("email_verified", "email_verified");
                    options.Events.OnRemoteFailure = context =>
                    {
                        // e.g. the user pressed "Cancel" on Google's consent screen.
                        var returnUrl =
                            context.Properties?.Items.TryGetValue("returnUrl", out var value)
                            == true
                                ? value
                                : "/";
                        context.Response.Redirect(
                            $"/auth/login?returnUrl={Uri.EscapeDataString(CookieSession.SafeReturnUrl(returnUrl))}&error=google"
                        );
                        context.HandleResponse();
                        return Task.CompletedTask;
                    };
                }
            );
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

        foreach (var roleName in new[] { AuthRoles.Admin, AuthRoles.User, AuthRoles.Scientist })
        {
            if (roles.SingleOrDefault(r => r.Name == roleName) is null)
            {
                roles.Add(new Role(roleName));
            }
        }

        context.SaveChanges();

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
