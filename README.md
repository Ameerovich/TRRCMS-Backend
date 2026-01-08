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
- **Evidence Management** - File metadata tracking with versioning and entity linking
- **Document Management** - Official document metadata with verification workflow ⭐ **NEW**
- **Ownership Claims** - Documentation and verification of property claims
- **Field Surveys** - Mobile data collection for on-site verification
- **Secure Document Storage** - Legal document tracking and evidence management
- **Conflict Resolution** - Tracking disputed claims and resolution processes

---

## ✨ Features

### ✅ Implemented (v0.7 - Current)
✅ **Clean Architecture** - Domain-driven design with clear separation of concerns  
✅ **Building CRUD** - Complete Create, Read, Update, Delete operations  
✅ **Property Unit CRUD** - Apartment/shop/commercial unit management  
✅ **Person Registry** - Individual registration with Arabic name support  
✅ **Household Management** - Family unit tracking with demographics and vulnerability indicators  
✅ **Person-Property Relations** - Ownership/tenancy linkage with evidence support  
✅ **Evidence Management** - File metadata tracking with versioning and entity linking  
✅ **Document Management** - Official document metadata with verification workflow ⭐ **NEW**  
✅ **PostgreSQL Database** - Robust relational data storage with 7 entity tables  
✅ **Entity Framework Core** - Code-first migrations and LINQ queries  
✅ **CQRS Pattern** - Command/Query separation with MediatR  
✅ **Repository Pattern** - Consistent data access layer  
✅ **Swagger/OpenAPI** - Interactive API documentation  
✅ **Arabic Support** - Full UTF-8 encoding for Arabic text (names, addresses)  
✅ **Audit Trails** - Comprehensive tracking (Created/Modified/Deleted timestamps & users)  
✅ **Soft Delete** - Data preservation with IsDeleted flag  
✅ **Computed Properties** - Dynamic calculations (DependencyRatio, IsVulnerable, DurationInDays, IsOngoing, IsExpired, IsExpiringSoon)  

### 📅 Planned
📅 **Authentication & Authorization** - JWT-based security with role-based access  
📅 **Claims Workflow** - Submission, review, verification, and resolution  
📅 **Document Upload** - PDF/image attachment system with actual file storage  
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
│   │   │   ├── Evidence.cs         # ✅ Implemented
│   │   │   ├── Document.cs         # ✅ Implemented (NEW)
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
│   │   ├── Evidences/              # ✅ Evidence use cases
│   │   │   ├── Commands/
│   │   │   │   └── CreateEvidence/   # CreateEvidenceCommand & Handler
│   │   │   ├── Queries/
│   │   │   │   ├── GetAllEvidences/  # GetAllEvidencesQuery & Handler
│   │   │   │   └── GetEvidence/      # GetEvidenceQuery & Handler
│   │   │   └── Dtos/
│   │   │       └── EvidenceDto.cs    # DTO with IsExpired computed property
│   │   ├── Documents/              # ✅ Document use cases (NEW)
│   │   │   ├── Commands/
│   │   │   │   └── CreateDocument/   # CreateDocumentCommand & Handler
│   │   │   ├── Queries/
│   │   │   │   ├── GetAllDocuments/  # GetAllDocumentsQuery & Handler
│   │   │   │   └── GetDocument/      # GetDocumentQuery & Handler
│   │   │   └── Dtos/
│   │   │       └── DocumentDto.cs    # DTO with IsExpired/IsExpiringSoon computed properties
│   │   └── Common/
│   │       ├── Interfaces/         # Repository interfaces
│   │       │   ├── IBuildingRepository.cs
│   │       │   ├── IPropertyUnitRepository.cs
│   │       │   ├── IPersonRepository.cs
│   │       │   ├── IHouseholdRepository.cs
│   │       │   ├── IPersonPropertyRelationRepository.cs
│   │       │   ├── IEvidenceRepository.cs
│   │       │   └── IDocumentRepository.cs  # NEW
│   │       └── Mappings/
│   │           └── MappingProfile.cs  # AutoMapper configuration
│   │
│   ├── TRRCMS.Infrastructure/      # External concerns
│   │   └── Persistence/
│   │       ├── ApplicationDbContext.cs  # DbContext with 7 entities
│   │       ├── Configurations/     # EF Core entity configurations
│   │       │   ├── BuildingConfiguration.cs
│   │       │   ├── PropertyUnitConfiguration.cs
│   │       │   ├── PersonConfiguration.cs
│   │       │   ├── HouseholdConfiguration.cs
│   │       │   ├── PersonPropertyRelationConfiguration.cs
│   │       │   ├── EvidenceConfiguration.cs
│   │       │   └── DocumentConfiguration.cs  # NEW - Comprehensive
│   │       ├── Repositories/       # Repository implementations
│   │       │   ├── BuildingRepository.cs
│   │       │   ├── PropertyUnitRepository.cs
│   │       │   ├── PersonRepository.cs
│   │       │   ├── HouseholdRepository.cs
│   │       │   ├── PersonPropertyRelationRepository.cs
│   │       │   ├── EvidenceRepository.cs
│   │       │   └── DocumentRepository.cs  # NEW
│   │       └── Migrations/         # Database migrations (7 tables)
│   │
│   └── TRRCMS.WebAPI/              # API layer
│       ├── Controllers/
│       │   ├── BuildingsController.cs
│       │   ├── PropertyUnitsController.cs
│       │   ├── PersonsController.cs
│       │   ├── HouseholdsController.cs
│       │   ├── PersonPropertyRelationsController.cs
│       │   ├── EvidencesController.cs
│       │   └── DocumentsController.cs  # NEW
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

### Endpoints (v0.7)

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

#### 📄 Evidences
- `POST /api/v1/evidences` - Create new evidence (file metadata)
- `GET /api/v1/evidences` - Get all evidences
- `GET /api/v1/evidences/{id}` - Get evidence by ID

#### 📋 Documents ⭐ **NEW**
- `POST /api/documents` - Create new document
- `GET /api/documents` - Get all documents
- `GET /api/documents/{id}` - Get document by ID

**Total Endpoints:** 21

---

### Example Requests

#### Create Document ⭐ **NEW**
```json
POST /api/documents
{
  "documentType": 0,
  "documentNumber": "12345/2024",
  "documentTitle": "سند ملكية أخضر",
  "issueDate": "2024-01-15T00:00:00Z",
  "expiryDate": "2034-01-15T00:00:00Z",
  "issuingAuthority": "مديرية السجل العقاري - حلب",
  "issuingPlace": "حلب",
  "notes": "سند ملكية أصلي",
  "createdByUserId": "00000000-0000-0000-0000-000000000001"
}
```

**Document Types (DocumentType enum):**
- `0` - TabuGreen (Green Tabu - ownership deed)
- `1` - TabuPink (Pink Tabu - shared ownership)
- `2` - RentalContract
- `3` - NationalIdCard
- `4` - FamilyRegistry
- `5` - BirthCertificate
- `6` - DeathCertificate
- `7` - MarriageCertificate
- `8` - DivorceCertificate
- `9` - PowerOfAttorney
- `10` - CourtRuling
- `11` - InheritanceDocument
- `12` - SaleContract
- `13` - Other

**Response (201 Created):**
```json
{
  "id": "ba1fc82b-46a8-4365-a8b7-a2c2b6485d4d",
  "documentType": "TabuGreen",
  "documentNumber": "12345/2024",
  "documentTitle": "سند ملكية أخضر",
  "issueDate": "2024-01-15T00:00:00Z",
  "expiryDate": "2034-01-15T00:00:00Z",
  "issuingAuthority": "مديرية السجل العقاري - حلب",
  "issuingPlace": "حلب",
  "isVerified": false,
  "verificationStatus": "Pending",
  "verificationDate": null,
  "verifiedByUserId": null,
  "verificationNotes": null,
  "evidenceId": null,
  "documentHash": null,
  "notes": "سند ملكية أصلي",
  "personId": null,
  "propertyUnitId": null,
  "personPropertyRelationId": null,
  "claimId": null,
  "isLegallyValid": true,
  "legalValidityNotes": null,
  "isOriginal": true,
  "originalDocumentId": null,
  "isNotarized": false,
  "notaryOffice": null,
  "notarizationDate": null,
  "notarizationNumber": null,
  "createdAtUtc": "2026-01-08T12:29:56.782342Z",
  "createdBy": "00000000-0000-0000-0000-000000000001",
  "lastModifiedAtUtc": "2026-01-08T12:29:56.782342Z",
  "lastModifiedBy": "00000000-0000-0000-0000-000000000001",
  "isDeleted": false,
  "deletedAtUtc": null,
  "deletedBy": null,
  "isExpired": false,
  "isExpiringSoon": false
}
```

**Key Features:**
- ✅ **Document classification** - Type, number, title tracking
- ✅ **Issuance information** - Issue/expiry dates, issuing authority and place
- ✅ **Verification workflow** - Pending/Verified/Rejected status with verification notes
- ✅ **Document content** - Link to Evidence (file), document hash for integrity
- ✅ **Entity relationships** - Link to Person, PropertyUnit, PersonPropertyRelation, Claim
- ✅ **Legal validity** - Legal validity flag with notes
- ✅ **Original/Copy tracking** - IsOriginal flag with reference to original document
- ✅ **Notarization** - Notarization status, office, date, and number
- ✅ **Computed properties** - `isExpired` (checks if expired), `isExpiringSoon` (expires within 30 days)
- ✅ **Audit trail** - Complete tracking of creation and modifications
- ✅ **Soft delete** - Data preservation

**Use Cases:**
- Track property ownership documents (Tabu Green/Pink)
- Manage rental contracts with expiry tracking
- Store national ID and personal documents metadata
- Link supporting documents to claims
- Verify document authenticity and legal validity
- Track document expiry and alert for renewal
- Maintain notarization records
- Support document workflow (pending → verified → rejected)

---

#### Create Document with Entity Links ⭐ **NEW**
```json
POST /api/documents
{
  "documentType": 0,
  "documentNumber": "TD-2024-00123",
  "documentTitle": "سند ملكية شقة رقم 5",
  "issueDate": "2015-03-15T00:00:00Z",
  "expiryDate": null,
  "issuingAuthority": "مديرية السجل العقاري - حلب",
  "issuingPlace": "حلب",
  "personId": "d2c8e6e7-ce38-42a8-8597-671bd6e24cde",
  "propertyUnitId": "a5b3c7d9-1234-5678-90ab-cdef12345678",
  "evidenceId": "f4fd3c07-3eaa-44ca-8458-2a56db31b069",
  "isNotarized": true,
  "notaryOffice": "كاتب العدل الأول - حلب",
  "notarizationDate": "2015-03-20T00:00:00Z",
  "notarizationNumber": "NOT-2015-456",
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
| Evidence | ✅ Complete | `Evidences` | Production ready |
| Document | ✅ Complete | `Documents` | **NEW - Production ready** |
| Claim | 📅 Planned | - | Not started |
| Certificate | 📅 Planned | - | Not started |

### Implementation Progress: 7/19 Entities (37%)

### Entity Completion Checklist
Each entity follows this pattern:

**Document Entity** ✅ (Latest - Jan 8, 2026)
- [x] Domain entity with factory methods & 10+ domain methods
- [x] EF Core configuration with comprehensive constraints
- [x] Repository interface & implementation (16 methods)
- [x] DTOs with AutoMapper mapping
- [x] CQRS Commands (Create)
- [x] CQRS Queries (GetAll, GetById)
- [x] API Controller with 3 endpoints
- [x] Database migration applied
- [x] Tested in Swagger
- [x] Audit trail working
- [x] Soft delete support
- [x] Computed properties (IsExpired, IsExpiringSoon)
- [x] Column comments in database
- [x] Default values (IsVerified: false, VerificationStatus: Pending, IsLegallyValid: true, IsOriginal: true, IsNotarized: false)
- [x] 12 indexes for performance
- [x] UTC timestamp handling for PostgreSQL
- [x] Entity linking (Person, PropertyUnit, PersonPropertyRelation, Evidence, Claim, self-reference)
- [x] Verification workflow (Pending/Verified/Rejected)
- [x] Notarization tracking
- [x] Legal validity assessment
- [x] Document expiry tracking

**Evidence Entity** ✅ (Complete - Jan 8, 2026)
- [x] All checklist items completed
- [x] File metadata tracking
- [x] Versioning support

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
feat: Implement Document entity with full CRUD operations (Jan 8, 2026)
  - Add official document metadata tracking (type, number, title)
  - Implement issuance information (date, authority, place)
  - Add verification workflow (pending/verified/rejected status)
  - Implement notarization tracking (office, date, number)
  - Add legal validity assessment
  - Add document expiry tracking with computed properties
  - Add entity linking (Person, PropertyUnit, PersonPropertyRelation, Evidence, Claim)
  - Add self-referencing for document copies (original/copy tracking)
  - Add computed properties (IsExpired, IsExpiringSoon)
  - Add comprehensive EF Core configuration with 12 indexes
  - Add 16 repository methods including filtered queries
  - Implement 10+ domain methods for document lifecycle
  - Test all endpoints successfully in Swagger
  Closes TRRCMS-MOB-07

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
4. Implement following the established pattern (see Document or Evidence entity as reference)
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
5. ✅ ~~Document Entity~~ - **COMPLETED Jan 8, 2026**
6. 📅 Claim Entity - Next priority
7. 📅 Claims workflow implementation

### Milestone Progress
- **M2: Core Platform Ready** - 100% complete ✅
  - ✅ Building management
  - ✅ Property unit management
  - ✅ Person registry
  - ✅ Household tracking
  - ✅ Person-property relations
  - ✅ Evidence management
  - ✅ Document metadata
- **M3: Claims System** - 0% complete 📅 (Next)

---

**Last Updated:** January 8, 2026  
**Version:** 0.7.0  
**Status:** 🟢 Active Development  
**Latest Feature:** Document Management with Verification Workflow & Expiry Tracking