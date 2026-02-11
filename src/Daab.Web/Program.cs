using Daab.Modules.Activities;
using Daab.Modules.Auth;
using Daab.Modules.Scientists;
using Daab.Web.Configuration;
using FastEndpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddCors(policybuilder =>
    policybuilder.AddDefaultPolicy(policy =>
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    )
);

builder.Services.ConfigureLogging(config).ConfigureProblemDetails().ConfigureEndpoints();

builder.Services.AddAuthModule(config).AddScientistsModule(config).AddActivitiesModule(config);

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthModule();
app.UseScientistsModule();
app.UseActivitiesModule();
app.UseFastEndpoints();

app.UseOpenApi(options => options.Path = "/openapi/{documentName}.json");
app.MapScalarApiReference();

app.Run();