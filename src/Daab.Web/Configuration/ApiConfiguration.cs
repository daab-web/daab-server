using FastEndpoints;
using FastEndpoints.Swagger;

namespace Daab.Web.Configuration;

public static class ApiConfiguration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureProblemDetails()
        {
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
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