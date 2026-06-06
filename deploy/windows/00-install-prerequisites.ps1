<#
.SYNOPSIS
    Installs / verifies Windows Server 2019 prerequisites for TRRCMS.
.DESCRIPTION
    - Enables the IIS features required by the ASP.NET Core Module.
    - Verifies the .NET 8 ASP.NET Core Hosting Bundle is installed.
    Run from an ELEVATED PowerShell prompt.
.NOTES
    PostgreSQL + PostGIS is installed via its own installer
    (see docs/DEPLOYMENT_WINDOWS_SERVER_2019.md §1.3).
#>
#Requires -RunAsAdministrator
$ErrorActionPreference = 'Stop'

Write-Host "==> Enabling IIS features..." -ForegroundColor Cyan
Install-WindowsFeature -Name `
    Web-Server, Web-Mgmt-Console, Web-Common-Http, Web-Static-Content, `
    Web-Default-Doc, Web-Http-Errors, Web-App-Dev, Web-Net-Ext45, `
    Web-Asp-Net45, Web-Websockets, Web-Http-Logging, Web-Filtering, `
    Web-AppInit `
    -IncludeManagementTools

Write-Host "==> Checking for .NET 8 ASP.NET Core runtime / Hosting Bundle..." -ForegroundColor Cyan
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Warning "dotnet not found on PATH. Install the .NET 8 ASP.NET Core HOSTING BUNDLE:"
    Write-Warning "  https://dotnet.microsoft.com/download/dotnet/8.0  (Hosting Bundle)"
} else {
    $aspnet = & dotnet --list-runtimes | Select-String 'Microsoft.AspNetCore.App 8\.'
    if ($aspnet) {
        Write-Host "    Found: $($aspnet -join '; ')" -ForegroundColor Green
    } else {
        Write-Warning "ASP.NET Core 8 runtime not found. Install the .NET 8 HOSTING BUNDLE (not just the SDK/runtime)."
    }
}

Write-Host "==> Restarting IIS so the ASP.NET Core Module is loaded..." -ForegroundColor Cyan
iisreset | Out-Host

Write-Host "Done. Next: setup-database.ps1" -ForegroundColor Green
