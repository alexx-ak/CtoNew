using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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

    // Use in-memory database for testing - no external dependencies
    public static string InMemoryConnectionString => "InMemoryTestDatabase";

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
/// Database fixture for integration tests using in-memory database
/// </summary>
public class TestDatabaseFixture : IDisposable
{
    private readonly DbContextOptions<VoxBoxDbContext> _options;
    private readonly TestTenantContext _tenantContext;
    private readonly VoxBoxDbContext _context;
    private bool _disposed;

    public TestDatabaseFixture()
    {
        _tenantContext = new TestTenantContext();
        _options = new DbContextOptionsBuilder<VoxBoxDbContext>()
            .UseInMemoryDatabase(TestConfiguration.InMemoryConnectionString)
            .Options;

        // Ensure database is created
        _context = CreateDbContext();
        _context.Database.EnsureCreated();
    }

    public VoxBoxDbContext CreateDbContext()
    {
        return new VoxBoxDbContext(_options, _tenantContext);
    }

    public TestTenantContext GetTenantContext() => _tenantContext;

    // Helper method to clear all data for fresh test runs
    public void ClearDatabase()
    {
        using var context = CreateDbContext();
        
        // Clear all entities
        context.Tenants.RemoveRange(context.Tenants.ToList());
        context.Users.RemoveRange(context.Users.ToList());
        
        context.SaveChanges();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _context?.Dispose();
            _disposed = true;
        }
    }
}
