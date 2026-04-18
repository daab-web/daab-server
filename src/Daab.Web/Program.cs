using Daab.Infrastructure;
using Daab.Modules.Activities;
using Daab.Modules.Auth;
using Daab.Modules.Scientists;
using Daab.SharedKernel.Options;
using Daab.Web.Configuration;
using FastEndpoints;
using FastEndpoints.Swagger;
using Scalar.AspNetCore;
using SixLabors.ImageSharp.Web.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder
    .Services.ConfigureLogging(config)
    .ConfigureCors(config)
    .ConfigureCache()
    .ConfigureProblemDetails();

builder
    .Services.AddImageSharp()
    .ClearProviders()
    .AddScientistsModuleImages(config)
    .AddActivitiesModuleImages(config);

builder.Services.Configure<LocaleOptions>(config.GetRequiredSection(nameof(LocaleOptions)));

builder
    .Services.AddInfrastructure(config)
    .AddAuthModule(config)
    .AddScientistsModule(config)
    .AddActivitiesModule(config)
    .ConfigureEndpoints();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseImageSharp();
app.UseStaticFiles();
app.UseAuthModule();
app.UseScientistsModule();
app.UseActivitiesModule();
app.UseFastEndpoints(static c => c.Errors.UseProblemDetails());

app.UseSwaggerGen(options => options.Path = "/openapi/{documentName}.json");
app.MapScalarApiReference(options =>
{
    options.Theme = ScalarTheme.DeepSpace;
    options.DarkMode = true;
    options.Layout = ScalarLayout.Modern;
});
app.Run();
