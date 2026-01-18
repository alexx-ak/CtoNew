# VoxBox Backend

.NET 8 Web API application for VoxBox, following Clean Architecture and SOLID principles.

## Structure

```
backend/
├── src/
│   ├── VoxBox.Api/            # API Layer (Presentation)
│   │   ├── Program.cs         # Application entry point
│   │   ├── appsettings.json   # Configuration
│   │   └── Properties/        # Launch settings
│   ├── VoxBox.Core/           # Business Logic Layer
│   │   ├── Entities/          # Domain entities
│   │   ├── Interfaces/        # Abstractions
│   │   │   └── Persistence/   # Repository & UnitOfWork interfaces
│   │   └── Common/            # Shared types
│   └── VoxBox.Infrastructure/ # Data Access Layer
│       ├── Data/              # Data configuration
│       └── Persistence/       # EF Core implementation
└── tests/
    └── VoxBox.Tests/          # Unit Tests (xUnit + Moq)
```

## Technology Stack

- **Framework**: .NET 8.0 Web API
- **Language**: C# 12
- **Database**: SQL Server
- **ORM**: Entity Framework Core 8.0 (Code First)
- **Testing**: xUnit + Moq

## Architecture

### Clean Architecture Layers

1. **VoxBox.Api** (Presentation)
   - Minimal APIs
   - Dependency injection setup
   - Request/Response handling

2. **VoxBox.Core** (Business Logic)
   - Domain entities
   - Business rules
   - Persistence abstractions (interfaces)

3. **VoxBox.Infrastructure** (Data Access)
   - EF Core DbContext
   - Repository implementations
   - Database configuration

### SOLID Principles Applied

- **Single Responsibility**: Each class has one purpose
- **Open/Closed**: Extensible without modification
- **Liskov Substitution**: Proper inheritance patterns
- **Interface Segregation**: Focused interfaces
- **Dependency Inversion**: Abstractions over implementations

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB, Express, or Full)

### Configuration

Update `src/VoxBox.Api/home/engine/projectsettings.json` with your connection string:

```json
{
  "ConnectionStrings": {
    "VoxBoxConnection": "Server=localhost;Database=VoxBoxDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Running the API

```bash
cd src/VoxBox.Api
dotnet restore
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: https://localhost:5001/swagger

## Code First Migrations

### Create a new migration

```bash
cd src/VoxBox.Infrastructure
dotnet ef migrations add MigrationName --project ../VoxBox.Infrastructure/VoxBox.Infrastructure.csproj --startup-project ../../VoxBox.Api/VoxBox.Api.csproj
```

### Update the database

```bash
dotnet ef database update --project ../VoxBox.Infrastructure/VoxBox.Infrastructure.csproj --startup-project ../../VoxBox.Api/VoxBox.Api.csproj
```

## Adding New Entities

1. **Create entity** in `VoxBox.Core/Entities/`
2. **Add DbSet** in `VoxBoxDbContext`
3. **Configure** in `OnModelCreating`
4. **Create migration**

## DTO conventions

- DTOs in the API layer should be implemented as C# `record` types (not `class`) to encourage immutability and value-based semantics.
- Prefer `init`-only properties for DTOs.

## Running Tests

```bash
cd tests/VoxBox.Tests
dotnet test
```
