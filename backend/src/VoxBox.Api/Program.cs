using VoxBox.Infrastructure.Data;
using VoxBox.Infrastructure.Middleware;
using VoxBox.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "VoxBox API", Version = "v1" });
});

// Configure SQL Server with EF Core Code First
builder.Services.ConfigureSqlServer(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VoxBox API v1");
    });
}

// Add tenant context middleware before other middleware
app.UseTenantContext();

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new { message = "VoxBox API is running", version = "1.0.0" }));

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
