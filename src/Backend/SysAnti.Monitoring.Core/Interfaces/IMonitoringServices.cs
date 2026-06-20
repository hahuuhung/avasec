using AVASec.Monitoring.Core.Models;

namespace AVASec.Monitoring.Core.Interfaces;

/// <summary>
/// Device service interface - READ ONLY operations
/// Giao diện dịch vụ thiết bị - Chỉ đọc
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// Get all devices for a user / Lấy tất cả thiết bị của người dùng
    /// </summary>
    Task<List<DeviceInfo>> GetUserDevicesAsync(string userId);
    
    /// <summary>
    /// Get device by ID / Lấy thiết bị theo ID
    /// </summary>
    Task<DeviceInfo?> GetDeviceByIdAsync(string deviceId);
    
    /// <summary>
    /// Get current device status / Lấy trạng thái thiết bị hiện tại
    /// </summary>
    Task<DeviceStatus?> GetDeviceStatusAsync(string deviceId);
    
    /// <summary>
    /// Get optimization history / Lấy lịch sử tối ưu hóa
    /// </summary>
    Task<List<OptimizationHistory>> GetOptimizationHistoryAsync(
        string deviceId, 
        int skip = 0, 
        int take = 50);
    
    /// <summary>
    /// Get scan results / Lấy kết quả quét
    /// </summary>
    Task<List<ScanResult>> GetScanResultsAsync(
        string deviceId, 
        int skip = 0, 
        int take = 50);
    
    /// <summary>
    /// Get metrics data for charts / Lấy dữ liệu metrics cho biểu đồ
    /// </summary>
    Task<MetricsData> GetMetricsAsync(
        string deviceId, 
        DateTime from, 
        DateTime to);
    
    /// <summary>
    /// Get activity logs / Lấy nhật ký hoạt động
    /// </summary>
    Task<List<ActivityLog>> GetActivityLogsAsync(
        string deviceId, 
        int skip = 0, 
        int take = 100);
    
    /// <summary>
    /// Check if device belongs to user / Kiểm tra thiết bị có thuộc người dùng không
    /// </summary>
    Task<bool> IsDeviceOwnedByUserAsync(string deviceId, string userId);
}

/// <summary>
/// Status reporting service interface - For Windows desktop to report status
/// Giao diện dịch vụ báo cáo trạng thái - Cho Windows desktop báo cáo
/// </summary>
public interface IStatusReportingService
{
    /// <summary>
    /// Register new device / Đăng ký thiết bị mới
    /// </summary>
    Task<string> RegisterDeviceAsync(string userId, DeviceInfo deviceInfo);
    
    /// <summary>
    /// Update device status / Cập nhật trạng thái thiết bị
    /// </summary>
    Task UpdateDeviceStatusAsync(string deviceId, DeviceStatus status);
    
    /// <summary>
    /// Report optimization completed / Báo cáo tối ưu hóa hoàn thành
    /// </summary>
    Task ReportOptimizationAsync(string deviceId, OptimizationHistory history);
    
    /// <summary>
    /// Report scan completed / Báo cáo quét hoàn thành
    /// </summary>
    Task ReportScanResultAsync(string deviceId, ScanResult result);
    
    /// <summary>
    /// Log activity / Ghi nhật ký hoạt động
    /// </summary>
    Task LogActivityAsync(string deviceId, ActivityLog activity);
    
    /// <summary>
    /// Update device last seen / Cập nhật lần thấy cuối
    /// </summary>
    Task UpdateLastSeenAsync(string deviceId);
}

/// <summary>
/// Notification service interface / Giao diện dịch vụ thông báo
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send push notification / Gửi thông báo đẩy
    /// </summary>
    Task SendNotificationAsync(
        string userId, 
        string title, 
        string message, 
        Dictionary<string, string>? data = null);
    
    /// <summary>
    /// Send threat alert / Gửi cảnh báo mối đe dọa
    /// </summary>
    Task SendThreatAlertAsync(
        string userId, 
        string deviceName, 
        ThreatInfo threat);
}
