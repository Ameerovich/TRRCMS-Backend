<#
.SYNOPSIS
    Creates the IIS app pool + site for TRRCMS, grants file permissions, and
    (optionally) binds a self-signed cert for internal testing.
.DESCRIPTION
    Run from an ELEVATED PowerShell prompt on the server AFTER copying the
    publish output and appsettings.Production.json into $SitePath.
    EDIT the variables below before running.
#>
#Requires -RunAsAdministrator
$ErrorActionPreference = 'Stop'
Import-Module WebAdministration

# ---- EDIT THESE ------------------------------------------------------------
$SiteName    = "TRRCMS"
$AppPoolName = "TRRCMS"
$SitePath    = "C:\inetpub\TRRCMS"               # where the publish output lives
$HostHeader  = "trrcms.your-domain.org"
$HttpsPort   = 443
$CreateSelfSignedCert = $false                   # $true for internal test only
# ---------------------------------------------------------------------------

if (-not (Test-Path (Join-Path $SitePath "TRRCMS.WebAPI.dll"))) {
    throw "TRRCMS.WebAPI.dll not found in $SitePath. Copy the publish output there first."
}
if (-not (Test-Path (Join-Path $SitePath "appsettings.Production.json"))) {
    Write-Warning "appsettings.Production.json not found in $SitePath — the app will start with default config."
}

# 1. App pool: "No Managed Code" (ASP.NET Core Module hosts .NET itself).
if (Test-Path "IIS:\AppPools\$AppPoolName") {
    Write-Host "App pool '$AppPoolName' already exists." -ForegroundColor Yellow
} else {
    New-WebAppPool -Name $AppPoolName | Out-Null
}
Set-ItemProperty "IIS:\AppPools\$AppPoolName" managedRuntimeVersion ""
Set-ItemProperty "IIS:\AppPools\$AppPoolName" startMode "AlwaysRunning"
Set-ItemProperty "IIS:\AppPools\$AppPoolName" processModel.idleTimeout "00:00:00"

# 2. Site bound to HTTPS.
if (Test-Path "IIS:\Sites\$SiteName") {
    Write-Host "Site '$SiteName' already exists." -ForegroundColor Yellow
} else {
    New-Website -Name $SiteName -PhysicalPath $SitePath -ApplicationPool $AppPoolName `
        -Port $HttpsPort -Protocol https -HostHeader $HostHeader | Out-Null
}
Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationDefaults.preloadEnabled -Value $true

# 3. File-write permissions for the app-pool identity on data folders.
$id = "IIS AppPool\$AppPoolName"
foreach ($d in @("wwwroot\uploads", "wwwroot\packages", "archives")) {
    $full = Join-Path $SitePath $d
    if (-not (Test-Path $full)) { New-Item -ItemType Directory -Force -Path $full | Out-Null }
    icacls $full /grant "${id}:(OI)(CI)M" /T | Out-Null
    Write-Host "Granted Modify to '$id' on $full" -ForegroundColor Green
}

# 3b. Install the corrected web.config (out-of-process + 500 MB upload limit +
#     ASPNETCORE_ENVIRONMENT=Production) over the one publish generated, and
#     pre-create the ANCM stdout log folder.
$refWebConfig = Join-Path $PSScriptRoot "web.config"
if (Test-Path $refWebConfig) {
    Copy-Item $refWebConfig (Join-Path $SitePath "web.config") -Force
    Write-Host "Installed deploy/windows/web.config -> $SitePath\web.config" -ForegroundColor Green
} else {
    Write-Warning "Reference web.config not found next to this script. Ensure the site's web.config uses hostingModel=outofprocess and maxAllowedContentLength=629145600 (see docs §5.3)."
}
$logsDir = Join-Path $SitePath "logs"
if (-not (Test-Path $logsDir)) { New-Item -ItemType Directory -Force -Path $logsDir | Out-Null }
icacls $logsDir /grant "${id}:(OI)(CI)M" /T | Out-Null

# 4. (Optional) self-signed cert for internal testing.
if ($CreateSelfSignedCert) {
    Write-Host "==> Creating self-signed cert for $HostHeader (TEST ONLY)..." -ForegroundColor Yellow
    $cert = New-SelfSignedCertificate -DnsName $HostHeader -CertStoreLocation "cert:\LocalMachine\My"
    $binding = "IIS:\SslBindings\0.0.0.0!$HttpsPort"
    if (Test-Path $binding) { Remove-Item $binding }
    New-Item -Path $binding -Value $cert | Out-Null
    Write-Host "Self-signed cert bound. Replace with a real CA cert for production." -ForegroundColor Yellow
} else {
    Write-Host "Skipping cert creation. Bind your CA/Let's Encrypt cert to the site on port $HttpsPort." -ForegroundColor Cyan
}

# 5. Firewall for HTTPS.
if (-not (Get-NetFirewallRule -DisplayName "TRRCMS HTTPS" -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName "TRRCMS HTTPS" -Direction Inbound -Protocol TCP -LocalPort $HttpsPort -Action Allow | Out-Null
}

Restart-WebAppPool -Name $AppPoolName
Write-Host "Deployed. Verify:  Invoke-RestMethod https://$HostHeader/health" -ForegroundColor Green
Write-Host "Reminder: the DB + PostGIS extension must already exist (run setup-database.ps1 first), and appsettings.Production.json must be filled in." -ForegroundColor Cyan
