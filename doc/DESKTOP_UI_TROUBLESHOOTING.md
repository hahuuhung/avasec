# SysAnti Desktop UI - Troubleshooting Guide
**Date:** 08/02/2026 17:45  
**Issue:** SysAnti.UI.exe không chạy được

---

## 🔍 Vấn đề chính / Main Issue

**Error:** "The requested operation requires elevation"

**Nguyên nhân:** SysAnti.UI yêu cầu quyền Administrator để:
- Truy cập system-level APIs
- Quản lý services (Game Booster)
- Scan registry (Registry Doctor)
- Xóa file system (Privacy Shredder)
- Monitor network (Network Fortress)

---

## ✅ Giải pháp / Solutions

### Solution 1: Run as Administrator (Recommended)

**Cách 1 - File Explorer:**
1. Mở File Explorer
2. Đi đến: `d:\sysanti-UInew\sysanti-UInew\SysAnti.UI\bin\Release\net9.0-windows\`
3. Click phải `SysAnti.UI.exe`
4. Chọn **"Run as administrator"**

**Cách 2 - PowerShell Admin:**
```powershell
# Mở PowerShell as Administrator, sau đó chạy:
cd "d:\sysanti-UInew\sysanti-UInew\SysAnti.UI\bin\Release\net9.0-windows"
.\SysAnti.UI.exe
```

**Cách 3 - Create Shortcut:**
1. Click phải `SysAnti.UI.exe` → Create shortcut
2. Click phải shortcut → Properties
3. Tab "Shortcut" → Click "Advanced"
4. Check ☑ "Run as administrator"
5. OK → Apply
6. Double-click shortcut để chạy

---

### Solution 2: Modify App Manifest (Permanent)

Nếu muốn app tự động yêu cầu admin mỗi lần chạy:

**File:** `SysAnti.UI\app.manifest`

Thêm/sửa dòng này:
```xml
<requestedExecutionLevel level="requireAdministrator" uiAccess="false" />
```

Sau đó rebuild:
```powershell
dotnet build SysAnti.UI\SysAnti.UI.csproj --configuration Release
```

---

## 🔧 Kiểm tra Dependencies

### .NET Runtime Required
- **Version:** .NET 9.0 Desktop Runtime
- **Download:** https://dotnet.microsoft.com/download/dotnet/9.0

**Check installed:**
```powershell
dotnet --list-runtimes
```

Cần thấy:
```
Microsoft.WindowsDesktop.App 9.x.x
```

---

## 🐛 Common Issues

### Issue 1: Missing DLL
**Error:** "Could not load file or assembly..."

**Solution:**
```powershell
# Rebuild với dependencies
dotnet publish SysAnti.UI\SysAnti.UI.csproj -c Release --self-contained false
```

### Issue 2: Database Connection
**Error:** "Cannot connect to MySQL"

**Check:**
1. MySQL service đang chạy?
2. `.env` file có đúng credentials?
3. Database `avasec_db` đã tạo?

**Test connection:**
```powershell
mysql -u root -p -e "SHOW DATABASES;"
```

### Issue 3: Server Not Running
**Error:** "Failed to connect to http://localhost:3000"

**Solution:**
```powershell
cd SysAnti.Server
npm start
```

---

## 📋 Pre-Launch Checklist

Trước khi chạy Desktop UI, đảm bảo:

- [ ] ✅ MySQL service running
- [ ] ✅ Database `avasec_db` created
- [ ] ✅ Server running on http://localhost:3000
- [ ] ✅ .NET 9.0 Desktop Runtime installed
- [ ] ✅ Run as Administrator

---

## 🚀 Quick Start Script

Tạo file `start-sysanti.bat`:

```batch
@echo off
echo Starting SysAnti System...

REM Check if running as admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Please run as Administrator!
    pause
    exit /b 1
)

REM Start MySQL (if not running)
net start MySQL80

REM Start Server
start "AVA Security Server" cmd /k "cd /d d:\sysanti-UInew\sysanti-UInew\SysAnti.Server && npm start"

REM Wait for server
timeout /t 5 /nobreak

REM Start Desktop UI
cd /d "d:\sysanti-UInew\sysanti-UInew\SysAnti.UI\bin\Release\net9.0-windows"
start "" "SysAnti.UI.exe"

echo SysAnti started successfully!
pause
```

**Sử dụng:**
1. Save file trên
2. Click phải → **Run as administrator**

---

## 📊 System Requirements

- **OS:** Windows 10/11 (64-bit)
- **RAM:** 4GB minimum, 8GB recommended
- **.NET:** 9.0 Desktop Runtime
- **MySQL:** 8.0+
- **Node.js:** 18+
- **Privileges:** Administrator required

---

## 🔗 Related Files

- Executable: [`SysAnti.UI.exe`](file:///d:/sysanti-UInew/sysanti-UInew/SysAnti.UI/bin/Release/net9.0-windows/SysAnti.UI.exe)
- Config: [`appsettings.json`](file:///d:/sysanti-UInew/sysanti-UInew/SysAnti.UI/appsettings.json)
- Server: [`server.js`](file:///d:/sysanti-UInew/sysanti-UInew/SysAnti.Server/server.js)

---

**Last Updated:** 08/02/2026 17:45  
**Status:** ✅ Solutions provided
