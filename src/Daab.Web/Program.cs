using Daab.Infrastructure;
using Daab.Modules.Activities;
using Daab.Modules.Auth;
using Daab.Modules.Auth.Common;
using Daab.Modules.Blog;
using Daab.Modules.Blog.Common;
using Daab.Modules.ReferenceData;
using Daab.Modules.Scientists;
using Daab.Modules.Translation;
using Daab.Modules.Translation.Glossary;
using Daab.SharedKernel.Options;
using Daab.Web.Accounts;
using Daab.Web.Blog;
using Daab.Web.Configuration;
using Daab.Web.Translation;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.HttpOverrides;
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

builder.Services.AddHttpContextAccessor();
builder
    .Services.AddImageSharp()
    .ClearProviders()
    .AddScientistsModuleImages(config)
    .AddActivitiesModuleImages(config);

builder.Services.Configure<LocaleOptions>(config.GetRequiredSection(nameof(LocaleOptions)));

builder
    .Services.AddInfrastructure(config)
    .AddAuthModule(config)
    .AddReferenceDataModule(config)
    .AddScientistsModule(config)
    .AddActivitiesModule(config)
    .AddTranslationModule(config)
    .AddBlogModule(config)
    .ConfigureEndpoints();

// Glossary bridges the translation module to ReferenceData; registered by the host that owns both.
builder.Services.AddScoped<IGlossaryProvider, ReferenceDataGlossaryProvider>();

// Scientist accounts bridge Auth and Scientists: grants the scientist role at token issuance.
builder.Services.AddScoped<IAccountLinker, ScientistAccountLinker>();

// Blog content verification and comment translation run on the translation module's model.
builder.Services.AddScoped<IContentVerifier, GeminiContentVerifier>();
builder.Services.AddScoped<ICommentLanguageService, GeminiCommentLanguageService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseForwardedHeaders(
    new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    }
);
app.UseImageSharp();
app.UseStaticFiles();
app.UseAuthModule();
app.UseReferenceDataModule();
app.UseScientistsModule();
app.UseActivitiesModule();
app.UseTranslationModule();
app.UseBlogModule();
app.UseFastEndpoints(static c => c.Errors.UseProblemDetails());

app.UseSwaggerGen(options => options.Path = "/openapi/{documentName}.json");
app.MapScalarApiReference(options =>
{
    options.Theme = ScalarTheme.DeepSpace;
    options.DarkMode = true;
    options.Layout = ScalarLayout.Modern;
});
app.Run();
