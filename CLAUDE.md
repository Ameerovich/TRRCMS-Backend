# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**TRRCMS** (Tenure Rights Registration & Claims Management System) is a comprehensive .NET 8 solution developed for UN-Habitat to support property rights registration, claims management, and land tenure documentation in Aleppo, Syria. The system manages buildings, property units, persons, surveys, claims, and related tenure documentation with full audit trail and offline tablet synchronization support.

**Current Status:** v0.12.0 - Survey Workflows Complete (95% backend API complete)

---

## Development Commands

All commands should be run from the repository root (`/e/Work/UN/Project/My Solution/TRRCMS/`).

### Build
```bash
dotnet build
```

### Run API
```bash
dotnet run --project src/TRRCMS.WebAPI
# API starts at https://localhost:7204
# Swagger UI at https://localhost:7204/swagger
```

### Restore Packages
```bash
dotnet restore
```

### Database Migrations
```bash
# View pending migrations
dotnet ef migrations list --project src/TRRCMS.Infrastructure

# Apply migrations to database
dotnet ef database update --project src/TRRCMS.Infrastructure --startup-project src/TRRCMS.WebAPI

# Add new migration (Infrastructure project)
dotnet ef migrations add [MigrationName] --project src/TRRCMS.Infrastructure --startup-project src/TRRCMS.WebAPI
```

### Docker (Quick Start)
```bash
docker compose up --build      # Start with fresh build
docker compose up              # Start (fast)
docker compose down            # Stop containers
docker compose down -v         # Stop and delete database volume
docker compose logs -f api     # View API logs
docker compose restart api     # Restart API after code changes
```

**Docker Access:** http://localhost:8080/swagger (or http://192.168.99.100:8080 for Docker Toolbox)

### Setup Local Development
1. Install PostgreSQL 16+ and PostGIS 3.x extension
2. Create database: `TRRCMS_Dev`
3. Enable PostGIS: `CREATE EXTENSION IF NOT EXISTS postgis;`
4. Copy `src/TRRCMS.WebAPI/appsettings.example.json` → `appsettings.Development.json`
5. Update connection string with your PostgreSQL password
6. Run: `dotnet ef database update --project src/TRRCMS.Infrastructure --startup-project src/TRRCMS.WebAPI`
7. Run: `dotnet run --project src/TRRCMS.WebAPI`

See `SETUP_GUIDE.md` for detailed instructions.

---

## Architecture Overview

### Layer Dependencies (Clean Architecture)

```
TRRCMS.Domain (No dependencies except NetTopologySuite)
    ↑
TRRCMS.Application (Depends on Domain; exposes MediatR handlers)
    ↑
TRRCMS.Infrastructure (Depends on Domain & Application; implements interfaces)
    ↑
TRRCMS.WebAPI (Depends on all layers; entry point)
```

**Key Principle:** Dependency flows inward toward the Domain layer.

### Project Responsibilities

| Project | Role |
|---------|------|
| **Domain** | Enterprise business rules - Entities (Building, PropertyUnit, Claim, Survey, Person), Enums, Value Objects. Zero external dependencies. |
| **Application** | Business logic layer - CQRS Commands/Queries (MediatR), DTOs, Repository interfaces, Services, Validation (FluentValidation), Mapping (AutoMapper). |
| **Infrastructure** | Technical implementation - EF Core database context, Repository implementations, External services (FileStorage, Audit, Import), Security (JWT, BCrypt). |
| **WebAPI** | HTTP interface - Controllers, Dependency Injection setup, Middleware, Swagger/OpenAPI documentation. |

### Architectural Patterns Used

1. **Clean Architecture** - Layered design with dependency inversion
2. **CQRS (Command Query Responsibility Segregation)** - MediatR for command/query separation
3. **Repository Pattern** - Data access abstraction with unit of work
4. **Domain-Driven Design (DDD)** - Rich domain entities with domain methods
5. **Factory Pattern** - Static `Create()` methods for entity instantiation
6. **Soft Delete Pattern** - `IsDeleted` flag with audit trail (CreatedBy, ModifiedBy, DeletedBy)
7. **AutoMapper** - Entity ↔ DTO transformations

---

## Key Patterns & Conventions

### Commands & Queries Structure

**Folder Pattern:**
```
TRRCMS.Application/
├── {Feature}/
│   ├── Commands/
│   │   ├── Create{Entity}/
│   │   │   ├── Create{Entity}Command.cs       (record, implements IRequest<TDto>)
│   │   │   ├── Create{Entity}CommandHandler.cs (IRequestHandler<TRequest, TResponse>)
│   │   │   └── Create{Entity}CommandValidator.cs (AbstractValidator<TCommand>)
│   ├── Queries/
│   │   ├── Get{Entity}/
│   │   │   ├── Get{Entity}Query.cs            (record, implements IRequest<TDto>)
│   │   │   └── Get{Entity}QueryHandler.cs
│   └── Dtos/
│       └── {Entity}Dto.cs
```

**One handler per command/query** - Each CQRS operation has its own folder for clarity.

### Entity Creation Pattern

Entities use **static factory methods**, not constructors:

```csharp
// In Domain entity
public static Claim Create(string claimNumber, Guid propertyUnitId, ..., Guid createdByUserId)
{
    return new Claim
    {
        Id = Guid.NewGuid(),
        ClaimNumber = claimNumber,
        // ... other properties
        CreatedAtUtc = DateTime.UtcNow,
        CreatedByUserId = createdByUserId
    };
}

// In Command Handler
var claim = Claim.Create(claimNumber, propertyUnitId, ..., userId);
await _repository.AddAsync(claim);
```

### BaseAuditableEntity

All entities inherit from `BaseAuditableEntity` providing:
- `Id` (Guid primary key)
- `CreatedAtUtc`, `CreatedByUserId` - Creation audit
- `LastModifiedAtUtc`, `LastModifiedByUserId` - Modification audit
- `IsDeleted`, `DeletedAtUtc`, `DeletedByUserId` - Soft delete support

**Soft Delete Queries:** Repository methods automatically filter `IsDeleted == false`.

### Repository Pattern

**Interfaces in Application layer:**
```csharp
public interface IPropertyUnitRepository
{
    Task<PropertyUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<PropertyUnit>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(PropertyUnit entity);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
}
```

**Implementations in Infrastructure layer** - One repository per aggregate.

**Unit of Work pattern** - `IUnitOfWork` facade for transaction management:
```csharp
var uow = _unitOfWork;
uow.PropertyUnits.Add(unit);
uow.Claims.Add(claim);
await uow.SaveChangesAsync(); // Single SaveChanges
```

### Validation Pattern

Commands use **FluentValidation**:
```csharp
public class CreateClaimCommandValidator : AbstractValidator<CreateClaimCommand>
{
    public CreateClaimCommandValidator(IPropertyUnitRepository propertyUnitRepo)
    {
        RuleFor(x => x.PropertyUnitId)
            .NotEmpty()
            .Must(id => propertyUnitRepo.ExistsAsync(id).Result);
    }
}
```

### DTO Mapping Pattern

**AutoMapper profiles** - One profile per feature:
```csharp
public class ClaimsProfile : Profile
{
    public ClaimsProfile()
    {
        CreateMap<Claim, ClaimDto>();
        CreateMap<CreateClaimCommand, Claim>();
    }
}
```

---

## Important Domain Concepts

### Core Entities

| Entity | Purpose |
|--------|---------|
| **Building** | Physical buildings with PostGIS geometry (POLYGON), building code (17-digit admin hierarchy), status, type |
| **PropertyUnit** | Individual occupiable spaces (Apartment, Shop, Office, Warehouse, Other) with status (Occupied, Vacant, Damaged, etc.) |
| **Person** | Individual records with demographics, identification documents |
| **Household** | Family unit within a property with demographics (gender/age counts), displacement status, economic indicators |
| **Claim** | Tenure rights claim with lifecycle (Draft → Submitted → Approved/Rejected → Certificate Issued) |
| **Survey** | Field survey (tablet, GPS-enabled, offline-capable) or Office survey (with auto-claim creation) |
| **PersonPropertyRelation** | Links persons to property units (Owner, Tenant, Heir, Occupant) with evidence |
| **Evidence** & **Document** | Supporting materials (photos, ID documents, tenure documents) |
| **Certificate** | Issued tenure certificate |
| **SyncSession** | Tablet LAN synchronization session (4-step: session → upload → download → acknowledge) |
| **Vocabulary** | Controlled lookup tables with semantic versioning (MAJOR.MINOR.PATCH) |

### Claim Lifecycle States

```
DraftPendingSubmission → Submitted → InitialScreening → UnderReview →
AwaitingDocuments → ConflictDetected → InAdjudication →
Approved/Rejected → CertificateIssued → Archived
```

### Building Code Format

**Format:** `GGDDSSCCCCNNBBBBB` (17 digits)
- GG: Governorate (2)
- DD: District (2)
- SS: Sub-District (2)
- CCC: Community (3)
- NNN: Neighborhood (3)
- BBBBB: Building Number (5)

**Display:** `GG-DD-SS-CCC-NNN-BBBBB` (via BuildingIdFormatted)

### Claim Number Format

**Format:** `CLM-YYYY-NNNNNNNNN`
- CLM: Claim prefix
- YYYY: Year
- NNNNNNNNN: Sequential number (9 digits)

Example: `CLM-2026-000000001`

### Enums (for filtering & classification)

**PropertyUnitType:** Apartment (1), Shop (2), Office (3), Warehouse (4), Other (5)

**PropertyUnitStatus:** Occupied (1), Vacant (2), Damaged (3), UnderRenovation (4), Uninhabitable (5), Locked (6), Unknown (99)

---

## Database & Persistence

### PostgreSQL with PostGIS

- **Database:** PostgreSQL 16+
- **ORM:** Entity Framework Core 8
- **Spatial Support:** PostGIS 3.x with NetTopologySuite
- **Soft Delete:** All queries filter `IsDeleted == false`

### Entity Framework Conventions

- Entity configurations in `Infrastructure/Persistence/Configurations/`
- Migrations in `Infrastructure/Migrations/`
- DbContext: `ApplicationDbContext`

### Connection String

**Development:** `Host=localhost;Database=TRRCMS_Dev;Username=postgres;Password=YourPassword`

**Environment Variable:** `ConnectionStrings:DefaultConnection` in `appsettings.Development.json`

---

## File Organization

### Application Layer Structure

```
TRRCMS.Application/
├── Auth/                    # Authentication
├── Buildings/               # Building CRUD
├── Claims/                  # Claims lifecycle
├── PropertyUnits/           # Property unit CRUD
├── Surveys/                 # Field & Office surveys
├── Persons/                 # Person management
├── Households/              # Household management
├── BuildingAssignments/     # Field collector assignments
├── Sync/                    # Tablet LAN sync (4-step protocol)
├── Vocabularies/            # Controlled vocabularies
├── Import/                  # UHC import pipeline
├── Common/
│   ├── Behaviors/           # MediatR pipeline behaviors
│   ├── Exceptions/          # NotFoundException, ValidationException
│   ├── Interfaces/          # Repository & Service contracts
│   ├── Mappings/            # AutoMapper profiles
│   └── Models/              # Shared response models
└── {Feature}/
    ├── Commands/
    ├── Queries/
    └── Dtos/
```

### Infrastructure Layer Structure

```
TRRCMS.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/      # Entity configurations
│   ├── Migrations/          # EF migrations
│   └── Repositories/        # Repository implementations (27+)
├── Services/                # Business services
└── Security/                # JWT, BCrypt, Auth
```

---

## Current Implementation Notes

### PropertyUnit Queries (Current)

1. **GetAllPropertyUnitsQuery** - Returns all units (no filtering), ordered by BuildingId
2. **GetPropertyUnitsByBuildingQuery** - Filter by single BuildingId
3. **GetPropertyUnitQuery** - Single unit by ID

**Current Limitations:**
- No filtering by PropertyUnitType or PropertyUnitStatus
- No pagination support
- No grouping capability

### Tablet LAN Sync Protocol (v0.12.0)

4-step synchronization for offline field survey data:
1. **CreateSyncSession** - Establish session
2. **UploadSyncPackage** - Field tablet uploads data (with SHA-256 checksum)
3. **GetSyncAssignments** - Download assignments & vocabularies
4. **AcknowledgeSyncAssignments** - Confirm download

---

## Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | .NET / ASP.NET Core | 8.0 |
| Language | C# | 12 |
| Database | PostgreSQL | 16+ |
| ORM | Entity Framework Core | 8.0.11 |
| API Architecture | MediatR | 12.4.1 |
| Object Mapping | AutoMapper | 13.0.1 |
| Validation | FluentValidation | 12.1.1 |
| Spatial Data | NetTopologySuite | 2.5.0 |
| PostGIS Driver | Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.11 |
| Authentication | JWT Bearer | .NET built-in |
| Password Hashing | BCrypt.Net-Next | Latest |
| API Documentation | Swagger/Swashbuckle | 6.6.2 |

---

## Important Resources

| Resource | Location | Purpose |
|----------|----------|---------|
| **README.md** | `/README.md` | Complete project overview, API endpoints, workflows |
| **QUICK_START.md** | `/QUICK_START.md` | Docker-based quick start (5 min setup) |
| **SETUP_GUIDE.md** | `/SETUP_GUIDE.md` | Manual setup for local development |
| **Postman Collection** | `/postman/TRRCMS_Survey_API.postman_collection.json` | 40+ API requests with test scripts |
| **Documentation** | `/docs/` | API specifications and testing guides |
| **Test Users** | See SETUP_GUIDE.md | Seed endpoint: `POST /api/Auth/seed` |

---

## Best Practices for This Codebase

1. **Respect layer boundaries** - Domain has no external dependencies; keep it pure
2. **Use repositories for data access** - Never EF Core directly in handlers
3. **One handler per command/query** - Organize in dedicated folders
4. **Validate in Application layer** - Use FluentValidation on commands
5. **Factory methods for entities** - Use `Entity.Create()` not constructors
6. **Soft delete queries** - Repository methods automatically filter deleted entities
7. **Audit trail** - All modifications tracked via BaseAuditableEntity properties
8. **DTOs at boundaries** - Controllers return DTOs, not domain entities
9. **Transaction management** - Use IUnitOfWork for multi-repo operations
10. **Pagination support** - Implement for list endpoints (see PropertyUnitDto patterns)
