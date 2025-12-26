# VoxBox Integration Tests

This directory contains integration tests for the VoxBox repository layer using real SQL Server 2022 database.

## Test Configuration

### Connection String
Integration tests use a SQL Server 2022 database hosted on site4now.net:
```
Data Source=SQL6033.site4now.net;Initial Catalog=db_a88d4a_ctonewvoxbox;User Id=db_a88d4a_ctonewvoxbox_admin;Password= ctonewvoxbox@123456
```

**Note:** The connection string is defined in `TestConfiguration.cs` and should be updated for different environments.

## Test Classes

### 1. EfRepositoryIntegrationTests (11 tests)
Tests using real SQL Server 2022 database. These tests verify:
- **AddAsync**: Creating new tenants with correct properties and tenant context
- **GetByIdAsync**: Retrieving tenants by ID (including soft-deleted ones)
- **GetAllAsync**: Retrieving all tenants with optional soft-delete filtering
- **UpdateAsync**: Modifying tenant properties with audit timestamps
- **DeleteAsync**: Soft deleting tenants with audit trail (IsDeleted, DeletedAt, DeletedBy)
- **Tenant Isolation**: Verifying tenant context is respected

### 2. EfRepositoryInMemoryTests (6 tests)
Tests using In-Memory database provider. Faster for development and CI/CD. Tests the same core scenarios as integration tests.

## Running Tests

### Run All Tests
```bash
cd /home/engine/project/backend
dotnet test
```

### Run Only Integration Tests (SQL Server)
```bash
dotnet test --filter "FullyQualifiedName~EfRepositoryIntegrationTests"
```

### Run Only In-Memory Tests
```bash
dotnet test --filter "FullyQualifiedName~EfRepositoryInMemoryTests"
```

## Test Infrastructure

### TestConfiguration.cs
- `TestConfiguration`: Static class with connection string and test constants
- `TestTenantContext`: Mock implementation of ITenantContext for testing
- `TestDatabaseFixture`: Database fixture managing DbContext lifecycle

### Unique Naming Strategy
Tests use a `GetUniqueTenancyName()` helper that generates unique names like:
- `testadd_20241226104855_1`
- `list1_20241226104855_2`

This prevents duplicate key violations when tests run in parallel against the shared database.

## Entity Under Test

Tests primarily use the `Tenant` entity which maps to the `Tenants` table:
- Inherits from `BaseEntity` (Id, CreatedAt, TenantId, IsDeleted, etc.)
- Tenant-specific properties (Name, TenancyName, IsPrivate, VoteWeightMode, etc.)
- **Important**: Tenant entity is excluded from tenant query filters in `VoxBoxDbContext` to allow global (host) access

## Covered Scenarios

### CRUD Operations (11 tests)
1. ✅ Create tenant with automatic CreatedAt and TenantId
2. ✅ Read tenant by ID (with soft-deleted entities still accessible)
3. ✅ Read all tenants
4. ✅ Update tenant with UpdatedAt timestamp
5. ✅ Soft delete tenant with DeletedAt and DeletedBy audit trail

### Advanced Scenarios
1. ✅ Tenant isolation via ITenantContext
2. ✅ Global query filters bypass verification
3. ✅ Audit field population on entity state changes

## Test Results Summary

```
Test summary: total: 17, failed: 0, succeeded: 17, skipped: 0
- In-Memory tests: 6 passed
- Integration tests: 11 passed
```

## Notes

- Integration tests require network access to SQL Server 2022
- Tests use shared database - data persists between test runs
- Unique naming strategy prevents duplicate key conflicts
- For CI/CD, In-Memory tests are faster and more reliable
- Both test classes implement IDisposable for proper cleanup
- Tenant entity behavior: excluded from soft-delete filter (host needs global access)
