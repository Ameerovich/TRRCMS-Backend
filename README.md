# TRRCMS - Tenure Rights Registration & Claims Management System

**Version:** 0.9.0  
**Last Updated:** January 10, 2026  
**Status:** Authentication & RBAC Module Complete ✅

---

## 📋 **Project Overview**

The Tenure Rights Registration & Claims Management System (TRRCMS) is a comprehensive solution developed for UN-Habitat to support property rights registration, claims management, and land tenure documentation in Aleppo, Syria.

### **Current Status: v0.9.0**

- **Backend API:** ~85% Complete
- **Authentication & RBAC:** ✅ 100% Complete (Sprint 1 - TRRCMS-BE-02)
- **Claims Management:** ✅ 100% Complete
- **Core Entities:** ✅ Complete (Buildings, Property Units, Persons, Households, Claims, Evidence, Documents)

---

## 🎯 **Latest Milestone: Authentication & RBAC (v0.9.0)**

### **Completed Features:**

#### **1. JWT Authentication System**

- ✅ Secure login with username/password
- ✅ JWT access tokens (15 min expiry in production, 60 min in dev)
- ✅ Refresh token rotation (7 days in production, 30 days in dev)
- ✅ Password change functionality
- ✅ Logout support (stateless JWT)
- ✅ Device tracking for audit compliance

#### **2. Security Features**

- ✅ BCrypt password hashing (work factor 12 = 4096 rounds)
- ✅ Account lockout protection (5 failed attempts = 30 min lockout)
- ✅ Password expiry checking
- ✅ Security stamp for token invalidation
- ✅ Active/Inactive account validation
- ✅ FSD-compliant audit trail with device tracking

#### **3. Role-Based Access Control (RBAC)**

- ✅ 6 User Roles:
  - **Administrator** - Full system access (desktop only)
  - **DataManager** - Data verification and management (desktop only)
  - **OfficeClerk** - Office data entry and claims processing (desktop only)
  - **FieldCollector** - Field data collection (mobile only)
  - **FieldSupervisor** - Field team supervision (desktop read-only)
  - **Analyst** - Data analysis and reporting (desktop read-only)
- ✅ Mobile/Desktop access flags
- ✅ Role-based authorization infrastructure ready

#### **4. Authentication API Endpoints**

- `POST /api/Auth/login` - User authentication
- `POST /api/Auth/refresh` - Refresh access token
- `POST /api/Auth/change-password` - Change user password
- `POST /api/Auth/logout` - Logout (client-side token discard)
- `GET /api/Auth/me` - Get current user information

#### **5. Development Tools**

- ✅ Seed endpoint for test users (`POST /api/Seed/users`)
- ✅ Swagger JWT bearer integration
- ✅ 6 test users with different roles pre-configured

---

## 🏗️ **Architecture**

### **Technology Stack:**

- **Backend:** ASP.NET Core 8.0 (C#)
- **Database:** PostgreSQL 16+
- **ORM:** Entity Framework Core 8.0
- **API Documentation:** Swagger/OpenAPI
- **Authentication:** JWT Bearer Tokens
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

### **Current Tables (11 total):**

1. **Users** - User accounts, roles, authentication ✅ **NEW in v0.9.0**
2. **Buildings** - Building records with geometry
3. **PropertyUnits** - Property units within buildings
4. **Persons** - Individual person records
5. **Households** - Household information
6. **PersonPropertyRelations** - Person-property relationships
7. **Claims** - Property ownership claims
8. **Evidences** - Evidence records linked to claims
9. **Documents** - Document attachments
10. **Referrals** - Referral records
11. **\_\_EFMigrationsHistory** - EF Core migrations tracking

### **Users Table Highlights:**

- 43 columns including authentication, profile, audit fields
- 7 performance indexes (Username, Email, Role, etc.)
- Self-referencing foreign key for supervisor relationships
- Supports tablet assignment for field collectors
- Complete audit trail (CreatedBy, CreatedAtUtc, etc.)

---

## 🔐 **Authentication & Security**

### **JWT Token Structure:**

**Access Token Claims:**

- User ID (`NameIdentifier`)
- Username (`Name`)
- Role (`Role`)
- Full Name (Arabic) (`full_name_arabic`)
- Security Stamp (`security_stamp`)
- Mobile/Desktop Access Flags
- Email (if provided)
- Device ID (for audit trail)
- Standard JWT claims (iss, aud, exp, nbf, iat)

**Token Lifetimes:**

- **Production:**
  - Access Token: 15 minutes
  - Refresh Token: 7 days
- **Development:**
  - Access Token: 60 minutes
  - Refresh Token: 30 days

### **Test Users (Development Only):**

| Username    | Password    | Role            | Access Type       |
| ----------- | ----------- | --------------- | ----------------- |
| admin       | Admin@123   | Administrator   | Desktop Only      |
| datamanager | Data@123    | DataManager     | Desktop Only      |
| clerk       | Clerk@123   | OfficeClerk     | Desktop Only      |
| collector   | Field@123   | FieldCollector  | Mobile Only       |
| supervisor  | Super@123   | FieldSupervisor | Desktop Read-Only |
| analyst     | Analyst@123 | Analyst         | Desktop Read-Only |

**⚠️ Note:** These test users are created via the seed endpoint and should only be used in development environments.

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
       "DefaultConnection": "Host=localhost;Database=TRRCMS_Dev;Username=postgres;Password=YOUR_PASSWORD"
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
   dotnet ef database update
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

   This creates 6 test users for development.

7. **Test Authentication**

   In Swagger:

   - Navigate to **Auth** section → **POST /api/Auth/login**
   - Login with: `{"username": "admin", "password": "Admin@123"}`
   - Copy the `accessToken` from the response
   - Click the green "Authorize" button (top-right)
   - Paste the token (without "Bearer" prefix)
   - Click "Authorize" → "Close"
   - Now you can test protected endpoints!

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

### **Seed Endpoints (Development Only):**

| Method | Endpoint          | Description          | Auth Required |
| ------ | ----------------- | -------------------- | ------------- |
| POST   | `/api/Seed/users` | Create test users    | ❌ No         |
| GET    | `/api/Seed/info`  | Get seed information | ❌ No         |

### **Claims Management Endpoints:**

| Method | Endpoint                      | Description      | Auth Required |
| ------ | ----------------------------- | ---------------- | ------------- |
| POST   | `/api/v1/Claims`              | Create new claim | ✅ Yes        |
| GET    | `/api/v1/Claims`              | Get all claims   | ✅ Yes        |
| GET    | `/api/v1/Claims/{id}`         | Get claim by ID  | ✅ Yes        |
| PUT    | `/api/v1/Claims/{id}/submit`  | Submit claim     | ✅ Yes        |
| PUT    | `/api/v1/Claims/{id}/assign`  | Assign claim     | ✅ Yes        |
| PUT    | `/api/v1/Claims/{id}/verify`  | Verify claim     | ✅ Yes        |
| PUT    | `/api/v1/Claims/{id}/approve` | Approve claim    | ✅ Yes        |
| PUT    | `/api/v1/Claims/{id}/reject`  | Reject claim     | ✅ Yes        |

**Note:** Additional endpoints exist for Buildings, PropertyUnits, Persons, Households, etc.

---

## 🧪 **Testing**

### **Manual Testing:**

- ✅ All authentication endpoints tested via Swagger
- ✅ Claims workflow tested (create → submit → assign → verify → approve)
- ✅ Token refresh tested
- ✅ Password change tested
- ✅ Account lockout tested

### **Automated Tests:**

- ⏳ Unit tests (planned)
- ⏳ Integration tests (planned)

---

## 📊 **Project Progress**

### **Completed Tasks (Sprint 1):**

- ✅ **TRRCMS-BE-01** - Core database schema & migrations (3 days)
  - 11 tables with complete relationships
  - Audit trail framework (BaseAuditableEntity)
  - Migration scripts
- ✅ **TRRCMS-BE-02** - Authentication & RBAC (5 days) **← Latest Completion**

  - JWT authentication system
  - Password hashing & security
  - 6 user roles with permissions
  - Account lockout & security features
  - Device tracking for audit compliance
  - 5 authentication endpoints
  - Seed data for testing

- ✅ **Claims Entity Complete** (v0.8.0)
  - Complete claims lifecycle workflow
  - 8 API endpoints tested
  - Status transitions & validation

### **Next Tasks (Sprint 2):**

- ⏳ **TRRCMS-BE-03** - Evidence storage service (3 days)
- ⏳ **TRRCMS-BE-04** - API documentation baseline (4 days)
- ⏳ **TRRCMS-ADM-01** - User & Role management UI (2 days)
- ⏳ **TRRCMS-ADM-02** - User/role APIs (3 days)

### **Overall Progress:**

- Backend API: **~85%** complete
- Database Schema: **~90%** complete
- Authentication: **100%** complete ✅
- CRUD Operations: **~70%** complete
- Admin Features: **~20%** complete

---

## 🔜 **Roadmap**

### **v0.10.0 - User Management UI (Planned)**

- Admin endpoints for user CRUD operations
- User listing with filters
- Role assignment
- Account activation/deactivation

### **v0.11.0 - Evidence Storage (Planned)**

- File upload API
- Evidence metadata management
- Deduplication
- Integration with Claims

### **v1.0.0 - MVP Release (Planned)**

- Complete backend API
- Field survey mobile app (tablet)
- Office/Admin desktop app
- Import/Export functionality
- Conflict resolution workflows

---

## 📝 **Development Notes**

### **Naming Conventions:**

- **Entities:** PascalCase (e.g., `PropertyUnit`)
- **Database Tables:** PascalCase Plural (e.g., `PropertyUnits`)
- **API Routes:** kebab-case (e.g., `/api/property-units`)
- **Properties:** PascalCase (C# convention)

### **Code Standards:**

- Clean Architecture principles
- CQRS pattern with MediatR
- Repository pattern for data access
- Async/await throughout
- Comprehensive XML documentation

### **Security Best Practices:**

- Never commit JWT secrets to Git
- Use environment variables for production secrets
- Implement HTTPS in production
- Enable token blacklist for enhanced security (optional)
- Regular security audits

---

## 📚 **Documentation**

- **API Documentation:** Available at `/swagger` endpoint
- **FSD:** UN_Habitat_TRRCMS_FSD_v5.docx
- **Use Cases:** UN_Habitat_TRRCMS_Use_Cases_V2.xlsx
- **Delivery Plan:** TRRCMS_Internal_Delivery_Plan.docx
- **ERD:** (Coming soon)

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

## 📧 **Support**

For technical support or questions:

- Tech Lead: [Contact Info]
- Project Manager: [Contact Info]

---

## 🎉 **Change Log**

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

**Status:** Ready for Sprint 2 Development 🚀
