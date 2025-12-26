using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoxBox.Core.Interfaces.Persistence;
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

        // Register TenantContext as scoped service
        services.AddScoped<ITenantContext, TenantContext>();

        // Register DbContext with scoped TenantContext
        services.AddDbContext<VoxBoxDbContext>((serviceProvider, options) =>
        {
            var tenantContext = serviceProvider.GetRequiredService<ITenantContext>();
            options.UseSqlServer(connectionString);
        }, ServiceLifetime.Scoped);

        // Register IVoxBoxDbContextFactory
        services.AddScoped(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<DbContextOptions<VoxBoxDbContext>>();
            var tenantContext = serviceProvider.GetRequiredService<ITenantContext>();
            return new VoxBoxDbContextFactory(options, tenantContext);
        });
    }
}

/// <summary>
/// Factory implementation for creating DbContext instances with proper options
/// </summary>
public class VoxBoxDbContextFactory(
    DbContextOptions<VoxBoxDbContext> options,
    ITenantContext tenantContext) : IVoxBoxDbContextFactory
{
    public object CreateDbContext()
    {
        return new VoxBoxDbContext(options, tenantContext);
    }
}
