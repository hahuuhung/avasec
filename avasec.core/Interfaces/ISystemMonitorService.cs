using System;

namespace AVASec.Core.Interfaces
{
    /// <summary>
    /// Interface cho dịch vụ giám sát hệ thống (CPU, RAM).
    /// Interface for system monitoring service (CPU, RAM).
    /// </summary>
    public interface ISystemMonitorService : IDisposable
    {
        /// <summary>
        /// Phần trăm sử dụng CPU hiện tại (0-100).
        /// Current CPU usage percentage (0-100).
        /// </summary>
        float CpuUsagePercent { get; }

        /// <summary>
        /// Phần trăm sử dụng RAM hiện tại (0-100).
        /// Current RAM usage percentage (0-100).
        /// </summary>
        float RamUsagePercent { get; }

        /// <summary>
        /// Tổng RAM (MB).
        /// Total RAM in MB.
        /// </summary>
        float TotalRamMB { get; }

        /// <summary>
        /// RAM đã sử dụng (MB).
        /// Used RAM in MB.
        /// </summary>
        float UsedRamMB { get; }

        /// <summary>
        /// RAM khả dụng (MB).
        /// Available RAM in MB.
        /// </summary>
        float AvailableRamMB { get; }

        /// <summary>
        /// Cập nhật các giá trị giám sát. Gọi phương thức này để làm mới dữ liệu.
        /// Update monitoring values. Call this method to refresh data.
        /// </summary>
        void Refresh();
    }
}
