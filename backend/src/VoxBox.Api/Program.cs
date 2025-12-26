using VoxBox.Infrastructure.Data;
using VoxBox.Infrastructure.Middleware;
using VoxBox.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Configure SQL Server with EF Core Code First
builder.Services.ConfigureSqlServer(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add tenant context middleware before other middleware
app.UseTenantContext();

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new { message = "VoxBox API is running", version = "1.0.0" }));

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
