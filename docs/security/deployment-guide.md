# OpenHR Deployment Guide

**Version:** 2026-03-21
**Scope:** Production deployment of OpenHR for Swedish regions/municipalities
**Prerequisite:** Docker + Docker Compose installed on host

---

## 1. Architecture Overview

OpenHR runs as a containerized application with the following services:

| Service | Container | Port | Purpose |
|---------|-----------|------|---------|
| **OpenHR Web** | `openhr-web` | 5076 (internal) | Blazor Server application |
| **PostgreSQL 17** | `openhr-db` | 5432 (internal only) | Primary database |
| **Reverse Proxy** | `openhr-proxy` | 443, 80 | TLS termination, load balancing |

In production, the database port must NOT be exposed to the public network. Only the reverse proxy exposes ports 80/443.

---

## 2. Docker Compose Production Setup

### 2.1 docker-compose.production.yml

```yaml
version: "3.8"

services:
  web:
    image: openhr/web:latest
    build:
      context: .
      dockerfile: Dockerfile
    container_name: openhr-web
    restart: always
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5076
      - ConnectionStrings__RegionHR=${DB_CONNECTION_STRING}
      - Email__SmtpHost=${SMTP_HOST}
      - Email__SmtpPort=${SMTP_PORT}
      - Email__FromEmail=${SMTP_FROM}
      - Email__FromName=OpenHR
      - OpenHR__SessionTimeoutMinutes=30
    depends_on:
      db:
        condition: service_healthy
    networks:
      - openhr-internal
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5076/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 30s

  db:
    image: postgres:17-alpine
    container_name: openhr-db
    restart: always
    environment:
      POSTGRES_DB: openhr
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - pgdata:/var/lib/postgresql/data
      - ./db/postgresql.conf:/etc/postgresql/postgresql.conf
      - ./db/pg_hba.conf:/etc/postgresql/pg_hba.conf
      - ./db/server.crt:/etc/postgresql/server.crt
      - ./db/server.key:/etc/postgresql/server.key
    command: postgres -c config_file=/etc/postgresql/postgresql.conf
    networks:
      - openhr-internal
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER} -d openhr"]
      interval: 10s
      timeout: 5s
      retries: 5

  proxy:
    image: caddy:2-alpine
    container_name: openhr-proxy
    restart: always
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./Caddyfile:/etc/caddy/Caddyfile
      - caddy_data:/data
      - caddy_config:/config
    depends_on:
      - web
    networks:
      - openhr-internal
      - openhr-public

volumes:
  pgdata:
    driver: local
  caddy_data:
    driver: local
  caddy_config:
    driver: local

networks:
  openhr-internal:
    internal: true
  openhr-public:
    driver: bridge
```

### 2.2 Environment File (.env)

Create a `.env` file in the same directory. **Never commit this file to version control.**

```bash
# Database
DB_USER=openhr_app
DB_PASSWORD=<generated-64-char-random-password>
DB_CONNECTION_STRING=Host=db;Port=5432;Database=openhr;Username=openhr_app;Password=<same-password>;SslMode=Prefer

# SMTP (example for Postfix relay)
SMTP_HOST=mail.region.se
SMTP_PORT=587
SMTP_FROM=noreply@openhr.region.se
```

Generate a strong database password:

```bash
openssl rand -base64 48
```

---

## 3. PostgreSQL Security

### 3.1 TLS Encryption

Generate self-signed certificates for database TLS (or use CA-signed certs):

```bash
# Generate server certificate (valid 10 years)
openssl req -new -x509 -days 3650 -nodes \
  -out db/server.crt -keyout db/server.key \
  -subj "/CN=openhr-db"

# Set correct permissions
chmod 600 db/server.key
chmod 644 db/server.crt
```

### 3.2 postgresql.conf (security-relevant settings)

```ini
# TLS
ssl = on
ssl_cert_file = '/etc/postgresql/server.crt'
ssl_key_file = '/etc/postgresql/server.key'
ssl_min_protocol_version = 'TLSv1.2'

# Logging
log_connections = on
log_disconnections = on
log_statement = 'ddl'
log_line_prefix = '%m [%p] %u@%d '

# Connection limits
max_connections = 100

# Password hashing
password_encryption = scram-sha-256
```

### 3.3 pg_hba.conf (authentication)

```
# TYPE  DATABASE  USER        ADDRESS         METHOD
local   all       all                         scram-sha-256
host    openhr    openhr_app  172.16.0.0/12   scram-sha-256
host    all       all         0.0.0.0/0       reject
```

This ensures:
- Only `openhr_app` can connect to the `openhr` database
- Only from the internal Docker network (172.16.0.0/12)
- All other connections are rejected
- SCRAM-SHA-256 password hashing (not MD5)

### 3.4 Network Isolation

The database container is on the `openhr-internal` network only. It has no route to the public network. Only the `web` container can reach it.

### 3.5 Separate Database Roles (recommended)

For additional security, create separate PostgreSQL roles:

```sql
-- Application role (limited privileges)
CREATE ROLE openhr_app LOGIN PASSWORD '<password>';
GRANT CONNECT ON DATABASE openhr TO openhr_app;
GRANT USAGE ON ALL SCHEMAS IN DATABASE openhr TO openhr_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO openhr_app;

-- Migration role (DDL privileges, used only during upgrades)
CREATE ROLE openhr_migrate LOGIN PASSWORD '<different-password>';
GRANT ALL PRIVILEGES ON DATABASE openhr TO openhr_migrate;

-- Read-only role (for reporting/analytics)
CREATE ROLE openhr_readonly LOGIN PASSWORD '<different-password>';
GRANT CONNECT ON DATABASE openhr TO openhr_readonly;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO openhr_readonly;
```

---

## 4. Reverse Proxy with TLS Termination

### 4.1 Caddy (recommended — automatic HTTPS)

Create `Caddyfile`:

```
openhr.region.se {
    reverse_proxy web:5076

    header {
        # These headers are also set by SecurityHeadersMiddleware,
        # but having them at proxy level provides defense-in-depth
        Strict-Transport-Security "max-age=31536000; includeSubDomains"
        X-Content-Type-Options "nosniff"
        X-Frame-Options "DENY"
    }

    log {
        output file /var/log/caddy/access.log
        format json
    }
}
```

Caddy automatically obtains and renews TLS certificates via Let's Encrypt.

### 4.2 nginx Alternative

```nginx
server {
    listen 443 ssl http2;
    server_name openhr.region.se;

    ssl_certificate /etc/nginx/ssl/fullchain.pem;
    ssl_certificate_key /etc/nginx/ssl/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;

    # HSTS
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;

    # WebSocket support (required for Blazor Server + SignalR)
    location / {
        proxy_pass http://web:5076;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        # Blazor Server circuits need long timeouts
        proxy_read_timeout 3600s;
        proxy_send_timeout 3600s;
    }
}

server {
    listen 80;
    server_name openhr.region.se;
    return 301 https://$server_name$request_uri;
}
```

---

## 5. Environment Variable Configuration

All secrets are passed via environment variables, never hardcoded.

| Variable | Required | Description | Example |
|----------|----------|-------------|---------|
| `ConnectionStrings__RegionHR` | Yes | PostgreSQL connection string | `Host=db;Port=5432;Database=openhr;...` |
| `Email__SmtpHost` | Yes | SMTP relay server | `mail.region.se` |
| `Email__SmtpPort` | Yes | SMTP port | `587` |
| `Email__FromEmail` | Yes | Sender email address | `noreply@openhr.region.se` |
| `ASPNETCORE_ENVIRONMENT` | Yes | Must be `Production` | `Production` |
| `OpenHR__SessionTimeoutMinutes` | No | Session timeout (default: 30) | `30` |

For Kubernetes deployments, use Secrets objects instead of `.env` files.

---

## 6. Backup Strategy

### 6.1 Automated Database Backups

Add a backup service to docker-compose or use cron on the host:

```bash
#!/bin/bash
# /opt/openhr/backup.sh — Run via cron: 0 2 * * * /opt/openhr/backup.sh

BACKUP_DIR="/opt/openhr/backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
RETENTION_DAYS=30

# Create compressed backup
docker exec openhr-db pg_dump -U openhr_app -d openhr -Fc \
  > "${BACKUP_DIR}/openhr_${TIMESTAMP}.dump"

# Encrypt backup (use a GPG key stored securely)
gpg --encrypt --recipient backup@region.se \
  "${BACKUP_DIR}/openhr_${TIMESTAMP}.dump"
rm "${BACKUP_DIR}/openhr_${TIMESTAMP}.dump"

# Remove backups older than retention period
find "${BACKUP_DIR}" -name "openhr_*.dump.gpg" -mtime +${RETENTION_DAYS} -delete

# Optional: sync to off-site storage
# rsync -az "${BACKUP_DIR}/" backup-server:/backups/openhr/
```

### 6.2 Backup Verification

Test restore weekly:

```bash
# Restore to a test database
docker exec -i openhr-db pg_restore -U openhr_app -d openhr_test \
  < /opt/openhr/backups/openhr_latest.dump
```

### 6.3 Retention Policy

| Backup Type | Frequency | Retention |
|-------------|-----------|-----------|
| Full database dump | Daily at 02:00 | 30 days |
| Weekly full backup | Sunday 03:00 | 12 weeks |
| Monthly archive | 1st of month | 7 years (bokforingslagen) |
| Upload documents | Included in DB backup (stored as BLOBs or file paths) | Same as database |

---

## 7. Update/Upgrade Process

### 7.1 Standard Update (patch/minor version)

```bash
# 1. Pull latest image
docker compose -f docker-compose.production.yml pull web

# 2. Create pre-update backup
/opt/openhr/backup.sh

# 3. Rolling restart (zero-downtime if running multiple replicas)
docker compose -f docker-compose.production.yml up -d --no-deps web

# 4. Verify health
curl -f https://openhr.region.se/health

# 5. Check logs for migration output
docker logs openhr-web --tail 50
```

### 7.2 Major Version Update

1. Read the CHANGELOG for breaking changes
2. Test in staging environment first
3. Schedule maintenance window (communicate to users)
4. Create full backup
5. Stop the application: `docker compose down`
6. Update and start: `docker compose up -d`
7. Run manual smoke tests
8. Monitor logs and health endpoint for 1 hour

### 7.3 Rollback

```bash
# Stop current version
docker compose down

# Restore database from backup
docker exec -i openhr-db pg_restore -U openhr_app -d openhr --clean \
  < /opt/openhr/backups/openhr_pre_update.dump

# Start previous version
docker compose -f docker-compose.production.yml up -d
```

---

## 8. Monitoring and Alerting

### 8.1 Health Endpoint

OpenHR exposes `/health` which checks:
- PostgreSQL connectivity
- Application responsiveness

### 8.2 Recommended Monitoring

| What to Monitor | Tool | Alert Threshold |
|-----------------|------|-----------------|
| `/health` endpoint | Uptime Kuma / Prometheus | Any non-200 response |
| Container CPU/memory | cAdvisor + Prometheus | CPU > 80%, Memory > 90% |
| PostgreSQL connections | pg_stat_activity | > 80% of max_connections |
| Disk space | Node Exporter | < 20% free |
| TLS certificate expiry | Certbot / Caddy auto | < 14 days |
| Backup success | Cron job exit code | Any non-zero exit |

### 8.3 Log Aggregation

OpenHR uses structured JSON logging. Forward logs to a centralized system:

```yaml
# Add to docker-compose for log forwarding
services:
  web:
    logging:
      driver: "json-file"
      options:
        max-size: "50m"
        max-file: "5"
```

For production, consider forwarding to Grafana Loki, Elasticsearch, or similar FOSS log aggregation.

---

## 9. Security Checklist for Production

Before going live, verify:

- [ ] Database password is randomly generated (64+ characters)
- [ ] `.env` file is NOT in version control
- [ ] Database is on internal-only Docker network
- [ ] HTTPS is enforced (HTTP redirects to HTTPS)
- [ ] HSTS header is present (`Strict-Transport-Security`)
- [ ] CSP headers are configured (via SecurityHeadersMiddleware)
- [ ] Rate limiting is active (100 req/min per IP)
- [ ] PostgreSQL uses SCRAM-SHA-256 authentication
- [ ] PostgreSQL TLS is enabled
- [ ] Backups run daily and are encrypted
- [ ] Backup restore has been tested
- [ ] Health endpoint is monitored
- [ ] Session timeout is configured (default: 30 minutes)
- [ ] All default passwords have been changed
