# Quy định Cấu trúc Tài liệu (Documentation Structure)

Để đảm bảo khả năng quản lý dự án lâu dài, cấu trúc thư mục tài liệu được quy định như sau:

## 1. `doc/` - Tài liệu Nội bộ & Quản lý (Internal & Management)

Thư mục này chứa các tài liệu "sống", thay đổi hàng ngày phục vụ cho team phát triển và quản lý dự án.

* **Nội dung:**
  * Kế hoạch dự án (Project Plans)
  * Báo cáo tiến độ hàng ngày (Daily Logs)
  * Danh sách công việc (Task Lists - `task.md`)
  * Các ghi chú kỹ thuật nháp (Drafts)
  * Lịch sử thay đổi (Changelogs)

* **Đối tượng đọc:** PM, Dev Team (Internal).

## 2. `Docs/` - Tài liệu Hướng dẫn Chính thức (Official Documentation)

Thư mục này chứa các tài liệu hoàn chỉnh, hướng dẫn sử dụng và vận hành. Đây là tài liệu output của dự án.

* **Cấu trúc:**
  * `01_Introduction/`: Giới thiệu sản phẩm, Whitepaper (Dành cho Khách hàng/Sale).
  * `02_User_Documentation/`: Hướng dẫn sử dụng cho người dùng cuối (User Manual).
  * `03_Administrator_Guide/`: Hướng dẫn quản trị hệ thống, kiến trúc server (Admin Guide).
  * `For_Designers/`: Design System, Style Guide.

* **Đối tượng đọc:** End Users, System Admins, Clients, Designers.

---
*Lưu ý: Khi tạo tài liệu mới, hãy xác định mục đích để đặt vào đúng thư mục.*
