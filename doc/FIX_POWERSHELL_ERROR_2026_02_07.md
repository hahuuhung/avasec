# Kế hoạch xử lý lỗi PowerShell Execution Policy
**Ngày giờ:** 2026-02-07 13:23
**Người thực hiện:** Antigravity AI

## 1. Vấn đề
Người dùng gặp lỗi `PSSecurityException` khi chạy `npm install` do chính sách thực thi script của PowerShell đang bị chặn (`Disabled`).

## 2. Giải pháp
- Hướng dẫn người dùng thay đổi chính sách thực thi (`Execution Policy`) sang `RemoteSigned` để cho phép chạy script cục bộ.
- Cung cấp lệnh khắc phục nhanh trong phiên làm việc hiện tại.

## 3. Các bước hướng dẫn
1. Mở PowerShell với quyền Administrator.
2. Chạy lệnh: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`.
3. Kiểm tra lại bằng cách chạy `npm install`.

---
*Tài liệu này được tạo tự động theo quy trình làm việc của dự án.*
