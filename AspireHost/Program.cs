var builder = DistributedApplication.CreateBuilder(args);

// Add the ToDo.Api project as a project reference
var backofficeapi = builder.AddProject<Projects.Backoffice_API>("backofficeapi");
var todoApi = builder.AddProject<Projects.ToDo_Api>("todoapi").WithReference(backofficeapi);

builder.Build().Run();