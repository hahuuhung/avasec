# 🏗️ SysAnti - Project Structure & Architecture

> Document created: 2026-02-04
> Status: Living Document (Cập nhật thường xuyên)

## 📂 Directory Structure (Cấu trúc thư mục)

```plaintext
SysAnti/
├── 📂 .git/                 # Git Version Control
├── 📂 Analysis_Copy/        # Backup/Analysis artifacts
├── 📂 doc/                  # Documentation & Planning (Tài liệu & Kế hoạch)
│   ├── 📂 design/           # Design resources, diagrams (Tài nguyên thiết kế)
│   ├── Plan_*.md            # Execution plans (Kế hoạch triển khai)
│   └── *.md                 # Logs & Guides (Nhật ký & Hướng dẫn)
├── 📂 Publish/              # Build outputs (Kết quả build)
├── 📂 SysAnti.API/          # Backend REST API (.NET 8)
├── 📂 SysAnti.Antivirus/    # Core Antivirus Logic
├── 📂 SysAnti.Authentication/ # Auth Service & Logic
├── 📂 SysAnti.Core/         # Shared Interfaces, Models, Constants
├── 📂 SysAnti.Database/     # Data Access Layer (SQLite/EF Core)
├── 📂 SysAnti.Optimization/ # System Cleaning & Speedup Logic
├── 📂 SysAnti.UI/           # Desktop Application (WPF)
├── 📂 SysAnti.Web/          # Web Dashboard
└── SysAnti.sln              # Visual Studio Solution File
```

## 🧩 Modules Overview (Tổng quan các module)

### 1. **SysAnti.Core** (Shared Kernel)

- **Role:** Lớp lõi chứa các thành phần dùng chung cho toàn bộ hệ thống.
- **Key Components:**
  - `Interfaces`: Các interface định nghĩa dịch vụ (ISettingsService, etc.).
  - `Models`: Các DTO và Model dữ liệu chung.
  - `Constants`: Hằng số hệ thống.

### 2. **SysAnti.UI** (Desktop Application)

- **Technology:** WPF, .NET 8, XAML.
- **Role:** Giao diện người dùng chính trên Windows.
- **Features:**
  - Dashboard giám sát hệ thống.
  - Các công cụ quét virus, dọn rác.
  - Cài đặt và cấu hình ứng dụng.

### 3. **SysAnti.API** (Backend Service)

- **Technology:** ASP.NET Core Web API.
- **Role:** Xử lý logic phía server, xác thực, và quản lý license.
- **Features:**
  - RESTful endpoints cho Client.
  - Kết nối Database.

### 4. **SysAnti.Optimization**

- **Role:** Thư viện xử lý các tác vụ tối ưu hóa hệ thống.
- **Features:**
  - `DiskCleanup`: Quét và xóa file rác.
  - `MemoryBoost`: Tối ưu RAM.

### 5. **SysAnti.Antivirus**

- **Role:** Engine quét và phát hiện mã độc.
- **Features:**
  - `FileScanner`: Quét file theo pattern/signature.
  - `RealtimeProtection`: Giám sát thời gian thực.

### 6. **SysAnti.Database**

- **Role:** Lớp truy cập dữ liệu (DAL).
- **Features:**
  - Quản lý SQLite/SQL Server connection.
  - Migration và Schema dữ liệu.

## 🎨 Design Guidelines (Hướng dẫn thiết kế)

> *Place design assets, mockups, and UI references in* `doc/design/`

- **UI Style:** Modern, Glassmorphism.
- **Primary Colors:** (cần cập nhật theo design system)
- **Icons:** (cần cập nhật)

---
*Note: Cập nhật file này khi thêm project hoặc thay đổi cấu trúc lớn.*
