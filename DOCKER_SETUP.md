# NotiBlock Docker Deployment Guide

This guide covers containerization, deployment, and troubleshooting for the NotiBlock application stack.

## Table of Contents
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Environment Configuration](#environment-configuration)
- [Running Services](#running-services)
- [Health Checks](#health-checks)
- [Persistence & Backups](#persistence--backups)
- [Production Deployment](#production-deployment)
- [Troubleshooting](#troubleshooting)

## Quick Start

### Prerequisites
- Docker Engine 20.10+ ([Install](https://docs.docker.com/engine/install/))
- Docker Compose 2.0+ ([Install](https://docs.docker.com/compose/install/))
- Git
- At least 2GB free disk space
- Ports available: 80, 5271, 5432

### One-Command Startup

```bash
# Clone and enter project
git clone https://github.com/Ali-Taherii/NotiBlock.git && cd NotiBlock

# Copy and configure environment file
cp .env.example .env
# Edit .env with your settings (see below)

# Start everything
docker-compose up -d

# Verify all services are running
docker-compose ps

# Check logs
docker-compose logs -f
```

**Access points**:
- Frontend: http://localhost (or http://localhost:3000)
- Backend API: http://localhost:5271/api
- Health check: curl http://localhost:5271/api/auth/health

## Architecture

### Service Stack

```
┌─────────────────────────────────────────────────────┐
│                    NotiBlock Stack                   │
├─────────────────────────────────────────────────────┤
│  Frontend (React + Nginx)                    :80    │
│  - SPA (Single Page Application)                    │
│  - Built-in /api/ proxy to backend                  │
├─────────────────────────────────────────────────────┤
│  Backend (.NET 8 API)                    :5271      │
│  - RESTful API endpoints                            │
│  - JWT authentication                               │
│  - Blockchain integration                           │
├─────────────────────────────────────────────────────┤
│  PostgreSQL Database                     :5432      │
│  - Persistent data storage                          │
│  - Auto-initialization on first run                 │
└─────────────────────────────────────────────────────┘
```

### Network Communication

- **Frontend → Backend**: Http via nginx proxy (`http://backend:5271`)
- **Backend → Database**: TCP/IP via internal Docker network (`db:5432`)
- **Client → Frontend**: HTTP via published port (`:80`)
- **Client → Backend**: HTTP via published port (`:5271`)

## Environment Configuration

### `.env` File Template

Create a `.env` file in the project root with the following variables:

```env
# ====== DATABASE ======
# PostgreSQL admin password (use strong password in production)
DB_PASSWORD=notiblock_secure_password_change_me

# ====== SECURITY ======
# JWT signing key (minimum 32 characters, use random string)
# Example: openssl rand -base64 32
JWT_KEY=your-super-secret-jwt-key-at-least-32-characters-long

# ====== BLOCKCHAIN ======
# Polygon Mumbai testnet RPC endpoint
BLOCKCHAIN_RPC_URL=https://rpc-mumbai.maticvigil.com

# Your private key for signing blockchain transactions (no '0x' prefix)
# Only needed if using blockchain features
BLOCKCHAIN_PRIVATE_KEY=

# Deployed RecallRegistry contract address
BLOCKCHAIN_CONTRACT_ADDRESS=0x0f41904d1F083989B70BD223FCa6feD911002aFD

# ====== API CONFIGURATION ======
# CORS origin allowed by backend
CORS_ORIGIN=http://localhost

# ASP.NET environment (Development/Staging/Production)
ASPNETCORE_ENVIRONMENT=Production

# Backend URL that frontend JavaScript connects to
VITE_API_BASE_URL=http://localhost:5271
```

### Security Best Practices

1. **Never commit `.env` file** — it contains secrets
2. **Use strong passwords**:
   ```bash
   # Generate strong random password
   openssl rand -base64 16
   
   # Generate JWT key
   openssl rand -base64 32
   ```
3. **In production**, use Docker secrets or environment variable injection:
   ```bash
   docker run --secret db_password -e DB_PASSWORD_FILE=/run/secrets/db_password ...
   ```

## Running Services

### Start All Services

```bash
# Start in background (detached mode)
docker-compose up -d

# Or with live logs (foreground)
docker-compose up

# Force rebuild of images before starting
docker-compose up -d --build
```

### Stop Services

```bash
# Stop all services (preserve data)
docker-compose stop

# Stop and remove containers (preserve volumes)
docker-compose down

# Stop, remove containers, and delete data
docker-compose down -v

# Remove unused images and volumes
docker system prune -a
```

### View Logs

```bash
# All services, follow mode
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f db

# Last 100 lines
docker-compose logs --tail=100

# Timestamps
docker-compose logs -f -t
```

### Restart Services

```bash
# Restart all
docker-compose restart

# Restart specific service
docker-compose restart backend

# Rebuild and restart
docker-compose up -d --build backend
```

### Execute Commands in Running Containers

```bash
# Access database CLI
docker-compose exec db psql -U notiblock_user -d notiblock_db

# Run backend health check
docker-compose exec backend curl http://localhost:5271/api/auth/health

# View backend logs file
docker-compose exec backend cat /app/logs/notiblock-$(date +%Y%m%d).txt

# Interactive shell in backend container
docker-compose exec backend /bin/bash
```

## Health Checks

### Automated Health Checks

All services include health checks that are automatically performed by Docker:

```bash
# View health status
docker-compose ps

# Example output:
# NAME                STATUS
# notiblock-api       Up 2 minutes (healthy)
# notiblock-web       Up 2 minutes (healthy)
# notiblock-db        Up 2 minutes (healthy)
```

### Manual Health Verification

```bash
# Database health
docker-compose exec db pg_isready -U notiblock_user -d notiblock_db

# Backend health
curl http://localhost:5271/api/auth/health

# Frontend health
curl -I http://localhost/

# Full backend status
curl -v http://localhost:5271/api/auth/me \
  -H "Content-Type: application/json"
```

### Common Health Issues

| Service | Issue | Check |
|---------|-------|-------|
| Database | Connection timeout | `docker-compose logs db` |
| Backend | API unavailable | `curl http://localhost:5271/api/auth/health` |
| Frontend | Can't load | `docker-compose logs frontend` |
| Network | Services can't reach each other | Check docker network: `docker network ls` |

## Persistence & Backups

### Data Volumes

Data is stored in Docker volumes for persistence:

```bash
# List all named volumes
docker volume ls

# Inspect volume location
docker volume inspect notiblock_postgres_data

# Example output:
# [
#     {
#         "Mountpoint": "/var/lib/docker/volumes/notiblock_postgres_data/_data"
#     }
# ]
```

### Database Backup

```bash
# Backup database
docker-compose exec db pg_dump -U notiblock_user notiblock_db > backup.sql

# Restore from backup
docker-compose exec -T db psql -U notiblock_user notiblock_db < backup.sql

# Compress backup
docker-compose exec db pg_dump -U notiblock_user notiblock_db | gzip > backup.sql.gz
```

### Application Logs Backup

```bash
# Copy logs from backend container
docker cp notiblock-api:/app/logs ./logs_backup

# Or via docker-compose
docker-compose cp backend:/app/logs ./logs_backup
```

## Production Deployment

### Docker Swarm

For multi-node deployments, use Docker Swarm:

```bash
# Initialize swarm (on manager node)
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.yml notiblock

# View stack status
docker stack ps notiblock

# Scale services
docker service scale notiblock_backend=3

# Update service
docker service update --force notiblock_backend
```

### Kubernetes Deployment

For Kubernetes clusters, generate manifests from docker-compose:

```bash
# Install kompose
curl -L https://github.com/kubernetes/kompose/releases/latest/download/kompose-linux-amd64 -o kompose
chmod +x kompose

# Convert to Kubernetes manifests
./kompose convert -f docker-compose.yml

# Deploy to cluster
kubectl apply -f *.yaml
```

### Environment-Specific Override

Create `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  backend:
    image: notiblock-api:prod
    environment:
      ASPNETCORE_ENVIRONMENT: Production
    restart: unless-stopped
    deploy:
      replicas: 2
      resources:
        limits:
          cpus: '1'
          memory: 1G

  db:
    environment:
      POSTGRES_PASSWORD_FILE: /run/secrets/db_password
    secrets:
      - db_password

secrets:
  db_password:
    external: true
```

Deploy with:
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## Troubleshooting

### Services Won't Start

```bash
# Check logs for errors
docker-compose logs

# Verify Docker daemon is running
docker ps

# Check disk space
docker system df

# Check ports aren't in use
# Windows:
netstat -ano | findstr :5271

# Linux/Mac:
lsof -i :5271
```

### Database Connection Errors

```bash
# Check database is healthy
docker-compose logs db

# Test connection
docker-compose exec db psql -U notiblock_user -d notiblock_db -c "SELECT 1"

# Check environment variables passed correctly
docker-compose exec backend env | grep -i connection

# Verify network connectivity
docker-compose exec backend ping db
```

### Frontend Can't Connect to Backend

```bash
# Check nginx configuration
docker-compose exec frontend cat /etc/nginx/conf.d/default.conf

# Test backend from frontend container
docker-compose exec frontend wget -O- http://backend:5271/api/auth/health

# Check CORS origin setting
docker-compose exec backend env | grep -i cors

# Verify frontend API base URL
docker-compose exec frontend env | grep VITE
```

### Permission Denied Errors

```bash
# Check file permissions in volumes
docker-compose exec backend ls -la /app/

# Fix ownership (from host)
sudo chown -R 1000:1000 ./logs

# Or run container as current user
docker run --user $(id -u):$(id -g) ...
```

### Memory/CPU Issues

```bash
# Monitor resource usage
docker stats

# Set resource limits (in docker-compose.yml)
services:
  backend:
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M
```

### Clean Up

```bash
# Remove stopped containers
docker container prune

# Remove dangling images
docker image prune

# Remove unused volumes
docker volume prune

# Full cleanup (CAUTION: removes all)
docker system prune -a --volumes
```

## Advanced Configuration

### Custom Nginx Configuration

Edit `frontend/nginx.conf` to customize:
- SSL/TLS certificates
- Caching policies
- Compression settings
- Rate limiting
- Custom headers

### Backend Configuration Files

Edit `backend/NotiBlock.Backend/appsettings.json` for:
- Swagger documentation
- Logging levels
- Default values
- Feature flags

### Database Initialization

Add SQL scripts to `init-db.sql` for:
- Initial schema setup
- Seed data
- Test users
- Performance indexes

## References

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Nginx Docker Image](https://hub.docker.com/_/nginx)
