# TRRCMS - Tenure Rights Registration & Claims Management System

**Version:** 0.10.0  
**Last Updated:** January 14, 2026  
**Status:** Authorization & Audit System Complete ✅

---

## 📋 **Project Overview**

The Tenure Rights Registration & Claims Management System (TRRCMS) is a comprehensive solution developed for UN-Habitat to support property rights registration, claims management, and land tenure documentation in Aleppo, Syria.

### **Current Status: v0.10.0**

- **Backend API:** ~90% Complete
- **Authentication & RBAC:** ✅ 100% Complete
- **Authorization & Permissions:** ✅ 100% Complete (NEW)
- **Audit Logging:** ✅ 100% Complete (NEW)
- **Claims Management:** ✅ 100% Complete
- **Documents & Evidence:** ✅ 100% Complete (NEW)
- **Core Entities:** ✅ Complete

---

## 🎯 **Latest Milestone: Authorization & Audit System (v0.10.0)**

### **Completed Features:**

#### **1. Permission System**

- ✅ Fine-grained permission-based authorization
- ✅ 30+ granular permissions across 6 modules:
  - **Claims Management** - View, Create, Update, Submit, Assign, Verify, Approve, Reject, Delete
  - **Documents Management** - View, Create, Update, Verify, Delete
  - **Evidence Management** - View, Create, Update, Verify, Version, Delete
  - **Users Management** - View, Create, Update, Delete, ResetPassword, ManageRoles
  - **Reports** - ViewAll, Export, Advanced
  - **System** - Configuration, AuditLogs, Maintenance
- ✅ UserPermission junction table with inheritance support
- ✅ Role-based default permissions

#### **2. Authorization Policies**

- ✅ Policy-based authorization using ASP.NET Core policies
- ✅ `PermissionRequirement` and `PermissionAuthorizationHandler`
- ✅ Applied to Documents and Evidences controllers:
  - **View permissions** - GET endpoints (ViewDocuments, ViewEvidence)
  - **Create permissions** - POST endpoints (CreateDocuments, CreateEvidence)
  - **Update permissions** - PUT endpoints (UpdateDocuments, UpdateEvidence)
  - **Verify permissions** - Verification endpoints (VerifyDocuments, VerifyEvidence)
  - **Delete permissions** - DELETE endpoints (DeleteDocuments, DeleteEvidence)

#### **3. Audit Logging System**

- ✅ Comprehensive audit trail for all database operations
- ✅ Automatic tracking via `SaveChangesAsync` interceptor
- ✅ Captures:
  - Entity name and operation (Create, Update, Delete)
  - Old and new values (JSON format)
  - User ID, timestamp, IP address
  - Request path and HTTP method
- ✅ AuditLogs table with 14 columns
- ✅ Performance indexes on UserId, EntityName, Timestamp
- ✅ Soft delete support with IsDeleted flag

#### **4. Sequential Claim Numbers**

- ✅ Database sequence for unique claim numbers
- ✅ Format: CLM-YYYY-NNNNNNNNN (e.g., CLM-2026-000000001)
- ✅ Thread-safe generation
- ✅ Automatic assignment on claim creation

#### **5. Database Schema Improvements**

- ✅ Fixed table naming consistency (all plural):
  - Document → **Documents**
  - Evidence → **Evidences**
  - Referral → **Referrals**
- ✅ Fixed enum storage (DocumentType: string → int)
- ✅ Fixed default values using PostgreSQL SQL:
  - Documents: IsVerified, IsLegallyValid, IsOriginal, IsNotarized, IsDeleted
  - Evidences: IsCurrentVersion, VersionNumber, IsDeleted
  - All tables: IsDeleted defaults properly set
- ✅ Fixed index names to match plural table names

#### **6. Workflow Simplification**

- ✅ Removed Approve/Reject claim operations (Phase 1.5 Step 1)
- ✅ Streamlined claims workflow for MVP
- ✅ Focus on data collection and verification

---

## 🏗️ **Architecture**

### **Technology Stack:**

- **Backend:** ASP.NET Core 8.0 (C#)
- **Database:** PostgreSQL 16+
- **ORM:** Entity Framework Core 8.0
- **API Documentation:** Swagger/OpenAPI
- **Authentication:** JWT Bearer Tokens
- **Authorization:** Policy-Based with Custom Permissions
- **Password Hashing:** BCrypt.Net (work factor 12)
- **Patterns:** Clean Architecture, CQRS (MediatR), Repository Pattern

### **Project Structure:**

```
TRRCMS/
├── src/
│   ├── TRRCMS.Domain/          # Domain entities, enums, value objects
│   ├── TRRCMS.Application/     # Business logic, CQRS commands/queries
│   ├── TRRCMS.Infrastructure/  # Data access, external services
│   └── TRRCMS.WebAPI/          # REST API endpoints, Swagger
└── tests/ (planned)
```

---

## 🗄️ **Database Schema**

### **Current Tables (13 total):**

1. **Users** - User accounts, roles, authentication
2. **UserPermissions** - User-specific permissions ✅ **NEW in v0.10.0**
3. **AuditLogs** - Comprehensive audit trail ✅ **NEW in v0.10.0**
4. **Buildings** - Building records with geometry
5. **PropertyUnits** - Property units within buildings
6. **Persons** - Individual person records
7. **Households** - Household information
8. **PersonPropertyRelations** - Person-property relationships
9. **Claims** - Property ownership claims (with sequential numbers)
10. **Evidences** - Evidence records linked to claims ✅ **FIXED: Plural**
11. **Documents** - Document attachments ✅ **FIXED: Plural**
12. **Referrals** - Referral records ✅ **FIXED: Plural**
13. **\_\_EFMigrationsHistory** - EF Core migrations tracking

### **Schema Improvements (v0.10.0):**

- ✅ All table names now plural for consistency
- ✅ All boolean columns have SQL DEFAULT values
- ✅ All indexes properly named with table names
- ✅ Enums stored as integers (not strings)
- ✅ Audit trail on all major tables

---

## 🔐 **Authorization & Permissions**

### **Permission Structure:**

**Format:** `{Module}.{Action}` (e.g., `Claims.Approve`, `Documents.Verify`)

**Available Permissions:**

```
Claims Management:
  - Claims.View, Claims.Create, Claims.Update
  - Claims.Submit, Claims.Assign, Claims.Verify
  - Claims.Approve, Claims.Reject, Claims.Delete

Documents Management:
  - Documents.View, Documents.Create, Documents.Update
  - Documents.Verify, Documents.Delete

Evidence Management:
  - Evidence.View, Evidence.Create, Evidence.Update
  - Evidence.Verify, Evidence.Version, Evidence.Delete

Users Management:
  - Users.View, Users.Create, Users.Update, Users.Delete
  - Users.ResetPassword, Users.ManageRoles

Reports:
  - Reports.ViewAll, Reports.Export, Reports.Advanced

System:
  - System.Configuration, System.AuditLogs, System.Maintenance
```

### **Role-Permission Mapping:**

| Role            | Key Permissions                           |
| --------------- | ----------------------------------------- |
| Administrator   | All permissions (full system access)      |
| DataManager     | Create, Update, Verify all entities       |
| OfficeClerk     | Create, Update Claims/Documents/Evidence  |
| FieldCollector  | Create Claims/Documents/Evidence (mobile) |
| FieldSupervisor | View all, limited updates                 |
| Analyst         | View all, export reports (read-only)      |

### **Authorization Usage:**

Controllers use `[Authorize(Policy = "PermissionPolicy")]` with permission requirements:

```csharp
// Example: Documents Controller
[Authorize(Policy = "ViewDocuments")]     // GET endpoints
[Authorize(Policy = "CreateDocuments")]   // POST endpoints
[Authorize(Policy = "UpdateDocuments")]   // PUT endpoints
[Authorize(Policy = "DeleteDocuments")]   // DELETE endpoints
```

---

## 📊 **Audit Logging**

### **Audit Log Entry Structure:**

```json
{
  "id": "uuid",
  "userId": "user-uuid",
  "userName": "admin",
  "action": "Update",
  "entityName": "Document",
  "entityId": "entity-uuid",
  "timestamp": "2026-01-14T12:00:00Z",
  "ipAddress": "192.168.1.1",
  "requestPath": "/api/Documents/123",
  "httpMethod": "PUT",
  "oldValues": "{\"IsVerified\":false}",
  "newValues": "{\"IsVerified\":true}",
  "changes": "IsVerified: false → true"
}
```

### **Audit Features:**

- ✅ Automatic tracking on Create/Update/Delete
- ✅ Captures field-level changes
- ✅ User context (ID, username)
- ✅ Request context (IP, path, method)
- ✅ Queryable by entity, user, date range
- ✅ Soft delete support

---

## 🔢 **Sequential Claim Numbers**

### **Format:** `CLM-YYYY-NNNNNNNNN`

**Examples:**

- First claim of 2026: `CLM-2026-000000001`
- Second claim: `CLM-2026-000000002`
- 100th claim: `CLM-2026-000000100`

### **Implementation:**

- ✅ PostgreSQL sequence: `seq_claim_number`
- ✅ Automatically assigned on claim creation
- ✅ Year extracted from current date
- ✅ 9-digit zero-padded number
- ✅ Guaranteed unique across system

---

## 🚀 **Getting Started**

### **Prerequisites:**

- .NET 8.0 SDK
- PostgreSQL 16+
- Visual Studio 2022 or VS Code
- Postman or similar API client (optional, Swagger UI included)

### **Setup Instructions:**

1. **Clone the Repository**

   ```bash
   git clone <repository-url>
   cd TRRCMS
   ```

2. **Configure Database Connection**

   Update `src/TRRCMS.WebAPI/appsettings.Development.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TRRCMS_Dev;Username=postgres;Password=YOUR_PASSWORD;Collation=Arabic_Saudi Arabia.1256"
     }
   }
   ```

3. **Configure JWT Settings**

   JWT settings are pre-configured in `appsettings.Development.json`:

   ```json
   {
     "JwtSettings": {
       "Secret": "dev-jwt-secret-key-minimum-32-characters-long-change-in-production",
       "Issuer": "TRRCMS.API",
       "Audience": "TRRCMS.Clients",
       "AccessTokenExpirationMinutes": 60,
       "RefreshTokenExpirationDays": 30
     }
   }
   ```

   **⚠️ Production:** Generate a secure 64+ character secret before deploying!

4. **Apply Database Migrations**

   Open Package Manager Console in Visual Studio:

   ```powershell
   Update-Database
   ```

   Or use .NET CLI:

   ```bash
   cd src/TRRCMS.Infrastructure
   dotnet ef database update --startup-project ../TRRCMS.WebAPI
   ```

5. **Run the Application**

   Press F5 in Visual Studio or:

   ```bash
   cd src/TRRCMS.WebAPI
   dotnet run
   ```

   The API will be available at: `https://localhost:7284` (or similar)

6. **Seed Test Users**

   Open Swagger UI: `https://localhost:7284/swagger`

   Navigate to **Seed** section → **POST /api/Seed/users**

   Click "Try it out" → "Execute"

   This creates 6 test users with appropriate permissions.

7. **Test Authentication**

   In Swagger:

   - Navigate to **Auth** section → **POST /api/Auth/login**
   - Login with: `{"username": "admin", "password": "Admin@123"}`
   - Copy the `accessToken` from the response
   - Click the green "Authorize" button (top-right)
   - Paste the token (without "Bearer" prefix)
   - Click "Authorize" → "Close"
   - Now you can test protected endpoints!

8. **Test Authorization**

   Try accessing Documents or Evidences endpoints with different user roles to see permission-based access control in action.

---

## 📡 **API Endpoints**

### **Authentication Endpoints:**

| Method | Endpoint                    | Description             | Auth Required |
| ------ | --------------------------- | ----------------------- | ------------- |
| POST   | `/api/Auth/login`           | Login with credentials  | ❌ No         |
| POST   | `/api/Auth/refresh`         | Refresh access token    | ❌ No         |
| POST   | `/api/Auth/change-password` | Change password         | ✅ Yes        |
| POST   | `/api/Auth/logout`          | Logout (discard tokens) | ✅ Yes        |
| GET    | `/api/Auth/me`              | Get current user info   | ✅ Yes        |

### **Documents Endpoints:**

| Method | Endpoint              | Description        | Required Permission |
| ------ | --------------------- | ------------------ | ------------------- |
| GET    | `/api/Documents`      | List all documents | Documents.View      |
| GET    | `/api/Documents/{id}` | Get document by ID | Documents.View      |
| POST   | `/api/Documents`      | Create document    | Documents.Create    |
| PUT    | `/api/Documents/{id}` | Update document    | Documents.Update    |
| DELETE | `/api/Documents/{id}` | Delete document    | Documents.Delete    |

### **Evidences Endpoints:**

| Method | Endpoint                 | Description        | Required Permission |
| ------ | ------------------------ | ------------------ | ------------------- |
| GET    | `/api/v1/Evidences`      | List all evidences | Evidence.View       |
| GET    | `/api/v1/Evidences/{id}` | Get evidence by ID | Evidence.View       |
| POST   | `/api/v1/Evidences`      | Create evidence    | Evidence.Create     |
| PUT    | `/api/v1/Evidences/{id}` | Update evidence    | Evidence.Update     |
| DELETE | `/api/v1/Evidences/{id}` | Delete evidence    | Evidence.Delete     |

### **Claims Endpoints:**

| Method | Endpoint                     | Description     | Required Permission |
| ------ | ---------------------------- | --------------- | ------------------- |
| POST   | `/api/v1/Claims`             | Create claim    | Claims.Create       |
| GET    | `/api/v1/Claims`             | List claims     | Claims.View         |
| GET    | `/api/v1/Claims/{id}`        | Get claim by ID | Claims.View         |
| PUT    | `/api/v1/Claims/{id}/submit` | Submit claim    | Claims.Submit       |
| PUT    | `/api/v1/Claims/{id}/assign` | Assign claim    | Claims.Assign       |
| PUT    | `/api/v1/Claims/{id}/verify` | Verify claim    | Claims.Verify       |

**Note:** Approve/Reject endpoints removed in v0.10.0 as part of workflow simplification.

---

## 🧪 **Testing**

### **Manual Testing:**

- ✅ All authentication endpoints tested
- ✅ Authorization policies tested (Documents, Evidences)
- ✅ Permission-based access control verified
- ✅ Audit logging tested (Create, Update, Delete operations)
- ✅ Sequential claim numbers tested
- ✅ Document creation with default values tested
- ✅ Evidence creation with default values tested
- ✅ Database schema consistency verified

### **Automated Tests:**

- ⏳ Unit tests (planned)
- ⏳ Integration tests (planned)

---

## 📊 **Project Progress**

### **Completed Tasks:**

- ✅ **TRRCMS-BE-01** - Core database schema & migrations
- ✅ **TRRCMS-BE-02** - Authentication & RBAC
- ✅ **TRRCMS-BE-03** - Permission System & Authorization ✅ **NEW**
- ✅ **TRRCMS-BE-04** - Audit Logging System ✅ **NEW**
- ✅ **TRRCMS-BE-05** - Sequential Claim Numbers ✅ **NEW**
- ✅ **TRRCMS-BE-06** - Database Schema Fixes ✅ **NEW**
- ✅ **Phase 1.5 Step 1** - Workflow Simplification ✅ **NEW**
- ✅ **Phase 1.5 Step 2** - Documents/Evidences Authorization ✅ **NEW**

### **Next Tasks (Sprint 3):**

- ⏳ **Phase 1.5 Steps 3-9** - Remaining workflow steps
- ⏳ **Phase 2** - State machine implementation
- ⏳ **TRRCMS-BE-07** - Evidence file storage service
- ⏳ **TRRCMS-ADM-01** - User management UI

### **Overall Progress:**

- Backend API: **~90%** complete ⬆️
- Database Schema: **~95%** complete ⬆️
- Authentication: **100%** complete ✅
- Authorization: **100%** complete ✅ **NEW**
- Audit System: **100%** complete ✅ **NEW**
- CRUD Operations: **~85%** complete ⬆️

---

## 🔜 **Roadmap**

### **v0.11.0 - State Machine & Workflow (Planned)**

- Complete Phase 1.5 steps 3-9
- Implement state machine for claims lifecycle
- Enhanced workflow validation

### **v0.12.0 - User Management UI (Planned)**

- Admin endpoints for user CRUD operations
- User listing with filters
- Role and permission assignment
- Account activation/deactivation

### **v1.0.0 - MVP Release (Planned)**

- Complete backend API
- Field survey mobile app (tablet)
- Office/Admin desktop app
- Import/Export functionality
- Conflict resolution workflows

---

## 📝 **Development Notes**

### **Key Improvements in v0.10.0:**

1. **Authorization:** Moved from role-based to permission-based for finer control
2. **Audit Trail:** Comprehensive logging of all database changes
3. **Data Integrity:** Fixed default values using PostgreSQL SQL (not EF Core)
4. **Naming Consistency:** All tables now plural (Documents, Evidences, Referrals)
5. **Workflow:** Simplified claims workflow for MVP (removed Approve/Reject)

### **Database Best Practices:**

- ✅ Always use SQL for setting defaults in PostgreSQL (not `AlterColumn`)
- ✅ Use plural table names for consistency
- ✅ Store enums as integers for performance
- ✅ Include Designer files for all migrations
- ✅ Test migrations on fresh database before committing

### **Security Best Practices:**

- Never commit JWT secrets to Git
- Use environment variables for production secrets
- Implement HTTPS in production
- Enable token blacklist for enhanced security (optional)
- Regular security audits
- Audit all sensitive operations

---

## 📚 **Documentation**

- **API Documentation:** Available at `/swagger` endpoint
- **FSD:** UN_Habitat_TRRCMS_FSD_v5.docx
- **Use Cases:** UN_Habitat_TRRCMS_Use_Cases_V2.xlsx
- **Delivery Plan:** TRRCMS_Internal_Delivery_Plan.docx
- **ERD:** (Coming soon)

---

## 🎉 **Change Log**

### **v0.10.0 - January 14, 2026** ✅ **LATEST**

**Authorization & Audit System Release**

- ✅ **NEW:** Fine-grained permission system (30+ permissions)
- ✅ **NEW:** Policy-based authorization infrastructure
- ✅ **NEW:** Comprehensive audit logging system
- ✅ **NEW:** UserPermissions table with role inheritance
- ✅ **NEW:** AuditLogs table with field-level change tracking
- ✅ **NEW:** Sequential claim number generation (CLM-YYYY-NNNNNNNNN)
- ✅ **NEW:** Documents controller with authorization policies
- ✅ **NEW:** Evidences controller with authorization policies
- ✅ **FIXED:** Table names to plural (Documents, Evidences, Referrals)
- ✅ **FIXED:** Enum storage (DocumentType: string → int)
- ✅ **FIXED:** Default values using PostgreSQL SQL
- ✅ **FIXED:** Index names to match plural tables
- ✅ **FIXED:** EvidenceConfiguration index names
- ✅ **REMOVED:** Approve/Reject claim endpoints (workflow simplification)
- ✅ **UPDATED:** Database migrations with proper SQL defaults
- ✅ **TESTED:** Document and Evidence creation with default values

### **v0.9.0 - January 10, 2026**

- ✅ **NEW:** Complete JWT authentication system
- ✅ **NEW:** BCrypt password hashing
- ✅ **NEW:** 6 user roles with RBAC infrastructure
- ✅ **NEW:** Account lockout protection
- ✅ **NEW:** Password change functionality
- ✅ **NEW:** Device tracking for audit compliance
- ✅ **NEW:** 5 authentication API endpoints
- ✅ **NEW:** Seed endpoint for test users
- ✅ **NEW:** Users table (43 columns, 7 indexes)
- ✅ **UPDATED:** Swagger with JWT bearer authentication

### **v0.8.0 - January 9, 2026**

- ✅ Claims entity with complete lifecycle
- ✅ 8 Claims API endpoints
- ✅ Claims workflow testing complete

### **v0.7.0 and earlier**

- Core entities (Buildings, PropertyUnits, Persons, Households, etc.)
- Repository pattern implementation
- Initial API structure
- Database migrations framework

---

**Status:** Ready for Phase 2 Development 🚀

---

## 👥 **Team & Roles**

| Role               | Responsibility                                  |
| ------------------ | ----------------------------------------------- |
| Project Manager    | Planning, tracking, stakeholder communication   |
| Tech Lead          | Architecture, technical decisions, code reviews |
| Backend Developer  | API development, database design                |
| Frontend Developer | UI/UX implementation (mobile & desktop)         |
| QA Engineer        | Testing, quality assurance                      |
| DevOps Engineer    | CI/CD, deployment, monitoring                   |

---

## 📄 **License**

Proprietary - UN-Habitat © 2024-2026

---

## 🤝 **Contributing**

This is an internal UN-Habitat project. For questions or contributions, please contact the project manager.

---
