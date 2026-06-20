# Kế hoạch Hoàn thiện Dự án SysAnti - 2026-02-07

## 1. Tổng quan
Dự án hiện tại đang gặp lỗi Runtime (XamlParseException) do thiếu các tài nguyên giao diện (Resources) và từ ngữ (Localization) khi khởi chạy. Mục tiêu là sửa lỗi này để ứng dụng có thể chạy ổn định.

## 2. Phân tích Lỗi
Dựa trên log `startup_error.txt`, các lỗi chính bao gồm:
1. **Thiếu Resource Brush**: `Cannot find resource named 'AccentWarningBrush'`.
2. **Thiếu Resource Styles**: `Cannot find resource named 'ActionButtonStyle'` (đã kiểm tra thấy có, nhưng có thể do thứ tự merge).
3. **Thiếu Localization**: `Cannot find resource named 'Lang.Msg.LoginSuccess'`.

**Nguyên nhân:**
- `AccentWarningBrush` chưa được định nghĩa trong `Colors.xaml`.
- `App.xaml` chưa merge các file ngôn ngữ (`Resources/Languages/vi-VN.xaml`), dẫn đến việc không tìm thấy các key `Lang.*`.

## 3. Kế hoạch Thực hiện

### Bước 1: Cập nhật `Colors.xaml`
- Thêm `AccentWarningBrush` vào `SysAnti.UI/Resources/Colors.xaml`.
- Giá trị đề xuất: Trỏ tới `WarningColor` (#FF9800).

### Bước 2: Cập nhật `App.xaml`
- Thêm việc merge `Resources/Languages/vi-VN.xaml` vào `App.xaml` để nạp ngôn ngữ mặc định.
- Đảm bảo thứ tự merge đúng (Colors -> Typography -> Languages -> Styles).

### Bước 3: Kiểm tra và Build
- Chạy `dotnet build SysAnti.UI` để đảm bảo không còn lỗi biên dịch.
- (Nếu có thể) Chạy thử ứng dụng để xác nhận lỗi Runtime đã hết.

## 4. Tài liệu hóa
- Cập nhật file log này sau khi hoàn thành.
- Kiểm tra lại các file tài liệu khác để đảm bảo đồng bộ.

## 5. Thời gian thực hiện
- Bắt đầu: Ngay lập tức.
- Dự kiến hoàn thành: 15 phút.
