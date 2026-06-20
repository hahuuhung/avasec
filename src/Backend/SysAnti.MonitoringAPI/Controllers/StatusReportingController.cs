using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AVASec.Monitoring.Core.Interfaces;
using AVASec.Monitoring.Core.Models;
using System.Security.Claims;

namespace AVASec.MonitoringAPI.Controllers;

/// <summary>
/// Status reporting controller - For Windows desktop to report status
/// Controller báo cáo trạng thái - Cho Windows desktop báo cáo
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatusReportingController : ControllerBase
{
    private readonly IStatusReportingService _statusReportingService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<StatusReportingController> _logger;

    public StatusReportingController(
        IStatusReportingService statusReportingService,
        INotificationService notificationService,
        ILogger<StatusReportingController> logger)
    {
        _statusReportingService = statusReportingService;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Register new device / Đăng ký thiết bị mới
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> RegisterDevice([FromBody] DeviceInfo deviceInfo)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var deviceId = await _statusReportingService.RegisterDeviceAsync(userId, deviceInfo);
        _logger.LogInformation("Device {DeviceId} registered for user {UserId}", deviceId, userId);
        
        return Ok(deviceId);
    }

    /// <summary>
    /// Update device status / Cập nhật trạng thái thiết bị
    /// </summary>
    [HttpPost("{deviceId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(
        string deviceId,
        [FromBody] DeviceStatus status)
    {
        status.DeviceId = deviceId;
        status.Timestamp = DateTime.UtcNow;

        await _statusReportingService.UpdateDeviceStatusAsync(deviceId, status);
        await _statusReportingService.UpdateLastSeenAsync(deviceId);

        // Send alert if threats detected
        if (status.ThreatsCount > 0)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await _notificationService.SendNotificationAsync(
                    userId,
                    "Virus Detected / Phát hiện Virus",
                    $"{status.ThreatsCount} threats detected on your Windows PC",
                    new Dictionary<string, string>
                    {
                        ["deviceId"] = deviceId,
                        ["threatsCount"] = status.ThreatsCount.ToString()
                    });
            }
        }

        return Ok();
    }

    /// <summary>
    /// Report optimization completed / Báo cáo tối ưu hóa hoàn thành
    /// </summary>
    [HttpPost("{deviceId}/optimization")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReportOptimization(
        string deviceId,
        [FromBody] OptimizationHistory history)
    {
        history.DeviceId = deviceId;
        history.Timestamp = DateTime.UtcNow;

        await _statusReportingService.ReportOptimizationAsync(deviceId, history);
        await _statusReportingService.UpdateLastSeenAsync(deviceId);

        _logger.LogInformation(
            "Optimization {Type} reported for device {DeviceId}: {BytesFreed} bytes freed",
            history.Type, deviceId, history.BytesFreed);

        return Ok();
    }

    /// <summary>
    /// Report scan completed / Báo cáo quét hoàn thành
    /// </summary>
    [HttpPost("{deviceId}/scan")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReportScan(
        string deviceId,
        [FromBody] ScanResult result)
    {
        result.DeviceId = deviceId;
        result.ScanTime = DateTime.UtcNow;

        await _statusReportingService.ReportScanResultAsync(deviceId, result);
        await _statusReportingService.UpdateLastSeenAsync(deviceId);

        // Send alerts for threats
        if (result.ThreatsFound > 0)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                foreach (var threat in result.Threats.Where(t => t.Level >= ThreatLevel.High))
                {
                    await _notificationService.SendThreatAlertAsync(
                        userId,
                        "Windows PC", // TODO: Get device name
                        threat);
                }
            }
        }

        _logger.LogInformation(
            "Scan completed for device {DeviceId}: {FilesScanned} files, {ThreatsFound} threats",
            deviceId, result.FilesScanned, result.ThreatsFound);

        return Ok();
    }

    /// <summary>
    /// Log activity / Ghi nhật ký hoạt động
    /// </summary>
    [HttpPost("{deviceId}/activity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> LogActivity(
        string deviceId,
        [FromBody] ActivityLog activity)
    {
        activity.DeviceId = deviceId;
        activity.Timestamp = DateTime.UtcNow;

        await _statusReportingService.LogActivityAsync(deviceId, activity);
        await _statusReportingService.UpdateLastSeenAsync(deviceId);

        return Ok();
    }

    /// <summary>
    /// Heartbeat - Keep device online / Nhịp tim - Giữ thiết bị online
    /// </summary>
    [HttpPost("{deviceId}/heartbeat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Heartbeat(string deviceId)
    {
        await _statusReportingService.UpdateLastSeenAsync(deviceId);
        return Ok();
    }
}
