# VoxBox

VoxBox is a full-stack application built with Angular frontend and .NET backend, following SOLID, KISS, and DRY principles.

## Project Structure

```
VoxBox/
├── frontend/             # Angular 18 application (TypeScript)
│   ├── src/
│   │   ├── app/         # Angular components, services, modules
│   │   ├── assets/      # Static assets
│   │   ├── environments/# Environment configurations
│   │   └── styles/      # Global styles
│   └── public/          # Public assets
├── backend/              # .NET 8 Web API
│   ├── src/
│   │   ├── VoxBox.Api/           # API layer (presentation)
│   │   ├── VoxBox.Core/          # Business logic & domain models
│   │   └── VoxBox.Infrastructure/# Data access & external services
│   └── tests/
│       └── VoxBox.Tests/         # Unit tests (xUnit + Moq)
├── docs/                 # Project documentation
└── README.md             # This file
```

## Technology Stack

### Frontend
- **Framework**: Angular 18 (standalone components)
- **Language**: TypeScript 5.x
- **Styles**: SCSS
- **Routing**: Angular Router (enabled)
- **SSR**: Disabled (for simpler deployment)

### Backend
- **Framework**: .NET 8.0 Web API
- **Language**: C# 12
- **Database**: SQL Server
- **ORM**: Entity Framework Core 8.0 (Code First)
- **Architecture**: Clean Architecture with SOLID principles

## Architecture Principles

### SOLID Principles
- **S**ingle Responsibility: Each class has one reason to change
- **O**pen/Closed: Open for extension, closed for modification
- **L**iskov Substitution: Derived classes extend base types correctly
- **I**nterface Segregation: Focused interfaces over large ones
- **D**ependency Inversion: Depend on abstractions, not concretions

### KISS (Keep It Simple, Stupid)
- Simple, focused classes with minimal responsibility
- Straightforward data flow
- No unnecessary abstraction layers

### DRY (Don't Repeat Yourself)
- Shared code moved to appropriate layers
- Generic repository patterns for common operations
- Common result types and base entities

## Backend Architecture (Clean Architecture)

```
┌─────────────────────────────────────────────────────────────┐
│                      VoxBox.Api                              │
│                 (Presentation Layer)                         │
│  - Controllers / Minimal APIs                                │
│  - Request/Response Models                                   │
│  - Dependency Injection Setup                                │
├─────────────────────────────────────────────────────────────┤
│                    VoxBox.Core                               │
│                (Business Logic Layer)                        │
│  - Domain Entities                                           │
│  - Business Rules & Interfaces                               │
│  - Common Utilities (Result, BaseEntity)                     │
├─────────────────────────────────────────────────────────────┤
│                 VoxBox.Infrastructure                        │
│               (Data Access Layer)                            │
│  - DbContext & Repositories                                  │
│  - EF Core Migrations                                        │
│  - External Services                                         │
└─────────────────────────────────────────────────────────────┘
```

### Key Components

#### Core Layer
- **Entities**: Domain models (e.g., `SampleEntity`)
- **Interfaces/Persistence**: Abstractions (`IRepository<T>`, `IUnitOfWork`)
- **Common**: Shared types (`BaseEntity`, `Result`)

#### Infrastructure Layer
- **Persistence**: EF Core implementation (`VoxBoxDbContext`, `EfRepository<T>`)
- **Data**: Configuration helpers

#### API Layer
- Minimal APIs or Controllers
- Swagger/OpenAPI documentation
- Dependency injection configuration

## Database Setup (Code First)

### Prerequisites
- SQL Server (LocalDB, Express, or Full)
- .NET 8.0 SDK

### Creating Migrations

```bash
cd backend/src/VoxBox.Infrastructure
dotnet ef migrations add InitialCreate --project ../VoxBox.Infrastructure/VoxBox.Infrastructure.csproj --startup-project ../../VoxBox.Api/VoxBox.Api.csproj
```

### Updating Database

```bash
dotnet ef database update --project ../VoxBox.Infrastructure/VoxBox.Infrastructure.csproj --startup-project ../../VoxBox.Api/VoxBox.Api.csproj
```

## Getting Started

### Frontend

```bash
cd frontend
npm install
ng serve
```

### Backend

```bash
cd backend/src/VoxBox.Api
dotnet restore
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`).

## Configuration

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "VoxBoxConnection": "Server=localhost;Database=VoxBoxDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Development Guidelines

1. **New Entities**: Add to `VoxBox.Core/Entities/`, create DbSet in `VoxBoxDbContext`, and configure in `OnModelCreating`
2. **New Services**: Create interfaces in `VoxBox.Core/Interfaces/`, implement in `VoxBox.Infrastructure/`
3. **API Endpoints**: Add to `VoxBox.Api/Program.cs` or create controllers
4. **Tests**: Add to `VoxBox.Tests/` following AAA pattern (Arrange, Act, Assert)

## License

TBD
