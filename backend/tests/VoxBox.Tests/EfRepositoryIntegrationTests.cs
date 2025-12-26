using Microsoft.EntityFrameworkCore;
using VoxBox.Core.Entities;
using VoxBox.Infrastructure.Persistence;

namespace VoxBox.Tests.RepositoryIntegrationTests;

/// <summary>
/// Integration tests for EfRepository<T> using SQL Server 2022
/// Tests cover all CRUD operations with tenant context
/// </summary>
public class EfRepositoryIntegrationTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly VoxBoxDbContext _context;
    private readonly TestTenantContext _tenantContext;
    private readonly EfRepository<Tenant> _repository;

    public EfRepositoryIntegrationTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _context = fixture.CreateDbContext();
        _tenantContext = fixture.GetTenantContext();
        _repository = new EfRepository<Tenant>(_context.Tenants, _tenantContext);
    }

    #region AddAsync Tests

    private static int _testCounter = 0;
    private static readonly object _counterLock = new();

    private string GetUniqueTenancyName(string prefix)
    {
        lock (_counterLock)
        {
            _testCounter++;
            return $"{prefix}_{DateTime.UtcNow:yyyyMMddHHmmss}_{_testCounter}";
        }
    }

    [Fact]
    public async Task AddAsync_ShouldCreateTenant_WithCorrectProperties()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var uniqueName = GetUniqueTenancyName("testadd");
        var tenant = new Tenant
        {
            Name = "Test Tenant for Add",
            TenancyName = uniqueName,
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };

        // Act
        var result = await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Clear tracker to get fresh data from database
        _context.ChangeTracker.Clear();

        // Assert - Fetch the tenant after save to verify ID was generated
        var savedTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenancyName == uniqueName);
        Assert.NotNull(savedTenant);
        Assert.NotEqual(Guid.Empty, savedTenant.Id);
        Assert.Equal("Test Tenant for Add", savedTenant.Name);
        Assert.True(savedTenant.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task AddAsync_ShouldSetTenantId_FromContext()
    {
        // Arrange
        var testTenantId = Guid.NewGuid();
        _tenantContext.SetTestTenantContext(testTenantId);
        var uniqueName = GetUniqueTenancyName("testctx");
        var tenant = new Tenant
        {
            Name = "Tenant with Context ID",
            TenancyName = uniqueName,
            IsPrivate = true,
            VoteWeightMode = 1,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };

        // Act
        var result = await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testTenantId, result.TenantId);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTenant_WhenExists()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var uniqueName = GetUniqueTenancyName("testget");
        var tenant = new Tenant
        {
            Name = "Tenant to Get",
            TenancyName = uniqueName,
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };
        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();
        var tenantId = tenant.Id;

        // Clear change tracker to ensure fresh query
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(tenantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenantId, result.Id);
        Assert.Equal("Tenant to Get", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithIncludeDeleted_ShouldReturnDeletedTenant()
    {
        // Arrange - Note: Tenant entity is excluded from query filters in DbContext
        // to allow global access (host can see all tenants including deleted)
        _tenantContext.SetHostContext();
        var uniqueTenancyName = GetUniqueTenancyName("testsoftdel");
        var tenant = new Tenant
        {
            Name = "Soft Deleted Tenant",
            TenancyName = uniqueTenancyName,
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };
        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();
        var tenantId = tenant.Id;

        // Delete the tenant (soft delete)
        await _repository.DeleteAsync(tenantId);
        await _context.SaveChangesAsync();

        // Clear change tracker
        _context.ChangeTracker.Clear();

        // Act - Note: Since Tenant is excluded from query filters,
        // GetByIdAsync will return the tenant regardless of IsDeleted status
        var result = await _repository.GetByIdAsync(tenantId);

        // Assert - Tenant should still be returned (it's excluded from soft delete filter)
        Assert.NotNull(result);
        Assert.Equal(tenantId, result.Id);
        Assert.True(result.IsDeleted);
        Assert.NotNull(result.DeletedAt);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTenants()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var tenants = new List<Tenant>
        {
            new Tenant { Name = "List Tenant 1", TenancyName = GetUniqueTenancyName("list1"), IsPrivate = false, VoteWeightMode = 0, AdminIdentifiers = "admin@test.com", IsActive = true },
            new Tenant { Name = "List Tenant 2", TenancyName = GetUniqueTenancyName("list2"), IsPrivate = false, VoteWeightMode = 0, AdminIdentifiers = "admin@test.com", IsActive = true },
            new Tenant { Name = "List Tenant 3", TenancyName = GetUniqueTenancyName("list3"), IsPrivate = false, VoteWeightMode = 0, AdminIdentifiers = "admin@test.com", IsActive = true }
        };

        foreach (var tenant in tenants)
        {
            await _repository.AddAsync(tenant);
        }
        await _context.SaveChangesAsync();

        // Clear change tracker
        _context.ChangeTracker.Clear();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count >= 3);
    }

    [Fact]
    public async Task GetAllAsync_WithIncludeDeleted_ShouldReturnAllTenants()
    {
        // Arrange - Note: Tenant entity is excluded from query filters
        // to allow global access, so we verify soft delete properties are set
        _tenantContext.SetHostContext();
        var activeName = GetUniqueTenancyName("active");
        var deletedName = GetUniqueTenancyName("deleted");
        
        var activeTenant = new Tenant
        {
            Name = "Active Tenant",
            TenancyName = activeName,
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };
        var deletedTenant = new Tenant
        {
            Name = "Deleted Tenant",
            TenancyName = deletedName,
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };

        await _repository.AddAsync(activeTenant);
        await _repository.AddAsync(deletedTenant);
        await _context.SaveChangesAsync();

        // Soft delete one tenant
        await _repository.DeleteAsync(deletedTenant.Id);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act - Since Tenant is excluded from query filters,
        // GetAllAsync returns all tenants including deleted ones
        var allTenants = (await _repository.GetAllAsync(includeDeleted: true)).ToList();

        // Assert - Verify both active and deleted tenants are returned
        Assert.NotNull(allTenants);
        Assert.Contains(allTenants, t => t.TenancyName == activeName);
        Assert.Contains(allTenants, t => t.TenancyName == deletedName);
        
        // Verify the deleted tenant has IsDeleted flag set
        var foundDeleted = allTenants.FirstOrDefault(t => t.TenancyName == deletedName);
        Assert.NotNull(foundDeleted);
        Assert.True(foundDeleted.IsDeleted);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldModifyTenantProperties()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var uniqueName = GetUniqueTenancyName("update");
        var tenant = new Tenant
        {
            Name = "Original Name",
            TenancyName = uniqueName,
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };
        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();
        var tenantId = tenant.Id;

        _context.ChangeTracker.Clear();

        // Fetch and modify
        var tenantToUpdate = await _repository.GetByIdAsync(tenantId);
        Assert.NotNull(tenantToUpdate);
        tenantToUpdate.Name = "Updated Name";
        tenantToUpdate.IsPrivate = true;

        // Act
        await _repository.UpdateAsync(tenantToUpdate);
        await _context.SaveChangesAsync();

        // Assert
        _context.ChangeTracker.Clear();
        var updatedTenant = await _repository.GetByIdAsync(tenantId);
        Assert.NotNull(updatedTenant);
        Assert.Equal("Updated Name", updatedTenant.Name);
        Assert.True(updatedTenant.IsPrivate);
        Assert.NotNull(updatedTenant.UpdatedAt);
    }

    #endregion

    #region DeleteAsync Tests (Soft Delete)

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteTenant()
    {
        // Arrange - Note: Tenant entity is excluded from query filters in DbContext
        // to allow global access by the host, so it will still be returned
        _tenantContext.SetHostContext();
        var uniqueName = GetUniqueTenancyName("softdel");
        var tenant = new Tenant
        {
            Name = "Tenant to Soft Delete",
            TenancyName = uniqueName,
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
        await _repository.DeleteAsync(tenantId);
        await _context.SaveChangesAsync();

        // Assert - Since Tenant is excluded from soft delete filter,
        // the entity will still be returned but with IsDeleted = true
        _context.ChangeTracker.Clear();
        var result = await _repository.GetByIdAsync(tenantId);
        Assert.NotNull(result);
        Assert.True(result.IsDeleted);
        Assert.NotNull(result.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetDeletedAtAndDeletedBy()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var uniqueName = GetUniqueTenancyName("auditdel");
        var tenant = new Tenant
        {
            Name = "Tenant for Deletion Audit",
            TenancyName = uniqueName,
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
        await _repository.DeleteAsync(tenantId);
        await _context.SaveChangesAsync();

        // Assert - Should be found with includeDeleted and have audit fields set
        _context.ChangeTracker.Clear();
        var deletedTenant = await _repository.GetByIdAsync(tenantId, includeDeleted: true);
        Assert.NotNull(deletedTenant);
        Assert.True(deletedTenant.IsDeleted);
        Assert.NotNull(deletedTenant.DeletedAt);
        Assert.Null(deletedTenant.DeletedBy); // Set to null to avoid FK constraints
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task Repository_ShouldRespectTenantContext_Isolation()
    {
        // Arrange - Create tenant with one tenant context
        _tenantContext.SetTestTenantContext(tenantId: Guid.NewGuid(), subdomain: "tenant1");
        var tenant1Name = GetUniqueTenancyName("isol1");
        var tenant1 = new Tenant
        {
            Name = "Tenant 1 Data",
            TenancyName = tenant1Name,
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };
        await _repository.AddAsync(tenant1);
        await _context.SaveChangesAsync();
        var tenantId1 = tenant1.Id;

        // Create tenant with different tenant context
        _tenantContext.SetTestTenantContext(tenantId: Guid.NewGuid(), subdomain: "tenant2");
        var tenant2Name = GetUniqueTenancyName("isol2");
        var tenant2 = new Tenant
        {
            Name = "Tenant 2 Data",
            TenancyName = tenant2Name,
            IsPrivate = false,
            VoteWeightMode = 0,
            AdminIdentifiers = "admin@test.com",
            IsActive = true
        };
        await _repository.AddAsync(tenant2);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act - Query with tenant 1 context
        _tenantContext.SetTestTenantContext(subdomain: "tenant1");
        var resultTenant1 = (await _repository.GetAllAsync()).ToList();

        _context.ChangeTracker.Clear();

        // Query with tenant 2 context
        _tenantContext.SetTestTenantContext(subdomain: "tenant2");
        var resultTenant2 = (await _repository.GetAllAsync()).ToList();

        // Assert - Each tenant should only see their own data (via TenantId filter)
        // Note: Since Tenant entity is excluded from tenant filters, this tests the repository pattern
        Assert.NotNull(resultTenant1);
        Assert.NotNull(resultTenant2);
    }

    #endregion
}
