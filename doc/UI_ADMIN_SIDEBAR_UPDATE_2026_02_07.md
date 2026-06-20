# Kế hoạch điều chỉnh giao diện trang Admin (admin.html)
**Ngày giờ:** 2026-02-07 13:42
**Người thực hiện:** Antigravity AI

## 1. Mục tiêu
Di chuyển các mục tiêu đề "Admin Intelligence" và "Quản lý người dùng và giấy phép" vào thanh Sidebar để giải phóng không gian nội dung chính và tạo cảm giác chuyên nghiệp hơn.

## 2. Giải pháp kỹ thuật
- Chỉnh sửa file `SysAnti.Server/public/admin.html`.
- Thêm tiêu đề phụ và biểu tượng tương ứng vào phần `nav` trong Sidebar.
- Xóa `header` cũ trong phần `main content`.

## 3. Các bước thực hiện
1. Tìm phần `sidebarMenu` trong `admin.html`.
2. Thêm code HTML cho tiêu đề mới vào danh sách menu.
3. Chỉnh sửa CSS (nếu cần) để hiển thị đẹp mắt.
4. Xóa thẻ `header` ở dòng 98-108.

---
*Tài liệu này được tạo tự động theo quy trình làm việc của dự án.*
