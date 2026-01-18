using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Enrichers;
using VoxBox.Infrastructure.Data;
using VoxBox.Infrastructure.Middleware;
using VoxBox.Infrastructure.Persistence;
using VoxBox.ServiceDefaults;

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/VoxBox-.log", 
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB file size limit
        rollOnFileSizeLimit: true, // Create new file when size limit is reached
        retainedFileCountLimit: 10, // Keep max 10 log files
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting VoxBox API application");
    
    // Ensure configuration files are loaded explicitly
    var builder = WebApplication.CreateBuilder(args);
    
    // Explicitly add appsettings configuration
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    builder.Configuration.AddJsonFile($"home/engine/projectsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
    
    // Use Serilog for logging
    builder.Host.UseSerilog();

    builder.AddServiceDefaults();

    // Add services to the container
    builder.Services.AddControllers();

    // Configure OpenAPI with Aspire support
    builder.Services.AddOpenApi();
    
    // Configure SQL Server with EF Core Code First
    builder.Services.ConfigureSqlServer(builder.Configuration);
    
    var app = builder.Build();
    
    var isOpenApiDocumentGeneration = Environment.GetCommandLineArgs()
        .Any(arg => arg.Contains("dotnet-getdocument", StringComparison.OrdinalIgnoreCase));

    // Apply pending migrations automatically
    if (!isOpenApiDocumentGeneration)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<VoxBoxDbContext>();
        dbContext.Database.Migrate();
    }

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
    
    // Add global exception handler middleware first to catch all exceptions
    app.UseGlobalExceptionHandler();
    
    // Add tenant context middleware before other middleware
    app.UseTenantContext();
    
    app.UseHttpsRedirection();

    app.MapControllers();

    app.MapDefaultEndpoints();

    app.MapGet("/", () => Results.Ok(new { message = "VoxBox API is running", version = "1.0.0" }));
    
    // Test endpoint to verify exception handling
    app.MapGet("/test-exception", () =>
    {
        throw new Exception("This is a test exception to verify global exception handling is working");
    });
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program accessible for integration tests
public partial class Program { }
