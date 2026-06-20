# 🔧 Hướng dẫn Fix lỗi Login sau Git Update

## Vấn đề
Sau khi git pull/update, trang login không hoạt động do browser cache.

## Giải pháp

### Cách 1: Hard Refresh Browser (Khuyến nghị)
1. Mở http://localhost:3000/
2. Nhấn **Ctrl + Shift + R** (Windows) hoặc **Cmd + Shift + R** (Mac)
3. Hoặc nhấn **Ctrl + F5**
4. Thử đăng nhập lại

### Cách 2: Clear Browser Cache
**Chrome/Edge:**
1. Nhấn F12 để mở DevTools
2. Click chuột phải vào nút Refresh
3. Chọn "Empty Cache and Hard Reload"

**Firefox:**
1. Nhấn Ctrl + Shift + Delete
2. Chọn "Cached Web Content"
3. Click "Clear Now"

### Cách 3: Sử dụng Incognito/Private Mode
1. Mở cửa sổ Incognito (Ctrl + Shift + N)
2. Truy cập http://localhost:3000/
3. Đăng nhập

## Test Account
- **Username:** `testuser`
- **Password:** `test123`

## Verify API hoạt động
```powershell
# Test Register
Invoke-WebRequest -Uri "http://localhost:3000/api/auth/register" -Method POST -ContentType "application/json" -Body '{"username":"newuser","password":"pass123","email":"new@test.com"}'

# Test Login
Invoke-WebRequest -Uri "http://localhost:3000/api/auth/login" -Method POST -ContentType "application/json" -Body '{"username":"testuser","password":"test123"}'
```

## Nếu vẫn lỗi
1. Stop server (Ctrl + C)
2. Restart: `cd SysAnti.Server; npm start`
3. Hard refresh browser
4. Thử lại

---
**Ngày:** 08/02/2026 17:05
