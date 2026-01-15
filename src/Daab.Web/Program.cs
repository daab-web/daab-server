using Daab.Modules.Scientists;
using Daab.Modules.Auth;
using FastEndpoints;
using FastEndpoints.Swagger;
using Scalar.AspNetCore;
using Serilog;

var log = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.OpenTelemetry()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

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

builder.Services.AddAuthModule(config);
builder.Services.AddScientistsModule(config);

builder
    .Services.AddFastEndpoints()
    .SwaggerDocument(options =>
    {
        options.DocumentSettings = settings =>
        {
            settings.DocumentName = "v1";
            settings.Version = "v1.0";
        };
    });

var app = builder.Build();

app.UseAuthModule();
app.InitializeScientistsModule();
app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi(options => options.Path = "/openapi/{documentName}.json");
    app.MapScalarApiReference();
}

app.Run();
