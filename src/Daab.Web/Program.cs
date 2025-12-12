using Daab.Modules.Scientists;
using FastEndpoints;
using Scalar.AspNetCore;
using Serilog;

var log = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.OpenTelemetry()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

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

builder.Services.AddScientistsModule(config);

builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints();

var app = builder.Build();

app.InitializeScientistsModule();

app.MapFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();
