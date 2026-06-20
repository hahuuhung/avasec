namespace AVASec.Monitoring.Core.Models;

/// <summary>
/// Device information model / Mô hình thông tin thiết bị
/// </summary>
public class DeviceInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public bool IsOnline { get; set; }
}

/// <summary>
/// Real-time device status / Trạng thái thiết bị thời gian thực
/// </summary>
public class DeviceStatus
{
    public string DeviceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    
    // System Metrics / Metrics hệ thống
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    
    // Memory Info / Thông tin bộ nhớ
    public long TotalMemoryBytes { get; set; }
    public long AvailableMemoryBytes { get; set; }
    
    // Disk Info / Thông tin đĩa
    public long TotalDiskBytes { get; set; }
    public long FreeDiskBytes { get; set; }
    
    // Security Status / Trạng thái bảo mật
    public int ThreatsCount { get; set; }
    public DateTime? LastScanTime { get; set; }
    public string LastScanResult { get; set; } = string.Empty;
}

/// <summary>
/// Optimization history record / Bản ghi lịch sử tối ưu hóa
/// </summary>
public class OptimizationHistory
{
    public string Id { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public OptimizationType Type { get; set; }
    public bool Success { get; set; }
    public long BytesFreed { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> CleanedItems { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Optimization type enumeration / Liệt kê loại tối ưu hóa
/// </summary>
public enum OptimizationType
{
    DiskCleanup,
    MemoryOptimization,
    StartupOptimization,
    RegistryCleanup
}

/// <summary>
/// Virus scan result / Kết quả quét virus
/// </summary>
public class ScanResult
{
    public string Id { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public DateTime ScanTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int FilesScanned { get; set; }
    public int ThreatsFound { get; set; }
    public List<ThreatInfo> Threats { get; set; } = new();
    public ScanStatus Status { get; set; }
}

/// <summary>
/// Threat information / Thông tin mối đe dọa
/// </summary>
public class ThreatInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string ThreatName { get; set; } = string.Empty;
    public ThreatLevel Level { get; set; }
    public DateTime DetectedAt { get; set; }
    public bool Quarantined { get; set; }
}

/// <summary>
/// Scan status / Trạng thái quét
/// </summary>
public enum ScanStatus
{
    Completed,
    InProgress,
    Failed,
    Cancelled
}

/// <summary>
/// Threat level / Mức độ đe dọa
/// </summary>
public enum ThreatLevel
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Metrics data for charts / Dữ liệu metrics cho biểu đồ
/// </summary>
public class MetricsData
{
    public string DeviceId { get; set; } = string.Empty;
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<MetricPoint> CpuMetrics { get; set; } = new();
    public List<MetricPoint> MemoryMetrics { get; set; } = new();
    public List<MetricPoint> DiskMetrics { get; set; } = new();
}

/// <summary>
/// Single metric point / Điểm metric đơn
/// </summary>
public class MetricPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
}

/// <summary>
/// Activity log entry / Mục nhật ký hoạt động
/// </summary>
public class ActivityLog
{
    public string Id { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public ActivityType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Activity type / Loại hoạt động
/// </summary>
public enum ActivityType
{
    Optimization,
    Scan,
    ThreatDetected,
    SystemStartup,
    SystemShutdown,
    Error
}
