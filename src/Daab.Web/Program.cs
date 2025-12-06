using Serilog;

var log = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.OpenTelemetry()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSerilog(
    (services, loggerConfig) =>
        loggerConfig
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.OpenTelemetry()
);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
