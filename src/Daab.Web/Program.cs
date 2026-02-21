using System.Security.Claims;
using Daab.Infrastructure;
using Daab.Modules.Activities;
using Daab.Modules.Auth;
using Daab.Modules.Scientists;
using Daab.Web.Configuration;
using FastEndpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder
    .Services.ConfigureLogging(config)
    .ConfigureCors(config)
    .ConfigureCache()
    .ConfigureProblemDetails()
    .ConfigureEndpoints();

builder
    .Services.AddInfrastructure(config)
    .AddAuthModule(config)
    .AddScientistsModule(config)
    .AddActivitiesModule(config);

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthModule();
app.UseScientistsModule();
app.UseActivitiesModule();
app.UseFastEndpoints(c => c.Security.RoleClaimType = ClaimTypes.Role);

app.UseOpenApi(options => options.Path = "/openapi/{documentName}.json");
app.MapScalarApiReference();

app.Run();
