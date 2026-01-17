# TRRCMS Backend - Team Quick Start Guide

#### Step 1: Install Docker

**Windows/Mac:** https://www.docker.com/products/docker-desktop  
**Older Systems:** https://github.com/docker/toolbox/releases

#### Step 2: Clone & Run

```bash
git clone https://github.com/Ameerovich/TRRCMS-Backend.git
cd TRRCMS-Backend
docker compose up --build
```

#### Step 3: Test

Open: http://localhost:8080/swagger

Login:

- Username: `admin`
- Password: `Admin@123`

**Done!** ✅

---

## 📋 Daily Development Workflow

### Morning (Pull Latest Changes)

```bash
git pull origin main
docker compose up --build
```

### During Development (After Pulling Updates)

```bash
# Restart containers to get latest code
docker compose restart
```

### Evening (Before Going Home)

```bash
# Stop containers
docker compose down
```

---

## 🔧 Common Tasks

### Restart After Code Changes

```bash
docker compose restart api
```

### Fresh Database (Delete All Data)

```bash
docker compose down -v
docker compose up --build
```

### View Logs

```bash
docker compose logs -f api
```

### Check if Everything is Running

```bash
docker compose ps
```

Should show:

```
trrcms-api    Up (healthy)
trrcms-db     Up (healthy)
```

---

## 🐛 Problems? Quick Fixes

### "Port 8080 already in use"

Someone else is using port 8080. Either:

1. Stop other services
2. Or change port in `docker-compose.yml`:
   ```yaml
   api:
     ports:
       - "8081:8080" # Use 8081
   ```

### "Cannot connect to database"

```bash
docker compose down
docker compose up
```

### "Docker is not running"

- Check Docker Desktop is open (system tray)
- Green icon = good
- Red icon = start Docker Desktop

### Containers Keep Crashing

```bash
# Nuclear option - fresh start
docker compose down -v
docker system prune -f
docker compose up --build
```

---

## 📱 For Docker Toolbox Users

If using Docker Toolbox (older systems):

- Access at: `http://192.168.99.100:8080/swagger`
- Not `localhost`!

---

## 🎯 Testing Checklist

After starting Docker:

- [ ] Swagger loads at http://localhost:8080/swagger
- [ ] Health endpoint returns `Healthy` at /health
- [ ] Can login with admin/Admin@123
- [ ] Can see Buildings endpoint
- [ ] Can create a test building

All checked? You're good to go! ✅
