using System.Runtime.CompilerServices;
using FastEndpoints;
using FastEndpoints.Swagger;
using Serilog;

namespace Daab.Web.Configuration;

public static class ApiConfiguration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureOptions()
        {
            return services;
        }

        public IServiceCollection ConfigureCors(IConfiguration config)
        {
            var allowedOrigins =
                config.GetRequiredSection("Cors:AllowedOrigins").Get<string[]>()
                ?? throw new ArgumentException("There was no allowed origins in CORS config");

            Log.Information("CORS Allowed origins: {origins}", string.Join(',', allowedOrigins));

            services.AddCors(policyBuilder =>
                policyBuilder.AddDefaultPolicy(policy =>
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                )
            );

            return services;
        }

        public IServiceCollection ConfigureCache()
        {
            return services;
        }

        public IServiceCollection ConfigureProblemDetails()
        {
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Detail =
                        "An unexpected error occurred. Please contact support with trace ID.";
                    context.ProblemDetails.Instance = context.HttpContext.Request.Path;
                };
            });

            return services;
        }

        public void ConfigureEndpoints()
        {
            services
                .AddFastEndpoints()
                .AddResponseCaching()
                .SwaggerDocument(options =>
                    options.DocumentSettings = settings =>
                    {
                        settings.DocumentName = "v1";
                        settings.Version = "v1.0";
                    }
                );
        }
    }

    [ModuleInitializer]
    public static void EnsureDbFolderCreated()
    {
        Directory.CreateDirectory("sqlite");
    }
}
