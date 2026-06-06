# TRRCMS — Windows Server 2019 deployment scripts

Helper scripts for the native (IIS + PostgreSQL/PostGIS) deployment described in
[`docs/DEPLOYMENT_WINDOWS_SERVER_2019.md`](../../docs/DEPLOYMENT_WINDOWS_SERVER_2019.md).

Run each from an **elevated** PowerShell prompt. Open each script and edit the
variables at the top (paths, passwords, host name) before running.

## Order

| # | Script | Where | What it does |
|---|--------|-------|--------------|
| 1 | `00-install-prerequisites.ps1` | server | Enables IIS features, checks .NET 8 Hosting Bundle, runs `iisreset`. |
| 2 | `setup-database.ps1` | DB server | Creates `TRRCMS_Prod` + app login, enables PostGIS. |
| 3 | `publish.ps1` | build machine or server (needs SDK) | `dotnet publish -c Release`. |
| 4 | *(manual)* | — | Copy publish output to `C:\inetpub\TRRCMS`; fill in & add `appsettings.Production.json`. |
| 5 | `deploy-iis.ps1` | server | App pool + site, installs the correct `web.config`, file/log permissions, optional self-signed cert, firewall. |

> Step 5 copies `web.config` from this folder into the site automatically, so you
> don't merge it by hand. It sets `hostingModel=outofprocess` (required for the
> 500 MB upload limit to work) + the IIS upload limit + `ASPNETCORE_ENVIRONMENT=Production`.

## Still install manually (own installers)

- **.NET 8 ASP.NET Core Hosting Bundle** — <https://dotnet.microsoft.com/download/dotnet/8.0>
- **PostgreSQL 16 + PostGIS 3.4** — EnterpriseDB installer + Stack Builder
- A **TLS certificate** for the site (CA or Let's Encrypt / win-acme)

## After deploy

```powershell
Invoke-RestMethod https://trrcms.your-domain.org/health   # -> Healthy
```
Log in with `admin` / `Admin@123` and change the password immediately.
