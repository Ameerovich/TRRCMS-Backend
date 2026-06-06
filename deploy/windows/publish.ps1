<#
.SYNOPSIS
    Publishes TRRCMS.WebAPI in Release for deployment.
.DESCRIPTION
    Requires the .NET 8 SDK (build machine OR the server). Output is a
    framework-dependent publish (the server has the ASP.NET Core runtime via the
    Hosting Bundle, so no self-contained runtime is bundled).
.EXAMPLE
    .\publish.ps1 -OutputPath C:\inetpub\trrcms-publish
#>
param(
    [string]$OutputPath = "C:\inetpub\trrcms-publish",
    [string]$Configuration = "Release"
)
$ErrorActionPreference = 'Stop'

# Resolve repo root (two levels up from deploy/windows).
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$project  = Join-Path $repoRoot "src\TRRCMS.WebAPI\TRRCMS.WebAPI.csproj"

if (-not (Test-Path $project)) { throw "Project not found at $project" }
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw "dotnet SDK not found on PATH." }

Write-Host "==> Publishing $project ($Configuration) -> $OutputPath" -ForegroundColor Cyan
dotnet publish $project -c $Configuration -o $OutputPath
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed (exit $LASTEXITCODE)." }

# Pre-create the runtime data folders so permissions can be granted before first run.
foreach ($d in @("wwwroot\uploads", "wwwroot\packages", "archives")) {
    $full = Join-Path $OutputPath $d
    if (-not (Test-Path $full)) { New-Item -ItemType Directory -Force -Path $full | Out-Null }
}

Write-Host "Publish complete." -ForegroundColor Green
Write-Host "Next: copy '$OutputPath' to the server deploy folder, drop in" -ForegroundColor Cyan
Write-Host "appsettings.Production.json, then run deploy-iis.ps1." -ForegroundColor Cyan
