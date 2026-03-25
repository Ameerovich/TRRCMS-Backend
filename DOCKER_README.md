# TRRCMS Docker Deployment Guide

## Quick Start

```bash
# Clone the repository
git clone https://github.com/Ameerovich/TRRCMS.git
cd TRRCMS

# Start containers
docker compose up --build
```

Wait 2-5 minutes for first build.

---

## Access Points

| Service | URL |
|---------|-----|
| **Swagger UI** | http://localhost:8080/swagger |
| **API Base** | http://localhost:8080/api/v1 |
| **Database** | localhost:5432 |

---

## What's Included

| Container | Image | Port |
|-----------|-------|------|
| trrcms-api | Custom .NET 8 | 8080 |
| trrcms-db | postgis/postgis:16-3.4-alpine | 5432 |

PostGIS is included. Spatial queries work out of the box.

---

## Database Connection

| Setting | Value |
|---------|-------|
| Host | `localhost` (from host) or `db` (from container) |
| Port | `5432` |
| Database | `TRRCMS_Dev` |
| Username | `postgres` |
| Password | Set via `DB_PASSWORD` env var (default: `ChangeThisPassword`) |

### Connect with pgAdmin:
- Host: `localhost`
- Port: `5432`
- Username: `postgres`
- Password: Your `DB_PASSWORD` value

### Changing the Database Password

Set the `DB_PASSWORD` environment variable before starting containers:

```bash
# Linux/Mac
export DB_PASSWORD=YourSecurePassword
docker compose up --build

# Windows (PowerShell)
$env:DB_PASSWORD="YourSecurePassword"
docker compose up --build

# Or use a .env file in the project root:
echo DB_PASSWORD=YourSecurePassword > .env
docker compose up --build
```

The password is used automatically in both the database and API connection string.

---

## Useful Commands

```bash
# Start containers (first time / after changes)
docker compose up --build

# Start in background
docker compose up -d

# View logs
docker compose logs -f

# View API logs only
docker compose logs -f api

# View database logs only
docker compose logs -f db

# Stop containers
docker compose down

# Stop and remove volumes (fresh start)
docker compose down -v

# Restart containers
docker compose restart

# Check container status
docker ps

# Execute command in API container
docker exec -it trrcms-api bash

# Execute command in DB container
docker exec -it trrcms-db psql -U postgres -d TRRCMS_Dev
```

---

## Verify PostGIS

```bash
docker exec -it trrcms-db psql -U postgres -d TRRCMS_Dev -c "SELECT PostGIS_Version();"
```

Expected output:
```
         postgis_version          
----------------------------------
 3.4 USE_GEOS=1 USE_PROJ=1 USE_STATS=1
```

---

## Test the API

### 1. Check Health
```bash
curl http://localhost:8080/health
```

### 2. Login
```bash
curl -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "Admin@123"}'
```

### 3. Get Buildings (with token)
```bash
curl http://localhost:8080/api/v1/buildings \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## Troubleshooting

### Container won't start
```bash
# Check logs
docker compose logs -f

# Rebuild from scratch
docker compose down -v
docker compose up --build
```

### Database connection error
- Wait 30-60 seconds for database to initialize
- Check if db container is healthy: `docker ps`

### Port already in use
```bash
# Check what's using the port
netstat -ano | findstr :8080
netstat -ano | findstr :5432

# Stop local PostgreSQL service if running
# Or change ports in docker-compose.yml
```

### Migrations not applied
```bash
# Check API logs for migration errors
docker compose logs api

# Manually run migrations (if needed)
docker exec -it trrcms-api dotnet ef database update
```

### Permission denied errors
```bash
# Reset volumes
docker compose down -v
docker compose up --build
```

---

## Development vs Production

This setup is for **Development**. For production:

1. Change `ASPNETCORE_ENVIRONMENT` to `Production`
2. Use strong passwords
3. Use proper SSL certificates
4. Don't expose database port (5432) externally
5. Use Docker secrets for sensitive data

---

## File Structure

```
TRRCMS/
├── docker-compose.yml    # Container orchestration
├── Dockerfile            # API image build
├── .dockerignore         # Files to exclude from build
├── src/
│   ├── TRRCMS.Domain/
│   ├── TRRCMS.Application/
│   ├── TRRCMS.Infrastructure/
│   └── TRRCMS.WebAPI/
└── TRRCMS.sln
```

---

## Team Notes

- **First build** takes 3-10 minutes (downloading images + building)
- **Subsequent builds** take 1-3 minutes
- Database data persists in `trrcms-postgres-data` volume
- To reset database: `docker compose down -v`
