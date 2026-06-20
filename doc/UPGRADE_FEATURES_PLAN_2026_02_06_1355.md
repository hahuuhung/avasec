# Kế hoạch Nâng cấp Tính năng Nâng cao / Advanced Features Upgrade Plan

(Created: 2026-02-06 13:55:00)

## Mục tiêu / Goals

- Nâng cấp Disk Cleanup, RAM, Startup Manager, Registry Tweaks
- Tối ưu hiệu suất và chạy mượt mà
- Tích hợp các tính năng từ phiên bản cũ

## Các thay đổi dự kiến / Planned Changes

### 1. Dashboard Integration

- Thêm Registry Tweaks button
- Thêm Windows 11 Tweaks button

### 2. RAM Optimizer

- Sử dụng WMI để lấy chính xác Total RAM
- Thêm progress tracking

### 3. Startup Manager

- UI cho danh sách startup items
- Enable/Disable toggle

### 4. Performance

- Async operations
- Proper dispose pattern
- Error handling thân thiện

## Files sẽ sửa / Files to Modify

- `DashboardView.xaml` / `.xaml.cs`
- `RamOptimizerService.cs`
- `StartupManagerService.cs`
