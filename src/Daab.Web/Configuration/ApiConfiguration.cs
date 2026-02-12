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
            var allowedOrigins = config.GetRequiredSection("Cors:AllowedOrigins")?.Get<string[]>()
                ?? throw new ArgumentException("There was no allowed origins in CORS config");

            Log.Information("CORS Allowed origins: {origins}", string.Join(',', allowedOrigins));

            services.AddCors(policybuilder =>
                policybuilder.AddDefaultPolicy(policy =>
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                )
            );

            return services;
        }

        public IServiceCollection ConfigureProblemDetails()
        {
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions["traceId"] = context
                        .HttpContext
                        .TraceIdentifier;
                };
            });

            return services;
        }

        public void ConfigureEndpoints()
        {
            services
                .AddFastEndpoints()
                .SwaggerDocument(options =>
                {
                    options.DocumentSettings = settings =>
                    {
                        settings.DocumentName = "v1";
                        settings.Version = "v1.0";
                    };
                });
        }
    }
}
