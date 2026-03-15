using System.Text.Json;
using Daab.Infrastructure;
using Daab.Modules.Activities;
using Daab.Modules.Auth;
using Daab.Modules.Scientists;
using Daab.Web.Configuration;
using FastEndpoints;
using FastEndpoints.Swagger;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder
    .Services.ConfigureLogging(config)
    .ConfigureCors(config)
    .ConfigureCache()
    .ConfigureProblemDetails();

builder
    .Services.AddInfrastructure(config)
    .AddAuthModule(config)
    .AddScientistsModule(config)
    .AddActivitiesModule(config)
    .ConfigureEndpoints();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthModule();
app.UseScientistsModule();
app.UseActivitiesModule();
app.UseFastEndpoints(static c => c.Errors.UseProblemDetails());

app.UseSwaggerGen(options => options.Path = "/openapi/{documentName}.json");
app.MapScalarApiReference();

app.Run();
