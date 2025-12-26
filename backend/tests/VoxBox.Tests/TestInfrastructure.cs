using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VoxBox.Core.Entities;
using VoxBox.Core.Interfaces.Persistence;
using VoxBox.Infrastructure.Persistence;

namespace VoxBox.Tests;

/// <summary>
/// Test configuration and connection strings for integration tests
/// </summary>
public static class TestConfiguration
{
    private static IConfiguration? _configuration;

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Integration test connection string for SQL Server 2022 - reads from appsettings.json
    public static string IntegrationTestConnectionString => 
        _configuration?["ConnectionStrings:SqlServer2022"] 
        ?? "Data Source=SQL6033.site4now.net;Initial Catalog=db_a88d4a_ctonewvoxbox;User Id=db_a88d4a_ctonewvoxbox_admin;Password= ctonewvoxbox@123456";

    // Test tenant settings
    public static readonly Guid TestTenantId = Guid.NewGuid();
    public const string TestSubdomain = "testtenant";
    public const bool TestIsHost = false;
}

/// <summary>
/// Mock implementation of ITenantContext for testing
/// </summary>
public class TestTenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public bool IsHost { get; private set; }
    public string? Subdomain { get; private set; }

    public void SetTenant(Guid? tenantId, bool isHost, string? subdomain)
    {
        TenantId = tenantId;
        IsHost = isHost;
        Subdomain = subdomain;
    }

    public void Clear()
    {
        TenantId = null;
        IsHost = false;
        Subdomain = null;
    }

    public void SetHostContext()
    {
        SetTenant(Guid.Empty, true, "host");
    }

    public void SetTestTenantContext(Guid? tenantId = null, string? subdomain = null)
    {
        SetTenant(tenantId ?? TestConfiguration.TestTenantId, false, subdomain ?? TestConfiguration.TestSubdomain);
    }
}

/// <summary>
/// Database fixture for integration tests using real SQL Server 2022
/// </summary>
public class TestDatabaseFixture : IDisposable
{
    private readonly DbContextOptions<VoxBoxDbContext> _options;
    private readonly TestTenantContext _tenantContext;
    private bool _disposed;

    public TestDatabaseFixture()
    {
        _tenantContext = new TestTenantContext();
        _options = new DbContextOptionsBuilder<VoxBoxDbContext>()
            .UseSqlServer(TestConfiguration.IntegrationTestConnectionString)
            .Options;

        // Ensure database is created
        using var context = CreateDbContext();
        context.Database.EnsureCreated();
    }

    public VoxBoxDbContext CreateDbContext()
    {
        return new VoxBoxDbContext(_options, _tenantContext);
    }

    public TestTenantContext GetTenantContext() => _tenantContext;

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}
