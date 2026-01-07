# 🏗️ TRRCMS - Tenure Rights Registration & Claims Management System

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue)](https://www.postgresql.org/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-green)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

**Backend API for UN-Habitat Aleppo Tenure Rights Registration Project**

A comprehensive system for documenting property ownership, managing displacement claims, and supporting reconstruction efforts in Aleppo, Syria.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [API Documentation](#api-documentation)
- [Development Status](#development-status)
- [Team](#team)
- [License](#license)

---

## 🎯 Overview

TRRCMS is designed to help UN-Habitat document and verify property rights in post-conflict Aleppo. The system supports:

- **Building & Property Unit Registration** - Comprehensive cadastral data management
- **Person & Household Records** - Displaced persons and household tracking with Arabic name support
- **Person-Property Relations** - Ownership, tenancy, and occupancy documentation ⭐ **NEW**
- **Ownership Claims** - Documentation and verification of property claims
- **Field Surveys** - Mobile data collection for on-site verification
- **Document Management** - Secure storage of legal documents and evidence
- **Conflict Resolution** - Tracking disputed claims and resolution processes

---

## ✨ Features

### ✅ Implemented (v0.4 - Current)
✅ **Clean Architecture** - Domain-driven design with clear separation of concerns  
✅ **Building CRUD** - Complete Create, Read, Update, Delete operations  
✅ **Property Unit CRUD** - Apartment/shop/commercial unit management  
✅ **Person Registry** - Individual registration with Arabic name support  
✅ **Household Management** - Family unit tracking with demographics and vulnerability indicators  
✅ **Person-Property Relations** - Ownership/tenancy linkage with evidence support ⭐ **NEW**  
✅ **PostgreSQL Database** - Robust relational data storage with 5 entity tables  
✅ **Entity Framework Core** - Code-first migrations and LINQ queries  
✅ **CQRS Pattern** - Command/Query separation with MediatR  
✅ **Repository Pattern** - Consistent data access layer  
✅ **Swagger/OpenAPI** - Interactive API documentation  
✅ **Arabic Support** - Full UTF-8 encoding for Arabic text (names, addresses)  
✅ **Audit Trails** - Comprehensive tracking (Created/Modified/Deleted timestamps & users)  
✅ **Soft Delete** - Data preservation with IsDeleted flag  
✅ **Computed Properties** - Dynamic calculations (DependencyRatio, IsVulnerable, DurationInDays, IsOngoing)  

### 📅 Planned
📅 **Authentication & Authorization** - JWT-based security with role-based access  
📅 **Claims Workflow** - Submission, review, verification, and resolution  
📅 **Document Upload** - PDF/image attachment system with versioning  
📅 **Evidence Management** - Document linking to claims and persons  
📅 **Search & Filtering** - Advanced queries across entities  
📅 **Reporting** - Statistical reports and data export  
📅 **Certificate Generation** - Automated tenure rights certificate creation  

---

## 🛠️ Technology Stack

### Backend
- **.NET 8** - Latest LTS version of .NET
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core 8** - ORM and database migrations
- **MediatR** - CQRS pattern implementation
- **AutoMapper** - Object-to-object mapping
- **PostgreSQL 16** - Primary database
- **Npgsql** - PostgreSQL .NET provider

### Architecture Patterns
- **Clean Architecture** - Independent of frameworks, UI, and databases
- **Domain-Driven Design** - Rich domain models with business logic
- **CQRS** - Command Query Responsibility Segregation
- **Repository Pattern** - Data access abstraction layer
- **Factory Pattern** - Entity creation through static factory methods

### Development Tools
- **Visual Studio 2022** - Primary IDE
- **Swagger UI** - API testing and documentation
- **pgAdmin 4** - Database management
- **Git** - Version control

---

## 🚀 Getting Started

### 👥 For Team Members

**New to the project?** Follow the complete **[Team Setup Guide](./SETUP_GUIDE.md)** for step-by-step instructions!

### ⚡ Quick Start (Experienced Developers)

#### Prerequisites
- .NET 8 SDK
- PostgreSQL 16+
- Visual Studio 2022 or VS Code

#### Setup Steps
```bash
# 1. Clone repository
git clone https://github.com/Ameerovich/TRRCMS.git
cd TRRCMS

# 2. Create database in PostgreSQL
# Database name: TRRCMS_Dev

# 3. Configure connection string
# Copy appsettings.json to appsettings.Development.json
# Update password in connection string

# 4. Restore packages
dotnet restore

# 5. Run migrations
cd src/TRRCMS.WebAPI
dotnet ef database update --project ../TRRCMS.Infrastructure

# 6. Run application
dotnet run

# 7. Open browser to:
# https://localhost:7204/swagger
```

⏱️ **Setup time:** ~30 minutes

---

## 📁 Project Structure
```
TRRCMS/
├── src/
│   ├── TRRCMS.Domain/              # Enterprise business rules
│   │   ├── Entities/               # Domain entities
│   │   │   ├── Building.cs         # ✅ Implemented
│   │   │   ├── PropertyUnit.cs     # ✅ Implemented
│   │   │   ├── Person.cs           # ✅ Implemented
│   │   │   ├── Household.cs        # ✅ Implemented
│   │   │   ├── PersonPropertyRelation.cs  # ✅ Implemented (NEW)
│   │   │   ├── Claim.cs            # 📅 Planned
│   │   │   ├── Evidence.cs         # 📅 Planned
│   │   │   └── Certificate.cs      # 📅 Planned
│   │   ├── Enums/                  # Domain enumerations (28 enums)
│   │   └── Common/                 # Base classes (BaseEntity, BaseAuditableEntity)
│   │
│   ├── TRRCMS.Application/         # Application business rules
│   │   ├── Buildings/              # ✅ Building use cases
│   │   │   ├── Commands/           # Create, Update, Delete
│   │   │   ├── Queries/            # GetAll, GetById
│   │   │   └── Dtos/               # BuildingDto
│   │   ├── PropertyUnits/          # ✅ PropertyUnit use cases
│   │   ├── Persons/                # ✅ Person use cases
│   │   │   ├── Commands/
│   │   │   │   └── CreatePerson/   # CreatePersonCommand & Handler
│   │   │   ├── Queries/
│   │   │   │   ├── GetAllPersons/  # GetAllPersonsQuery & Handler
│   │   │   │   └── GetPerson/      # GetPersonQuery & Handler
│   │   │   └── Dtos/
│   │   │       └── PersonDto.cs    # DTO with computed properties
│   │   ├── Households/             # ✅ Household use cases
│   │   │   ├── Commands/
│   │   │   │   └── CreateHousehold/  # CreateHouseholdCommand & Handler
│   │   │   ├── Queries/
│   │   │   │   ├── GetAllHouseholds/  # GetAllHouseholdsQuery & Handler
│   │   │   │   └── GetHousehold/      # GetHouseholdQuery & Handler
│   │   │   └── Dtos/
│   │   │       └── HouseholdDto.cs    # DTO with vulnerability metrics
│   │   ├── PersonPropertyRelations/  # ✅ PersonPropertyRelation use cases (NEW)
│   │   │   ├── Commands/
│   │   │   │   └── CreatePersonPropertyRelation/  # CreatePersonPropertyRelationCommand & Handler
│   │   │   ├── Queries/
│   │   │   │   ├── GetAllPersonPropertyRelations/  # GetAllPersonPropertyRelationsQuery & Handler
│   │   │   │   └── GetPersonPropertyRelation/      # GetPersonPropertyRelationQuery & Handler
│   │   │   └── Dtos/
│   │   │       └── PersonPropertyRelationDto.cs    # DTO with computed properties
│   │   └── Common/
│   │       ├── Interfaces/         # Repository interfaces
│   │       │   ├── IBuildingRepository.cs
│   │       │   ├── IPropertyUnitRepository.cs
│   │       │   ├── IPersonRepository.cs
│   │       │   ├── IHouseholdRepository.cs
│   │       │   └── IPersonPropertyRelationRepository.cs  # NEW
│   │       └── Mappings/
│   │           └── MappingProfile.cs  # AutoMapper configuration
│   │
│   ├── TRRCMS.Infrastructure/      # External concerns
│   │   └── Persistence/
│   │       ├── ApplicationDbContext.cs  # DbContext with 5 entities
│   │       ├── Configurations/     # EF Core entity configurations
│   │       │   ├── BuildingConfiguration.cs
│   │       │   ├── PropertyUnitConfiguration.cs
│   │       │   ├── PersonConfiguration.cs
│   │       │   ├── HouseholdConfiguration.cs
│   │       │   ├── PersonPropertyRelationConfiguration.cs  # NEW - Comprehensive
│   │       │   └── ClaimConfiguration.cs
│   │       ├── Repositories/       # Repository implementations
│   │       │   ├── BuildingRepository.cs
│   │       │   ├── PropertyUnitRepository.cs
│   │       │   ├── PersonRepository.cs
│   │       │   ├── HouseholdRepository.cs
│   │       │   └── PersonPropertyRelationRepository.cs  # NEW
│   │       └── Migrations/         # Database migrations (5 tables)
│   │
│   └── TRRCMS.WebAPI/              # API layer
│       ├── Controllers/
│       │   ├── BuildingsController.cs
│       │   ├── PropertyUnitsController.cs
│       │   ├── PersonsController.cs
│       │   ├── HouseholdsController.cs
│       │   └── PersonPropertyRelationsController.cs  # NEW
│       ├── Program.cs              # DI configuration
│       └── appsettings.json        # Configuration template
│
├── docs/                           # Documentation
│   ├── TRRCMS_Analysis_NextSteps.md
│   └── TRRCMS_HowToExtend.md
│
├── .gitignore                      # Excludes appsettings.Development.json
├── SETUP_GUIDE.md
└── README.md
```

---

## 📚 API Documentation

### Endpoints (v0.4)

#### 🏢 Buildings
- `POST /api/v1/buildings` - Create new building
- `GET /api/v1/buildings` - Get all buildings
- `GET /api/v1/buildings/{id}` - Get building by ID

#### 🏠 Property Units
- `POST /api/v1/propertyunits` - Create new property unit
- `GET /api/v1/propertyunits` - Get all property units
- `GET /api/v1/propertyunits/{id}` - Get property unit by ID

#### 👤 Persons
- `POST /api/v1/persons` - Create new person
- `GET /api/v1/persons` - Get all persons
- `GET /api/v1/persons/{id}` - Get person by ID

#### 👨‍👩‍👧‍👦 Households
- `POST /api/v1/households` - Create new household
- `GET /api/v1/households` - Get all households
- `GET /api/v1/households/{id}` - Get household by ID

#### 🔗 Person-Property Relations ⭐ **NEW**
- `POST /api/v1/personpropertyrelations` - Create new person-property relation
- `GET /api/v1/personpropertyrelations` - Get all person-property relations
- `GET /api/v1/personpropertyrelations/{id}` - Get person-property relation by ID

**Total Endpoints:** 15

---

### Example Requests

#### Create Building
```json
POST /api/v1/buildings
{
  "governorateCode": "01",
  "districtCode": "02",
  "subDistrictCode": "03",
  "communityCode": "001",
  "neighborhoodCode": "002",
  "buildingNumber": "00001",
  "governorateName": "حلب",
  "districtName": "منطقة الفرقان",
  "subDistrictName": "ناحية السليمانية",
  "communityName": "تجمع الشهباء",
  "neighborhoodName": "حي الصاخور",
  "buildingType": 0,
  "latitude": 36.2021,
  "longitude": 37.1343,
  "createdByUserId": "00000000-0000-0000-0000-000000000001"
}
```

#### Create Person
```json
POST /api/v1/persons
{
  "firstNameArabic": "أحمد",
  "fatherNameArabic": "محمد",
  "familyNameArabic": "الحسن",
  "motherNameArabic": "فاطمة",
  "fullNameEnglish": "Ahmad Mohammed Al-Hassan",
  "nationalId": "123456789",
  "yearOfBirth": 1985,
  "gender": "M",
  "nationality": "Syrian",
  "primaryPhoneNumber": "+963991234567",
  "isContactPerson": true,
  "hasIdentificationDocument": true,
  "createdByUserId": "00000000-0000-0000-0000-000000000001"
}
```

#### Create Household
```json
POST /api/v1/households
{
  "propertyUnitId": "8aa1ec0c-84f3-48a2-8e3e-bd3ef372c320",
  "headOfHouseholdName": "أحمد محمد الحسن",
  "householdSize": 5,
  "createdByUserId": "00000000-0000-0000-0000-000000000001",
  "maleCount": 2,
  "femaleCount": 3,
  "infantCount": 0,
  "childCount": 1,
  "minorCount": 1,
  "adultCount": 2,
  "elderlyCount": 1,
  "personsWithDisabilitiesCount": 0,
  "isFemaleHeaded": false,
  "employedPersonsCount": 1,
  "unemployedPersonsCount": 0,
  "isDisplaced": true,
  "originLocation": "حلب القديمة",
  "arrivalDate": "2023-01-15",
  "displacementReason": "Conflict",
  "notes": "Family returned to rehabilitated property",
  "specialNeeds": "Elderly member requires ground floor access"
}
```

#### Create Person-Property Relation ⭐ **NEW**
```json
POST /api/v1/personpropertyrelations
{
  "personId": "d2c8e6e7-ce38-42a8-8587-671bd6e24cde",
  "propertyUnitId": "8aa21e0c-84f1-48a2-8e3e-bd3af172c820",
  "relationType": "owner",
  "ownershipShare": 1.0,
  "startDate": "2020-01-01T00:00:00",
  "notes": "Original property owner",
  "createdByUserId": "00000000-0000-0000-0000-000000000001"
}
```

**Response (201 Created):**
```json
{
  "id": "d5532dce-4cd9-453a-af1b-b5ebcb4a968c",
  "personId": "d2c8e6e7-ce38-42a8-8587-671bd6e24cde",
  "propertyUnitId": "8aa21e0c-84f1-48a2-8e3e-bd3af172c820",
  "relationType": "owner",
  "relationTypeOtherDesc": null,
  "ownershipShare": 1.0,
  "contractDetails": null,
  "startDate": "2020-01-01T00:00:00Z",
  "endDate": null,
  "notes": "Original property owner",
  "isActive": true,
  "createdAtUtc": "2026-01-07T19:13:17.471892Z",
  "createdBy": "00000000-0000-0000-0000-000000000001",
  "lastModifiedAtUtc": "2026-01-07T19:13:17.189227Z",
  "lastModifiedBy": "00000000-0000-0000-0000-000000000001",
  "isDeleted": false,
  "deletedAtUtc": null,
  "deletedBy": null,
  "durationInDays": null,
  "isOngoing": true
}
```

**Key Features:**
- ✅ **Relation types** - owner, tenant, occupant, guest, heir, other
- ✅ **Ownership share tracking** - Percentage or fractional ownership (e.g., 0.5 = 50%)
- ✅ **Contract details** - Store agreement information
- ✅ **Date tracking** - Start and end dates for relations
- ✅ **Active status** - Mark relations as active/inactive
- ✅ **Computed properties** - `durationInDays` (calculated from dates), `isOngoing` (no end date)
- ✅ **Other relation type** - Custom description when type is "other"
- ✅ **Audit trail** - Complete tracking of creation and modifications
- ✅ **UTC timestamps** - Proper timezone handling for PostgreSQL

**Use Cases:**
- Document original property owners
- Track tenant relationships with lease dates
- Record occupancy arrangements
- Maintain heir information
- Support co-ownership scenarios (multiple owners with shares)

---

### Interactive Documentation
Start the application and navigate to: **https://localhost:7204/swagger**

---

## 📊 Development Status

### Database Schema
| Entity | Status | Table | Records |
|--------|--------|-------|---------|
| Building | ✅ Complete | `Buildings` | Production ready |
| PropertyUnit | ✅ Complete | `PropertyUnits` | Production ready |
| Person | ✅ Complete | `Persons` | Production ready |
| Household | ✅ Complete | `Households` | Production ready |
| PersonPropertyRelation | ✅ Complete | `PersonPropertyRelations` | **NEW - Production ready** |
| Claim | 📅 Planned | - | Not started |
| Evidence | 📅 Planned | - | Not started |
| Certificate | 📅 Planned | - | Not started |

### Implementation Progress: 5/19 Entities (26%)

### Entity Completion Checklist
Each entity follows this pattern:

**PersonPropertyRelation Entity** ✅ (Latest - Jan 7, 2026)
- [x] Domain entity with factory methods
- [x] EF Core configuration with comprehensive constraints
- [x] Repository interface & implementation
- [x] DTOs with AutoMapper mapping
- [x] CQRS Commands (Create)
- [x] CQRS Queries (GetAll, GetById)
- [x] API Controller with 3 endpoints
- [x] Database migration applied
- [x] Tested in Swagger
- [x] Audit trail working
- [x] Soft delete support
- [x] Computed properties (DurationInDays, IsOngoing)
- [x] Column comments in database
- [x] Default values (IsActive: true, IsDeleted: false)
- [x] Composite indexes for performance
- [x] UTC timestamp handling for PostgreSQL

**Household Entity** ✅ (Complete - Jan 6, 2026)
- [x] All checklist items completed
- [x] Comprehensive demographics tracking
- [x] Vulnerability indicators
- [x] Displacement tracking

**Person Entity** ✅ (Complete - Jan 6, 2026)
- [x] All checklist items completed
- [x] Arabic name support with computed FullNameArabic
- [x] Age calculation from YearOfBirth

**Building Entity** ✅ (Complete)
- [x] All checklist items completed

**PropertyUnit Entity** ✅ (Complete)
- [x] All checklist items completed

---

## 🔄 Git Workflow

### Branch Strategy
```bash
# Feature development
git checkout -b feature/entity-name
git commit -m "feat: Add EntityName CRUD endpoints"
git push origin feature/entity-name

# Bug fixes
git checkout -b fix/bug-description
git commit -m "fix: Resolve issue with X"
git push origin fix/bug-description

# Documentation
git checkout -b docs/what-changed
git commit -m "docs: Update README with entity changes"
git push origin docs/what-changed
```

### Commit Message Convention
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `refactor:` - Code refactoring
- `test:` - Adding tests
- `chore:` - Maintenance tasks

### Recent Commits
```
feat: Implement PersonPropertyRelation entity with full CRUD operations (Jan 7, 2026)
  - Add relation type tracking (owner, tenant, occupant, guest, heir, other)
  - Implement ownership share tracking for co-owners
  - Add contract details and date tracking (start/end dates)
  - Add computed properties (DurationInDays, IsOngoing)
  - Add comprehensive EF Core configuration with column comments
  - Add composite indexes for query performance
  - Fix UTC timestamp handling for PostgreSQL
  - Test all endpoints successfully in Swagger
  Closes TRRCMS-MOB-05

feat: Implement Household entity with full CRUD operations (Jan 6, 2026)
  - Add comprehensive demographics and vulnerability tracking
  - Implement computed properties (DependencyRatio, IsVulnerable)
  - Add displacement tracking (origin, arrival date, reason)
  - Add economic indicators (employment, income)
  Closes TRRCMS-MOB-04

feat: Implement Person entity with full CRUD operations (Jan 6, 2026)
  - Add Arabic name support with computed FullNameArabic
  - Implement age calculation from YearOfBirth
  - Add household relationship support
  Closes TRRCMS-MOB-03

feat: Add PropertyUnit CRUD endpoints (Dec 2025)
feat: Add Building CRUD endpoints (Dec 2025)
chore: Initial project setup with Clean Architecture (Dec 2025)
```

---

## 👥 Team

**Project:** UN-Habitat Aleppo Tenure Rights Registration  
**Organization:** United Nations Human Settlements Programme  
**Location:** Aleppo, Syria

### Contributors
- **Ameer Yousef** - Backend Developer

---

## 📄 License

This project is developed for UN-Habitat. All rights reserved.

---

## 🤝 Contributing

1. Follow the [Setup Guide](./SETUP_GUIDE.md)
2. Pick an entity from the development status table
3. Create a feature branch (`feature/entity-name`)
4. Implement following the established pattern (see PersonPropertyRelation entity as reference)
5. Test thoroughly in Swagger
6. Commit with conventional commit messages
7. Push and create Pull Request

### Code Quality Standards
- ✅ Follow Clean Architecture principles
- ✅ Use CQRS pattern for all operations
- ✅ Implement Repository pattern
- ✅ Add comprehensive XML documentation
- ✅ Include audit fields (Created/Modified/Deleted)
- ✅ Support soft delete
- ✅ Add computed properties where applicable
- ✅ Test all endpoints in Swagger
- ✅ Follow existing naming conventions
- ✅ Add column comments in EF Core configuration
- ✅ Use appropriate indexes for performance
- ✅ Handle UTC timestamps correctly for PostgreSQL

---

## 📞 Support

- **Issues:** Use GitHub Issues for bug reports
- **Questions:** Ask in team chat
- **Documentation:** Check the `/docs` folder
- **API Docs:** https://localhost:7204/swagger (when running)

---

## 🎯 Next Steps

### Immediate Priorities
1. ✅ ~~Person Entity~~ - **COMPLETED Jan 6, 2026**
2. ✅ ~~Household Entity~~ - **COMPLETED Jan 6, 2026**
3. ✅ ~~PersonPropertyRelation Entity~~ - **COMPLETED Jan 7, 2026**
4. 📅 Evidence & Document entities - Next priority
5. 📅 Claims workflow implementation

### Milestone Progress
- **M2: Core Platform Ready** - 90% complete
  - ✅ Building management
  - ✅ Property unit management
  - ✅ Person registry
  - ✅ Household tracking
  - ✅ Person-property relations
  - 📅 Evidence & Documents (next)

---

**Last Updated:** January 7, 2026  
**Version:** 0.4.0  
**Status:** 🟢 Active Development  
**Latest Feature:** Person-Property Relations with Ownership & Tenancy Tracking