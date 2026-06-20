# TEST LOGIN - HƯỚNG DẪN KIỂM TRA / LOGIN TEST - TROUBLESHOOTING GUIDE

## 🔍 KIỂM TRA LỖI / ERROR CHECKING

### **Lỗi có thể gặp:**

1. **"Invalid username or password"**
   - Chưa đăng ký user
   - Password sai
   - Username sai

2. **"License has expired"**
   - License hết hạn (sau 14 ngày)

3. **"Account is deactivated"**
   - Tài khoản bị vô hiệu

---

## ✅ GIẢI PHÁP / SOLUTIONS

### **Option 1: Tạo user mới (Khuyến nghị)**

```
1. Mở app
2. Click tab "REGISTER / ĐĂNG KÝ"
3. Điền thông tin:
   Username: test
   Email: test@test.com
   Password: test123
4. Click "Register"
5. Quay lại tab "LOGIN"
6. Đăng nhập với: test / test123
```

---

### **Option 2: Kiểm tra Database**

**Vị trí database:**

```
%AppData%\SysAnti\avasec.db
```

**Mở bằng DB Browser for SQLite:**

1. Download: <https://sqlitebrowser.org/>
2. Mở file avasec.db
3. Xem table "Users"
4. Xem table "Licenses"

---

### **Option 3: Script tạo test user**

Tôi sẽ tạo tool để thêm test user vào database!

---

## 🧪 TEST ACCOUNT MẪU

**Default test account (nếu đã tạo):**

```
Username: admin
Password: admin123
```

**Hoặc tạo mới:**

```
Username: test
Email: test@test.com  
Password: test123
```

---

## ❓ CHI TIẾT LỖI

**Bạn gặp lỗi gì khi login?**

- [ ] "Invalid username or password"
- [ ] "License has expired"
- [ ] "Account is deactivated"
- [ ] Lỗi khác (ghi rõ)

**Thông tin:**

- Username đang dùng: _______
- Đã đăng ký chưa: Yes / No
- Error message chính xác: _______
