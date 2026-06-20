# Kế hoạch điều chỉnh giao diện trang Nâng cấp (donate.html)
**Ngày giờ:** 2026-02-07 13:40
**Người thực hiện:** Antigravity AI

## 1. Mục tiêu
Di chuyển phần "Quét QR để tham gia nhóm Zalo" từ bên trong khung "Gói Premium" sang một khung riêng biệt nằm bên cạnh các khung giá.

## 2. Giải pháp kỹ thuật
- Chỉnh sửa file `SysAnti.Server/public/donate.html`.
- Thay đổi cấu trúc từ 2 cột (Free/Premium) sang 3 cột (nếu màn hình đủ rộng) hoặc điều chỉnh grid để hỗ trợ khung thứ 3.
- Tạo một khung `glass-panel` mới cho thông tin hỗ trợ Zalo.
- Đảm bảo tính thẩm mỹ và đồng bộ với giao diện hiện tại.

## 3. Các bước thực hiện
1. Thay đổi class `row-cols-md-2` thành `row-cols-lg-3` để hỗ trợ 3 cột trên màn hình lớn.
2. Tách phần mã nguồn QR Zalo ra khỏi card Premium.
3. Tạo thẻ `col` mới chứa card Zalo.
4. Kiểm tra độ phản hồi (Responsive) của giao diện.

---
*Tài liệu này được tạo tự động theo quy trình làm việc của dự án.*
