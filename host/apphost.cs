#:sdk Aspire.AppHost.Sdk@13.0.2

#:project ../src/Daab.Web/Daab.Web.csproj

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Daab_Web>("api");

builder.Build().Run();
