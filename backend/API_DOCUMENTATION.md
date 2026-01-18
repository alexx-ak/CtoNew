# VoxBox API Documentation

## Overview

The VoxBox API provides a RESTful interface for managing tenants in a multi-tenant voting system. The API includes OpenAPI documentation with Aspire support for easy exploration and testing of endpoints.

## Accessing API Documentation

When running the application in **Development** mode, the API documentation is available at:

- **OpenAPI Specification**: `https://localhost:<port>/openapi/v1.json`

The OpenAPI specification can be imported into tools like:
- Postman for testing API endpoints
- Swagger UI for interactive documentation
- Insomnia for API development

## Available Endpoints

### Tenants API

Base route: `/api/tenants`

#### GET /api/tenants
Get all tenants (excluding soft-deleted records)

**Response**: `200 OK`
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Example Tenant",
    "tenancyName": "example",
    "isPrivate": false,
    "voteWeightMode": 0,
    "adminIdentifiers": "admin@example.com",
    "isActive": true,
    "createdAt": "2024-01-18T10:30:00Z",
    "createdBy": null,
    "updatedAt": null,
    "modifiedBy": null
  }
]
```

#### GET /api/tenants/{id}
Get a specific tenant by ID

**Parameters**:
- `id` (path, required): Tenant GUID

**Responses**:
- `200 OK`: Tenant found
- `404 Not Found`: Tenant not found

#### POST /api/tenants
Create a new tenant

**Request Body**:
```json
{
  "name": "New Tenant",
  "tenancyName": "newtenant",
  "isPrivate": false,
  "voteWeightMode": 0,
  "adminIdentifiers": "admin@example.com",
  "isActive": true
}
```

**Validations**:
- `name`: Required, max 128 characters
- `tenancyName`: Required, max 64 characters
- `adminIdentifiers`: Max 50 characters

**Responses**:
- `201 Created`: Tenant created successfully
- `400 Bad Request`: Validation errors

#### PUT /api/tenants/{id}
Update an existing tenant

**Parameters**:
- `id` (path, required): Tenant GUID

**Request Body**:
```json
{
  "name": "Updated Tenant",
  "tenancyName": "updated",
  "isPrivate": true,
  "voteWeightMode": 1,
  "adminIdentifiers": "admin@updated.com",
  "isActive": true
}
```

**Responses**:
- `200 OK`: Tenant updated successfully
- `400 Bad Request`: Validation errors
- `404 Not Found`: Tenant not found

#### DELETE /api/tenants/{id}
Soft delete a tenant

**Parameters**:
- `id` (path, required): Tenant GUID

**Responses**:
- `204 No Content`: Tenant deleted successfully
- `404 Not Found`: Tenant not found

**Note**: This performs a soft delete - the tenant is marked as deleted but not physically removed from the database.

## Data Models

**Implementation note**: API DTOs are implemented as C# `record` types with `init`-only properties.

### TenantDto
Response model for tenant data
- `id`: Unique identifier (GUID/UUID v7)
- `name`: Tenant display name
- `tenancyName`: Unique tenant identifier (used in URLs)
- `isPrivate`: Whether tenant is private
- `voteWeightMode`: Voting weight calculation mode (0 = default)
- `adminIdentifiers`: Comma-separated admin identifiers
- `isActive`: Whether tenant is active
- `createdAt`: Creation timestamp (UTC)
- `createdBy`: Creator user ID
- `updatedAt`: Last modification timestamp (UTC)
- `modifiedBy`: Last modifier user ID

### CreateTenantDto
Request model for creating a tenant
- All fields from TenantDto except: `id`, `createdAt`, `createdBy`, `updatedAt`, `modifiedBy`

### UpdateTenantDto
Request model for updating a tenant
- All fields from TenantDto except: `id`, `createdAt`, `createdBy`, `updatedAt`, `modifiedBy`

## Entity Relationships

### User → Tenant Foreign Key
The `User` entity has a foreign key relationship to `Tenant`:
- Column: `TenantId` (nullable GUID)
- References: `Tenants.Id`
- Delete Behavior: `Restrict` (prevents deletion of tenant if users exist)

This ensures referential integrity between users and their associated tenants.

## Multitenancy

The VoxBox API implements a multi-tenant architecture where:
- Each request can be scoped to a specific tenant via the `TenantContext`
- The `Tenant` entity itself is not tenant-scoped (can be accessed globally)
- All other entities are automatically filtered by `TenantId` via global query filters
- Soft deletes are implemented globally - deleted records are filtered out by default

## Authentication & Authorization

**Note**: The current implementation does not include authentication/authorization. The `CreatedBy`, `ModifiedBy`, and `DeletedBy` fields are set to `null` until auth is implemented.

## Technology Stack

- **Framework**: ASP.NET Core 10.0
- **API Documentation**: Microsoft.AspNetCore.OpenApi 10.0.1 with Aspire support
- **Database**: SQL Server with Entity Framework Core 10.0
- **ID Generation**: UUID v7 (time-sortable GUIDs)
- **Architecture**: Clean Architecture with Repository Pattern

## Running Migrations

The API automatically applies pending migrations on startup. To create a new migration:

```bash
cd src/VoxBox.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../VoxBox.Api
```

## Testing the API

1. **Using OpenAPI tools**: Import the OpenAPI specification from `/openapi/v1.json` into your preferred API tool
2. **Using curl**:
   ```bash
   # Get all tenants
   curl -X GET https://localhost:5001/api/tenants
   
   # Create a tenant
   curl -X POST https://localhost:5001/api/tenants \
     -H "Content-Type: application/json" \
     -d '{"name":"Test","tenancyName":"test","isActive":true}'
   ```
