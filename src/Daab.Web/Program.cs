using Daab.Modules.Activities;
using Daab.Modules.Auth;
using Daab.Modules.Scientists;
using FastEndpoints;
using FastEndpoints.Swagger;
using Scalar.AspNetCore;
using Serilog;

var log = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddSerilog(
    (services, loggerConfig) =>
        loggerConfig
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
);

builder.Services.AddAuthModule(config);
builder.Services.AddScientistsModule(config);
builder.Services.AddActivitiesModule(config);

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

app.UseExceptionHandler();
app.UseAuthModule();
app.UseScientistsModule();
app.UseActivitiesModule();
app.UseFastEndpoints();

app.UseOpenApi(options => options.Path = "/openapi/{documentName}.json");
app.MapScalarApiReference();

app.Run();
