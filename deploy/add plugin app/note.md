TÍnh năng System Booter là code ở file nào
Searched
System Booster
0 results
Searched with query System Booster in d:\sysanti-UInew\sysanti-UInew case insensitive
Searched
*Booster*
42 results
Analyzed
SpeedBoosterWindow.xaml.cs#L1-125
Analyzed
d:\sysanti-UInew\sysanti-UInew\SysAnti.Optimization\Services
Tính năng System Booster (hoặc Speed Booster) trong dự án này được chia thành hai phần chính: Giao diện (UI) và Logic xử lý (Services).

Dưới đây là các file liên quan:

1. Phần Giao diện (UI)
Đây là các file bạn đang mở, quy định giao diện người dùng, các nút bấm và hiệu ứng:

XAML:
SpeedBoosterWindow.xaml
Code-behind:
SpeedBoosterWindow.xaml.cs
 (Xử lý sự kiện nút bấm
BoostButton_Click
 và cập nhật biểu đồ CPU/RAM).
2. Phần Logic xử lý (Core Services)
Các file này nằm trong project SysAnti.Optimization, chứa code thực tế để tối ưu hệ thống:

Tối ưu RAM:
RamOptimizerService.cs
 (Dùng để giải phóng bộ nhớ).
Quản lý Tiến trình:
ProcessService.cs
 (Dùng để liệt kê các ứng dụng đang chạy chiếm tài nguyên).
Giám sát Hệ thống:
SystemMonitorService.cs
 (Dùng để lấy thông số % CPU và RAM).
3. Phần Plugin (Mở rộng)
Ngoài ra còn có một bản nâng cao dành riêng cho Game:

Game Booster:
GameBoosterPlugin.cs
 nằm trong thư mục Plugins.
