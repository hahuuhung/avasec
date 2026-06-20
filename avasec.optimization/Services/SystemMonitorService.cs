using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AVASec.Core.Interfaces;

namespace AVASec.Optimization.Services
{
    /// <summary>
    /// Dịch vụ giám sát hệ thống thời gian thực (CPU, RAM).
    /// Real-time system monitoring service (CPU, RAM).
    /// </summary>
    public class SystemMonitorService : ISystemMonitorService
    {
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _ramCounter;
        private bool _disposed;

        /// <summary>
        /// Phần trăm sử dụng CPU hiện tại.
        /// Current CPU usage percentage.
        /// </summary>
        public float CpuUsagePercent { get; private set; }

        /// <summary>
        /// Phần trăm sử dụng RAM hiện tại.
        /// Current RAM usage percentage.
        /// </summary>
        public float RamUsagePercent { get; private set; }

        /// <summary>
        /// Tổng RAM (MB).
        /// Total RAM in MB.
        /// </summary>
        public float TotalRamMB { get; private set; }

        /// <summary>
        /// RAM đã sử dụng (MB).
        /// Used RAM in MB.
        /// </summary>
        public float UsedRamMB { get; private set; }

        /// <summary>
        /// RAM khả dụng (MB).
        /// Available RAM in MB.
        /// </summary>
        public float AvailableRamMB { get; private set; }

        public SystemMonitorService()
        {
            InitializeCounters();
            InitializeMemoryInfo();
        }

        private void InitializeCounters()
        {
            try
            {
                // Khởi tạo bộ đếm hiệu suất CPU
                // Initialize CPU performance counter
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                
                // Khởi tạo bộ đếm RAM khả dụng
                // Initialize available RAM counter
                _ramCounter = new PerformanceCounter("Memory", "Available MBytes");

                // Đọc lần đầu để khởi động bộ đếm (giá trị đầu tiên thường là 0)
                // First read to initialize counters (first value is usually 0)
                _cpuCounter.NextValue();
                _ramCounter.NextValue();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemMonitorService] Error initializing counters: {ex.Message}");
            }
        }

        private void InitializeMemoryInfo()
        {
            try
            {
                // Lấy tổng RAM từ WMI hoặc API hệ thống
                // Get total RAM from WMI or system API
                var memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    TotalRamMB = memStatus.ullTotalPhys / (1024f * 1024f);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemMonitorService] Error getting total RAM: {ex.Message}");
                TotalRamMB = 8192; // Giá trị mặc định / Default fallback
            }
        }

        /// <summary>
        /// Cập nhật các giá trị giám sát.
        /// Update monitoring values.
        /// </summary>
        public void Refresh()
        {
            if (_disposed) return;

            try
            {
                // Cập nhật CPU
                // Update CPU
                if (_cpuCounter != null)
                {
                    CpuUsagePercent = _cpuCounter.NextValue();
                }

                // Cập nhật RAM
                // Update RAM
                if (_ramCounter != null)
                {
                    AvailableRamMB = _ramCounter.NextValue();
                    UsedRamMB = TotalRamMB - AvailableRamMB;
                    RamUsagePercent = (UsedRamMB / TotalRamMB) * 100f;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemMonitorService] Error refreshing: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cpuCounter?.Dispose();
            _ramCounter?.Dispose();
        }

        #region Native API for Memory Info
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
        #endregion
    }
}
