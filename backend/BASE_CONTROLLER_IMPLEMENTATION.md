# Base Controller Implementation Summary

## Overview

This implementation adds a custom base controller architecture that provides standard CRUD operations with built-in support for paging, sorting, filtering, and DTO mapping. All controllers in the application can now inherit from these base controllers to automatically get these features.

## Components Created

### 1. Models (`/src/VoxBox.Api/Models/`)

- **PagedRequest.cs**: Request model for pagination with parameters:
  - `PageNumber` (default: 1)
  - `PageSize` (default: 10, max: 100)
  - `SortBy` (property name to sort by)
  - `SortDirection` (Ascending/Descending)
  - `Filter` (filter expression)

- **PagedResponse.cs**: Response model for paginated results with:
  - `Items` (collection of results)
  - `PageNumber`, `PageSize`, `TotalCount`, `TotalPages`
  - `HasPreviousPage`, `HasNextPage`

- **SortDirection.cs**: Enum for sort direction (Ascending/Descending)

### 2. Interfaces (`/src/VoxBox.Api/Interfaces/`)

- **IEntityMapper.cs**: Interface for entity-DTO mapping with methods:
  - `TDto MapToDto(TEntity entity)`
  - `TEntity MapToEntity(TCreateDto createDto)`
  - `void MapToEntity(TUpdateDto updateDto, TEntity entity)`

### 3. Extensions (`/src/VoxBox.Api/Extensions/`)

- **QueryableExtensions.cs**: Extension methods for IQueryable with:
  - `ApplySorting()` - Dynamic sorting by property name
  - `ApplyFilter()` - Single filter application
  - `ApplyFilters()` - Multiple filters application
  - Supported filter types: string, bool, int, decimal, DateTime, Guid (and nullable versions)

### 4. Base Controllers (`/src/VoxBox.Api/Controllers/`)

- **BaseController.cs**: For entities inheriting from `BaseEntity`
- **BaseControllerLong.cs**: For entities inheriting from `BaseEntityLong`

Both provide:
- Standard CRUD endpoints (GET all, GET paged, GET by ID, POST, PUT, DELETE)
- Lifecycle hooks (BeforeCreate, AfterCreate, BeforeUpdate, AfterUpdate, BeforeDelete, AfterDelete)
- Virtual methods for customization
- Protected `ApplyCustomFilters` method for complex filtering

### 5. Mappers (`/src/VoxBox.Api/Mappers/`)

- **TenantMapper.cs**: Mapper for Tenant entity
- **UserMapper.cs**: Mapper for User entity (example)

### 6. Example Controllers

#### TenantsController
```csharp
public class TenantsController(VoxBoxDbContext dbContext, TenantMapper mapper)
    : BaseController<Tenant, TenantDto, CreateTenantDto, UpdateTenantDto>(dbContext, mapper)
{
}
```

#### UsersController
```csharp
public class UsersController(VoxBoxDbContext dbContext, UserMapper mapper)
    : BaseControllerLong<User, UserDto, CreateUserDto, UpdateUserDto>(dbContext, mapper)
{
    protected override void BeforeCreate(User entity)
    {
        entity.UserName = entity.UserName.ToLower();
    }

    protected override void BeforeUpdate(User entity)
    {
        entity.UserName = entity.UserName.ToLower();
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetActiveUsers(CancellationToken cancellationToken)
    {
        var repository = DbContext.GetRepositoryLong<User>();
        var users = await repository.GetAllAsync(includeDeleted: false, cancellationToken);
        var activeUsers = users.Where(u => u.IsActive);
        var dtos = activeUsers.Select(u => Mapper.MapToDto(u));
        return Ok(dtos);
    }
}
```

### 7. DTOs Created

#### Tenant DTOs (already existed, refactored to use base controller)
- TenantDto, CreateTenantDto, UpdateTenantDto

#### User DTOs (created as example)
- UserDto, CreateUserDto, UpdateUserDto

### 8. Documentation

- **Controllers/README.md**: Comprehensive documentation with:
  - Feature overview
  - API endpoint documentation
  - Usage examples
  - Customization guide
  - Best practices

## Key Features

### Pagination
```
GET /api/tenants/paged?pageNumber=1&pageSize=10
```

Response includes:
- Items array
- Pagination metadata (totalCount, totalPages, hasNextPage, etc.)

### Sorting
```
GET /api/tenants/paged?sortBy=Name&sortDirection=Ascending
GET /api/users/paged?sortBy=CreatedAt&sortDirection=Descending
```

- Case-insensitive property name matching
- Supports any entity property

### Filtering
```
GET /api/tenants/paged?filter=IsActive:true
GET /api/users/paged?filter=Name:John;IsActive:true
```

Filter syntax:
- Single filter: `PropertyName:Value`
- Multiple filters: separated by semicolon (`;`)
- String filters: case-insensitive substring match
- Other types: exact match

Supported types:
- string (contains)
- bool, int, decimal, DateTime, Guid (exact match)
- Nullable versions of all types

### DTO Mapping
Abstracted through `IEntityMapper` interface:
- Bidirectional mapping (Entity ↔ DTO)
- Separate create and update DTOs
- Easy to implement for new entities

### Lifecycle Hooks
Controllers can override:
- `BeforeCreate(TEntity entity)`
- `AfterCreate(TEntity entity)`
- `BeforeUpdate(TEntity entity)`
- `AfterUpdate(TEntity entity)`
- `BeforeDelete(TEntity entity)`
- `AfterDelete(TEntity entity)`

### Custom Filters
Override `ApplyCustomFilters` for complex scenarios:
```csharp
protected override IQueryable<Tenant> ApplyCustomFilters(IQueryable<Tenant> query, PagedRequest request)
{
    // Add custom filtering logic
    return query;
}
```

## Changes to Existing Code

### Program.cs
Added mapper registrations:
```csharp
builder.Services.AddScoped<TenantMapper>();
builder.Services.AddScoped<UserMapper>();
```

### TenantsController
Refactored from 163 lines to 11 lines:
- Removed all CRUD implementation
- Now inherits from BaseController
- All functionality preserved through base class

## Benefits

1. **Code Reusability**: Controllers become much smaller (10-20 lines vs 150+ lines)
2. **Consistency**: All controllers follow the same patterns
3. **Maintainability**: Changes to CRUD logic only need to be made in base controller
4. **Extensibility**: Easy to add custom behavior through overrides and hooks
5. **Type Safety**: Generic constraints ensure type safety
6. **Testing**: Common functionality tested once in base controller

## Usage Pattern

To create a new controller:

1. Create DTOs (TDto, CreateTDto, UpdateTDto)
2. Create mapper implementing IEntityMapper
3. Register mapper in Program.cs
4. Create controller inheriting from BaseController or BaseControllerLong
5. Optionally override methods or add custom endpoints

## Testing

All existing tests pass (26 tests):
- Repository tests
- Integration tests
- No breaking changes to existing functionality

## API Endpoints

Each controller inheriting from base controller gets:

- `GET /api/{controller}` - Get all items
- `GET /api/{controller}/paged` - Get paginated items
- `GET /api/{controller}/{id}` - Get item by ID
- `POST /api/{controller}` - Create new item
- `PUT /api/{controller}/{id}` - Update item
- `DELETE /api/{controller}/{id}` - Soft delete item

Plus any custom endpoints defined in the controller.

## Example Requests

### Get All Tenants
```bash
GET /api/tenants
```

### Get Paginated Tenants with Sorting
```bash
GET /api/tenants/paged?pageNumber=1&pageSize=10&sortBy=Name&sortDirection=Ascending
```

### Get Filtered Tenants
```bash
GET /api/tenants/paged?filter=IsActive:true;IsPrivate:false
```

### Get Active Users
```bash
GET /api/users/active
```

### Create Tenant
```bash
POST /api/tenants
Content-Type: application/json

{
  "name": "Test Tenant",
  "tenancyName": "test",
  "isPrivate": false,
  "voteWeightMode": 0,
  "adminIdentifiers": "admin",
  "isActive": true
}
```

## Future Enhancements

Potential improvements:
1. Add support for range filters (e.g., `CreatedAt:>=2024-01-01`)
2. Add support for sorting by multiple properties
3. Add support for complex filter expressions (AND/OR logic)
4. Integration with AutoMapper for more complex mapping scenarios
5. Add caching support for frequently accessed data
6. Add validation attributes to PagedRequest
7. Add support for field selection (sparse fieldsets)
