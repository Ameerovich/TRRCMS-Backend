# TRRCMS - Team Setup Guide

## Prerequisites

Before you start, make sure you have:
- [ ] .NET 8 SDK installed - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- [ ] Visual Studio 2022 (or VS Code with C# extension)
- [ ] PostgreSQL 16+ installed - [Download](https://www.postgresql.org/download/)
- [ ] **PostGIS 3.x extension** for PostgreSQL - [Download](https://postgis.net/windows_downloads/)
- [ ] Git installed - [Download](https://git-scm.com/)

---

## Step 1: Clone Repository
```bash
git clone https://github.com/Ameerovich/TRRCMS.git
cd TRRCMS
```

---

## Step 2: Setup PostgreSQL Database

### 2.1 Install PostgreSQL
- Download PostgreSQL 16 from the link above
- During installation, **remember your `postgres` user password**
- Accept default port: **5432**

### 2.2 Install PostGIS Extension (Required!)

PostGIS adds spatial/geographic capabilities to PostgreSQL.

1. Download **PostGIS 3.x Bundle for PostgreSQL 16** from: https://postgis.net/windows_downloads/
2. Run the installer (`postgis-bundle-pg16-setup.exe`)
3. Select your PostgreSQL 16 installation directory (usually `C:\Program Files\PostgreSQL\16`)
4. Complete installation
5. **Restart PostgreSQL service** (optional but recommended):
   - Press `Win + R` → type `services.msc` → Enter
   - Find **"postgresql-x64-16"**
   - Right-click → **Restart**

### 2.3 Create Database
1. Open **pgAdmin 4** (installed with PostgreSQL)
2. Connect to **PostgreSQL 16** server
3. Right-click **Databases** → **Create** → **Database**
4. **Name:** `TRRCMS_Dev`
5. **Owner:** `postgres`
6. Click **Save**

### 2.4 Enable PostGIS Extension

1. In pgAdmin, click on your **TRRCMS_Dev** database
2. Click **Tools** → **Query Tool**
3. Run this SQL command:
```sql
CREATE EXTENSION IF NOT EXISTS postgis;
```
4. Verify installation:
```sql
SELECT PostGIS_Version();
```
5. **Expected output:** `3.6 USE_GEOS=1 USE_PROJ=1 USE_STATS=1` (or similar version)

---

## ⚙️ Step 3: Configure Connection String

### 3.1 Create Your Local Configuration

1. Navigate to: `src/TRRCMS.WebAPI/`
2. **Copy** the file `appsettings.example.json`
3. **Paste** and rename to: `appsettings.Development.json`
4. **Open** `appsettings.Development.json` in any text editor
5. **Replace** `YOUR_POSTGRES_PASSWORD` with your actual PostgreSQL password:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=TRRCMS_Dev;Username=postgres;Password=YourActualPassword"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

6. **Save** the file

**IMPORTANT:** Do NOT commit this file! It contains your password and is already in `.gitignore`.

---

## Step 4: Restore Packages & Build

### Using Visual Studio:
1. Open `TRRCMS.sln` (double-click the solution file)
2. Wait for Visual Studio to load
3. Right-click **Solution 'TRRCMS'** → **Restore NuGet Packages**
4. **Build** → **Rebuild Solution** (or press Ctrl + Shift + B)
5. Should see: "Rebuild All succeeded"

### Using Command Line:
```bash
dotnet restore
dotnet build
```

---

## Step 5: Run Database Migrations

This creates the database tables from the code.

**IMPORTANT:** Only run `Update-Database` - do NOT run `Add-Migration`!

### Using Visual Studio:
1. **Tools** → **NuGet Package Manager** → **Package Manager Console**
2. In the dropdown, select **Default project: TRRCMS.Infrastructure**
3. Run this command:
```powershell
Update-Database -StartupProject TRRCMS.WebAPI
```

### Using Command Line:
```bash
dotnet ef database update --project src/TRRCMS.Infrastructure --startup-project src/TRRCMS.WebAPI
```

**Expected output:** 
```
Applying migration '20260102233937_InitialCreate'.
Applying migration '20260127151113_AddBuildingLocationDescription'.
Applying migration '20260128084951_EnablePostGIS'.
Done.
```

---

## Step 6: Run the Application

### Using Visual Studio:
1. Make sure **TRRCMS.WebAPI** is the **Startup Project** (it should be **bold**)
   - If not: Right-click **TRRCMS.WebAPI** → **Set as Startup Project**
2. Press **F5** (or click green ▶️ play button)
3. Browser should automatically open to: `https://localhost:7204/swagger`

### Using Command Line:
```bash
cd src/TRRCMS.WebAPI
dotnet run
```
Then manually open browser to: `https://localhost:7204/swagger`

---

## Step 7: Test the API (Verify Everything Works!)

In the **Swagger UI** page:

### Test 1: Create a Building

1. **Click** on **POST /api/Buildings** (green bar)
2. **Click** "Try it out"
3. **Paste** this test data:
```json
{
  "governorateCode": "01",
  "districtCode": "02",
  "subDistrictCode": "03",
  "communityCode": "001",
  "neighborhoodCode": "002",
  "buildingNumber": "00001",
  "buildingType": 1,
  "buildingStatus": 1,
  "numberOfPropertyUnits": 10,
  "numberOfApartments": 8,
  "numberOfShops": 2,
  "latitude": 36.2021,
  "longitude": 37.1343,
  "buildingGeometryWkt": "POLYGON((37.1340 36.2018, 37.1346 36.2018, 37.1346 36.2024, 37.1340 36.2024, 37.1340 36.2018))",
  "locationDescription": "بجانب المسجد الكبير",
  "notes": "بناء سكني مؤلف من 5 طوابق"
}
```

4. **Click** "Execute"
5. **Should see:** `201 Created` response with full building data including:
   - `id` (GUID) - Use this for other API calls
   - `buildingId` (17 digits) - Stored format
   - `buildingIdFormatted` - Display format with dashes

### Test 2: Get Building by ID

1. **Click** on **GET /api/Buildings/{id}** (blue bar)
2. **Click** "Try it out"
3. **Paste** the `id` (GUID) from Test 1
4. **Click** "Execute"
5. **Should see:** `200 OK` with full building details including `buildingGeometryWkt`

### Test 3: Update Building Geometry

1. **Click** on **PUT /api/Buildings/{id}/geometry** (orange bar)
2. **Click** "Try it out"
3. **Paste** the `id` (GUID) from Test 1
4. **Paste** this request body:
```json
{
  "latitude": 36.2025,
  "longitude": 37.1350,
  "geometryWkt": "POLYGON((37.1345 36.2020, 37.1355 36.2020, 37.1355 36.2030, 37.1345 36.2030, 37.1345 36.2020))"
}
```
5. **Click** "Execute"
6. **Should see:** `200 OK` with updated coordinates and geometry

### Test 4: Verify PostGIS in Database (Optional)

In pgAdmin, run this query:
```sql
SELECT 
    "BuildingId",
    ST_AsText("BuildingGeometry") as geometry_wkt,
    "Latitude",
    "Longitude"
FROM "Buildings"
WHERE "BuildingGeometry" IS NOT NULL;
```
**Should see:** Your building with the polygon geometry

**If all tests pass, your setup is complete!** 🎉

---

## 📋 Building API Field Reference

### Building Types (نوع البناء)
| Value | Enum | Arabic |
|:-----:|------|--------|
| 1 | Residential | سكني |
| 2 | Commercial | تجاري |
| 3 | MixedUse | مختلط |
| 4 | Industrial | صناعي |

### Building Status (حالة البناء)
| Value | Enum | Arabic |
|:-----:|------|--------|
| 1 | Intact | سليم |
| 2 | MinorDamage | أضرار طفيفة |
| 3 | ModerateDamage | أضرار متوسطة |
| 4 | MajorDamage | أضرار كبيرة |
| 5 | SeverelyDamaged | أضرار شديدة |
| 6 | Destroyed | مدمر |
| 7 | UnderConstruction | قيد الإنشاء |
| 8 | Abandoned | مهجور |
| 99 | Unknown | غير معروف |

### Building Code Format (رمز البناء)
- **Stored:** `GGDDSSCCCCNNBBBBB` (17 digits, no dashes)
- **Displayed:** `GG-DD-SS-CCC-NNN-BBBBB` (via `buildingIdFormatted`)

| Segment | Digits | Arabic |
|---------|:------:|--------|
| GG | 2 | محافظة |
| DD | 2 | مدينة |
| SS | 2 | بلدة |
| CCC | 3 | قرية |
| NNN | 3 | حي |
| BBBBB | 5 | رقم البناء |

---

## 🆘 Troubleshooting

### Problem: "Cannot connect to database"
**Solutions:**
- Check PostgreSQL service is running (search "Services" in Windows, find "postgresql-x64-16")
- Verify your password in `appsettings.Development.json` is correct
- Ensure database `TRRCMS_Dev` exists in pgAdmin
- Check connection string format is exactly: `Host=localhost;Database=TRRCMS_Dev;Username=postgres;Password=YourPassword`

### Problem: "extension postgis is not available"
**Solutions:**
- PostGIS is not installed. Download and install from: https://postgis.net/windows_downloads/
- Make sure you download the version matching your PostgreSQL (e.g., pg16 for PostgreSQL 16)
- Restart PostgreSQL service after installing PostGIS

### Problem: "type geometry does not exist"
**Solutions:**
- Enable PostGIS extension in your database:
```sql
CREATE EXTENSION IF NOT EXISTS postgis;
```
- Run this in pgAdmin on your `TRRCMS_Dev` database

### Problem: "No such table: Buildings"
**Solution:**
- Run migrations again: `Update-Database` in Package Manager Console
- Or via CLI: `dotnet ef database update --project src/TRRCMS.Infrastructure --startup-project src/TRRCMS.WebAPI`

### Problem: Build errors / missing packages
**Solutions:**
- Clean solution: **Build** → **Clean Solution**
- Restore packages: Right-click Solution → **Restore NuGet Packages**
- Rebuild: **Build** → **Rebuild Solution**
- If still failing, delete all `bin` and `obj` folders and rebuild

### Problem: "Port already in use"
**Solution:**
- Another instance is running. Stop it from Task Manager or change port in `launchSettings.json`

### Problem: Swagger page doesn't open
**Solution:**
- Manually open browser to: `https://localhost:7204/swagger`
- Or check the console output for the actual URL

### Problem: "NetTopologySuite" or "Geometry" errors
**Solutions:**
- Restore NuGet packages: `dotnet restore`
- Check that `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` package is installed in Infrastructure project

---

## 📁 Project Structure

```
TRRCMS/
├── src/
│   ├── TRRCMS.Domain/           # Entities, Enums, Value Objects
│   ├── TRRCMS.Application/      # Commands, Queries, DTOs, Interfaces
│   ├── TRRCMS.Infrastructure/   # Database, Repositories, Services
│   └── TRRCMS.WebAPI/           # Controllers, API Configuration
├── tests/                       # Unit & Integration Tests
└── TRRCMS.sln                   # Solution file
```

---

## Key Technologies

| Technology | Purpose |
|------------|---------|
| .NET 8 | Backend framework |
| PostgreSQL 16 | Database |
| PostGIS 3.x | Spatial/Geographic queries |
| Entity Framework Core 8 | ORM |
| MediatR | CQRS pattern |
| FluentValidation | Request validation |
| AutoMapper | Object mapping |
| JWT | Authentication |

---

**Happy Coding!**
