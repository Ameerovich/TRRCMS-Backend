# TRRCMS — Windows Server 2019 Deployment Guide

This guide walks through deploying the TRRCMS backend (ASP.NET Core 8 API +
PostgreSQL 16 / PostGIS 3.4) on a **Windows Server 2019** host as a production
service behind IIS.

> **Why native (not Docker)?** The project's containers are *Linux* images
> (`postgis/postgis:16-3.4-alpine`, `mcr.microsoft.com/dotnet/aspnet:8.0`).
> Windows Server 2019 cannot run Linux containers reliably in production
> (no WSL2 in the Server 2019 GA branch). The supported path on Server 2019 is
> a **native install**: PostgreSQL+PostGIS as a Windows service, and the .NET
> app hosted by **IIS** via the ASP.NET Core Module. If you have a Linux host
> available, use the existing `docker compose` flow instead (see
> `DOCKER_README.md`).

---

## 0. Architecture on the server

```
                 (TLS 443)            (HTTP loopback)
  Internet ───►  IIS / ASP.NET ───►  Kestrel (TRRCMS.WebAPI)  ───►  PostgreSQL 16
  / LAN          Core Module v2       127.0.0.1:<dynamic>             + PostGIS 3.4
                 + cert binding       (out-of-process)                localhost:5432
```

The app:
- auto-applies EF Core migrations on startup (`context.Database.MigrateAsync()`),
- seeds users/permissions/vocabularies/admin hierarchy on first run,
- writes uploaded files to disk under its content root (`wwwroot/uploads`,
  `wwwroot/packages`, `archives`) — these folders need write permission for the
  IIS app-pool identity and must be **backed up / kept off the deploy folder**
  if you redeploy by wiping the folder.

---

## 1. Prerequisites — install on the server (once)

Run PowerShell **as Administrator** for everything in this section.

### 1.1 .NET 8 — ASP.NET Core Hosting Bundle (required for IIS)

Install the **Hosting Bundle** (not just the runtime — it includes the
ASP.NET Core Module v2 for IIS):

- Download: <https://dotnet.microsoft.com/download/dotnet/8.0> → "Hosting Bundle".
- Or via winget: `winget install Microsoft.DotNet.HostingBundle.8`

After install, run `iisreset` (or reboot) so IIS picks up the module.
Verify:
```powershell
dotnet --info                 # should list "Microsoft.AspNetCore.App 8.0.x"
```

### 1.2 IIS

```powershell
# Enable IIS + the bits the ASP.NET Core Module needs
Install-WindowsFeature -Name Web-Server, Web-Mgmt-Console, `
    Web-Common-Http, Web-Static-Content, Web-Default-Doc, `
    Web-Http-Errors, Web-App-Dev, Web-Net-Ext45, Web-Asp-Net45, `
    Web-Websockets, Web-Http-Logging, Web-Filtering, Web-AppInit `
    -IncludeManagementTools
```
(`Web-AppInit` = Application Initialization, needed for the warm-start
`preloadEnabled` setting applied in §5.1.)

### 1.3 PostgreSQL 16 + PostGIS 3.4

1. Install **PostgreSQL 16** (EnterpriseDB installer). Record the `postgres`
   superuser password. Keep port **5432**.
2. In **Stack Builder** (bundled) → Spatial Extensions → install
   **PostGIS 3.4 for PostgreSQL 16**. (Or the standalone bundle:
   <https://postgis.net/windows_downloads/>.)
3. Confirm the service `postgresql-x64-16` is **Running** and set to
   **Automatic** start (`services.msc`).

See `deploy/windows/setup-database.ps1` to create the DB and enable PostGIS
non-interactively.

---

## 2. Database setup

Edit and run `deploy/windows/setup-database.ps1` (creates a dedicated DB + login
and enables PostGIS), **or** do it manually in pgAdmin / `psql`:

```sql
-- as postgres superuser
CREATE DATABASE "TRRCMS_Prod";
CREATE ROLE trrcms_app WITH LOGIN PASSWORD 'STRONG-PASSWORD-HERE';
GRANT ALL PRIVILEGES ON DATABASE "TRRCMS_Prod" TO trrcms_app;

\connect "TRRCMS_Prod"
CREATE EXTENSION IF NOT EXISTS postgis;     -- must be run by a superuser
GRANT ALL ON SCHEMA public TO trrcms_app;
SELECT PostGIS_Version();                    -- verify
```

> EF Core migrations run automatically when the app starts, so you do **not**
> need to run `dotnet ef database update` on the server. The app login
> (`trrcms_app`) must own / have full rights on the `public` schema so migrations
> can create tables.
>
> **Order matters — do this step BEFORE the first app start.** One migration
> (`EnablePostGIS`) issues `CREATE EXTENSION IF NOT EXISTS postgis`. Creating the
> PostGIS extension requires a **superuser**, which the app login is deliberately
> *not*. It only works because you pre-create the extension here as `postgres`:
> once it already exists, the migration's `IF NOT EXISTS` is a harmless no-op the
> app login can run. If you skip this step, the app crashes on first start with a
> "permission denied to create extension postgis" error.

---

## 3. Publish the application

On a build machine (or the server, if it has the .NET 8 **SDK**), from the repo
root run `deploy/windows/publish.ps1`, or manually:

```powershell
dotnet publish src/TRRCMS.WebAPI/TRRCMS.WebAPI.csproj `
    -c Release -o C:\inetpub\trrcms-publish
```

This produces `TRRCMS.WebAPI.dll`, the XML docs file, `Data\administrative_divisions.json`,
and a generated `web.config`. Copy the output to the server deploy folder, e.g.
`C:\inetpub\TRRCMS`.

**Do not** copy `appsettings.Development.json`. Configure production via
`appsettings.Production.json` + environment (next section).

---

## 4. Configuration (`appsettings.Production.json`)

The repo already ships a template at
`src/TRRCMS.WebAPI/appsettings.Production.json`. Fill it in on the server (place
it next to `TRRCMS.WebAPI.dll`). Minimum required values:

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TRRCMS_Prod;Username=trrcms_app;Password=STRONG-PASSWORD-HERE"
  },
  "JwtSettings": {
    "Secret": "<64+ random chars — generate with the snippet below>",
    "Issuer": "TRRCMS.API",
    "Audience": "TRRCMS.Clients",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7,
    "RequireHttpsMetadata": true
  },
  "FileStorage": {
    "LocalPath": "wwwroot/uploads",
    "MaxFileSizeMB": 10
  },
  "ImportPipeline": {
    "MaxUploadSizeMB": 500,
    "ArchiveBasePath": "archives",
    "PackageStoragePath": "wwwroot/packages"
  },
  "Cors": {
    "AllowedOrigins": [ "https://trrcms.your-domain.org" ]
  },
  "AllowedHosts": "trrcms.your-domain.org"
}
```

Generate a strong JWT secret:
```powershell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Max 256 }) -as [byte[]])
```

**Security checklist for production config:**
- `JwtSettings:Secret` — unique, ≥32 chars (use 48–64), never the dev default.
- `JwtSettings:RequireHttpsMetadata` — `true`.
- `Cors:AllowedOrigins` — only your real frontend origin(s). (In non-Development
  the app restricts CORS to this list; in Development it allows all.)
- `AllowedHosts` — your real host name(s), not `*`.
- Connection string uses a **least-privilege app login**, not `postgres`.

> Secrets can also be supplied as **environment variables** instead of in the
> JSON file (double-underscore syntax), e.g.
> `ConnectionStrings__DefaultConnection`, `JwtSettings__Secret`. Set them at
> machine scope or in the IIS app-pool environment so they're not in source
> control.

---

## 5. Host under IIS

### 5.1 Create the site / app pool

```powershell
Import-Module WebAdministration

# App pool: "No Managed Code" — the ASP.NET Core Module hosts .NET itself
New-WebAppPool -Name "TRRCMS"
Set-ItemProperty IIS:\AppPools\TRRCMS managedRuntimeVersion ""
Set-ItemProperty IIS:\AppPools\TRRCMS startMode "AlwaysRunning"   # avoid cold start
Set-ItemProperty IIS:\AppPools\TRRCMS processModel.idleTimeout "00:00:00"

# Site bound to HTTPS
New-Website -Name "TRRCMS" -PhysicalPath "C:\inetpub\TRRCMS" `
    -ApplicationPool "TRRCMS" -Port 443 -Protocol https `
    -HostHeader "trrcms.your-domain.org"
```

Set the app pool to start automatically and set
`Set-ItemProperty IIS:\Sites\TRRCMS -Name applicationDefaults.preloadEnabled -Value $true`
for warm starts.

> **Single worker — do not enable a web garden.** Leave the app pool at
> `processModel.maxProcesses = 1` (the default). The app auto-applies EF Core
> migrations on startup and keeps in-memory state (the rate limiter and the
> vocabulary cache). Two+ worker processes would race on migrations and hold
> divergent caches. This is a single-instance deployment; scaling out to multiple
> workers/servers would require externalizing those (distributed cache, run
> migrations as a separate step) and is out of scope for this Server 2019 setup.

### 5.2 TLS certificate

Bind a real certificate (from your CA / Let's Encrypt via win-acme) to the site
on 443. For a quick internal test you can create a self-signed cert:
```powershell
$cert = New-SelfSignedCertificate -DnsName "trrcms.your-domain.org" -CertStoreLocation "cert:\LocalMachine\My"
New-Item -Path IIS:\SslBindings\0.0.0.0!443 -Value $cert
```

### 5.3 web.config — hosting model, large uploads, environment

`dotnet publish` generates a `web.config`. The reference file at
**`deploy/windows/web.config`** replaces it — **`deploy-iis.ps1` copies it into
the site for you** (so you normally don't edit this by hand). It's reproduced
here so you understand what's being set; three settings matter:

```xml
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <security>
        <requestFiltering>
          <!-- 600 MB — IIS request filter; default ~28.6 MB rejects big .uhc with 404.13 -->
          <requestLimits maxAllowedContentLength="629145600" />
        </requestFiltering>
      </security>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\TRRCMS.WebAPI.dll"
                  hostingModel="outofprocess" requestTimeout="00:20:00"
                  stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

- **`hostingModel="outofprocess"` is required, not optional.** The app sets its
  500 MB upload limit on **Kestrel** (`Program.cs` → `ConfigureKestrel`). That
  limit only applies when Kestrel handles the request — i.e. out-of-process,
  where IIS reverse-proxies to Kestrel. With the default `inprocess` model
  Kestrel is bypassed and the limit falls back to ~28.6 MB
  (`IISServerOptions.MaxRequestBodySize`, never set in code), which would reject
  `.uhc` imports above ~28.6 MB. Out-of-process keeps the 500 MB code limit valid
  **without any code change**.
- **`maxAllowedContentLength="629145600"`** raises IIS's own request filter
  (default ~28.6 MB → HTTP 404.13) above the 500 MB app limit.
- **`ASPNETCORE_ENVIRONMENT=Production`** activates `appsettings.Production.json`,
  restricted CORS, HTTPS-metadata enforcement, and disables Swagger.
- `requestTimeout="00:20:00"` stops the ANCM default 2-minute timeout from
  killing a slow 500 MB upload (out-of-process only).

### 5.4 File-write permissions

The app writes uploads/packages/archives to disk. Grant the app-pool identity
(`IIS AppPool\TRRCMS`) Modify rights on those folders:

```powershell
$acl = "C:\inetpub\TRRCMS"
icacls "$acl\wwwroot\uploads"  /grant "IIS AppPool\TRRCMS:(OI)(CI)M" /T
icacls "$acl\wwwroot\packages" /grant "IIS AppPool\TRRCMS:(OI)(CI)M" /T
icacls "$acl\archives"         /grant "IIS AppPool\TRRCMS:(OI)(CI)M" /T
```
Create the folders first if the publish output doesn't include them.

> **Redeploy warning:** these three folders hold live user data (extracted
> building/ID documents, evidence, survey uploads, imported packages, archives).
> If you redeploy by deleting the site folder, **move them out first** or host
> them on a path outside the publish folder and update the config paths. This is
> the Windows equivalent of the Docker named-volume mounts in `docker-compose.yml`.

---

## 6. Firewall

```powershell
New-NetFirewallRule -DisplayName "TRRCMS HTTPS" -Direction Inbound `
    -Protocol TCP -LocalPort 443 -Action Allow
# Keep PostgreSQL (5432) closed to the outside — app talks to it on localhost.
```

> **HTTP / port 80:** the site binds only **443**. There is no port-80 binding,
> so plain `http://` requests get nothing — clients (the frontend, mobile sync,
> Postman) must use `https://`. This is intentional and why no URL Rewrite
> HTTP→HTTPS rule is needed. If you later want port 80 to *redirect* to 443, add
> an 80 binding plus a URL Rewrite rule; it is not required for the API to work.

---

## 7. Verify the deployment

```powershell
# Health check (DB connectivity included)
Invoke-RestMethod https://trrcms.your-domain.org/health   # -> "Healthy"
```

Then log in to confirm seeding ran (default admin is created on first start):
```
POST https://trrcms.your-domain.org/api/auth/login
{ "username": "admin", "password": "Admin@123" }
```
**Change the admin password immediately** after first login (the app enforces a
must-change-password flow).

Swagger UI is **disabled outside Development** by design. To inspect the API,
use the Postman collections in `postman/` or temporarily set the environment to
Development on a non-public box.

---

## 8. Operations

- **Logs:** stdout logging is off by default. For startup troubleshooting set
  `stdoutLogEnabled="true"` in `web.config` (turn it back off after) — output
  goes to `logs\stdout*.log`. `deploy-iis.ps1` already creates the `logs` folder
  with write permission for the app pool. App logs also go to the Windows Event
  Log. The single most useful startup check is the `/health` endpoint: it returns
  `Unhealthy` (not 500) when the DB/PostGIS connection is the problem.
- **Updates / redeploy:** publish to a staging folder, stop the site, copy over
  (preserving `appsettings.Production.json` and the data folders), restart.
  Migrations apply automatically on the next start.
- **Backups:** schedule `pg_dump` of `TRRCMS_Prod` **and** file-system backups of
  `wwwroot/uploads`, `wwwroot/packages`, `archives`.
- **Restart:** `Restart-WebAppPool -Name TRRCMS` or `iisreset`.

---

## 9. Scripts in this repo

Under `deploy/windows/`:

| Script | Purpose |
|--------|---------|
| `00-install-prerequisites.ps1` | Enable IIS features, check for .NET Hosting Bundle. |
| `setup-database.ps1` | Create `TRRCMS_Prod`, app login, enable PostGIS. |
| `publish.ps1` | `dotnet publish` the WebAPI in Release. |
| `deploy-iis.ps1` | Create app pool + site, install `web.config`, set file/log permissions, bind cert, firewall. |
| `web.config` | Reference `web.config` (out-of-process + 500 MB upload limit + Production env); copied into the site by `deploy-iis.ps1`. |

Run each from an **elevated** PowerShell prompt. Read the comments at the top of
each script and edit the variables (paths, passwords, host name) before running.
