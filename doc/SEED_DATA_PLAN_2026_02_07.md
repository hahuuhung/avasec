# Kế hoạch thêm dữ liệu kiểm thử (50 Users)
**Ngày giờ:** 2026-02-07 13:45
**Người thực hiện:** Antigravity AI

## 1. Mục tiêu
Tạo ra 50 người dùng giả lập cùng với các giấy phép (licenses) tương ứng để kiểm tra tính năng hiển thị danh sách và biểu đồ trên trang Admin.

## 2. Dữ liệu giả lập
- **Users:** Username dạng `user1`, `user2`, ... `user50`.
- **Passwords:** Mặc định là `password123`.
- **Licenses:** Phân bổ ngẫu nhiên giữa `Trial`, `Pro`, và `Premium`.
- **ExpiryDate:** Ngẫu nhiên từ 1 đến 90 ngày tới.

## 3. Các bước thực hiện
1. Tạo file SQL `SysAnti.Server/seed_test_data.sql` chứa các câu lệnh INSERT.
2. Tạo script Node.js `SysAnti.Server/scripts/seed.js` để tự động chèn vào database MySQL (nếu người dùng muốn chạy bằng lệnh).
3. Hướng dẫn người dùng cách chạy.

---
*Tài liệu này được tạo tự động theo quy trình làm việc của dự án.*
