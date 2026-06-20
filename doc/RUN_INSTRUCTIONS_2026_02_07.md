# Hướng dẫn Chạy ứng dụng SysAnti (Desktop & Server)
**Ngày cập nhật:** 07/02/2026

Tài liệu này hướng dẫn cách khởi động toàn bộ hệ thống SysAnti bao gồm ứng dụng Desktop và máy chủ Web/API.

---

## 1. 🖥️ Ứng dụng Desktop (WPF)

Ứng dụng Desktop là giao diện chính để người dùng tối ưu hóa hệ thống.

### Yêu cầu tiên quyết:
- **.NET 9.0 SDK**: [Tải về tại đây](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Hệ điều hành**: Windows 10 hoặc Windows 11

### Cách chạy:

#### Cách 1: Sử dụng Terminal (Khuyên dùng cho Code)
1. Mở PowerShell hoặc Command Prompt tại thư mục gốc của dự án.
2. Chạy lệnh:
   ```powershell
   dotnet run --project SysAnti.UI
   ```

#### Cách 2: Sử dụng Script có sẵn
1. Chạy file `run_admin.bat` tại thư mục gốc.
2. Lưu ý: File này yêu cầu bạn đã build dự án trước đó (`dotnet build`).

---

## 2. 🌍 Máy chủ Web & API (Node.js)

Máy chủ này xử lý việc đăng nhập, quản lý license và cung cấp giao diện Dashboard Web.

### Yêu cầu tiên quyết:
- **Node.js**: Phiên bản 18 trở lên.
- **MySQL**: Để lưu trữ dữ liệu người dùng và license.

### Các bước cài đặt & Chạy:

1. **Truy cập thư mục Server**:
   ```bash
   cd SysAnti.Server
   ```

2. **Cài đặt thư viện**:
   ```bash
   npm install
   ```

3. **Cấu hình Cơ sở dữ liệu**:
   - Mở file `.env` (copy từ `.env.example` nếu chưa có).
   - Cập nhật thông tin kết nối MySQL:
     ```env
     DB_HOST=localhost
     DB_USER=your_username
     DB_PASSWORD=your_password
     DB_NAME=avasec_db
     ```
   - Chạy file `database.sql` trong MySQL để khởi tạo các bảng cần thiết.

4. **Khởi động Server**:
   - Chế độ phát triển (Tự động tải lại):
     ```bash
     npm run dev
     ```
   - Chế độ chính thức:
     ```bash
     npm start
     ```

5. **Truy cập Giao diện Web**:
   - Mở trình duyệt và truy cập: `http://localhost:3000` (hoặc cổng cấu hình trong `.env`).

---

## ⚠️ Lưu ý chung
- Đảm bảo MySQL đang chạy trước khi khởi động `SysAnti.Server`.
- Ứng dụng Desktop kết nối với API của Server, vì vậy nên chạy Server trước.
