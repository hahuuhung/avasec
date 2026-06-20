# SysAnti Deployment Guide

## Prerequisites

- Docker and Docker Compose installed
- Git (for cloning repository)
- Minimum 2GB RAM
- 10GB free disk space

## Quick Start

### 1. Clone Repository
```bash
git clone https://github.com/yourusername/sysanti.git
cd sysanti
```

### 2. Configure Environment
```bash
# Copy environment template
cp .env.example .env

# Edit .env file with your secure passwords
nano .env
```

**Important:** Change these values in `.env`:
- `DB_PASSWORD` - MySQL database password
- `MYSQL_ROOT_PASSWORD` - MySQL root password
- `JWT_SECRET` - JWT secret key (minimum 32 characters)

### 3. Start Services
```bash
# Build and start all services
docker-compose up -d

# Check service status
docker-compose ps

# View logs
docker-compose logs -f
```

### 4. Access Application

- **Web Admin Panel:** http://localhost:8080
- **API Server:** http://localhost:3001
- **MySQL Database:** localhost:3306

### 5. Initial Setup

1. Access admin panel at http://localhost:8080/admin.html
2. Login with default credentials (change immediately):
   - Username: `admin`
   - Password: `admin123`
3. Create user accounts
4. Configure system settings

## Desktop Application Setup

### Download Installer
- Download `SysAnti-Setup.exe` from releases
- Run installer as Administrator
- Follow installation wizard

### Configuration
1. Launch SysAnti
2. Login with your account
3. Application will connect to server at `http://localhost:3001`

## Production Deployment

### SSL/HTTPS Setup

1. Obtain SSL certificate (Let's Encrypt recommended)
2. Update `nginx.conf` with SSL configuration
3. Update Desktop app config to use HTTPS

### Domain Configuration

Update `.env`:
```
API_URL=https://api.yourdomain.com
WEB_URL=https://yourdomain.com
```

### Security Checklist

- [ ] Change all default passwords
- [ ] Enable firewall rules
- [ ] Configure SSL/TLS
- [ ] Set up database backups
- [ ] Enable rate limiting
- [ ] Review CORS settings
- [ ] Update JWT secret

## Maintenance

### Backup Database
```bash
docker-compose exec mysql mysqldump -u root -p sysanti > backup.sql
```

### Restore Database
```bash
docker-compose exec -T mysql mysql -u root -p sysanti < backup.sql
```

### Update Application
```bash
# Pull latest changes
git pull

# Rebuild containers
docker-compose down
docker-compose up -d --build
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f avasec-server
```

### Stop Services
```bash
docker-compose down
```

### Remove All Data (CAUTION)
```bash
docker-compose down -v
```

## Troubleshooting

### Server won't start
- Check logs: `docker-compose logs avasec-server`
- Verify environment variables in `.env`
- Ensure MySQL is healthy: `docker-compose ps`

### Database connection failed
- Check MySQL container: `docker-compose ps mysql`
- Verify credentials in `.env`
- Check network connectivity: `docker network ls`

### Desktop app can't connect
- Verify server is running: `curl http://localhost:3001/health`
- Check firewall settings
- Verify API URL in Desktop app settings

## Support

For issues and support:
- GitHub Issues: https://github.com/yourusername/sysanti/issues
- Documentation: https://docs.sysanti.com
- Email: support@sysanti.com
