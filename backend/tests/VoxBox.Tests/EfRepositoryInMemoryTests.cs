using Microsoft.EntityFrameworkCore;
using VoxBox.Core.Entities;
using VoxBox.Infrastructure.Persistence;

namespace VoxBox.Tests.InMemoryTests;

/// <summary>
/// In-memory database tests for repository methods
/// These tests are faster and suitable for CI/CD pipelines
/// </summary>
public class EfRepositoryInMemoryTests : IDisposable
{
    private readonly DbContextOptions<VoxBoxDbContext> _options;
    private readonly TestTenantContext _tenantContext;
    private readonly VoxBoxDbContext _context;
    private readonly EfRepository<Tenant> _repository;
    private bool _disposed;

    public EfRepositoryInMemoryTests()
    {
        _tenantContext = new TestTenantContext();
        _options = new DbContextOptionsBuilder<VoxBoxDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid():N}")
            .Options;

        _context = new VoxBoxDbContext(_options, _tenantContext);
        _context.Database.EnsureCreated();
        _repository = new EfRepository<Tenant>(_context.Tenants, _tenantContext);
    }

    [Fact]
    public async Task InMemory_AddAsync_ShouldCreateTenant()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var tenant = new Tenant
        {
            Name = "InMemory Test Tenant",
            TenancyName = $"inmem_{Guid.NewGuid():N}".Substring(0, 8),
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };

        // Act
        var result = await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("InMemory Test Tenant", result.Name);
    }

    [Fact]
    public async Task InMemory_GetByIdAsync_ShouldReturnTenant()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var tenant = new Tenant
        {
            Name = "Get By ID Test",
            TenancyName = $"getid_{Guid.NewGuid():N}".Substring(0, 8),
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };
        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();
        var tenantId = tenant.Id;

        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(tenantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenantId, result.Id);
    }

    [Fact]
    public async Task InMemory_UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var tenant = new Tenant
        {
            Name = "Original Name",
            TenancyName = $"upd_{Guid.NewGuid():N}".Substring(0, 8),
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };
        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var tenantToUpdate = await _repository.GetByIdAsync(tenant.Id);
        Assert.NotNull(tenantToUpdate);
        tenantToUpdate.Name = "Updated Name";

        // Act
        await _repository.UpdateAsync(tenantToUpdate);
        await _context.SaveChangesAsync();

        // Assert
        _context.ChangeTracker.Clear();
        var updatedTenant = await _repository.GetByIdAsync(tenant.Id);
        Assert.NotNull(updatedTenant);
        Assert.Equal("Updated Name", updatedTenant.Name);
    }

    [Fact]
    public async Task InMemory_DeleteAsync_ShouldSetDeletedProperties()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var tenant = new Tenant
        {
            Name = "To Delete",
            TenancyName = $"del_{Guid.NewGuid():N}".Substring(0, 8),
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };
        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();
        var tenantId = tenant.Id;

        _context.ChangeTracker.Clear();

        // Act - Note: In-Memory database doesn't fully support query filters
        // so GetByIdAsync may still return deleted entities
        await _repository.DeleteAsync(tenantId);
        await _context.SaveChangesAsync();

        // Assert - Verify soft delete properties are set
        _context.ChangeTracker.Clear();
        var result = await _repository.GetByIdAsync(tenantId, includeDeleted: true);
        Assert.NotNull(result);
        Assert.True(result.IsDeleted);
        Assert.NotNull(result.DeletedAt);
        Assert.Null(result.DeletedBy);
    }

    [Fact]
    public async Task InMemory_GetAllAsync_ShouldReturnAllTenants()
    {
        // Arrange
        _tenantContext.SetHostContext();
        for (int i = 1; i <= 5; i++)
        {
            var tenant = new Tenant
            {
                Name = $"Bulk Tenant {i}",
                TenancyName = $"bulk{i}_{Guid.NewGuid():N}".Substring(0, 6),
                IsPrivate = false,
                VoteWeightMode = 0,
                AdminIdentifiers = "admin@test.com",
                IsActive = true
            };
            await _repository.AddAsync(tenant);
        }
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count >= 5);
    }

    [Fact]
    public async Task InMemory_TenantContext_ShouldSetTenantId()
    {
        // Arrange
        var expectedTenantId = 12345;
        _tenantContext.SetTestTenantContext(expectedTenantId);
        
        var tenant = new Tenant
        {
            Name = "Context Test",
            TenancyName = $"ctx_{Guid.NewGuid():N}".Substring(0, 8),
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };

        // Act
        var result = await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(expectedTenantId, result.TenantId);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
    }
}
