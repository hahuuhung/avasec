# Hướng dẫn Testing & QA - SysAnti

**Ngày tạo**: 2026-02-07 20:52  
**Phiên bản**: 1.0  
**Tác giả**: Antigravity AI

---

## 📋 Tổng quan

Tài liệu này hướng dẫn chi tiết cách thực hiện testing và QA cho hệ thống SysAnti, bao gồm cả Server (Node.js) và Desktop App (WPF/C#).

---

## 🎯 Testing Strategy

### 1. Manual Testing (Ưu tiên cao)
- Kiểm tra các tính năng chính end-to-end
- Phát hiện UI/UX issues
- Validation của user workflows

### 2. Automated Testing (Tương lai)
- Unit tests cho business logic
- Integration tests cho APIs
- UI automation tests

---

## 🧪 Test Cases

### A. Notification System Testing

#### Test Case 1: Gửi thông báo từ Admin
**Mục tiêu**: Verify admin có thể gửi notification thành công

**Pre-conditions**:
- Server đang chạy (`node server.js`)
- Admin đã login vào admin panel

**Steps**:
1. Mở browser: `http://localhost:3000/admin.html`
2. Login với credentials admin
3. Navigate đến "Trung tâm Thông báo"
4. Nhập Title: "Test Notification"
5. Nhập Message: "This is a test message"
6. Chọn Type: "Info"
7. Target User: "Tất cả" hoặc nhập User ID cụ thể
8. Click "Gửi"

**Expected Results**:
- ✅ Toast notification "Đã gửi thông báo" xuất hiện
- ✅ Notification được lưu vào database
- ✅ Check database: `SELECT * FROM Notifications ORDER BY CreatedAt DESC LIMIT 1;`

**Status**: 🟡 Pending

---

#### Test Case 2: Desktop nhận notification
**Mục tiêu**: Desktop app polling và hiển thị notification

**Pre-conditions**:
- Server đang chạy
- Desktop app đang chạy và user đã login
- Có notification mới trong database cho user đó

**Steps**:
1. Chạy Desktop app: `cd SysAnti.UI && dotnet run`
2. Login với user có notification
3. Đợi tối đa 30 giây (polling interval)
4. Check notification badge ở header
5. Click vào bell icon

**Expected Results**:
- ✅ Notification badge hiển thị số thông báo chưa đọc
- ✅ Popup hiển thị list notifications
- ✅ Notification items có đúng title, message, icon

**Status**: 🟡 Pending

---

#### Test Case 3: Mark notification as read
**Mục tiêu**: User có thể đánh dấu notification đã đọc

**Steps**:
1. Click vào một notification item
2. Check badge count giảm
3. Refresh app hoặc đợi next poll
4. Verify notification không còn xuất hiện

**Expected Results**:
- ✅ Badge count giảm đi 1
- ✅ Notification biến mất khỏi list
- ✅ Database: `IsRead = 1` cho notification đó

**Status**: 🟡 Pending

---

### B. Authentication System Testing

#### Test Case 4: Login với Server Online
**Steps**:
1. Start Server
2. Open Desktop app
3. Nhập username và password hợp lệ
4. Click Login

**Expected Results**:
- ✅ Redirect đến Dashboard
- ✅ User info load đúng
- ✅ License info hiển thị chính xác

**Status**: 🟡 Pending

---

#### Test Case 5: Login với Server Offline (Fallback)
**Steps**:
1. Stop Server
2. Open Desktop app
3. Nhập username và password đã từng login trước đó
4. Click Login

**Expected Results**:
- ✅ Fallback to local database
- ✅ Login thành công với cached credentials
- ✅ Toast warning: "Offline mode"

**Status**: 🟡 Pending

---

#### Test Case 6: Guest Login và Upgrade
**Steps**:
1. Click "Continue as Guest"
2. Navigate dashboard
3. Click "Upgrade VIP"
4. Verify redirect đến donate page

**Expected Results**:
- ✅ Guest mode active
- ✅ Limited features visible
- ✅ Upgrade button visible
- ✅ Donate page opens in browser

**Status**: 🟡 Pending

---

### C. Database Optimization Testing

#### Test Case 7: Query Performance với Large Dataset
**Setup**:
```sql
-- Insert 10000 test users
INSERT INTO Users (Username, Email, PasswordHash, LicenseID, CreatedAt) 
VALUES ...
```

**Test**:
```bash
curl http://localhost:3000/api/admin/users?page=1&limit=50
```

**Measure**:
- Response time < 200ms
- Check if indexes are used: `EXPLAIN SELECT ...`

**Expected Results**:
- ✅ Response time fast với large dataset
- ✅ Indexes được sử dụng (shown in EXPLAIN)

**Status**: 🟡 Pending

---

### D. UI/UX Testing

#### Test Case 8: Toast Notifications
**Steps**:
1. Trigger các actions khác nhau (login, logout, errors)
2. Observe toast notifications

**Expected Results**:
- ✅ Toasts appear with correct style (success/error/warning)
- ✅ Auto-dismiss sau 3-4 giây
- ✅ Smooth animations

**Status**: 🟡 Pending

---

#### Test Case 9: Responsive Dashboard
**Steps**:
1. Resize browser window (for web dashboard)
2. Test on different screen resolutions

**Expected Results**:
- ✅ Layout adapts to screen size
- ✅ No horizontal scroll
- ✅ Elements không overlap

**Status**: 🟡 Pending

---

## 🔍 Regression Testing Checklist

Sau mỗi update code, chạy quick regression tests:

- [ ] Server khởi động không lỗi
- [ ] Desktop app build không lỗi
- [ ] Login/Logout works
- [ ] Dashboard loads với real-time metrics
- [ ] Navigation works (tất cả menu items)
- [ ] Notification polling hoạt động
- [ ] Database queries không timeout

---

## 🐛 Bug Tracking

### Known Issues
| ID | Description | Severity | Status |
|----|-------------|----------|--------|
| - | (None yet) | - | - |

### Bug Report Template
```markdown
**Bug ID**: BUG-001
**Title**: Short description
**Severity**: Critical/High/Medium/Low
**Steps to Reproduce**:
1. Step 1
2. Step 2

**Expected**: What should happen
**Actual**: What actually happened
**Environment**: Windows 11, .NET 9.0, Node.js 20.x
**Screenshots**: (nếu có)
```

---

## 📊 Test Coverage Goals

### Phase 1 (Current)
- ✅ Manual testing cho core features
- Target: 80% feature coverage

### Phase 2 (Future)
- Unit tests: 60% code coverage
- Integration tests: Core APIs
- E2E tests: Critical user flows

---

## 🛠️ Testing Tools

### Hiện tại
- Manual testing
- Database queries (MySQL Workbench/CLI)
- Browser DevTools
- Visual Studio debugging

### Đề xuất cho tương lai
- **Backend**: Jest, Mocha, Supertest
- **Desktop**: xUnit, NUnit
- **E2E**: Playwright, Selenium
- **Performance**: JMeter, k6

---

## 📝 Test Execution Log

### Session 1: 2026-02-07
**Tester**: [Tên]  
**Duration**: --  
**Test Cases Run**: 0 / 9  
**Pass**: -- | **Fail**: -- | **Blocked**: --

**Notes**:
- Chưa thực hiện testing

---

## ✅ Sign-off Criteria

Để considered "Test-Ready for Production":
- [ ] 100% critical test cases passed
- [ ] No critical or high severity bugs open
- [ ] Performance benchmarks met
- [ ] Security audit completed
- [ ] Documentation up-to-date

---

*Tài liệu này sẽ được cập nhật liên tục theo quá trình testing.*
