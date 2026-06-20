# 🧩 Kế hoạch: Hệ thống Plugin Cyber-Tools (Plugin Architecture Plan)

**Thời gian lập:** 2026-02-07 23:00  
**Phiên bản:** 1.0 - Ultra Concept

## 1. Tầm nhìn (Vision)

Chuyển đổi SysAnti từ một ứng dụng đơn khối (Monolithic) thành một nền tảng (Platform) cho phép mở rộng không giới hạn thông qua các "Ứng dụng nhỏ" (Micro-Apps) hoặc Plugin. Người dùng chỉ cần tải hoặc kích hoạt những công cụ họ thực sự cần.

## 2. Kiến trúc Kỹ thuật (Technical Architecture)

Chúng ta sẽ sử dụng mô hình **Dynamic Loading (Reflection)** kết hợp với **Interface Contract**.

### A. Interface Cơ bản (`ISysAntiPlugin`)

Mọi Plugin phải triển khai interface sau:

```csharp
public interface ICyberTool
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string Category { get; } // Optimizer, Security, Network, etc.
    string IconEmoji { get; }
    bool IsPremium { get; }
    void Execute(); // Hàm khởi chạy plugin
}
```

### B. Plugin Host (`PluginManager`)

- **Quản lý vòng đời**: Phát hiện, nạp (Load), và gỡ (Unload) các tệp `.dll` từ thư mục `Plugins/`.
- **Cơ chế gọi**: Sử dụng `Assembly.LoadFrom` để nạp code động khi người dùng nhấp vào icon.

## 3. Danh sách các Cyber-Tools đề xuất (Proposed Plugins)

| Plugin Name | Chức năng (Function) | Độ ưu tiên |
| :--- | :--- | :--- |
| **Network Fortress** | Giám sát lưu lượng mạng thời gian thực, chặn IP lạ. | Cao |
| **Privacy Shredder** | Xóa file vĩnh viễn chuẩn quân đội (Gutmann/DoD), không thể khôi phục. | Trung bình |
| **Registry Doctor** | Sửa chữa và tối ưu hóa các entries rác trong Windows Registry. | Trung bình |
| **Game Booster Ultra** | Tắt mọi dịch vụ thừa, ưu tiên tài nguyên cho game khi kích hoạt. | Cao |
| **Driver Auditor** | Kiểm tra và nhắc nhở cập nhật các trình điều khiển (Drivers) cũ. | Thấp |
| **Context Menu Dev** | Quản lý và dọn dẹp menu chuột phải của Windows. | Thấp |

## 4. Thiết kế Giao diện "Cyber-Toolbox" (UI Concept)

- **Kiến trúc**: Grid-base (3-4 cột).
- **Trạng thái**: Mỗi card sẽ có trạng thái "Installed", "Not Installed" (Download icon), hoặc "Unlock" (Premium).
- **Hiệu ứng**: Hover vào card sẽ phát sáng màu Neon tương ứng với Category (Security = Red, Optimization = Cyan).

### UI Mockup 2026

![Cyber Toolbox Mockup](C:/Users/Administrator/.gemini/antigravity/brain/e43b32c4-ffb7-4fa7-99b4-abae7560e0ea/cyber_toolbox_plugin_ui_2026_1770472562931.png)

## 5. Lộ trình Triển khai (Roadmap)

### Giai đoạn 1: Foundation (Tuần 1)

- [ ] Tạo Project `SysAnti.Plugin.Core` chứa các Interface.
- [ ] Triển khai `PluginManager` để quét thư mục.
- [ ] Cập nhật UI Dashboard: Thêm tab "Cyber-Toolbox".

### Giai đoạn 2: First Micro-Apps (Tuần 2)

- [ ] Chuyển đổi `DiskCleaner` và `RamOptimizer` hiện có về dạng Plugin.
- [ ] Xây dựng Plugin mới: **Privacy Shredder**.

### Giai đoạn 3: Store & Market (Tuần 3)

- [ ] Giao diện "Plugin Store" để tải xuống tệp DLL từ máy chủ Node.js.
- [ ] Cơ chế Verify Signature cho Plugin (đảm bảo an toàn).

---
**Ghi chú:** Tuân thủ triết lý thiết kế **Cyber-Professional 2026** - *Mọi công cụ đều phải có hiệu ứng mượt mà và giao diện Premium.*
