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
- [Database Setup](#database-setup)
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
- **Document Management** - Official document metadata with verification workflow
- **Claims Management** - Full lifecycle claim processing with workflow automation ⭐ **NEW**
- **Referral System** - Claim routing and reassignment between roles ⭐ **NEW**
- **Field Surveys** - Mobile data collection for on-site verification
- **Secure Document Storage** - Legal document tracking and evidence management
- **Conflict Resolution** - Tracking disputed claims and resolution processes

---

## ✨ Features

### ✅ Implemented (v0.8 - Current)
✅ **Clean Architecture** - Domain-driven design with clear separation of concerns  
✅ **Building CRUD** - Complete Create, Read, Update, Delete operations  
✅ **Property Unit CRUD** - Apartment/shop/commercial unit management  
✅ **Person Registry** - Individual registration with Arabic name support  
✅ **Household Management** - Family unit tracking with demographics and vulnerability indicators  
✅ **Person-Property Relations** - Ownership/tenancy linkage with evidence support  
✅ **Evidence Management** - File metadata tracking with versioning and entity linking  
✅ **Document Management** - Official document metadata with verification workflow  
✅ **Claims Management** - Full lifecycle claim processing with 47 fields ⭐ **NEW**  
✅ **Referral System** - Claim routing between case officers and roles ⭐ **NEW**  
✅ **PostgreSQL Database** - Robust relational data storage with 10 entity tables  
✅ **Entity Framework Core** - Code-first migrations with plural table naming convention  
✅ **CQRS Pattern** - Command/Query separation with MediatR  
✅ **Repository Pattern** - Consistent data access layer  
✅ **Swagger/OpenAPI** - Interactive API documentation  
✅ **Arabic Support** - Full UTF-8 encoding for Arabic text (names, addresses)  
✅ **Audit Trails** - Comprehensive tracking (Created/Modified/Deleted timestamps & users)  
✅ **Soft Delete** - Data preservation with IsDeleted flag  
✅ **Computed Properties** - Dynamic calculations (HasConflicts, IsOverdue, AwaitingDocuments, DaysActive, DaysInCurrentStage, etc.)  
✅ **Workflow Automation** - State transitions (Draft→Submitted→UnderReview→Verified→Approved/Rejected)  

### 📅 Planned
📅 **Authentication & Authorization** - JWT-based security with role-based access  
📅 **Advanced Claims Workflow** - Automated escalation and conflict resolution  
📅 **Document Upload** - PDF/image attachment system with actual file storage  
📅 **Advanced Search & Filtering** - Full-text search and complex queries  
📅 **Reporting & Analytics** - Statistical dashboards and data export  
📅 **Certificate Generation** - Automated tenure rights certificate creation  
📅 **Mobile App Integration** - Field data collection interface  

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
- **State Pattern** - Claim workflow state management

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

## 🗄️ Database Setup

### First-Time Setup

1. **Create PostgreSQL database:**
   ```sql
   CREATE DATABASE "TRRCMS_Dev" 
   OWNER postgres 
   ENCODING 'UTF8';
   ```

2. **Update connection string** in `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TRRCMS_Dev;Username=postgres;Password=YOUR_PASSWORD"
     }
   }
   ```

3. **Apply migrations:**
   ```bash
   # In Package Manager Console (Visual Studio)
   Update-Database -Project TRRCMS.Infrastructure -StartupProject TRRCMS.WebAPI
   
   # Or using .NET CLI
   dotnet ef database update --project src/TRRCMS.Infrastructure --startup-project src/TRRCMS.WebAPI
   ```

4. **Verify setup:** Database will be created with all tables using plural names and correct defaults.

### Database Naming Conventions

**IMPORTANT:** All tables follow a consistent naming convention to prevent conflicts:

| Component | Convention | Examples |
|-----------|-----------|----------|
| **Entity Classes** | Singular | `Claim`, `Person`, `Document`, `Evidence`, `Referral` |
| **Table Names** | **Plural** | `Claims`, `Persons`, `Documents`, `Evidences`, `Referrals` |
| **Navigation Collections** | Plural | `public ICollection<Document> Documents` |

**Why this matters:**
- All entity configurations explicitly specify `.ToTable("PluralName")` to ensure consistency
- This prevents naming conflicts and migration issues between developers
- Team members cloning fresh will get correct structure automatically
- No manual database fixes required

**Implementation:**
```csharp
// Example: ClaimConfiguration.cs
public void Configure(EntityTypeBuilder<Claim> builder)
{
    builder.ToTable("Claims"); // Explicit plural table name
    // ... rest of configuration
}
```

### Current Database Tables (v0.8)

1. ✅ `Buildings` - Property building registry
2. ✅ `PropertyUnits` - Individual units within buildings
3. ✅ `Persons` - Individual person records
4. ✅ `Households` - Family/household units
5. ✅ `PersonPropertyRelations` - Person-property linkages
6. ✅ `Evidences` - File metadata and versioning
7. ✅ `Documents` - Official document metadata
8. ✅ `Claims` - Ownership/tenure claims with full workflow ⭐ **NEW**
9. ✅ `Referrals` - Claim routing/reassignment ⭐ **NEW**
10. 📅 `Certificates` - Generated tenure certificates (planned)

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
│   │   │   ├── Document.cs         # ✅ Implemented
│   │   │   ├── Claim.cs            # ✅ Implemented (NEW - 47 fields, 12+ methods)
│   │   │   ├── Referral.cs         # ✅ Implemented (NEW - domain layer)
│   │   │   └── Certificate.cs      # 📅 Planned
│   │   ├── Enums/                  # Domain enumerations (35+ enums)
│   │   │   ├── ClaimType.cs
│   │   │   ├── ClaimSource.cs
│   │   │   ├── ClaimStatus.cs
│   │   │   ├── LifecycleStage.cs
│   │   │   ├── VerificationStatus.cs
│   │   │   ├── CasePriority.cs
│   │   │   └── ... (29 more enums)
│   │   └── Common/                 # Base classes
│   │       ├── BaseEntity.cs
│   │       └── BaseAuditableEntity.cs
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
│   │   ├── Documents/              # ✅ Document use cases
│   │   │   ├── Commands/
│   │   │   │   └── CreateDocument/   # CreateDocumentCommand & Handler
│   │   │   ├── Queries/
│   │   │   │   ├── GetAllDocuments/  # GetAllDocumentsQuery & Handler
│   │   │   │   └── GetDocument/      # GetDocumentQuery & Handler
│   │   │   └── Dtos/
│   │   │       └── DocumentDto.cs    # DTO with computed properties
│   │   ├── Claims/                 # ✅ Claim use cases (NEW)
│   │   │   ├── Commands/
│   │   │   │   ├── CreateClaim/        # CreateClaimCommand & Handler
│   │   │   │   ├── SubmitClaim/        # SubmitClaimCommand & Handler
│   │   │   │   ├── AssignClaim/        # AssignClaimCommand & Handler
│   │   │   │   ├── VerifyClaim/        # VerifyClaimCommand & Handler
│   │   │   │   ├── ApproveClaim/       # ApproveClaimCommand & Handler
│   │   │   │   └── RejectClaim/        # RejectClaimCommand & Handler
│   │   │   ├── Queries/
│   │   │   │   ├── GetAllClaims/       # GetAllClaimsQuery & Handler (10 filters)
│   │   │   │   └── GetClaim/           # GetClaimQuery & Handler
│   │   │   └── Dtos/
│   │   │       └── ClaimDto.cs         # DTO with 5 computed properties
│   │   └── Common/
│   │       ├── Interfaces/         # Repository interfaces
│   │       │   ├── IBuildingRepository.cs
│   │       │   ├── IPropertyUnitRepository.cs
│   │       │   ├── IPersonRepository.cs
│   │       │   ├── IHouseholdRepository.cs
│   │       │   ├── IPersonPropertyRelationRepository.cs
│   │       │   ├── IEvidenceRepository.cs
│   │       │   ├── IDocumentRepository.cs
│   │       │   └── IClaimRepository.cs  # NEW - 30 methods
│   │       └── Mappings/
│   │           └── MappingProfile.cs  # AutoMapper configuration
│   │
│   ├── TRRCMS.Infrastructure/      # External concerns
│   │   └── Persistence/
│   │       ├── ApplicationDbContext.cs  # DbContext with 9 entities
│   │       ├── Configurations/     # EF Core entity configurations
│   │       │   ├── BuildingConfiguration.cs
│   │       │   ├── PropertyUnitConfiguration.cs
│   │       │   ├── PersonConfiguration.cs
│   │       │   ├── HouseholdConfiguration.cs
│   │       │   ├── PersonPropertyRelationConfiguration.cs
│   │       │   ├── EvidenceConfiguration.cs
│   │       │   ├── DocumentConfiguration.cs
│   │       │   ├── ClaimConfiguration.cs         # NEW - 47 fields, 26 indexes
│   │       │   └── ReferralConfiguration.cs      # NEW - Explicit plural naming
│   │       ├── Repositories/       # Repository implementations
│   │       │   ├── BuildingRepository.cs
│   │       │   ├── PropertyUnitRepository.cs
│   │       │   ├── PersonRepository.cs
│   │       │   ├── HouseholdRepository.cs
│   │       │   ├── PersonPropertyRelationRepository.cs
│   │       │   ├── EvidenceRepository.cs
│   │       │   ├── DocumentRepository.cs
│   │       │   └── ClaimRepository.cs            # NEW - 30 methods
│   │       └── Migrations/         # Database migrations (6 migrations)
│   │           ├── 20260102233937_InitialCreate.cs
│   │           ├── 20260104104012_AddPropertyUnit.cs
│   │           ├── 20260106111526_AddPersonEntity.cs
│   │           ├── 20260106184023_UpdateHouseholdConfiguration.cs
│   │           ├── 20260107190244_UpdatePersonPropertyRelationConfiguration.cs
│   │           └── 20260109132855_FixClaimsDefaultsAndRenameAllTablesToPlural.cs  # NEW
│   │
│   └── TRRCMS.WebAPI/              # API layer
│       ├── Controllers/
│       │   ├── BuildingsController.cs
│       │   ├── PropertyUnitsController.cs
│       │   ├── PersonsController.cs
│       │   ├── HouseholdsController.cs
│       │   ├── PersonPropertyRelationsController.cs
│       │   ├── EvidencesController.cs
│       │   ├── DocumentsController.cs
│       │   └── ClaimsController.cs      # NEW - 8 endpoints
│       ├── appsettings.json
│       └── Program.cs
│
├── docs/                           # Documentation
├── README.md                       # This file
└── SETUP_GUIDE.md                  # Team setup instructions
```

---

## 📚 API Documentation

### Available Endpoints (v0.8)

#### Buildings API ✅
- `GET /api/Buildings` - Get all buildings
- `GET /api/Buildings/{id}` - Get building by ID
- `POST /api/Buildings` - Create new building
- `PUT /api/Buildings/{id}` - Update building
- `DELETE /api/Buildings/{id}` - Delete building

#### Property Units API ✅
- `GET /api/PropertyUnits` - Get all units
- `GET /api/PropertyUnits/{id}` - Get unit by ID
- `POST /api/PropertyUnits` - Create new unit
- `PUT /api/PropertyUnits/{id}` - Update unit
- `DELETE /api/PropertyUnits/{id}` - Delete unit

#### Persons API ✅
- `GET /api/Persons` - Get all persons
- `GET /api/Persons/{id}` - Get person by ID
- `POST /api/Persons` - Create new person
- `PUT /api/Persons/{id}` - Update person
- `DELETE /api/Persons/{id}` - Delete person

#### Households API ✅
- `GET /api/Households` - Get all households
- `GET /api/Households/{id}` - Get household by ID
- `POST /api/Households` - Create new household
- `PUT /api/Households/{id}` - Update household
- `DELETE /api/Households/{id}` - Delete household

#### Person-Property Relations API ✅
- `GET /api/PersonPropertyRelations` - Get all relations
- `GET /api/PersonPropertyRelations/{id}` - Get relation by ID
- `POST /api/PersonPropertyRelations` - Create new relation
- `PUT /api/PersonPropertyRelations/{id}` - Update relation
- `DELETE /api/PersonPropertyRelations/{id}` - Delete relation

#### Evidence API ✅
- `GET /api/Evidences` - Get all evidence
- `GET /api/Evidences/{id}` - Get evidence by ID
- `POST /api/Evidences` - Create new evidence

#### Documents API ✅
- `GET /api/Documents` - Get all documents
- `GET /api/Documents/{id}` - Get document by ID
- `POST /api/Documents` - Create new document

#### Claims API ✅ ⭐ **NEW**
**Basic Operations:**
- `POST /api/Claims` - Create new claim
  - **Request Body:** PropertyUnitId, PrimaryClaimantId, ClaimType, ClaimSource, CreatedByUserId, Priority, TenureContractType, OwnershipShare, ClaimDescription, LegalBasis, SupportingNarrative
  - **Response:** Created claim with computed properties (201 Created)
  
- `GET /api/Claims/{id}` - Get claim by ID
  - **Response:** Claim with all details + computed properties:
    - `hasConflicts` - Indicates if conflicts detected
    - `conflictCount` - Number of conflicts
    - `evidenceCount` - Number of evidence items
    - `allRequiredDocumentsSubmitted` - Document completion status
    - `isOverdue` - True if claim active > 30 days
    - `awaitingDocuments` - True if documents not submitted
    - `daysInCurrentStage` - Days since last lifecycle stage change
    - `daysActive` - Total days since creation (if active)

- `GET /api/Claims` - Get all claims with filtering
  - **Query Parameters:**
    - `lifecycleStage` - Draft / Active / Completed / Archived
    - `status` - Pending / UnderReview / Verified / Approved / Rejected / OnHold / RequiresMoreInfo / Withdrawn
    - `priority` - Low / Medium / High / Urgent
    - `assignedToUserId` - Filter by assigned case officer
    - `primaryClaimantId` - Filter by claimant
    - `propertyUnitId` - Filter by property
    - `verificationStatus` - Pending / Verified / Rejected / RequiresAdditionalInfo
    - `hasConflicts` - true/false
    - `isOverdue` - true/false
    - `awaitingDocuments` - true/false
  - **Response:** List of claims with computed properties

**Workflow Operations:**
- `PUT /api/Claims/{id}/submit` - Submit claim for processing
  - **Request Body:** SubmittedByUserId
  - **Effect:** Status: Draft → Pending, LifecycleStage: Draft → Active
  - **Response:** 204 No Content
  
- `PUT /api/Claims/{id}/assign` - Assign claim to case officer
  - **Request Body:** AssignedToUserId, AssignedByUserId, Notes
  - **Effect:** Assigns case officer, records assignment timestamp
  - **Response:** 204 No Content
  
- `PUT /api/Claims/{id}/verify` - Verify claim
  - **Request Body:** VerifiedByUserId, VerificationNotes, VerificationOutcome (Verified/Rejected/RequiresAdditionalInfo)
  - **Effect:** Updates verification status, records verifier & timestamp
  - **Response:** 204 No Content
  
- `PUT /api/Claims/{id}/approve` - Approve claim
  - **Request Body:** ApprovedByUserId, ApprovalNotes
  - **Effect:** Status: Verified → Approved, LifecycleStage → Completed, records approval timestamp
  - **Response:** 204 No Content
  
- `PUT /api/Claims/{id}/reject` - Reject claim
  - **Request Body:** RejectedByUserId, RejectionReason (required)
  - **Effect:** Status → Rejected, LifecycleStage → Completed, records rejection timestamp
  - **Response:** 204 No Content

**Swagger UI:** https://localhost:7204/swagger

---

## 🔄 Development Status

### Entity Implementation Progress

| Entity | Domain | Application | Infrastructure | API | Tests | Status |
|--------|--------|-------------|----------------|-----|-------|--------|
| Building | ✅ | ✅ | ✅ | ✅ | ✅ | **Complete** |
| PropertyUnit | ✅ | ✅ | ✅ | ✅ | ✅ | **Complete** |
| Person | ✅ | ✅ | ✅ | ✅ | ✅ | **Complete** |
| Household | ✅ | ✅ | ✅ | ✅ | ✅ | **Complete** |
| PersonPropertyRelation | ✅ | ✅ | ✅ | ✅ | ✅ | **Complete** |
| Evidence | ✅ | ✅ | ✅ | ✅ | ✅ | **Complete** |
| Document | ✅ | ✅ | ✅ | ✅ | ✅ | **Complete** |
| **Claim** | ✅ | ✅ | ✅ | ✅ | ✅ | **Complete** ⭐ |
| **Referral** | ✅ | 📅 | ✅ | 📅 | 📅 | **Partial** |
| Certificate | 📅 | 📅 | 📅 | 📅 | 📅 | **Planned** |

### Detailed Implementation Checklist

**Claim Entity** ✅ (Complete - Jan 9, 2026) ⭐ **NEW**
- [x] Domain entity with 47 fields (identification, relationships, description, status tracking, verification, resolution, computed properties, conflict tracking, priority, audit)
- [x] Rich domain model with 12+ business methods
  - [x] Create() factory method with initialization
  - [x] Submit() - Transition Draft → Active/Pending
  - [x] Assign() - Assign to case officer
  - [x] MarkAsUnderReview() - Begin review process
  - [x] Verify() - Complete verification
  - [x] Approve() - Approve claim
  - [x] Reject() - Reject with reason
  - [x] PutOnHold() - Pause processing
  - [x] RequestMoreInfo() - Request additional information
  - [x] Withdraw() - Claimant withdrawal
  - [x] Archive() - Move to archived stage
  - [x] CalculateDaysInCurrentStage() - Compute duration
- [x] Computed properties (5 properties)
  - [x] HasConflicts - Conflict detection
  - [x] IsOverdue - 30+ days active detection
  - [x] AwaitingDocuments - Document completion check
  - [x] DaysInCurrentStage - Stage duration calculation
  - [x] DaysActive - Total active duration
- [x] State machine workflow
  - [x] Lifecycle stages: Draft → Active → Completed → Archived
  - [x] Statuses: Pending → UnderReview → Verified → Approved/Rejected
  - [x] Status validations and transitions
- [x] Repository interface (30 methods)
  - [x] Basic CRUD (Add, Update, Delete, GetById, GetAll)
  - [x] Filtered queries (GetByStatus, GetByLifecycleStage, GetByPriority, GetByAssignedUser, GetByClaimant, GetByProperty, GetByVerificationStatus)
  - [x] Computed queries (GetOverdueClaims, GetClaimsWithConflicts, GetClaimsAwaitingDocuments)
  - [x] Relationship queries (GetClaimsByClaimant, GetClaimsByProperty)
  - [x] Specialized queries (GetActiveClaimsCount, GetClaimsByDateRange)
- [x] Repository implementation (30 methods)
- [x] DTOs with AutoMapper mapping
  - [x] ClaimDto with all fields
  - [x] Computed property mapping
- [x] CQRS Commands (6 commands)
  - [x] CreateClaimCommand & Handler
  - [x] SubmitClaimCommand & Handler
  - [x] AssignClaimCommand & Handler
  - [x] VerifyClaimCommand & Handler
  - [x] ApproveClaimCommand & Handler
  - [x] RejectClaimCommand & Handler
- [x] CQRS Queries (2 queries)
  - [x] GetAllClaimsQuery & Handler (with 10 filters)
  - [x] GetClaimQuery & Handler
- [x] API Controller with 8 endpoints
  - [x] POST /api/Claims
  - [x] GET /api/Claims/{id}
  - [x] GET /api/Claims (with filters)
  - [x] PUT /api/Claims/{id}/submit
  - [x] PUT /api/Claims/{id}/assign
  - [x] PUT /api/Claims/{id}/verify
  - [x] PUT /api/Claims/{id}/approve
  - [x] PUT /api/Claims/{id}/reject
- [x] Database migration applied
  - [x] Claims table created with 47 columns
  - [x] Default values set (HasConflicts=false, ConflictCount=0, EvidenceCount=0, AllRequiredDocumentsSubmitted=false, IsDeleted=false)
  - [x] Table renamed to plural: Claims (not Claim)
- [x] Tested in Swagger ✅
- [x] Audit trail working (CreatedBy, CreatedAtUtc, LastModifiedBy, LastModifiedAtUtc)
- [x] Soft delete support (IsDeleted, DeletedBy, DeletedAtUtc)
- [x] EF Core configuration comprehensive
  - [x] 26 indexes for performance
  - [x] Column comments for all fields
  - [x] Foreign key relationships configured
  - [x] Cascade delete restrictions
- [x] Conflict detection and tracking
- [x] Evidence/document counting
- [x] Overdue detection (30+ days active)
- [x] Priority escalation support
- [x] UTC timestamp handling for PostgreSQL

**Referral Entity** ✅ (Partial - Jan 9, 2026) ⭐ **NEW**
- [x] Domain entity created
  - [x] Referral number tracking (REF-YYYY-NNNN format)
  - [x] Claim relationship (ClaimId foreign key)
  - [x] Referral parties (FromRole, FromUserId, ToRole, ToUserId)
  - [x] Referral details (Reason, Notes, Priority, Urgency)
  - [x] Status tracking (Pending, Accepted, Rejected, Completed, Cancelled)
  - [x] Dates (ReferredDate, AcceptedDate, CompletedDate, ExpectedCompletionDate)
  - [x] Response tracking (ResponseRequired, ResponseReceivedDate, ResponseNotes)
  - [x] Escalation support (EscalationLevel, ActionsRequired, DocumentsRequired, TargetResolutionHours)
  - [x] Overdue detection (IsOverdue computed property)
  - [x] Version chain support (PreviousReferralId for tracking referral history)
- [x] EF Core configuration
  - [x] Explicit `.ToTable("Referrals")` (plural naming)
  - [x] 50 char max for ReferralNumber
  - [x] Column comments for all fields
  - [x] Foreign key relationships
  - [x] Indexes (ClaimId, ReferralNumber unique, IsDeleted)
  - [x] Self-referencing relationship (PreviousReferralId)
- [x] Database table created ✅
- [ ] Repository interface & implementation
- [ ] CQRS Commands & Queries
- [ ] API Controller with endpoints
- [ ] Tested in Swagger

**Document Entity** ✅ (Complete - Jan 8, 2026)
- [x] Domain entity with all fields
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
feat: Implement Claim entity with full lifecycle management (Jan 9, 2026) ⭐
  - Add comprehensive Claims entity with 47 fields covering all aspects of claim management
  - Implement full lifecycle workflow (Draft→Submitted→UnderReview→Verified→Approved/Rejected)
  - Add rich domain model with 12+ business methods for state transitions
  - Add 30 repository methods including complex filtered queries
  - Add 8 API endpoints (Create, Get, GetAll with 10 filters, Submit, Assign, Verify, Approve, Reject)
  - Implement 5 computed properties (HasConflicts, IsOverdue, AwaitingDocuments, DaysActive, DaysInCurrentStage)
  - Add ReferralConfiguration with explicit .ToTable("Referrals") for consistent naming
  - Fix BaseAuditableEntity IsDeleted initialization in constructors
  - Create comprehensive migration to standardize all table names to plural convention
  - Fix Claims table with default value constraints (HasConflicts=false, ConflictCount=0, EvidenceCount=0, AllRequiredDocumentsSubmitted=false, IsDeleted=false)
  - Rename Evidence→Evidences, Document→Documents, Referral→Referrals for consistency
  - Add 26 database indexes for optimal query performance
  - Implement conflict detection and priority escalation support
  - Add overdue claim detection (30+ days active)
  - Add comprehensive EF Core configuration with column comments
  - Test all 8 endpoints successfully in Swagger
  - Document database naming conventions in README
  Closes TRRCMS-MOB-08

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
4. Implement following the established pattern (see Claim or Document entity as reference)
5. Test thoroughly in Swagger
6. Commit with conventional commit messages
7. Push and create Pull Request

### Code Quality Standards
- ✅ Follow Clean Architecture principles
- ✅ Use CQRS pattern for all operations
- ✅ Implement Repository pattern
- ✅ Add comprehensive XML documentation
- ✅ Include audit fields (Created/Modified/Deleted)
- ✅ Support soft delete with default initialization (IsDeleted = false in constructors)
- ✅ Add computed properties where applicable
- ✅ Test all endpoints in Swagger
- ✅ Follow consistent naming conventions:
  - Entity classes: Singular (e.g., `Claim`, `Person`)
  - Table names: Plural (e.g., `Claims`, `Persons`)
  - Always use explicit `.ToTable("PluralName")` in configurations
- ✅ Add column comments in EF Core configuration
- ✅ Use appropriate indexes for performance
- ✅ Handle UTC timestamps correctly for PostgreSQL
- ✅ Implement default values for non-nullable fields in entity constructors
- ✅ Configure default values in EF Core for database constraints

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
6. ✅ ~~Claim Entity~~ - **COMPLETED Jan 9, 2026** ⭐
7. 📅 Complete Referral entity CRUD operations
8. 📅 Implement automated Claims workflow
9. 📅 Add Authentication & Authorization (JWT + Role-based)
10. 📅 Certificate entity implementation

### Milestone Progress
- **M2: Core Platform Ready** - 100% complete ✅
  - ✅ Building management
  - ✅ Property unit management
  - ✅ Person registry
  - ✅ Household tracking
  - ✅ Person-property relations
  - ✅ Evidence management
  - ✅ Document metadata
  
- **M3: Claims System** - 90% complete 🟡 ⭐
  - ✅ Claims entity with 47 fields
  - ✅ Claims full CRUD operations
  - ✅ Claims workflow (Submit, Assign, Verify, Approve, Reject)
  - ✅ Computed properties (conflicts, overdue, awaiting documents, days tracking)
  - ✅ Advanced filtering (10 filter options)
  - ✅ Referral entity (domain layer + database)
  - ✅ State machine workflow
  - 📅 Referral CRUD operations (Application + API layers)
  - 📅 Automated workflow triggers
  - 📅 Conflict resolution workflow

---

**Last Updated:** January 9, 2026  
**Version:** 0.8.0  
**Status:** 🟢 Active Development  
**Latest Feature:** Claims Management with Full Lifecycle Workflow & Computed Properties ⭐