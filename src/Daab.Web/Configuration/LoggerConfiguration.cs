using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Events;

namespace Daab.Web.Configuration;

public static class L
{
    [ModuleInitializer]
    public static void CreateBootstrapLogger()
    {
        var envName =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Production";

        var level = envName.Equals("Development", StringComparison.OrdinalIgnoreCase)
            ? LogEventLevel.Debug
            : LogEventLevel.Information;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Log level: {logLevel}", level);
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureLogging(IConfiguration config)
        {
            services.AddSerilog((services, loggerConfig) =>
                loggerConfig
                    .ReadFrom.Configuration(config)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
            );

            return services;
        }
    }
}