# Quy Trình Tài Liệu Hóa (Documentation Process)

Tài liệu này quy định quy trình làm việc và ghi chép nhật ký phát triển dự án SysAnti.

## 1. Nguyên Tắc Chung
- **Luôn lập kế hoạch trước khi code**: Mọi thay đổi code phải được gạch đầu dòng trong file kế hoạch tương ứng.
- **Cập nhật theo thời gian thực**: Ghi lại nhật ký (Log) ngay sau khi hoàn thành một tác vụ hoặc gặp lỗi.
- **Lưu trữ tập trung**: Tất cả tài liệu quy trình và nhật ký nằm trong thư mục `doc/`.

## 2. Quy Định Đặt Tên File
- Mỗi ngày làm việc tạo một file mới (hoặc cập nhật file hiện tại nếu cùng ngày).
- Định dạng: `PLAN_YYYY_MM_DD.md`
- Ví dụ: `PLAN_2026_02_03.md`

## 3. Cấu Trúc File Kế Hoạch
Mỗi file `PLAN_*.md` cần tuân thủ cấu trúc sau:

```markdown
# Kế hoạch và Nhật ký Phát triển - DD/MM/YYYY

## Thông tin
- **Ngày**: DD/MM/YYYY
- **Người thực hiện**: [Tên]
- **Trạng thái**: [Đang thực hiện / Hoàn thành]

## 1. Mục tiêu trong ngày
- [ ] Task 1
- [ ] Task 2

## 2. Nhật ký chi tiết (Log)
### [HH:MM] Tên Hành Động
- **Hành động**: Mô tả ngắn gọn.
- **Kết quả**: Thành công/Thất bại/Lỗi.
- **Ghi chú**: (Nếu có)

## 3. Kế hoạch tiếp theo
- Các đầu mục cho ngày hôm sau hoặc phiên làm việc tiếp theo.
```

## 4. Quy Trình Làm Việc
1.  **Đầu ngày**: Tạo/Mở file `PLAN_Hôm_Nay.md`.
2.  **Lập kế hoạch**: Điền mục "1. Mục tiêu".
3.  **Thực hiện**:
    -   Ghi giờ vào mục "2. Nhật ký".
    -   Thực hiện code/thao tác.
    -   Cập nhật kết quả vào nhật ký.
4.  **Cuối ngày**: Review lại file, cập nhật "Trạng thái", và điền "3. Kế hoạch tiếp theo".
