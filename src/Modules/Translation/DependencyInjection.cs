using Daab.Modules.Translation.Gemini;
using Daab.Modules.Translation.Memory;
using Daab.Modules.Translation.Persistence;
using Daab.Modules.Translation.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Daab.Modules.Translation;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the translation module. The host must also register an
        /// <c>IGlossaryProvider</c> (it bridges to the ReferenceData module, which this module
        /// does not reference).
        /// </summary>
        public IServiceCollection AddTranslationModule(IConfiguration config)
        {
            var connectionString =
                config.GetConnectionString("translation-module")
                ?? throw new ArgumentException(
                    "Translation module connection string cannot be null"
                );

            services.AddDbContextPool<TranslationDbContext>(optionsBuilder =>
                optionsBuilder.UseSqlite(connectionString)
            );

            var geminiSection = config.GetRequiredSection(GeminiOptions.SectionName);
            services.Configure<GeminiOptions>(geminiSection);
            var gemini = geminiSection.Get<GeminiOptions>();
            if (gemini is null || string.IsNullOrWhiteSpace(gemini.ApiKey))
            {
                throw new InvalidOperationException(
                    "Gemini:ApiKey is not configured. Set it in user-secrets: "
                        + "dotnet user-secrets set \"Gemini:ApiKey\" <key>"
                );
            }

            services.AddHttpClient<GeminiClient>(
                (serviceProvider, http) =>
                {
                    var options = serviceProvider
                        .GetRequiredService<IOptions<GeminiOptions>>()
                        .Value;
                    http.BaseAddress = new Uri(options.BaseUrl);
                    http.DefaultRequestHeaders.Add("x-goog-api-key", options.ApiKey);
                }
            );

            services.AddScoped<IEmbedder>(sp => sp.GetRequiredService<GeminiClient>());
            services.AddScoped<ITranslator>(sp => sp.GetRequiredService<GeminiClient>());
            services.AddScoped<ITranslationMemory, TranslationMemory>();
            services.AddScoped<ITranslationService, TranslationService>();

            return services;
        }
    }

    extension(WebApplication app)
    {
        public IHost UseTranslationModule()
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TranslationDbContext>();

            context.Database.Migrate();

            return app;
        }
    }
}
