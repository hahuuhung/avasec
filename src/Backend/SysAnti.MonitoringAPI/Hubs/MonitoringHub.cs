using Microsoft.AspNetCore.SignalR;
using AVASec.Monitoring.Core.Models;

namespace AVASec.MonitoringAPI.Hubs;

/// <summary>
/// SignalR Hub for real-time status broadcasting (ONE-WAY ONLY)
/// Hub SignalR cho phát sóng trạng thái thời gian thực (CHỈ MỘT CHIỀU)
/// </summary>
public class MonitoringHub : Hub
{
    private readonly ILogger<MonitoringHub> _logger;

    public MonitoringHub(ILogger<MonitoringHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Client joins device monitoring group / Client tham gia nhóm giám sát thiết bị
    /// </summary>
    public async Task JoinDeviceGroup(string deviceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"device_{deviceId}");
        _logger.LogInformation(
            "Client {ConnectionId} joined monitoring group for device {DeviceId}",
            Context.ConnectionId, deviceId);
    }

    /// <summary>
    /// Client leaves device monitoring group / Client rời nhóm giám sát thiết bị
    /// </summary>
    public async Task LeaveDeviceGroup(string deviceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"device_{deviceId}");
        _logger.LogInformation(
            "Client {ConnectionId} left monitoring group for device {DeviceId}",
            Context.ConnectionId, deviceId);
    }

    /// <summary>
    /// Broadcast device status update to all monitoring clients
    /// Phát sóng cập nhật trạng thái thiết bị đến tất cả clients giám sát
    /// </summary>
    public async Task BroadcastStatusUpdate(string deviceId, DeviceStatus status)
    {
        await Clients.Group($"device_{deviceId}")
            .SendAsync("StatusUpdated", status);
    }

    /// <summary>
    /// Broadcast optimization completed to all monitoring clients
    /// Phát sóng tối ưu hóa hoàn thành đến tất cả clients giám sát
    /// </summary>
    public async Task BroadcastOptimizationCompleted(string deviceId, OptimizationHistory history)
    {
        await Clients.Group($"device_{deviceId}")
            .SendAsync("OptimizationCompleted", history);
    }

    /// <summary>
    /// Broadcast scan completed to all monitoring clients
    /// Phát sóng quét hoàn thành đến tất cả clients giám sát
    /// </summary>
    public async Task BroadcastScanCompleted(string deviceId, ScanResult result)
    {
        await Clients.Group($"device_{deviceId}")
            .SendAsync("ScanCompleted", result);
    }

    /// <summary>
    /// Broadcast threat alert to all monitoring clients
    /// Phát sóng cảnh báo mối đe dọa đến tất cả clients giám sát
    /// </summary>
    public async Task BroadcastThreatAlert(string deviceId, ThreatInfo threat)
    {
        await Clients.Group($"device_{deviceId}")
            .SendAsync("ThreatDetected", threat);
    }

    /// <summary>
    /// Broadcast activity log to all monitoring clients
    /// Phát sóng nhật ký hoạt động đến tất cả clients giám sát
    /// </summary>
    public async Task BroadcastActivity(string deviceId, ActivityLog activity)
    {
        await Clients.Group($"device_{deviceId}")
            .SendAsync("ActivityLogged", activity);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "Client {ConnectionId} disconnected. Exception: {Exception}",
            Context.ConnectionId, exception?.Message);
        await base.OnDisconnectedAsync(exception);
    }

    // NOTE: NO remote command methods - this is monitoring only!
    // LƯU Ý: KHÔNG có phương thức lệnh từ xa - chỉ giám sát!
}
