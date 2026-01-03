# 🚀 TRRCMS - Team Setup Guide

## Prerequisites

Before you start, make sure you have:
- [ ] .NET 8 SDK installed - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- [ ] Visual Studio 2022 (or VS Code with C# extension)
- [ ] PostgreSQL 16+ installed - [Download](https://www.postgresql.org/download/)
- [ ] Git installed - [Download](https://git-scm.com/)

---

## 📦 Step 1: Clone Repository
```bash
git clone https://github.com/Ameerovich/TRRCMS.git
cd TRRCMS
```


---

## 🗄️ Step 2: Setup PostgreSQL Database

### 2.1 Install PostgreSQL
- Download PostgreSQL 16 from the link above
- During installation, **remember your `postgres` user password**
- Accept default port: **5432**

### 2.2 Create Database
1. Open **pgAdmin 4** (installed with PostgreSQL)
2. Connect to **PostgreSQL 16** server
3. Right-click **Databases** → **Create** → **Database**
4. **Name:** `TRRCMS_Dev`
5. **Owner:** `postgres`
6. Click **Save**

✅ You should now see `TRRCMS_Dev` in the database list.

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

⚠️ **IMPORTANT:** Do NOT commit this file! It contains your password and is already in `.gitignore`.

---

## 🔧 Step 4: Restore Packages & Build

### Using Visual Studio:
1. Open `TRRCMS.sln` (double-click the solution file)
2. Wait for Visual Studio to load
3. Right-click **Solution 'TRRCMS'** → **Restore NuGet Packages**
4. **Build** → **Rebuild Solution** (or press Ctrl + Shift + B)
5. ✅ Should see: "Rebuild All succeeded"

### Using Command Line:
```bash
dotnet restore
dotnet build
```

---

## 🗄️ Step 5: Run Database Migrations

This creates the database tables from the code.

### Using Visual Studio:
1. **Tools** → **NuGet Package Manager** → **Package Manager Console**
2. In the dropdown, select **Default project: TRRCMS.Infrastructure**
3. Run this command:
```powershell
Update-Database -StartupProject TRRCMS.WebAPI
```

### Using Command Line:
```bash
cd src/TRRCMS.WebAPI
dotnet ef database update --project ../TRRCMS.Infrastructure
```

✅ **Expected output:** 
```
Applying migration '20260102233937_InitialCreate'.
Done.
```

---

## 🚀 Step 6: Run the Application

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

## ✅ Step 7: Test the API (Verify Everything Works!)

In the **Swagger UI** page:

### Test 1: Create a Building

1. **Click** on **POST /api/v1/Buildings** (green bar)
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

4. **Click** "Execute"
5. ✅ **Should see:** `201 Created` response with a GUID like `"7e439aab-5dd1-4a8a-b6c4-265008e53b86"`

### Test 2: Get All Buildings

1. **Click** on **GET /api/v1/Buildings** (blue bar)
2. **Click** "Try it out"
3. **Click** "Execute"
4. ✅ **Should see:** `200 OK` with an array containing the building you just created

**If both tests pass, your setup is complete!** 🎉

---

## 🆘 Troubleshooting

### Problem: "Cannot connect to database"
**Solutions:**
- Check PostgreSQL service is running (search "Services" in Windows, find "postgresql-x64-16")
- Verify your password in `appsettings.Development.json` is correct
- Ensure database `TRRCMS_Dev` exists in pgAdmin
- Check connection string format is exactly: `Host=localhost;Database=TRRCMS_Dev;Username=postgres;Password=YourPassword`

### Problem: "No such table: Buildings"
**Solution:**
- Run migrations again: `Update-Database` in Package Manager Console
- Or via CLI: `dotnet ef database update --project ../TRRCMS.Infrastructure`

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



**Happy Coding!** 🚀