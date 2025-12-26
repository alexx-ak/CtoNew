using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using VoxBox.Core.Interfaces.Persistence;

namespace VoxBox.Infrastructure.Persistence;

/// <summary>
/// Design-time DbContext factory for EF Core migrations
/// </summary>
public class VoxBoxDbContextDesignFactory : IDesignTimeDbContextFactory<VoxBoxDbContext>
{
    public VoxBoxDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("VoxBoxConnection")
            ?? "Data Source=SQL6033.site4now.net;Initial Catalog=db_a88d4a_ctonewvoxbox;User Id=db_a88d4a_ctonewvoxbox_admin;Password=ctonewvoxbox@123456";

        // Create options
        var optionsBuilder = new DbContextOptionsBuilder<VoxBoxDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        // Create a mock tenant context for design time
        var tenantContext = new DesignTimeTenantContext();

        return new VoxBoxDbContext(optionsBuilder.Options, tenantContext);
    }
}

/// <summary>
/// Simple design-time tenant context implementation
/// </summary>
public class DesignTimeTenantContext : ITenantContext
{
    public Guid? TenantId => Guid.NewGuid(); // Default tenant ID for design time
    public bool IsHost => false;
    public string? Subdomain => "design";

    public void SetTenant(Guid? tenantId, bool isHost, string? subdomain)
    {
        // No-op for design time
    }

    public void Clear()
    {
        // No-op for design time
    }
}