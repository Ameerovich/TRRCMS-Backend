<#
.SYNOPSIS
    Creates the TRRCMS production database, a least-privilege app login, and
    enables the PostGIS extension.
.DESCRIPTION
    Uses psql. Prompts ONCE for the postgres superuser password and reuses it for
    all statements via PGPASSWORD. Idempotent: re-running skips objects that
    already exist. EDIT the variables below before running.
    Run from an ELEVATED PowerShell prompt on the DB server.
#>
$ErrorActionPreference = 'Stop'

# ---- EDIT THESE ------------------------------------------------------------
$PsqlPath    = "C:\Program Files\PostgreSQL\16\bin\psql.exe"
$DbName      = "TRRCMS_Prod"
$AppUser     = "trrcms_app"
$AppPassword = "CHANGE-ME-STRONG-PASSWORD"     # used by the app's connection string
$SuperUser   = "postgres"
$DbHost      = "localhost"
$DbPort      = 5432
# ---------------------------------------------------------------------------

if (-not (Test-Path $PsqlPath)) { throw "psql not found at $PsqlPath. Fix `$PsqlPath." }
if ($AppPassword -eq "CHANGE-ME-STRONG-PASSWORD") { throw "Set a real `$AppPassword first." }

# Prompt once; reuse for every psql call via PGPASSWORD so there's a single prompt.
$securePw = Read-Host "Enter the '$SuperUser' PostgreSQL password" -AsSecureString
$bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePw)
$env:PGPASSWORD = [Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)
[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)

try {
    $common = @("-h", $DbHost, "-p", "$DbPort", "-U", $SuperUser, "-v", "ON_ERROR_STOP=1")

    # 1. Create the app login if it doesn't exist.
    $createRole = @"
DO `$`$ BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '$AppUser') THEN
    CREATE ROLE $AppUser WITH LOGIN PASSWORD '$AppPassword';
  END IF;
END `$`$;
"@
    $createRole | & $PsqlPath @common -d postgres -f -
    Write-Host "App login '$AppUser' ensured." -ForegroundColor Green

    # 2. Create the database if missing (CREATE DATABASE can't run inside a DO block).
    $exists = (& $PsqlPath @common -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname = '$DbName'").Trim()
    if ($exists -ne "1") {
        & $PsqlPath @common -d postgres -c "CREATE DATABASE ""$DbName"" OWNER $AppUser"
        Write-Host "Created database '$DbName'." -ForegroundColor Green
    } else {
        Write-Host "Database '$DbName' already exists — skipping create." -ForegroundColor Yellow
    }
    & $PsqlPath @common -d postgres -c "GRANT ALL PRIVILEGES ON DATABASE ""$DbName"" TO $AppUser"

    # 3. Enable PostGIS (must be a superuser) and grant schema rights to the app login.
    #    Pre-creating the extension here is REQUIRED: the EnablePostGIS EF migration
    #    runs CREATE EXTENSION IF NOT EXISTS postgis, which only succeeds for the
    #    non-superuser app login once the extension already exists.
    $dbSql = @"
CREATE EXTENSION IF NOT EXISTS postgis;
GRANT ALL ON SCHEMA public TO $AppUser;
ALTER DATABASE "$DbName" OWNER TO $AppUser;
SELECT PostGIS_Version();
"@
    $dbSql | & $PsqlPath @common -d $DbName -f -
}
finally {
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host "Database '$DbName' ready with PostGIS. App login: $AppUser" -ForegroundColor Green
Write-Host "Connection string for appsettings.Production.json:" -ForegroundColor Cyan
Write-Host "  Host=$DbHost;Port=$DbPort;Database=$DbName;Username=$AppUser;Password=<the password you set>"
