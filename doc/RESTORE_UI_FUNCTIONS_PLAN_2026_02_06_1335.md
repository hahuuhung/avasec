# Kế hoạch Khôi phục Chức năng UI / UI Function Restoration Plan

(Created: 2026-02-06 13:35:00)

## Vấn đề / Problem

DashboardView mới có giao diện đẹp nhưng thiếu các chức năng thực tế của MainWindow cũ.

## Các chức năng bị thiếu / Missing Functionalities

### Event Handlers

- Settings Button - chưa có Click handler
- Disk Cleanup - chưa có Click handler  
- RAM Optimize - chưa có Click handler
- Quick Scan - chưa có Click handler
- Startup Manager - chưa có Click handler

### Backend Services

- Real-time CPU/RAM monitoring (DispatcherTimer)
- License information loading
- Virus database info & updates
- System health status

## Giải pháp / Solution

Sao chép logic từ [`MainWindow.xaml.cs`](file:///f:/VStudio/SysAnti/SysAnti.UI/Views/MainWindow.xaml.cs) vào [`DashboardView.xaml.cs`](file:///f:/VStudio/SysAnti/SysAnti.UI/Views/DashboardView.xaml.cs) và cập nhật XAML để thêm event handlers.

## Files cần sửa / Files to Modify

- `DashboardView.xaml` - Thêm Click events
- `DashboardView.xaml.cs` - Thêm services và event handlers
