var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Daab_Web>("api");

builder.Build().Run();
