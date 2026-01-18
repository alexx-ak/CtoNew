var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.VoxBox_Api>("voxbox-api");

builder.Build().Run();
