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
- **Person-Property Relations** - Ownership, tenancy, and occupancy documentation
- **Evidence Management** - Document/photo/file metadata tracking with versioning ⭐ **NEW**
- **Ownership Claims** - Documentation and verification of property claims
- **Field Surveys** - Mobile data collection for on-site verification
- **Document Management** - Secure storage of legal documents and evidence
- **Conflict Resolution** - Tracking disputed claims and resolution processes

---

## ✨ Features

### ✅ Implemented (v0.6 - Current)
✅ **Clean Architecture** - Domain-driven design with clear separation of concerns  
✅ **Building CRUD** - Complete Create, Read, Update, Delete operations  
✅ **Property Unit CRUD** - Apartment/shop/commercial unit management  
✅ **Person Registry** - Individual registration with Arabic name support  
✅ **Household Management** - Family unit tracking with demographics and vulnerability indicators  
✅ **Person-Property Relations** - Ownership/tenancy linkage with evidence support  
✅ **Evidence Management** - File metadata tracking with versioning and entity linking ⭐ **NEW**  
✅ **PostgreSQL Database** - Robust relational data storage with 6 entity tables  
✅ **Entity Framework Core** - Code-first migrations and LINQ queries  
✅ **CQRS Pattern** - Command/Query separation with MediatR  
✅ **Repository Pattern** - Consistent data access layer  
✅ **Swagger/OpenAPI** - Interactive API documentation  
✅ **Arabic Support** - Full UTF-8 encoding for Arabic text (names, addresses)  
✅ **Audit Trails** - Comprehensive tracking (Created/Modified/Deleted timestamps & users)  
✅ **Soft Delete** - Data preservation with IsDeleted flag  
✅ **Computed Properties** - Dynamic calculations (DependencyRatio, IsVulnerable, DurationInDays, IsOngoing, IsExpired)  

### 📅 Planned
📅 **Authentication & Authorization** - JWT-based security with role-based access  
📅 **Claims Workflow** - Submission, review, verification, and resolution  
📅 **Document Upload** - PDF/image attachment system with versioning  
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
│   │   │   ├── PersonPropertyRelation.cs  # ✅ Implemented
│   │   │   ├── Evidence.cs         # ✅ Implemented (NEW)
│   │   │   ├── Document.cs         # 📅 Planned
│   │   │   ├── Claim.cs            # 📅 Planned
│   │   │   └── Certificate.cs      # 📅 Planned
│   │   ├── Enums/                  # Domain enumerations (28 enums)
│   │   └── Common/                 # Base classes (BaseEntity, BaseAuditableEntity)
│   │
│   ├── TRRCMS.Application/         # Application business rules
│   │   ├── Buildings/              # ✅ Building use cases
│   │   ├── PropertyUnits/          # ✅ PropertyUnit use cases
│   │   ├── Persons/                # ✅ Person use cases
│   │   ├── Households/             # ✅ Household use cases
│   │   ├── PersonPropertyRelations/  # ✅ PersonPropertyRelation use cases
│   │   ├── Evidences/              # ✅ Evidence use cases (NEW)
│   │   │   ├── Commands/
│   │   │   │   └── CreateEvidence/   # CreateEvidenceCommand & Handler
│   │   │   ├── Queries/
│   │   │   │   ├── GetAllEvidences/  # GetAllEvidencesQuery & Handler
│   │   │   │   └── GetEvidence/      # GetEvidenceQuery & Handler
│   │   │   └── Dtos/
│   │   │       └── EvidenceDto.cs    # DTO with IsExpired computed property
│   │   └── Common/
│   │       ├── Interfaces/         # Repository interfaces
│   │       │   ├── IBuildingRepository.cs
│   │       │   ├── IPropertyUnitRepository.cs
│   │       │   ├── IPersonRepository.cs
│   │       │   ├── IHouseholdRepository.cs
│   │       │   ├── IPersonPropertyRelationRepository.cs
│   │       │   └── IEvidenceRepository.cs  # NEW
│   │       └── Mappings/
│   │           └── MappingProfile.cs  # AutoMapper configuration
│   │
│   ├── TRRCMS.Infrastructure/      # External concerns
│   │   └── Persistence/
│   │       ├── ApplicationDbContext.cs  # DbContext with 6 entities
│   │       ├── Configurations/     # EF Core entity configurations
│   │       │   ├── BuildingConfiguration.cs
│   │       │   ├── PropertyUnitConfiguration.cs
│   │       │   ├── PersonConfiguration.cs
│   │       │   ├── HouseholdConfiguration.cs
│   │       │   ├── PersonPropertyRelationConfiguration.cs
│   │       │   └── EvidenceConfiguration.cs  # NEW - Comprehensive
│   │       ├── Repositories/       # Repository implementations
│   │       │   ├── BuildingRepository.cs
│   │       │   ├── PropertyUnitRepository.cs
│   │       │   ├── PersonRepository.cs
│   │       │   ├── HouseholdRepository.cs
│   │       │   ├── PersonPropertyRelationRepository.cs
│   │       │   └── EvidenceRepository.cs  # NEW
│   │       └── Migrations/         # Database migrations (6 tables)
│   │
│   └── TRRCMS.WebAPI/              # API layer
│       ├── Controllers/
│       │   ├── BuildingsController.cs
│       │   ├── PropertyUnitsController.cs
│       │   ├── PersonsController.cs
│       │   ├── HouseholdsController.cs
│       │   ├── PersonPropertyRelationsController.cs
│       │   └── EvidencesController.cs  # NEW
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

### Endpoints (v0.6)

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

#### 🔗 Person-Property Relations
- `POST /api/v1/personpropertyrelations` - Create new person-property relation
- `GET /api/v1/personpropertyrelations` - Get all person-property relations
- `GET /api/v1/personpropertyrelations/{id}` - Get person-property relation by ID

#### 📄 Evidences ⭐ **NEW**
- `POST /api/v1/evidences` - Create new evidence (file metadata)
- `GET /api/v1/evidences` - Get all evidences
- `GET /api/v1/evidences/{id}` - Get evidence by ID

**Total Endpoints:** 18

---

### Example Requests

#### Create Evidence ⭐ **NEW**
```json
POST /api/v1/evidences
{
  "evidenceType": "Property Deed",
  "description": "Original Tabu Green deed for property unit",
  "originalFileName": "tabu_deed_2024.pdf",
  "filePath": "/uploads/evidences/2024/01/tabu_deed_2024.pdf",
  "fileSizeBytes": 2048576,
  "mimeType": "application/pdf",
  "fileHash": "abc123def456789",
  "documentIssuedDate": "2015-03-15T00:00:00",
  "issuingAuthority": "Aleppo Real Estate Registry",
  "documentReferenceNumber": "TD-2015-12345",
  "notes": "Original deed in good condition",
  "createdByUserId": "00000000-0000-0000-0000-000000000001"
}
```

**Response (201 Created):**
```json
{
  "id": "f4fd3c07-3eaa-44ca-8458-2a56db31b069",
  "evidenceType": "Property Deed",
  "description": "Original Tabu Green deed for property unit",
  "originalFileName": "tabu_deed_2024.pdf",
  "filePath": "/uploads/evidences/2024/01/tabu_deed_2024.pdf",
  "fileSizeBytes": 2048576,
  "mimeType": "application/pdf",
  "fileHash": "abc123def456789",
  "documentIssuedDate": "2015-03-15T00:00:00Z",
  "documentExpiryDate": null,
  "issuingAuthority": "Aleppo Real Estate Registry",
  "documentReferenceNumber": "TD-2015-12345",
  "notes": "Original deed in good condition",
  "versionNumber": 1,
  "previousVersionId": null,
  "isCurrentVersion": true,
  "personId": null,
  "personPropertyRelationId": null,
  "claimId": null,
  "createdAtUtc": "2026-01-08T03:05:55.762125Z",
  "createdBy": "00000000-0000-0000-0000-000000000001",
  "lastModifiedAtUtc": "2026-01-08T03:05:55.762125Z",
  "lastModifiedBy": "00000000-0000-0000-0000-000000000001",
  "isDeleted": false,
  "deletedAtUtc": null,
  "deletedBy": null,
  "isExpired": false
}
```

**Key Features:**
- ✅ **File metadata tracking** - Name, path, size, MIME type, SHA-256 hash
- ✅ **Document metadata** - Issue/expiry dates, issuing authority, reference numbers
- ✅ **Versioning support** - Version number, previous version tracking, current version flag
- ✅ **Entity linking** - Link to Person (ID documents), PersonPropertyRelation (contracts), Claims (supporting evidence)
- ✅ **Computed property** - `isExpired` (checks if document has expired)
- ✅ **Audit trail** - Complete tracking of creation and modifications
- ✅ **Soft delete** - Data preservation

**Use Cases:**
- Upload property deed scans (Tabu documents)
- Store national ID cards and personal identification
- Track rental contracts and agreements
- Maintain photograph evidence of properties
- Version control for updated documents
- Link supporting evidence to claims

---

#### Create Evidence with Person Link ⭐ **NEW**
```json
POST /api/v1/evidences
{
  "evidenceType": "National ID Card",
  "description": "Scanned copy of national ID",
  "originalFileName": "national_id_ahmad.jpg",
  "filePath": "/uploads/evidences/ids/national_id_ahmad.jpg",
  "fileSizeBytes": 512000,
  "mimeType": "image/jpeg",
  "documentIssuedDate": "2018-01-01T00:00:00",
  "documentExpiryDate": "2028-01-01T00:00:00",
  "issuingAuthority": "Ministry of Interior",
  "documentReferenceNumber": "ID-2018-67890",
  "personId": "d2c8e6e7-ce38-42a8-8597-671bd6e24cde",
  "createdByUserId": "00000000-0000-0000-0000-000000000001"
}
```

---

#### Create Evidence with Relation Link ⭐ **NEW**
```json
POST /api/v1/evidences
{
  "evidenceType": "Rental Contract",
  "description": "Signed rental agreement",
  "originalFileName": "rental_contract_2024.pdf",
  "filePath": "/uploads/contracts/rental_contract_2024.pdf",
  "fileSizeBytes": 1024000,
  "mimeType": "application/pdf",
  "documentIssuedDate": "2024-01-01T00:00:00",
  "notes": "2-year rental contract",
  "personPropertyRelationId": "d5532dce-4cd9-453a-af1b-b5ebcb4a968c",
  "createdByUserId": "00000000-0000-0000-0000-000000000001"
}
```

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
| PersonPropertyRelation | ✅ Complete | `PersonPropertyRelations` | Production ready |
| Evidence | ✅ Complete | `Evidences` | **NEW - Production ready** |
| Document | 📅 Planned | - | Not started |
| Claim | 📅 Planned | - | Not started |
| Certificate | 📅 Planned | - | Not started |

### Implementation Progress: 6/19 Entities (32%)

### Entity Completion Checklist
Each entity follows this pattern:

**Evidence Entity** ✅ (Latest - Jan 8, 2026)
- [x] Domain entity with factory methods
- [x] EF Core configuration with comprehensive constraints
- [x] Repository interface & implementation (11 methods)
- [x] DTOs with AutoMapper mapping
- [x] CQRS Commands (Create)
- [x] CQRS Queries (GetAll, GetById)
- [x] API Controller with 3 endpoints
- [x] Database migration applied
- [x] Tested in Swagger
- [x] Audit trail working
- [x] Soft delete support
- [x] Computed property (IsExpired)
- [x] Column comments in database
- [x] Default values (VersionNumber: 1, IsCurrentVersion: true)
- [x] 8 indexes for performance
- [x] UTC timestamp handling for PostgreSQL
- [x] Entity linking (Person, PersonPropertyRelation, Claim)
- [x] Versioning support (PreviousVersionId, version chain tracking)

**PersonPropertyRelation Entity** ✅ (Complete - Jan 7, 2026)
- [x] All checklist items completed
- [x] Ownership & tenancy tracking
- [x] UTC timestamp handling

**Household Entity** ✅ (Complete - Jan 6, 2026)
- [x] All checklist items completed
- [x] Comprehensive demographics tracking
- [x] Vulnerability indicators

**Person Entity** ✅ (Complete - Jan 6, 2026)
- [x] All checklist items completed
- [x] Arabic name support

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
feat: Implement Evidence entity with full CRUD operations (Jan 8, 2026)
  - Add file metadata tracking (filename, path, size, MIME type, hash)
  - Implement document metadata (issued/expiry dates, authority, reference)
  - Add versioning support (version chain, current version flag)
  - Add entity linking (Person, PersonPropertyRelation, Claim)
  - Add computed property (IsExpired)
  - Add comprehensive EF Core configuration with 8 indexes
  - Fix UTC timestamp handling for PostgreSQL
  - Test all endpoints successfully in Swagger
  Closes TRRCMS-MOB-06

feat: Implement PersonPropertyRelation entity with full CRUD operations (Jan 7, 2026)
  - Add relation type tracking (owner, tenant, occupant, guest, heir, other)
  - Implement ownership share tracking for co-owners
  - Add contract details and date tracking (start/end dates)
  - Add computed properties (DurationInDays, IsOngoing)
  Closes TRRCMS-MOB-05

feat: Implement Household entity with full CRUD operations (Jan 6, 2026)
  - Add comprehensive demographics and vulnerability tracking
  - Implement computed properties (DependencyRatio, IsVulnerable)
  Closes TRRCMS-MOB-04

feat: Implement Person entity with full CRUD operations (Jan 6, 2026)
  - Add Arabic name support with computed FullNameArabic
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
4. Implement following the established pattern (see Evidence entity as reference)
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
4. ✅ ~~Evidence Entity~~ - **COMPLETED Jan 8, 2026**
5. 📅 Document Entity - Next priority
6. 📅 Claims workflow implementation

### Milestone Progress
- **M2: Core Platform Ready** - 95% complete
  - ✅ Building management
  - ✅ Property unit management
  - ✅ Person registry
  - ✅ Household tracking
  - ✅ Person-property relations
  - ✅ Evidence management
  - 📅 Document metadata (next)

---

**Last Updated:** January 8, 2026  
**Version:** 0.6.0  
**Status:** 🟢 Active Development  
**Latest Feature:** Evidence Management with File Metadata Tracking & Versioning