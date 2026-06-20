using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AVASec.Monitoring.Core.Interfaces;
using AVASec.Monitoring.Core.Models;
using System.Security.Claims;

namespace AVASec.MonitoringAPI.Controllers;

/// <summary>
/// Monitoring controller - READ ONLY endpoints for monitoring clients
/// Controller giám sát - Endpoints CHỈ ĐỌC cho clients giám sát
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MonitoringController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger<MonitoringController> _logger;

    public MonitoringController(
        IDeviceService deviceService,
        ILogger<MonitoringController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    /// <summary>
    /// Get all devices for current user / Lấy tất cả thiết bị của người dùng hiện tại
    /// </summary>
    [HttpGet("devices")]
    [ProducesResponseType(typeof(List<DeviceInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DeviceInfo>>> GetDevices()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var devices = await _deviceService.GetUserDevicesAsync(userId);
        return Ok(devices);
    }

    /// <summary>
    /// Get device by ID / Lấy thiết bị theo ID
    /// </summary>
    [HttpGet("devices/{deviceId}")]
    [ProducesResponseType(typeof(DeviceInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DeviceInfo>> GetDevice(string deviceId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Check ownership
        if (!await _deviceService.IsDeviceOwnedByUserAsync(deviceId, userId))
        {
            return Forbid();
        }

        var device = await _deviceService.GetDeviceByIdAsync(deviceId);
        if (device == null)
        {
            return NotFound();
        }

        return Ok(device);
    }

    /// <summary>
    /// Get current device status / Lấy trạng thái thiết bị hiện tại
    /// </summary>
    [HttpGet("devices/{deviceId}/status")]
    [ProducesResponseType(typeof(DeviceStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DeviceStatus>> GetDeviceStatus(string deviceId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (!await _deviceService.IsDeviceOwnedByUserAsync(deviceId, userId))
        {
            return Forbid();
        }

        var status = await _deviceService.GetDeviceStatusAsync(deviceId);
        if (status == null)
        {
            return NotFound();
        }

        return Ok(status);
    }

    /// <summary>
    /// Get optimization history / Lấy lịch sử tối ưu hóa
    /// </summary>
    [HttpGet("devices/{deviceId}/history")]
    [ProducesResponseType(typeof(List<OptimizationHistory>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<OptimizationHistory>>> GetOptimizationHistory(
        string deviceId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (!await _deviceService.IsDeviceOwnedByUserAsync(deviceId, userId))
        {
            return Forbid();
        }

        var history = await _deviceService.GetOptimizationHistoryAsync(deviceId, skip, take);
        return Ok(history);
    }

    /// <summary>
    /// Get scan results / Lấy kết quả quét virus
    /// </summary>
    [HttpGet("devices/{deviceId}/scans")]
    [ProducesResponseType(typeof(List<ScanResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ScanResult>>> GetScanResults(
        string deviceId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (!await _deviceService.IsDeviceOwnedByUserAsync(deviceId, userId))
        {
            return Forbid();
        }

        var scans = await _deviceService.GetScanResultsAsync(deviceId, skip, take);
        return Ok(scans);
    }

    /// <summary>
    /// Get metrics data for charts / Lấy dữ liệu metrics cho biểu đồ
    /// </summary>
    [HttpGet("devices/{deviceId}/metrics")]
    [ProducesResponseType(typeof(MetricsData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MetricsData>> GetMetrics(
        string deviceId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (!await _deviceService.IsDeviceOwnedByUserAsync(deviceId, userId))
        {
            return Forbid();
        }

        // Default to last 24 hours
        var fromDate = from ?? DateTime.UtcNow.AddHours(-24);
        var toDate = to ?? DateTime.UtcNow;

        var metrics = await _deviceService.GetMetricsAsync(deviceId, fromDate, toDate);
        return Ok(metrics);
    }

    /// <summary>
    /// Get activity logs / Lấy nhật ký hoạt động
    /// </summary>
    [HttpGet("devices/{deviceId}/activities")]
    [ProducesResponseType(typeof(List<ActivityLog>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ActivityLog>>> GetActivityLogs(
        string deviceId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (!await _deviceService.IsDeviceOwnedByUserAsync(deviceId, userId))
        {
            return Forbid();
        }

        var activities = await _deviceService.GetActivityLogsAsync(deviceId, skip, take);
        return Ok(activities);
    }
}
