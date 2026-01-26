# TRRCMS - Tenure Rights Registration & Claims Management System

**Version:** 0.12.0  
**Last Updated:** January 25, 2026  
**Status:** Survey Workflows Complete ✅

---

## 📋 **Project Overview**

The Tenure Rights Registration & Claims Management System (TRRCMS) is a comprehensive solution developed for UN-Habitat to support property rights registration, claims management, and land tenure documentation in Aleppo, Syria.

### **Current Status: v0.12.0**

| Module | Status | Progress |
|--------|--------|----------|
| Backend API | ✅ Complete | 95% |
| Authentication & RBAC | ✅ Complete | 100% |
| Authorization & Permissions | ✅ Complete | 100% |
| Audit Logging | ✅ Complete | 100% |
| **Field Survey Workflow** | ✅ **Complete** | **100%** 🆕 |
| **Office Survey Workflow** | ✅ **Complete** | **100%** 🆕 |
| Claims Management | ✅ Complete | 100% |
| Documents & Evidence | ✅ Complete | 100% |
| Core Entities | ✅ Complete | 100% |

---

## 🎯 **Latest Milestone: Survey Workflows (v0.12.0)**

### **Completed Use Cases:**

| Use Case | Description | Status |
|----------|-------------|--------|
| **UC-001** | Field Survey - Create & conduct property survey in the field | ✅ Complete |
| **UC-002** | Resume Draft Field Survey - Continue incomplete surveys | ✅ Complete |
| **UC-004** | Office Survey - Create & conduct survey at registration office | ✅ Complete |
| **UC-005** | Finalize Office Survey - Complete survey with optional claim creation | ✅ Complete |

### **New Features in v0.12.0:**

#### **1. Field Survey API (UC-001/UC-002)**

Complete mobile-first survey workflow for field collectors:

- ✅ Create field survey with GPS coordinates
- ✅ List field surveys with filtering & pagination
- ✅ Get draft surveys for current user (resume functionality)
- ✅ Get full survey details with nested data
- ✅ Finalize survey for export to .uhc container
- ✅ Validation warnings for incomplete data

#### **2. Office Survey API (UC-004/UC-005)**

Complete desktop workflow for office clerks:

- ✅ Create office survey with registration details
- ✅ Office-specific fields (location, registration number, appointment)
- ✅ Update office survey details
- ✅ Finalize with **automatic claim creation**
- ✅ Finalize without claim (data collection only)
- ✅ Primary claimant designation

#### **3. Survey Data Collection (Shared)**

Common endpoints for both survey types:

- ✅ Property Unit management (create, link, update)
- ✅ Household creation with demographics
- ✅ Person registration with full details
- ✅ Head of household designation
- ✅ Person-property relations (Owner, Tenant, Heir, Occupant)
- ✅ Evidence upload (photos, ID documents, tenure documents)
- ✅ Draft save functionality

---

## 🏗️ **Architecture**

### **Technology Stack:**

| Component | Technology |
|-----------|------------|
| **Backend** | ASP.NET Core 8.0 (C#) |
| **Database** | PostgreSQL 16+ |
| **ORM** | Entity Framework Core 8.0 |
| **API Documentation** | Swagger/OpenAPI |
| **Authentication** | JWT Bearer Tokens |
| **Authorization** | Policy-Based with Custom Permissions |
| **Password Hashing** | BCrypt.Net (work factor 12) |
| **Patterns** | Clean Architecture, CQRS (MediatR), Repository Pattern |

### **Project Structure:**

```
TRRCMS/
├── src/
│   ├── TRRCMS.Domain/              # Domain entities, enums, value objects
│   │   ├── Entities/
│   │   │   ├── Survey.cs           # Survey entity (Field & Office)
│   │   │   ├── Household.cs        # Household with demographics
│   │   │   ├── Person.cs           # Person registration
│   │   │   ├── PersonPropertyRelation.cs
│   │   │   └── Evidence.cs
│   │   └── Enums/
│   │       ├── SurveyType.cs       # Field, Office
│   │       ├── SurveyStatus.cs     # Draft, Completed, Finalized, Exported
│   │       └── RelationType.cs     # Owner, Tenant, Heir, Occupant
│   │
│   ├── TRRCMS.Application/         # Business logic, CQRS commands/queries
│   │   ├── Common/
│   │   │   └── Interfaces/
│   │   │       ├── ISurveyRepository.cs
│   │   │       └── FieldSurveyFilterCriteria.cs
│   │   └── Surveys/
│   │       ├── Commands/
│   │       │   ├── CreateFieldSurvey/
│   │       │   ├── CreateOfficeSurvey/
│   │       │   ├── FinalizeFieldSurvey/
│   │       │   └── FinalizeOfficeSurvey/
│   │       ├── Queries/
│   │       │   ├── GetFieldSurveys/
│   │       │   ├── GetFieldDraftSurveys/
│   │       │   ├── GetFieldSurveyById/
│   │       │   ├── GetOfficeSurveys/
│   │       │   └── GetOfficeSurveyById/
│   │       └── Dtos/
│   │           ├── SurveyDto.cs
│   │           ├── FieldSurveyDtos.cs
│   │           ├── OfficeSurveyDtos.cs
│   │           └── SurveyDataSummaryDto.cs
│   │
│   ├── TRRCMS.Infrastructure/      # Data access, external services
│   │   └── Persistence/
│   │       └── Repositories/
│   │           └── SurveyRepository.cs
│   │
│   └── TRRCMS.WebAPI/              # REST API endpoints, Swagger
│       └── Controllers/
│           └── SurveysController.cs
│
├── docs/
│   ├── FieldSurvey_Mobile_API_Specification.md
│   ├── FieldSurvey_Swagger_Test_Guide.md
│   ├── OfficeSurvey_Swagger_Test_Guide.md
│   └── Postman_Usage_Guide.md
│
├── postman/
│   ├── TRRCMS_Survey_API.postman_collection.json
│   └── TRRCMS_Development.postman_environment.json
│
└── tests/ (planned)
```

---

## 📊 **Survey Workflows**

### **Field Survey Workflow (UC-001/UC-002)**

```
┌─────────────────────────────────────────────────────────────┐
│                    FIELD SURVEY WORKFLOW                     │
│                   (Mobile App - Tablet)                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. LOGIN (Field Collector)                                  │
│     POST /api/Auth/login                                     │
│                                                              │
│  2. CREATE FIELD SURVEY                                      │
│     POST /api/Surveys/field                                  │
│     → Survey created in "Draft" status                       │
│     → Reference code generated (FS-YYYY-NNNNNN)              │
│                                                              │
│  3. LINK PROPERTY UNIT                                       │
│     GET  /api/Surveys/{id}/property-units (list existing)    │
│     POST /api/Surveys/{id}/property-units (create new)       │
│     POST /api/Surveys/{id}/property-units/{unitId}/link      │
│                                                              │
│  4. ADD HOUSEHOLD                                            │
│     POST /api/Surveys/{id}/households                        │
│     → Capture demographics, displacement status              │
│                                                              │
│  5. ADD PERSONS                                              │
│     POST /api/Surveys/{id}/households/{hId}/persons          │
│     → Register family members with IDs                       │
│                                                              │
│  6. SET HEAD OF HOUSEHOLD                                    │
│     PUT /api/Surveys/{id}/households/{hId}/head/{personId}   │
│                                                              │
│  7. ADD RELATIONS                                            │
│     POST /api/Surveys/{id}/property-units/{uId}/relations    │
│     → Link persons as Owner, Tenant, Heir, etc.              │
│                                                              │
│  8. UPLOAD EVIDENCE                                          │
│     POST /api/Surveys/{id}/evidence/photos                   │
│     POST /api/Surveys/{id}/evidence/identification           │
│     POST /api/Surveys/{id}/evidence/tenure                   │
│                                                              │
│  9. SAVE PROGRESS (anytime)                                  │
│     PUT /api/Surveys/{id}/draft                              │
│                                                              │
│  10. FINALIZE SURVEY                                         │
│      POST /api/Surveys/field/{id}/finalize                   │
│      → Status changes to "Finalized"                         │
│      → Ready for export to .uhc container                    │
│                                                              │
│  RESUME DRAFT:                                               │
│  GET /api/Surveys/field/drafts → Select → Continue from #3   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### **Office Survey Workflow (UC-004/UC-005)**

```
┌─────────────────────────────────────────────────────────────┐
│                   OFFICE SURVEY WORKFLOW                     │
│                  (Desktop App - Office)                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. LOGIN (Office Clerk)                                     │
│     POST /api/Auth/login                                     │
│                                                              │
│  2. CREATE OFFICE SURVEY                                     │
│     POST /api/Surveys/office                                 │
│     → Includes: officeLocation, registrationNumber,          │
│       appointmentReference, contactPhone, contactEmail       │
│                                                              │
│  3-8. SAME AS FIELD SURVEY (Property, Household, etc.)       │
│                                                              │
│  9. UPDATE OFFICE DETAILS (if needed)                        │
│     PUT /api/Surveys/office/{id}                             │
│                                                              │
│  10. FINALIZE WITH CLAIM CREATION                            │
│      POST /api/Surveys/office/{id}/finalize                  │
│      {                                                       │
│        "createClaim": true,                                  │
│        "primaryClaimantPersonId": "person-guid",             │
│        "claimNotes": "Ownership claim details..."            │
│      }                                                       │
│      → Survey finalized                                      │
│      → Claim auto-created (CLM-YYYY-NNNNNNNNN)               │
│      → Primary claimant linked                               │
│                                                              │
│  ALTERNATIVE: Finalize without claim                         │
│  { "createClaim": false }                                    │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### **Key Differences: Field vs Office Survey**

| Feature | Field Survey | Office Survey |
|---------|--------------|---------------|
| **Location** | In the field (mobile) | At registration office (desktop) |
| **User** | Field Collector | Office Clerk |
| **Extra Fields** | GPS coordinates | officeLocation, registrationNumber, appointmentReference, contactPhone, contactEmail, inPersonVisit |
| **Finalization** | Ready for .uhc export | Can auto-create Claim |
| **Claim Creation** | During import (UC-003) | During finalization |

---

## 🔌 **API Endpoints**

### **Field Survey Endpoints (NEW)**

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| POST | `/api/Surveys/field` | Create new field survey | CanCreateSurveys |
| GET | `/api/Surveys/field` | List field surveys with filters | CanViewSurveys |
| GET | `/api/Surveys/field/drafts` | Get current user's drafts | CanViewOwnSurveys |
| GET | `/api/Surveys/field/{id}` | Get full survey details | CanViewOwnSurveys |
| POST | `/api/Surveys/field/{id}/finalize` | Finalize for export | CanEditOwnSurveys |

### **Office Survey Endpoints (NEW)**

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| POST | `/api/Surveys/office` | Create new office survey | CanCreateSurveys |
| GET | `/api/Surveys/office` | List office surveys with filters | CanViewSurveys |
| GET | `/api/Surveys/office/drafts` | Get current user's drafts | CanViewOwnSurveys |
| GET | `/api/Surveys/office/{id}` | Get full survey details | CanViewOwnSurveys |
| PUT | `/api/Surveys/office/{id}` | Update office survey | CanEditOwnSurveys |
| POST | `/api/Surveys/office/{id}/finalize` | Finalize + create claim | CanEditOwnSurveys |

### **Survey Data Collection Endpoints**

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Surveys/{id}/property-units` | Get building's property units |
| POST | `/api/Surveys/{id}/property-units` | Create new property unit |
| PUT | `/api/Surveys/{id}/property-units/{unitId}` | Update property unit |
| POST | `/api/Surveys/{id}/property-units/{unitId}/link` | Link unit to survey |
| POST | `/api/Surveys/{id}/households` | Create household |
| GET | `/api/Surveys/{id}/households/{hId}` | Get household details |
| GET | `/api/Surveys/{id}/households/{hId}/persons` | Get persons in household |
| POST | `/api/Surveys/{id}/households/{hId}/persons` | Add person to household |
| PUT | `/api/Surveys/{id}/households/{hId}/head/{personId}` | Set head of household |
| POST | `/api/Surveys/{id}/property-units/{uId}/relations` | Add person-property relation |
| POST | `/api/Surveys/{id}/evidence/photos` | Upload property photo |
| POST | `/api/Surveys/{id}/evidence/identification` | Upload ID document |
| POST | `/api/Surveys/{id}/evidence/tenure` | Upload tenure document |
| GET | `/api/Surveys/{id}/evidence` | Get all evidence |
| GET | `/api/Surveys/evidence/{evidenceId}` | Get evidence by ID |
| GET | `/api/Surveys/evidence/{evidenceId}/download` | Download evidence file |
| DELETE | `/api/Surveys/{id}/evidence/{evidenceId}` | Delete evidence |
| PUT | `/api/Surveys/{id}/draft` | Save survey progress |
| GET | `/api/Surveys/{id}` | Get survey by ID (generic) |

### **Authentication Endpoints**

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Auth/login` | User login |
| POST | `/api/Auth/logout` | User logout |
| POST | `/api/Auth/change-password` | Change password |
| GET | `/api/Auth/me` | Get current user |
| POST | `/api/Auth/seed` | Seed test users |

### **Claims Endpoints**

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| POST | `/api/Claims` | Create claim | Claims.Create |
| GET | `/api/Claims` | List claims | Claims.View |
| GET | `/api/Claims/{id}` | Get claim by ID | Claims.View |
| PUT | `/api/Claims/{id}/submit` | Submit claim | Claims.Submit |
| PUT | `/api/Claims/{id}/assign` | Assign claim | Claims.Assign |
| PUT | `/api/Claims/{id}/verify` | Verify claim | Claims.Verify |

### **Documents & Evidence Endpoints**

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/api/Documents` | List documents | Documents.View |
| GET | `/api/Documents/{id}` | Get document | Documents.View |
| POST | `/api/Documents` | Create document | Documents.Create |
| PUT | `/api/Documents/{id}` | Update document | Documents.Update |
| DELETE | `/api/Documents/{id}` | Delete document | Documents.Delete |
| GET | `/api/Evidences` | List evidences | Evidence.View |
| GET | `/api/Evidences/{id}` | Get evidence | Evidence.View |
| POST | `/api/Evidences` | Create evidence | Evidence.Create |
| PUT | `/api/Evidences/{id}` | Update evidence | Evidence.Update |
| DELETE | `/api/Evidences/{id}` | Delete evidence | Evidence.Delete |

---

## 🗄️ **Database Schema**

### **Current Tables (15 total):**

| # | Table | Description |
|---|-------|-------------|
| 1 | **Users** | User accounts, roles, authentication |
| 2 | **UserPermissions** | User-specific permissions |
| 3 | **AuditLogs** | Comprehensive audit trail |
| 4 | **Buildings** | Building records with geometry |
| 5 | **PropertyUnits** | Property units within buildings |
| 6 | **Persons** | Individual person records |
| 7 | **Households** | Household information with demographics |
| 8 | **PersonPropertyRelations** | Person-property relationships |
| 9 | **Surveys** | Field & Office surveys 🆕 |
| 10 | **Claims** | Property ownership claims |
| 11 | **Evidences** | Evidence records |
| 12 | **Documents** | Document attachments |
| 13 | **Referrals** | Referral records |
| 14 | **ExportPackages** | Export tracking 🆕 |
| 15 | **__EFMigrationsHistory** | EF Core migrations |

### **Key Entity: Survey**

```csharp
public class Survey
{
    // Identity
    public Guid Id { get; set; }
    public string ReferenceCode { get; set; }  // FS-YYYY-NNNNNN or OS-YYYY-NNNNNN
    
    // Type & Status
    public SurveyType Type { get; set; }       // Field, Office
    public SurveyStatus Status { get; set; }   // Draft, Completed, Finalized, Exported
    
    // Relationships
    public Guid BuildingId { get; set; }
    public Guid? PropertyUnitId { get; set; }
    public Guid FieldCollectorId { get; set; }
    public Guid? ClaimId { get; set; }
    
    // Survey Details
    public DateTime SurveyDate { get; set; }
    public string? GpsCoordinates { get; set; }
    public string? IntervieweeName { get; set; }
    public string? IntervieweeRelationship { get; set; }
    public string? Notes { get; set; }
    public int? DurationMinutes { get; set; }
    
    // Office Survey Specific
    public string? OfficeLocation { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? AppointmentReference { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool InPersonVisit { get; set; }
    
    // Export Tracking
    public DateTime? ExportedDate { get; set; }
    public Guid? ExportPackageId { get; set; }
    public DateTime? ImportedDate { get; set; }
    
    // Claim Linking
    public DateTime? ClaimCreatedDate { get; set; }
    
    // Audit
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
}
```

### **Key Entity: Household**

```csharp
public class Household
{
    // Identity
    public Guid Id { get; set; }
    public Guid PropertyUnitId { get; set; }
    
    // Head of Household
    public string HeadOfHouseholdName { get; set; }
    public Guid? HeadOfHouseholdPersonId { get; set; }
    
    // Demographics
    public int HouseholdSize { get; set; }
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
    public int InfantCount { get; set; }      // Under 2 years
    public int ChildCount { get; set; }       // 2-12 years
    public int MinorCount { get; set; }       // 13-17 years
    public int AdultCount { get; set; }       // 18-64 years
    public int ElderlyCount { get; set; }     // 65+ years
    
    // Vulnerability Indicators
    public int PersonsWithDisabilitiesCount { get; set; }
    public bool IsFemaleHeaded { get; set; }
    public int WidowCount { get; set; }
    public int OrphanCount { get; set; }
    
    // Displacement
    public bool IsDisplaced { get; set; }
    public string? OriginLocation { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public string? DisplacementReason { get; set; }
    
    // Economic
    public int EmployedPersonsCount { get; set; }
    public int UnemployedPersonsCount { get; set; }
    public string? PrimaryIncomeSource { get; set; }
    public decimal? MonthlyIncomeEstimate { get; set; }
}
```

---

## 🔐 **Authorization & Permissions**

### **Permission Structure:**

**Format:** `{Module}.{Action}` (e.g., `Claims.Approve`, `Documents.Verify`)

### **Survey Permissions (NEW):**

```
Surveys:
  - Surveys.View       # View all surveys
  - Surveys.ViewOwn    # View own surveys only
  - Surveys.Create     # Create new surveys
  - Surveys.Update     # Update any survey
  - Surveys.UpdateOwn  # Update own surveys only
  - Surveys.Finalize   # Finalize surveys
  - Surveys.Export     # Export to .uhc
```

### **Role-Permission Mapping:**

| Role | Survey Permissions |
|------|-------------------|
| Administrator | All permissions |
| DataManager | View, Create, Update, Finalize |
| OfficeClerk | ViewOwn, Create, UpdateOwn, Finalize |
| FieldCollector | ViewOwn, Create, UpdateOwn, Finalize |
| FieldSupervisor | View, ViewOwn |
| Analyst | View (read-only) |

---

## 🧪 **Testing**

### **API Testing Resources:**

| Resource | Description | Location |
|----------|-------------|----------|
| **Swagger UI** | Interactive API testing | `https://localhost:7001/swagger` |
| **Postman Collection** | Complete API collection | `/postman/TRRCMS_Survey_API.postman_collection.json` |
| **Postman Environment** | Environment variables | `/postman/TRRCMS_Development.postman_environment.json` |
| **Field Survey Test Guide** | Step-by-step Swagger guide | `/docs/FieldSurvey_Swagger_Test_Guide.md` |
| **Office Survey Test Guide** | Step-by-step Swagger guide | `/docs/OfficeSurvey_Swagger_Test_Guide.md` |

### **Postman Collection Features:**

- ✅ 40+ API requests organized in folders
- ✅ Auto-save variables (token, IDs)
- ✅ Test scripts with assertions
- ✅ Complete workflow sequences
- ✅ Environment file with variables

### **Manual Testing Completed:**

- ✅ Field survey creation and finalization
- ✅ Office survey with claim creation
- ✅ Draft survey resumption
- ✅ Property unit linking
- ✅ Household and person creation
- ✅ Person-property relations
- ✅ Evidence upload
- ✅ Finalization with validation
- ✅ All filter and pagination options

---

## 📱 **Mobile Integration Guide**

### **For Mobile Team (Field Survey):**

Documentation: `/docs/FieldSurvey_Mobile_API_Specification.md`

**Key Endpoints:**

| Priority | Endpoint | Use |
|----------|----------|-----|
| HIGH | `POST /Surveys/field` | Start new survey |
| HIGH | `GET /Surveys/field/drafts` | Resume surveys |
| HIGH | `GET /Surveys/field/{id}` | Load survey details |
| HIGH | `POST /Surveys/field/{id}/finalize` | Complete survey |
| MEDIUM | `PUT /Surveys/{id}/draft` | Save progress |

**Offline Considerations:**

- Cache building list locally
- Queue changes when offline
- Sync on reconnection
- Never auto-finalize offline

### **For Desktop Team (Office Survey):**

**Key Endpoints:**

| Priority | Endpoint | Use |
|----------|----------|-----|
| HIGH | `POST /Surveys/office` | Start new survey |
| HIGH | `GET /Surveys/office/{id}` | Load survey details |
| HIGH | `POST /Surveys/office/{id}/finalize` | Complete + create claim |
| MEDIUM | `PUT /Surveys/office/{id}` | Update details |
| MEDIUM | `GET /Claims/{id}` | Verify created claim |

---

## 📊 **Project Progress**

### **Completed Tasks:**

| Task | Description | Status |
|------|-------------|--------|
| TRRCMS-BE-01 | Core database schema & migrations | ✅ Complete |
| TRRCMS-BE-02 | Authentication & RBAC | ✅ Complete |
| TRRCMS-BE-03 | Permission System & Authorization | ✅ Complete |
| TRRCMS-BE-04 | Audit Logging System | ✅ Complete |
| TRRCMS-BE-05 | Sequential Claim Numbers | ✅ Complete |
| TRRCMS-BE-06 | Database Schema Fixes | ✅ Complete |
| **TRRCMS-BE-07** | **Field Survey API (UC-001/UC-002)** | ✅ **Complete** 🆕 |
| **TRRCMS-BE-08** | **Office Survey API (UC-004/UC-005)** | ✅ **Complete** 🆕 |
| **TRRCMS-BE-09** | **Survey Data Collection Endpoints** | ✅ **Complete** 🆕 |
| **TRRCMS-BE-10** | **API Documentation & Testing** | ✅ **Complete** 🆕 |

### **Overall Progress:**

| Component | Progress |
|-----------|----------|
| Backend API | **95%** ⬆️ |
| Database Schema | **98%** ⬆️ |
| Authentication | **100%** ✅ |
| Authorization | **100%** ✅ |
| Audit System | **100%** ✅ |
| Survey Workflows | **100%** ✅ 🆕 |
| CRUD Operations | **95%** ⬆️ |
| API Documentation | **100%** ✅ 🆕 |

---

## 🔜 **Roadmap**

### **v0.13.0 - Claim Update Workflows (Next)**

- UC-006: Update Claim Information
- Update claim details
- Add/update claimants
- Add/update evidence
- Change claim status
- Assign claims to officers

### **v0.14.0 - Export/Import System**

- UC-003: Import Field Survey Data
- Export field surveys to .uhc container
- Import and validate .uhc packages
- Conflict detection on import

### **v0.15.0 - Conflict Resolution**

- Detect overlapping claims
- Conflict flagging system
- Adjudication workflow
- Resolution tracking

### **v1.0.0 - MVP Release**

- Complete backend API
- Field survey mobile app (tablet)
- Office/Admin desktop app
- Full import/export functionality
- Conflict resolution workflows
- Production deployment

---

## 📝 **Development Notes**

### **Key Improvements in v0.12.0:**

1. **Survey Workflows:** Complete Field & Office survey APIs
2. **Mobile-First Design:** Field Survey API optimized for tablets
3. **Auto-Claim Creation:** Office surveys can create claims on finalization
4. **Comprehensive DTOs:** Nested data structures for efficient data loading
5. **Validation Warnings:** Incomplete data warnings without blocking finalization
6. **API Documentation:** Postman collection + Swagger guides

### **Code Quality:**

- ✅ Clean Architecture maintained
- ✅ CQRS pattern with MediatR
- ✅ Repository pattern for data access
- ✅ FluentValidation for request validation
- ✅ AutoMapper for DTO mapping
- ✅ Comprehensive XML documentation

### **Security Best Practices:**

- Never commit JWT secrets to Git
- Use environment variables for production
- Implement HTTPS in production
- Audit all sensitive operations
- Permission-based access control

---

## 📚 **Documentation**

| Document | Description | Location |
|----------|-------------|----------|
| API Documentation | Interactive Swagger | `/swagger` endpoint |
| Field Survey Mobile API | Mobile team integration guide | `/docs/FieldSurvey_Mobile_API_Specification.md` |
| Field Survey Test Guide | Swagger testing steps | `/docs/FieldSurvey_Swagger_Test_Guide.md` |
| Office Survey Test Guide | Swagger testing steps | `/docs/OfficeSurvey_Swagger_Test_Guide.md` |
| Postman Usage Guide | Collection import & usage | `/docs/Postman_Usage_Guide.md` |
| FSD | Functional Specification | `UN_Habitat_TRRCMS_FSD_v5.docx` |
| Use Cases | Use case specifications | `UN_Habitat_TRRCMS_Use_Cases_V2.xlsx` |
| Delivery Plan | Project delivery plan | `TRRCMS_Internal_Delivery_Plan.docx` |

---

## 🎉 **Change Log**

### **v0.12.0 - January 25, 2026** ✅ **LATEST**

**Survey Workflows Release**

- ✅ **NEW:** Field Survey API - UC-001 Create Field Survey
- ✅ **NEW:** Field Survey API - UC-002 Resume Draft Survey
- ✅ **NEW:** Office Survey API - UC-004 Create Office Survey
- ✅ **NEW:** Office Survey API - UC-005 Finalize with Claim Creation
- ✅ **NEW:** Survey data collection endpoints (property units, households, persons, relations, evidence)
- ✅ **NEW:** Draft survey save/resume functionality
- ✅ **NEW:** Finalization with validation warnings
- ✅ **NEW:** Automatic claim creation on office survey finalization
- ✅ **NEW:** FieldSurveyFilterCriteria for advanced filtering
- ✅ **NEW:** Comprehensive survey DTOs with nested data
- ✅ **NEW:** Postman collection (40+ requests)
- ✅ **NEW:** Mobile API specification document
- ✅ **NEW:** Swagger test guides for both workflows
- ✅ **FIXED:** Household entity property mappings
- ✅ **FIXED:** Evidence repository method signatures
- ✅ **TESTED:** Complete field survey workflow
- ✅ **TESTED:** Complete office survey workflow with claim creation

### **v0.11.0 - January 24, 2026**

**Office Survey Foundation**

- ✅ **NEW:** Office Survey entity fields
- ✅ **NEW:** Office Survey CRUD operations
- ✅ **NEW:** Office-specific endpoints

### **v0.10.0 - January 14, 2026**

**Authorization & Audit System Release**

- ✅ Fine-grained permission system (30+ permissions)
- ✅ Policy-based authorization infrastructure
- ✅ Comprehensive audit logging system
- ✅ Sequential claim number generation
- ✅ Database schema fixes

### **v0.9.0 - January 10, 2026**

- ✅ Complete JWT authentication system
- ✅ BCrypt password hashing
- ✅ 6 user roles with RBAC

### **v0.8.0 and earlier**

- Core entities and API structure
- Claims management
- Repository pattern implementation

---

## 👥 **Team & Roles**

| Role | Responsibility |
|------|----------------|
| Project Manager | Planning, tracking, stakeholder communication |
| Tech Lead | Architecture, technical decisions, code reviews |
| Backend Developer | API development, database design |
| Mobile Developer | Field survey tablet app |
| Desktop Developer | Office/Admin desktop app |
| QA Engineer | Testing, quality assurance |
| DevOps Engineer | CI/CD, deployment, monitoring |

---

## 📄 **License**

Proprietary - UN-Habitat © 2024-2026

---

## 🤝 **Contributing**

This is an internal UN-Habitat project. For questions or contributions, please contact the project manager.

---

**Status:** Survey Workflows Complete - Ready for Phase 2 (Claim Updates) 🚀
