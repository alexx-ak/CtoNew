using Microsoft.EntityFrameworkCore;
using VoxBox.Core.Entities;
using VoxBox.Infrastructure.Persistence;

namespace VoxBox.Tests.RepositoryIntegrationTests;

/// <summary>
/// Integration tests for User repository using SQL Server 2022
/// Tests cover all CRUD operations with tenant context
/// </summary>
public class UserRepositoryIntegrationTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly VoxBoxDbContext _context;
    private readonly TestTenantContext _tenantContext;
    private readonly EfRepositoryLong<User> _repository;

    public UserRepositoryIntegrationTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _context = fixture.CreateDbContext();
        _tenantContext = fixture.GetTenantContext();
        _repository = new EfRepositoryLong<User>(_context.Users, _tenantContext);
    }

    #region AddAsync Tests

    private static int _testCounter = 0;
    private static readonly object _counterLock = new();

    private string GetUniqueUsername(string prefix)
    {
        lock (_counterLock)
        {
            _testCounter++;
            return $"{prefix}_{DateTime.UtcNow:yyyyMMddHHmmss}_{_testCounter}";
        }
    }

    [Fact]
    public async Task AddAsync_ShouldCreateUser_WithCorrectProperties()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var uniqueUsername = GetUniqueUsername("testadd");
        var user = new User
        {
            UserName = uniqueUsername,
            Name = "Test",
            Surname = "User",
            EmailAddress = "test@example.com",
            PhoneNumber = "1234567890",
            IsActive = true,
            Identifier = "test-identifier",
            VoteWeight = 1.0m,
            IdentyumUuid = "test-uuid-1234567890",
            PreviousName = "OldName",
            PreviousSurname = "OldSurname"
        };

        // Act
        var result = await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Clear tracker to get fresh data from database
        _context.ChangeTracker.Clear();

        // Assert - Fetch the user after save to verify ID was generated
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == uniqueUsername);
        Assert.NotNull(savedUser);
        Assert.NotEqual(0, savedUser.Id);
        Assert.Equal(uniqueUsername, savedUser.UserName);
        Assert.True(savedUser.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task AddAsync_ShouldSetTenantId_FromContext()
    {
        // Arrange
        var testTenantId = 42;
        _tenantContext.SetTestTenantContext(testTenantId);
        var uniqueUsername = GetUniqueUsername("testctx");
        var user = new User
        {
            UserName = uniqueUsername,
            Name = "Tenant",
            Surname = "User",
            EmailAddress = "tenant@example.com",
            PhoneNumber = "1234567890",
            IsActive = true
        };

        // Act
        var result = await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testTenantId, result.TenantId);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var uniqueUsername = GetUniqueUsername("testget");
        var user = new User
        {
            UserName = uniqueUsername,
            Name = "Get",
            Surname = "User",
            EmailAddress = "get@example.com",
            PhoneNumber = "1234567890",
            IsActive = true
        };
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        var userId = user.Id;

        // Clear change tracker to ensure fresh query
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(uniqueUsername, result.UserName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var nonExistentId = -999L;

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithIncludeDeleted_ShouldReturnDeletedUser()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var uniqueUsername = GetUniqueUsername("testsoftdel");
        var user = new User
        {
            UserName = uniqueUsername,
            Name = "Soft",
            Surname = "Delete",
            EmailAddress = "softdelete@example.com",
            PhoneNumber = "1234567890",
            IsActive = true
        };
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        var userId = user.Id;

        // Delete the user (soft delete)
        await _repository.DeleteAsync(userId);
        await _context.SaveChangesAsync();

        // Clear change tracker
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(userId, includeDeleted: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.True(result.IsDeleted);
        Assert.NotNull(result.DeletedAt);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var users = new List<User>
        {
            new User { UserName = GetUniqueUsername("list1"), Name = "List1", Surname = "User", EmailAddress = "list1@example.com", PhoneNumber = "1234567890", IsActive = true },
            new User { UserName = GetUniqueUsername("list2"), Name = "List2", Surname = "User", EmailAddress = "list2@example.com", PhoneNumber = "1234567890", IsActive = true },
            new User { UserName = GetUniqueUsername("list3"), Name = "List3", Surname = "User", EmailAddress = "list3@example.com", PhoneNumber = "1234567890", IsActive = true }
        };

        foreach (var user in users)
        {
            await _repository.AddAsync(user);
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
    public async Task GetAllAsync_WithIncludeDeleted_ShouldReturnAllUsers()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var activeUsername = GetUniqueUsername("active");
        var deletedUsername = GetUniqueUsername("deleted");
        
        var activeUser = new User
        {
            UserName = activeUsername,
            Name = "Active",
            Surname = "User",
            EmailAddress = "active@example.com",
            PhoneNumber = "1234567890",
            IsActive = true
        };
        var deletedUser = new User
        {
            UserName = deletedUsername,
            Name = "Deleted",
            Surname = "User",
            EmailAddress = "deleted@example.com",
            PhoneNumber = "1234567890",
            IsActive = true
        };

        await _repository.AddAsync(activeUser);
        await _repository.AddAsync(deletedUser);
        await _context.SaveChangesAsync();

        // Soft delete one user
        await _repository.DeleteAsync(deletedUser.Id);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var allUsers = (await _repository.GetAllAsync(includeDeleted: true)).ToList();

        // Assert - Verify both active and deleted users are returned
        Assert.NotNull(allUsers);
        Assert.Contains(allUsers, u => u.UserName == activeUsername);
        Assert.Contains(allUsers, u => u.UserName == deletedUsername);
        
        // Verify the deleted user has IsDeleted flag set
        var foundDeleted = allUsers.FirstOrDefault(u => u.UserName == deletedUsername);
        Assert.NotNull(foundDeleted);
        Assert.True(foundDeleted.IsDeleted);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldModifyUserProperties()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var uniqueUsername = GetUniqueUsername("update");
        var user = new User
        {
            UserName = uniqueUsername,
            Name = "Original",
            Surname = "Name",
            EmailAddress = "update@example.com",
            PhoneNumber = "1234567890",
            IsActive = true
        };
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        var userId = user.Id;

        _context.ChangeTracker.Clear();

        // Fetch and modify
        var userToUpdate = await _repository.GetByIdAsync(userId);
        Assert.NotNull(userToUpdate);
        userToUpdate.Name = "Updated";
        userToUpdate.Surname = "User";
        userToUpdate.EmailAddress = "updated@example.com";

        // Act
        await _repository.UpdateAsync(userToUpdate);
        await _context.SaveChangesAsync();

        // Assert
        _context.ChangeTracker.Clear();
        var updatedUser = await _repository.GetByIdAsync(userId);
        Assert.NotNull(updatedUser);
        Assert.Equal("Updated", updatedUser.Name);
        Assert.Equal("User", updatedUser.Surname);
        Assert.Equal("updated@example.com", updatedUser.EmailAddress);
        Assert.NotNull(updatedUser.UpdatedAt);
    }

    #endregion

    #region DeleteAsync Tests (Soft Delete)

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteUser()
    {
        // Arrange
        _tenantContext.SetHostContext();
        var uniqueUsername = GetUniqueUsername("softdel");
        var user = new User
        {
            UserName = uniqueUsername,
            Name = "Soft",
            Surname = "Delete",
            EmailAddress = "softdel@example.com",
            PhoneNumber = "1234567890",
            IsActive = true
        };
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        var userId = user.Id;

        _context.ChangeTracker.Clear();

        // Act
        await _repository.DeleteAsync(userId);
        await _context.SaveChangesAsync();

        // Assert - User should be soft deleted
        _context.ChangeTracker.Clear();
        var result = await _repository.GetByIdAsync(userId, includeDeleted: true);
        Assert.NotNull(result);
        Assert.True(result.IsDeleted);
        Assert.NotNull(result.DeletedAt);
        Assert.Null(result.DeletedBy); // Set to null to avoid foreign key constraints
    }

    #endregion
}