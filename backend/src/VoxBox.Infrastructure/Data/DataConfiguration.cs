using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoxBox.Infrastructure.Persistence;

namespace VoxBox.Infrastructure.Data;

/// <summary>
/// Data configuration helper following KISS - simple connection string setup
/// </summary>
public static class DataConfiguration
{
    public const string ConnectionStringName = "VoxBoxConnection";
    
    public static void ConfigureSqlServer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'VoxBoxConnection' not found.");
        }
        
        services.AddDbContext<VoxBoxDbContext>(options =>
            options.UseSqlServer(connectionString));
    }
}
