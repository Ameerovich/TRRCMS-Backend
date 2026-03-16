# TRRCMS - Tenure Rights Registration & Claims Management System

A backend system developed for **UN-Habitat** to support property rights registration, claims management, and land tenure documentation in Aleppo, Syria.

Built with ASP.NET Core 8 following Clean Architecture, CQRS, and Domain-Driven Design principles.

---

## Quick Start

### Option 1: Docker (Recommended)

```bash
git clone https://github.com/Ameerovich/TRRCMS.git
cd TRRCMS
docker compose up --build
```

Open http://localhost:8080/swagger to access the API.

### Option 2: Local Development

**Prerequisites:** .NET 8 SDK, PostgreSQL 16+ with PostGIS 3.x

```bash
git clone https://github.com/Ameerovich/TRRCMS.git
cd TRRCMS/src

# Update connection string in TRRCMS.WebAPI/appsettings.Development.json
dotnet ef database update --project TRRCMS.Infrastructure --startup-project TRRCMS.WebAPI
dotnet run --project TRRCMS.WebAPI
```

Open http://localhost:5031/swagger to access the API (HTTP profile).
For HTTPS: `dotnet run --project TRRCMS.WebAPI --launch-profile https` then open https://localhost:7204/swagger.

See [SETUP_GUIDE.md](SETUP_GUIDE.md) for detailed local setup and [DOCKER_README.md](DOCKER_README.md) for Docker configuration.

---

## Technology Stack

| Component | Technology |
|-----------|------------|
| Runtime | ASP.NET Core 8.0 (C#) |
| Database | PostgreSQL 16 + PostGIS 3.4 |
| ORM | Entity Framework Core 8.0 |
| Architecture | Clean Architecture, CQRS (MediatR), Repository + Unit of Work |
| Authentication | JWT Bearer Tokens, BCrypt password hashing |
| Authorization | Policy-based with fine-grained permissions (30+) |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Spatial | NetTopologySuite + PostGIS (polygon search, geo-queries) |
| API Docs | Swagger / OpenAPI |
| Containerization | Docker Compose (API + PostGIS) |

---

## Architecture

```
TRRCMS/
├── src/
│   ├── TRRCMS.Domain/            # Entities, enums, value objects (no dependencies)
│   ├── TRRCMS.Application/       # CQRS commands/queries, interfaces, DTOs, validators
│   ├── TRRCMS.Infrastructure/    # EF Core, repositories, external services, migrations
│   ├── TRRCMS.WebAPI/            # Controllers, middleware, DI configuration
│   └── tools/                    # Development utilities
├── postman/                      # Postman collections and environment
├── docker-compose.yml
├── Dockerfile
└── TRRCMS.sln
```

**Layer dependencies flow inward:** WebAPI -> Infrastructure -> Application -> Domain

Each feature is implemented as a MediatR command or query with its own handler, validator, and DTO. Repositories abstract data access behind interfaces defined in the Application layer.

---

## API Modules

The system exposes **22 controllers** covering the following functional areas. Full endpoint documentation is available via Swagger at runtime.

### Core Workflows

| Module | Controller | Description |
|--------|-----------|-------------|
| Office Survey | `SurveysController` | Desktop survey workflow with automatic claim creation |
| Import | `ImportController` | .uhc package import pipeline with staging, validation, and commit |
| LAN Sync | `SyncController` | Tablet-to-server sync protocol (session, upload, download, ack) |
| Claims | `ClaimsController` | Claim lifecycle management with evidence and relation updates |
| Dashboard | `DashboardController` | Summary statistics and registration coverage metrics |

### Entity Management

| Module | Controller | Description |
|--------|-----------|-------------|
| Buildings | `BuildingsController` | Building CRUD with PostGIS polygon geometry and spatial search |
| Building Documents | `BuildingDocumentsController` | Photo and document attachments for buildings |
| Property Units | `PropertyUnitsController` | Units within buildings with filtering, grouping, and status tracking |
| Persons | `PersonsController` | Person registration with identity documents |
| Households | `HouseholdsController` | Household demographics, displacement, and vulnerability data |
| Relations | `PersonPropertyRelationsController` | Person-property relations (Owner, Tenant, Heir, Occupant) |
| Evidence | `EvidencesController` | Evidence documents with many-to-many relation linking |

### Administration

| Module | Controller | Description |
|--------|-----------|-------------|
| Auth | `AuthController` | JWT login/logout, password management, user profile |
| Users | `UsersController` | User management with role-based access control (6 roles) |
| Building Assignments | `BuildingAssignmentsController` | Assign buildings to field collectors for survey |
| Vocabularies | `VocabulariesController` | Bilingual controlled vocabularies (Arabic/English) with versioning |
| Administrative Divisions | `AdministrativeDivisionsController` | 4-level hierarchy: Governorate > District > Sub-District > Community |
| Neighborhoods | `NeighborhoodsController` | Spatial neighborhood data for map navigation |
| Landmarks | `LandmarksController` | Landmark reference data |
| Streets | `StreetsController` | Street reference data |
| Conflicts | `ConflictsController` | Duplicate detection, conflict resolution, and merge operations |
| Security Settings | `SecuritySettingsController` | Password policies, session lockout, and access control settings |

---

## Database

PostgreSQL 16 with PostGIS extension. The schema is managed through EF Core code-first migrations with automatic application on startup.

**Key tables:** Users, Buildings, PropertyUnits, Persons, Households, PersonPropertyRelations, Surveys, Claims, Evidences, EvidenceRelations, BuildingDocuments, ImportPackages, Vocabularies, Neighborhoods, Landmarks, Streets, BuildingAssignments, SyncSessions, SecurityPolicies, AuditLogs, ConflictResolutions, and the 4-level administrative hierarchy (Governorates, Districts, SubDistricts, Communities).

Administrative hierarchy data and default vocabularies are auto-seeded on first startup.

---

## Authentication and Authorization

**Authentication:** JWT Bearer tokens with configurable expiration. Default users are seeded on startup.

**Authorization:** Policy-based with 30+ fine-grained permissions using the format `{Module}_{Action}`.

| Role | Access Level |
|------|-------------|
| Administrator | Full system access |
| DataManager | Manage surveys, claims, and data |
| OfficeClerk | Own office surveys, finalize with claims |
| FieldCollector | Own field surveys, data collection, sync |
| FieldSupervisor | View surveys and assignments |
| Analyst | Read-only access |

**Default credentials (development):**

| Username | Password | Role |
|----------|----------|------|
| admin | Admin@123 | Administrator |
| datamanager | Data@123 | DataManager |
| clerk | Clerk@123 | OfficeClerk |
| collector | Field@123 | FieldCollector |
| supervisor | Super@123 | FieldSupervisor |
| analyst | Analyst@123 | Analyst |

---

## Key Features

- **Survey Workflows** - Field (mobile) and office (desktop) survey data collection with draft save/resume, contact person management, finalization, and cancellation
- **Import Pipeline** - .uhc package format for offline field data with staging, validation, duplicate detection, conflict resolution, and atomic commit
- **LAN Sync Protocol** - 4-step sync (session, upload, download assignments, acknowledge) for tablet-to-server data transfer
- **Spatial Queries** - PostGIS polygon search for buildings, neighborhood boundary queries, and GPS coordinate support
- **Evidence Linking** - Many-to-many evidence-to-relation linking via EvidenceRelation join entity
- **Bilingual Vocabularies** - Controlled dropdowns with Arabic and English labels, semantic versioning, JSON import/export
- **Administrative Hierarchy** - 4-level geographic hierarchy (Governorate > District > Sub-District > Community) with cascading lookups
- **Audit Trail** - All operations logged with before/after values, user attribution, and entity tracking
- **Soft Deletes** - All entities support soft deletion with cascading to dependent records
- **Security Policies** - Configurable password policies, session lockout, and access control settings
- **Duplicate Detection** - Person and property matching with configurable confidence thresholds and merge workflows

---

## Development

### Build

```bash
cd src
dotnet build
```

### Run

```bash
dotnet run --project TRRCMS.WebAPI
```

### Apply Migrations

```bash
dotnet ef database update --project TRRCMS.Infrastructure --startup-project TRRCMS.WebAPI
```

### Add a New Migration

```bash
dotnet ef migrations add MigrationName --project TRRCMS.Infrastructure --startup-project TRRCMS.WebAPI
```

### Docker

```bash
# Start
docker compose up --build

# Stop
docker compose down

# Reset database
docker compose down -v && docker compose up --build
```

| Service | URL |
|---------|-----|
| Swagger UI | http://localhost:8080/swagger |
| API Base | http://localhost:8080/api/v1 |
| Database | localhost:5432 (TRRCMS_Dev / postgres) |

---

## Documentation

| Document | Description |
|----------|-------------|
| [SETUP_GUIDE.md](SETUP_GUIDE.md) | Local development environment setup |
| [DOCKER_README.md](DOCKER_README.md) | Docker deployment and configuration |
| Swagger UI | Interactive API documentation (available at runtime) |

---

## License

Proprietary - UN-Habitat 2024-2026. Internal project.
