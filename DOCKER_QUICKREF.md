# Docker Quick Reference

## Files Created

```
NotiBlock/
├── docker-compose.yml          # Orchestration config (PostgreSQL, backend, frontend)
├── .env.example                # Environment template (copy to .env before running)
├── .dockerignore               # Files excluded from Docker builds
├── init-db.sql                 # Database initialization script
├── DOCKER_SETUP.md             # Comprehensive deployment guide
├── start-docker.sh             # Linux/macOS startup helper script
├── start-docker.ps1            # Windows PowerShell startup helper script
├── backend/
│   └── Dockerfile              # Multi-stage .NET 8 build (SDK → Runtime)
├── frontend/
│   ├── Dockerfile              # Node.js build → Nginx serving (SPA + API proxy)
│   └── nginx.conf              # Nginx configuration (routing, caching, compression)
└── README.md                   # Updated with Docker section
```

## Quick Start (3 Steps)

### Step 1: Configure Environment
```bash
cp .env.example .env
# Edit .env with your settings:
# - DB_PASSWORD: PostgreSQL admin password
# - JWT_KEY: 32+ character random string for token signing
# - BLOCKCHAIN_* settings if using blockchain features
```

### Step 2: Start Services
```bash
# Windows (PowerShell)
.\start-docker.ps1

# Linux/macOS (Bash)
chmod +x start-docker.sh
./start-docker.sh

# Or manually with docker-compose
docker-compose up -d
```

### Step 3: Access Application
- **Frontend**: http://localhost (or http://localhost:3000)
- **Backend API**: http://localhost:5271
- **Database**: localhost:5432

## Common Commands

```bash
# View status of all services
docker-compose ps

# Stream logs from all services
docker-compose logs -f

# Stream logs from specific service
docker-compose logs -f backend

# Stop services (preserve data)
docker-compose down

# Stop and delete all data
docker-compose down -v

# Rebuild images and restart
docker-compose up -d --build

# Database access
docker-compose exec db psql -U notiblock_user -d notiblock_db
```

## Service Architecture

```
┌─────────────────────────────────────────────────┐
│           NotiBlock Docker Stack                │
├─────────────────────────────────────────────────┤
│ Component    │ Port  │ Technology              │
├──────────────┼───────┼────────────────────────┤
│ Frontend     │ 80    │ React 19 + Nginx       │
│ Backend API  │ 5271  │ .NET 8 + Kestrel       │
│ Database     │ 5432  │ PostgreSQL 16 + Data  │
└─────────────────────────────────────────────────┘

Network Communication:
  - Frontend → Backend: http://backend:5271
  - Backend → Database: tcp://db:5432 (internal Docker network)
  - Client access: Published ports (80, 5271, 5432)
```

## Features

✅ **Multi-stage Docker builds** for optimized image sizes
✅ **Health checks** on all services (auto-restart on failure)
✅ **Volume persistence** for PostgreSQL data
✅ **Nginx reverse proxy** with built-in /api/ routing to backend
✅ **Environment-based configuration** via .env file
✅ **Development & Production** ready
✅ **One-command startup** with helper scripts
✅ **Comprehensive documentation** (see DOCKER_SETUP.md)

## Troubleshooting

**Port already in use?**
```bash
# Edit docker-compose.yml and change port mapping
# Example: "8080:80" instead of "80:80"
```

**Frontend can't reach backend?**
```bash
# Verify backend is running and healthy
curl http://localhost:5271/api/auth/health

# Check CORS configuration in backend
docker-compose logs backend | grep CORS
```

**Database connection failed?**
```bash
# Check database health
docker-compose logs db

# Test PostgreSQL connectivity
docker-compose exec db pg_isready -U notiblock_user -d notiblock_db
```

**Full cleanup and restart?**
```bash
docker-compose down -v
docker system prune -a
docker-compose up -d --build
```

## Configuration Reference

### Environment Variables (.env)

| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `DB_PASSWORD` | PostgreSQL password | Yes | - |
| `JWT_KEY` | JWT signing key (32+ chars) | Yes | - |
| `BLOCKCHAIN_RPC_URL` | Polygon RPC endpoint | No | Mumbai testnet |
| `BLOCKCHAIN_PRIVATE_KEY` | Private key for transactions | No | - |
| `BLOCKCHAIN_CONTRACT_ADDRESS` | RecallRegistry address | No | - |
| `CORS_ORIGIN` | Allowed CORS origin | No | http://localhost |
| `ASPNETCORE_ENVIRONMENT` | .NET environment | No | Production |
| `VITE_API_BASE_URL` | Frontend API base URL | No | http://localhost:5271 |

### Exposed Ports

| Service | HTTP | HTTPS | Internal |
|---------|------|-------|----------|
| Frontend | 80 (published) | - | - |
| Backend | 5271 (published) | 7179 (published) | http://backend:5271 |
| Database | - | - | tcp://db:5432 |

## Production Deployment

For production, create `docker-compose.prod.yml`:
- Use environment-specific .env files
- Store secrets in Docker secrets or external vaults
- Scale backend service to multiple replicas
- Add reverse proxy (Traefik/Nginx) for TLS termination
- Enable container resource limits
- Set up centralized logging and monitoring

See DOCKER_SETUP.md for detailed production guidelines.

## Next Steps

1. ✅ Configure `.env` file
2. ✅ Run `docker-compose up -d` or use helper script
3. ✅ Verify services are healthy: `docker-compose ps`
4. ✅ Access http://localhost to see the application
5. ✅ Check logs if issues: `docker-compose logs -f`

For comprehensive documentation, see [DOCKER_SETUP.md](DOCKER_SETUP.md) and [README.md](README.md#docker-setup).
