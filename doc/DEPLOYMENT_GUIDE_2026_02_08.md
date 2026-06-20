# Hướng dẫn Deployment - SysAnti

**Ngày tạo**: 2026-02-07 20:52  
**Phiên bản**: 1.0

---

## 📋 Tổng quan

Tài liệu này hướng dẫn deploy SysAnti lên production environment, bao gồm:
- **Server**: Node.js application với MySQL database
- **Desktop App**: WPF application cho Windows

---

## 🖥️ Server Deployment (Node.js)

### Option 1: Deploy trên VPS/Cloud (Linux)

#### Bước 1: Chuẩn bị Server
```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Node.js 20.x
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs

# Install MySQL
sudo apt install -y mysql-server

# Install PM2 (Process Manager)
sudo npm install -g pm2
```

#### Bước 2: Upload Code
```bash
# Sử dụng Git
cd /var/www
sudo git clone https://github.com/hahuuhung/SysAnti.git
cd SysAnti/SysAnti.Server

# Hoặc upload qua SCP/FTP
scp -r SysAnti.Server/ user@server:/var/www/
```

#### Bước 3: Cấu hình Database
```bash
# Login MySQL
sudo mysql -u root -p

# Chạy trong MySQL console:
CREATE DATABASE avasec_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'sysanti_user'@'localhost' IDENTIFIED BY 'your_strong_password';
GRANT ALL PRIVILEGES ON avasec_db.* TO 'sysanti_user'@'localhost';
FLUSH PRIVILEGES;
EXIT;

# Import database schema
mysql -u sysanti_user -p avasec_db < database_schema.sql
```

#### Bước 4: Cấu hình Environment Variables
```bash
# Tạo file .env
nano /var/www/SysAnti/SysAnti.Server/.env
```

Nội dung file `.env`:
```env
# Database Configuration
DB_HOST=localhost
DB_USER=sysanti_user
DB_PASSWORD=your_strong_password
DB_NAME=avasec_db

# Server Configuration
PORT=3000
NODE_ENV=production

# JWT Secret (generate strong random string)
JWT_SECRET=your_very_long_random_secret_key_here

# CORS Origin (Desktop app domain or *)
CORS_ORIGIN=*
```

#### Bước 5: Install Dependencies và Apply Optimizations
```bash
cd /var/www/SysAnti/SysAnti.Server

# Install dependencies
npm install --production

# Apply database optimizations
node apply_optimizations.js

# Test run
node server.js
# Ctrl+C để stop sau khi verify OK
```

#### Bước 6: Setup PM2 cho Auto-restart
```bash
# Start với PM2
pm2 start server.js --name avasec-server

# Save PM2 config
pm2 save

# Setup auto-start on server reboot
pm2 startup
# Copy và chạy command output từ pm2 startup

# Monitor logs
pm2 logs avasec-server
```

#### Bước 7: Setup Nginx Reverse Proxy (Optional nhưng recommended)
```bash
# Install Nginx
sudo apt install -y nginx

# Tạo Nginx config
sudo nano /etc/nginx/sites-available/sysanti
```

Nội dung file:
```nginx
server {
    listen 80;
    server_name your-domain.com;  # Thay bằng domain thực

    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}
```

```bash
# Enable site
sudo ln -s /etc/nginx/sites-available/sysanti /etc/nginx/sites-enabled/

# Test config
sudo nginx -t

# Restart Nginx
sudo systemctl restart nginx
```

#### Bước 8: Setup SSL với Let's Encrypt (Recommended)
```bash
# Install Certbot
sudo apt install -y certbot python3-certbot-nginx

# Obtain SSL certificate
sudo certbot --nginx -d your-domain.com

# Auto-renewal test
sudo certbot renew --dry-run
```

---

### Option 2: Deploy với Docker

#### Dockerfile
Tạo file `Dockerfile` trong `SysAnti.Server/`:
```dockerfile
FROM node:20-alpine

WORKDIR /app

COPY package*.json ./
RUN npm install --production

COPY . .

EXPOSE 3000

CMD ["node", "server.js"]
```

#### docker-compose.yml
```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: rootpassword
      MYSQL_DATABASE: avasec_db
      MYSQL_USER: sysanti_user
      MYSQL_PASSWORD: your_password
    volumes:
      - mysql_data:/var/lib/mysql
      - ./database_schema.sql:/docker-entrypoint-initdb.d/schema.sql
    ports:
      - "3306:3306"

  server:
    build: .
    ports:
      - "3000:3000"
    environment:
      DB_HOST: mysql
      DB_USER: sysanti_user
      DB_PASSWORD: your_password
      DB_NAME: avasec_db
      PORT: 3000
      NODE_ENV: production
      JWT_SECRET: your_jwt_secret
    depends_on:
      - mysql
    restart: unless-stopped

volumes:
  mysql_data:
```

#### Deploy Commands
```bash
# Build và start
docker-compose up -d

# View logs
docker-compose logs -f server

# Stop
docker-compose down

# Rebuild after code changes
docker-compose up -d --build
```

---

## 💻 Desktop App Deployment (Windows)

### Option 1: Manual Distribution (Simple)

#### Bước 1: Build Release Version
```powershell
cd d:\sysanti-UInew\sysanti-UInew\SysAnti.UI

# Build Release
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

Output folder: `bin/Release/net9.0-windows/win-x64/publish/`

#### Bước 2: Distribution
- Zip folder `publish/` thành `SysAnti-v1.0.zip`
- Upload lên website/Google Drive
- User download, extract, và chạy `SysAnti.UI.exe`

**Config cho Production**:
Sửa hardcoded server URL trong code:
```csharp
// Thay localhost bằng production domain
private readonly string _serverUrl = "https://your-domain.com";
```

---

### Option 2: Installer với Squirrel.Windows

#### Cài đặt Squirrel
```powershell
dotnet add package Squirrel.Windows
```

#### Tạo Installer Script
Tạo file `build-installer.ps1`:
```powershell
# Build Release
dotnet publish -c Release -r win-x64 --self-contained true

# Create Squirrel package
$version = "1.0.0"
squirrel --releasify SysAnti.UI.$version.nupkg --releaseDir=Releases
```

---

### Option 3: MSI Installer với WiX Toolset

#### Cài đặt WiX
```powershell
# Download từ https://wixtoolset.org/
# Cài đặt WiX Toolset
```

#### Tạo Product.wxs
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" Name="SysAnti" Language="1033" Version="1.0.0.0" 
           Manufacturer="Your Company" UpgradeCode="PUT-GUID-HERE">
    <Package InstallerVersion="200" Compressed="yes" InstallScope="perMachine" />
    
    <MajorUpgrade DowngradeErrorMessage="A newer version is already installed." />
    <MediaTemplate EmbedCab="yes" />

    <Feature Id="ProductFeature" Title="SysAnti" Level="1">
      <ComponentGroupRef Id="ProductComponents" />
    </Feature>
  </Product>

  <Fragment>
    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFilesFolder">
        <Directory Id="INSTALLFOLDER" Name="SysAnti" />
      </Directory>
    </Directory>
  </Fragment>

  <Fragment>
    <ComponentGroup Id="ProductComponents" Directory="INSTALLFOLDER">
      <!-- Add your files here -->
    </ComponentGroup>
  </Fragment>
</Wix>
```

#### Build MSI
```powershell
candle Product.wxs
light -ext WixUIExtension Product.wixobj -out SysAnti-Setup.msi
```

---

## 🔧 Production Configuration Checklist

### Server
- [ ] Database credentials secure (không commit vào Git)
- [ ] JWT secret strong và unique
- [ ] CORS configured đúng
- [ ] HTTPS enabled (SSL certificate)
- [ ] Environment variables set
- [ ] PM2 hoặc similar process manager
- [ ] Firewall rules (chỉ mở port 80, 443, 22)
- [ ] Database daily backup configured
- [ ] Logging setup properly
- [ ] Error monitoring (optional: Sentry, LogRocket)

### Desktop App
- [ ] Server URL point to production
- [ ] Error handling đầy đủ
- [ ] Offline mode works
- [ ] Auto-update mechanism (nếu có)
- [ ] App signing (code signing certificate)
- [ ] Installer tested trên clean Windows
- [ ] Uninstaller works properly

---

## 📦 Backup Strategy

### Database Backup
```bash
# Manual backup
mysqldump -u sysanti_user -p avasec_db > backup_$(date +%Y%m%d_%H%M%S).sql

# Automated daily backup với cron
crontab -e
# Add line:
0 2 * * * mysqldump -u sysanti_user -pYOUR_PASSWORD avasec_db > /backups/sysanti_$(date +\%Y\%m\%d).sql
```

### Application Code Backup
```bash
# Git là primary backup
# Nhưng cũng nên rsync định kỳ
rsync -avz /var/www/SysAnti/ /backups/code/
```

---

## 🚀 CI/CD Pipeline (Advanced)

### GitHub Actions Example
Tạo `.github/workflows/deploy.yml`:
```yaml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Deploy to Server
        uses: appleboy/ssh-action@master
        with:
          host: ${{ secrets.SERVER_HOST }}
          username: ${{ secrets.SERVER_USER }}
          key: ${{ secrets.SSH_KEY }}
          script: |
            cd /var/www/SysAnti
            git pull origin main
            cd SysAnti.Server
            npm install --production
            pm2 restart avasec-server
```

---

## 📊 Monitoring

### Server Health Monitoring
```bash
# Check PM2 status
pm2 status

# Monitor logs
pm2 logs avasec-server --lines 100

# System resources
htop
# or
pm2 monit
```

### Uptime Monitoring (Recommended Services)
- UptimeRobot (free tier available)
- Pingdom
- StatusCake

Ping endpoint: `https://your-domain.com/health`

---

## 🔄 Update/Rollback Procedures

### Server Update
```bash
cd /var/www/SysAnti
git pull origin main
cd SysAnti.Server
npm install --production
pm2 restart avasec-server
```

### Rollback
```bash
git log  # Find previous commit
git checkout <previous-commit-hash>
pm2 restart avasec-server
```

### Desktop App Update
- Build new version
- Upload to distribution channel
- (Optional) Auto-update notification to users

---

## 📞 Support & Troubleshooting

### Common Issues

**Server won't start**:
```bash
# Check logs
pm2 logs avasec-server

# Check if port 3000 is in use
sudo lsof -i :3000

# Restart
pm2 restart avasec-server
```

**Database connection failed**:
```bash
# Check MySQL is running
sudo systemctl status mysql

# Test connection
mysql -u sysanti_user -p avasec_db
```

**Desktop app can't connect**:
- Check server URL in app config
- Verify server is running: `curl https://your-domain.com/health`
- Check firewall/CORS settings

---

*Tài liệu này cần được update khi có thay đổi infrastructure.*
