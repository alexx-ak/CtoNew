# Base Controller Documentation

## Overview

The `BaseController<TEntity, TDto, TCreateDto, TUpdateDto>` and `BaseControllerLong<TEntity, TDto, TCreateDto, TUpdateDto>` provide a reusable foundation for all API controllers in the VoxBox application. They implement standard CRUD operations with built-in support for:

- **Paging**: Retrieve data in pages with configurable page size
- **Sorting**: Sort results by any property in ascending or descending order
- **Filtering**: Filter results using a flexible query syntax
- **DTO Mapping**: Automatic mapping between entity and DTO objects

**Note**: Use `BaseController` for entities inheriting from `BaseEntity` and `BaseControllerLong` for entities inheriting from `BaseEntityLong`.

## Features

### Standard Endpoints

Every controller inheriting from `BaseController` automatically gets these endpoints:

1. **GET `/api/{controller}`** - Get all items
2. **GET `/api/{controller}/paged`** - Get paginated items with sorting and filtering
3. **GET `/api/{controller}/{id}`** - Get a single item by ID
4. **POST `/api/{controller}`** - Create a new item
5. **PUT `/api/{controller}/{id}`** - Update an existing item
6. **DELETE `/api/{controller}/{id}`** - Soft delete an item

### Pagination

Use the `/paged` endpoint with query parameters:

```
GET /api/tenants/paged?pageNumber=1&pageSize=10&sortBy=Name&sortDirection=Ascending
```

**Query Parameters:**
- `pageNumber` (default: 1) - The page number to retrieve
- `pageSize` (default: 10, max: 100) - Number of items per page
- `sortBy` - Property name to sort by (case-insensitive)
- `sortDirection` - Either `Ascending` or `Descending` (default: Ascending)
- `filter` - Filter expression (see Filtering section)

**Response:**
```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Sorting

Sort results by any property of the entity:

```
GET /api/tenants/paged?sortBy=Name&sortDirection=Ascending
GET /api/tenants/paged?sortBy=CreatedAt&sortDirection=Descending
```

### Filtering

Filter results using a simple query syntax:

**Single Filter:**
```
GET /api/tenants/paged?filter=Name:VoxBox
GET /api/tenants/paged?filter=IsActive:true
```

**Multiple Filters** (separated by semicolon):
```
GET /api/tenants/paged?filter=IsActive:true;IsPrivate:false
```

**Filter Syntax:**
- `PropertyName:Value` - Basic equality filter
- For strings: Case-insensitive substring match
- For booleans: Exact match (true/false)
- For numbers: Exact match
- For GUIDs: Exact match

**Supported Types:**
- `string` - Contains match (case-insensitive)
- `bool`, `bool?` - Exact match
- `int`, `int?` - Exact match
- `decimal`, `decimal?` - Exact match
- `DateTime`, `DateTime?` - Exact match
- `Guid`, `Guid?` - Exact match

## Creating a New Controller

### Step 1: Create DTOs

Create three DTOs for your entity:

```csharp
// TenantDto.cs - Response DTO
public record TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    // ... other properties
}

// CreateTenantDto.cs - Create request DTO
public record CreateTenantDto
{
    [Required]
    public string Name { get; init; } = string.Empty;
    // ... other properties
}

// UpdateTenantDto.cs - Update request DTO
public record UpdateTenantDto
{
    [Required]
    public string Name { get; init; } = string.Empty;
    // ... other properties
}
```

### Step 2: Create a Mapper

Implement the `IEntityMapper` interface:

```csharp
public class TenantMapper : IEntityMapper<Tenant, TenantDto, CreateTenantDto, UpdateTenantDto>
{
    public TenantDto MapToDto(Tenant entity)
    {
        return new TenantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            // ... map other properties
        };
    }

    public Tenant MapToEntity(CreateTenantDto createDto)
    {
        return new Tenant
        {
            Name = createDto.Name,
            // ... map other properties
        };
    }

    public void MapToEntity(UpdateTenantDto updateDto, Tenant entity)
    {
        entity.Name = updateDto.Name;
        // ... update other properties
    }
}
```

### Step 3: Register the Mapper

Add the mapper to the DI container in `Program.cs`:

```csharp
builder.Services.AddScoped<TenantMapper>();
```

### Step 4: Create the Controller

Inherit from `BaseController` (for BaseEntity) or `BaseControllerLong` (for BaseEntityLong):

```csharp
// For entities inheriting from BaseEntity
public class TenantsController(VoxBoxDbContext dbContext, TenantMapper mapper)
    : BaseController<Tenant, TenantDto, CreateTenantDto, UpdateTenantDto>(dbContext, mapper)
{
    // Optional: Override methods to add custom behavior
}

// For entities inheriting from BaseEntityLong
public class UsersController(VoxBoxDbContext dbContext, UserMapper mapper)
    : BaseControllerLong<User, UserDto, CreateUserDto, UpdateUserDto>(dbContext, mapper)
{
    // Optional: Override methods to add custom behavior
}
```

## Customization

### Override Methods

You can override any method to customize behavior:

```csharp
public class TenantsController(VoxBoxDbContext dbContext, TenantMapper mapper)
    : BaseController<Tenant, TenantDto, CreateTenantDto, UpdateTenantDto>(dbContext, mapper)
{
    // Override GetAll to add custom logic
    public override async Task<ActionResult<IEnumerable<TDto>>> GetAll(CancellationToken cancellationToken)
    {
        // Custom implementation
        return await base.GetAll(cancellationToken);
    }
}
```

### Custom Filters

Override `ApplyCustomFilters` to add complex filtering logic:

```csharp
protected override IQueryable<Tenant> ApplyCustomFilters(IQueryable<Tenant> query, PagedRequest request)
{
    // Add custom filtering logic based on request parameters
    if (!string.IsNullOrEmpty(request.CustomParameter))
    {
        query = query.Where(t => t.SomeProperty == request.CustomParameter);
    }
    
    return base.ApplyCustomFilters(query, request);
}
```

### Lifecycle Hooks

Use lifecycle hooks to add custom logic before or after operations:

```csharp
protected override void BeforeCreate(Tenant entity)
{
    // Custom logic before creating
    entity.SomeProperty = "CustomValue";
}

protected override void AfterCreate(Tenant entity)
{
    // Custom logic after creating (e.g., send notification)
    _notificationService.SendNotification($"Tenant {entity.Name} created");
}
```

**Available Hooks:**
- `BeforeCreate(TEntity entity)`
- `AfterCreate(TEntity entity)`
- `BeforeUpdate(TEntity entity)`
- `AfterUpdate(TEntity entity)`
- `BeforeDelete(TEntity entity)`
- `AfterDelete(TEntity entity)`

### Add Custom Endpoints

You can add custom endpoints alongside the inherited ones:

```csharp
public class TenantsController(VoxBoxDbContext dbContext, TenantMapper mapper)
    : BaseController<Tenant, TenantDto, CreateTenantDto, UpdateTenantDto>(dbContext, mapper)
{
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetActiveTenants(CancellationToken cancellationToken)
    {
        var repository = DbContext.GetRepository<Tenant>();
        var tenants = await repository.GetAllAsync(includeDeleted: false, cancellationToken);
        var activeTenants = tenants.Where(t => t.IsActive);
        var dtos = activeTenants.Select(t => Mapper.MapToDto(t));
        return Ok(dtos);
    }
}
```

## Examples

### Example 1: Simple Controller

```csharp
public class UsersController(VoxBoxDbContext dbContext, UserMapper mapper)
    : BaseController<User, UserDto, CreateUserDto, UpdateUserDto>(dbContext, mapper)
{
}
```

This gives you all CRUD operations with paging, sorting, and filtering out of the box.

### Example 2: Controller with Custom Logic

```csharp
public class TenantsController(VoxBoxDbContext dbContext, TenantMapper mapper)
    : BaseController<Tenant, TenantDto, CreateTenantDto, UpdateTenantDto>(dbContext, mapper)
{
    protected override void BeforeCreate(Tenant entity)
    {
        // Ensure tenancy name is lowercase
        entity.TenancyName = entity.TenancyName.ToLower();
    }
    
    protected override void BeforeDelete(Tenant entity)
    {
        // Prevent deletion of host tenant
        if (entity.IsHost)
        {
            throw new InvalidOperationException("Cannot delete host tenant");
        }
    }
}
```

### Example 3: Controller with Custom Endpoint

```csharp
public class TenantsController(VoxBoxDbContext dbContext, TenantMapper mapper)
    : BaseController<Tenant, TenantDto, CreateTenantDto, UpdateTenantDto>(dbContext, mapper)
{
    [HttpGet("search")]
    public async Task<ActionResult<PagedResponse<TenantDto>>> Search(
        [FromQuery] string searchTerm,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var repository = DbContext.GetRepository<Tenant>();
        var query = (await repository.GetAllAsync(includeDeleted: false, cancellationToken))
            .AsQueryable()
            .Where(t => t.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        t.TenancyName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        
        query = query.ApplySorting(request.SortBy, request.SortDirection);
        
        var totalCount = query.Count();
        var entities = query.Skip(request.Skip).Take(request.Take).ToList();
        var dtos = entities.Select(e => Mapper.MapToDto(e));
        
        var response = new PagedResponse<TenantDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
        
        return Ok(response);
    }
}
```

## Best Practices

1. **Keep Controllers Thin**: Use the base controller for standard operations and only override when necessary
2. **Use Lifecycle Hooks**: Prefer using lifecycle hooks over overriding full methods
3. **Validate Input**: Use data annotations on DTOs for validation
4. **Custom Filters**: Use `ApplyCustomFilters` for complex filtering logic
5. **Mapper Logic**: Keep mapping logic simple; for complex scenarios, consider using a library like AutoMapper
6. **Async/Await**: Always use async operations and pass CancellationToken
7. **Error Handling**: The base controller handles common errors; add custom error handling for specific cases

## API Testing

Use the provided endpoints for testing:

```bash
# Get all tenants
curl -X GET "http://localhost:5000/api/tenants"

# Get paginated tenants
curl -X GET "http://localhost:5000/api/tenants/paged?pageNumber=1&pageSize=10"

# Get tenants with sorting
curl -X GET "http://localhost:5000/api/tenants/paged?sortBy=Name&sortDirection=Ascending"

# Get tenants with filtering
curl -X GET "http://localhost:5000/api/tenants/paged?filter=IsActive:true"

# Get tenants with multiple filters
curl -X GET "http://localhost:5000/api/tenants/paged?filter=IsActive:true;IsPrivate:false"

# Get tenant by ID
curl -X GET "http://localhost:5000/api/tenants/{id}"

# Create tenant
curl -X POST "http://localhost:5000/api/tenants" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Tenant","tenancyName":"test","isActive":true}'

# Update tenant
curl -X PUT "http://localhost:5000/api/tenants/{id}" \
  -H "Content-Type: application/json" \
  -d '{"name":"Updated Tenant","tenancyName":"updated","isActive":true}'

# Delete tenant
curl -X DELETE "http://localhost:5000/api/tenants/{id}"
```
