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
- [Team](#team)
- [License](#license)

---

## 🎯 Overview

TRRCMS is designed to help UN-Habitat document and verify property rights in post-conflict Aleppo. The system supports:

- **Building & Property Unit Registration** - Comprehensive cadastral data management
- **Person & Household Records** - Displaced persons and household tracking
- **Ownership Claims** - Documentation and verification of property claims
- **Field Surveys** - Mobile data collection for on-site verification
- **Document Management** - Secure storage of legal documents and evidence
- **Conflict Resolution** - Tracking disputed claims and resolution processes

---

## ✨ Features

### Current (v0.1 - MVP)
✅ **Clean Architecture** - Domain-driven design with clear separation of concerns  
✅ **Building CRUD** - Complete Create, Read, Update, Delete operations  
✅ **PostgreSQL Database** - Robust relational data storage  
✅ **Entity Framework Core** - Code-first migrations and LINQ queries  
✅ **Swagger/OpenAPI** - Interactive API documentation  
✅ **Arabic Support** - Full UTF-8 encoding for Arabic text  
✅ **Audit Trails** - Automatic tracking of created/modified timestamps  

### Planned
🚧 **Authentication & Authorization** - JWT-based security  
🚧 **Property Unit Management** - Apartment/shop registration  
🚧 **Person & Household Registry** - Displaced persons tracking  
🚧 **Claims Workflow** - Submission, review, verification, and resolution  
🚧 **Document Upload** - PDF/image attachment system  
🚧 **Search & Filtering** - Advanced queries across entities  
🚧 **Reporting** - Statistical reports and data export  

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

### Architecture
- **Clean Architecture** - Independent of frameworks, UI, and databases
- **Domain-Driven Design** - Rich domain models
- **CQRS** - Command Query Responsibility Segregation
- **Repository Pattern** - Data access abstraction

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
# Copy appsettings.example.json to appsettings.Development.json
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
│   │   ├── Entities/               # Domain entities (Building, Person, etc.)
│   │   ├── Enums/                  # Domain enumerations
│   │   └── Common/                 # Base classes and interfaces
│   │
│   ├── TRRCMS.Application/         # Application business rules
│   │   ├── Buildings/              # Building use cases
│   │   │   ├── Commands/           # Write operations (Create, Update, Delete)
│   │   │   ├── Queries/            # Read operations (Get, List)
│   │   │   └── Dtos/               # Data transfer objects
│   │   └── Common/                 # Shared application logic
│   │       ├── Interfaces/         # Repository interfaces
│   │       └── Mappings/           # AutoMapper profiles
│   │
│   ├── TRRCMS.Infrastructure/      # External concerns
│   │   └── Persistence/
│   │       ├── ApplicationDbContext.cs
│   │       ├── Configurations/     # EF Core entity configurations
│   │       ├── Repositories/       # Repository implementations
│   │       └── Migrations/         # Database migrations
│   │
│   └── TRRCMS.WebAPI/              # API layer
│       ├── Controllers/            # API endpoints
│       ├── Program.cs              # Application entry point
│       └── appsettings.json        # Configuration
│
├── docs/                           # Documentation
│   ├── TRRCMS_Analysis_NextSteps.md    # Full project analysis
│   └── TRRCMS_HowToExtend.md           # Development guide
│
├── SETUP_GUIDE.md                  # Team onboarding guide
├── .gitignore                      # Git ignore rules
└── README.md                       # This file
```

---

## 📚 API Documentation

### Endpoints (v0.1)

#### Buildings
- `POST /api/v1/Buildings` - Create new building
- `GET /api/v1/Buildings` - Get all buildings
- `GET /api/v1/Buildings/{id}` - Get building by ID

### Example Request
```json
POST /api/v1/Buildings
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
  "longitude": 37.1343
}
```

### Interactive Documentation
Start the application and navigate to: `https://localhost:7204/swagger`


---


### Git Workflow
```bash
# Create feature branch
git checkout -b feature/property-unit-crud

# Make changes, commit
git add .
git commit -m "feat: Add PropertyUnit CRUD endpoints"

# Push and create Pull Request
git push origin feature/property-unit-crud
```

### Branch Naming
- `feature/entity-name` - New features
- `fix/bug-description` - Bug fixes
- `docs/what-changed` - Documentation updates

---

## 👥 Team

**Project:** UN-Habitat Aleppo Tenure Rights Registration  
**Organization:** United Nations Human Settlements Programme  
**Location:** Aleppo, Syria

### Contributors
- Ameer Yousef

---

## 📄 License

This project is developed for UN-Habitat. All rights reserved.

---

## 🤝 Contributing

1. Follow the [Setup Guide](./SETUP_GUIDE.md)
2. Pick an entity from the roadmap
3. Create a feature branch
4. Implement following the established pattern
5. Test thoroughly in Swagger
6. Submit Pull Request

---

## 📞 Support

- **Issues:** Use GitHub Issues for bug reports
- **Questions:** Ask in team chat
- **Documentation:** Check the `/docs` folder
