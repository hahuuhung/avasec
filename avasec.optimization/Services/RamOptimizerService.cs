using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AVASec.Optimization.Services
{
    /// <summary>
    /// RAM optimizer service / Dịch vụ tối ưu RAM
    /// Optimizes memory usage / Tối ưu hóa sử dụng bộ nhớ
    /// </summary>
    public class RamOptimizerService
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        [DllImport("psapi.dll")]
        private static extern bool EmptyWorkingSet(IntPtr hwProc);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
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
        }

        // Cached total memory / Bộ nhớ tổng đã cache
        private static long? _cachedTotalMemoryMB = null;

        /// <summary>
        /// Get accurate total physical memory using Windows API / Lấy chính xác tổng bộ nhớ vật lý
        /// </summary>
        private long GetTotalPhysicalMemoryMB()
        {
            if (_cachedTotalMemoryMB.HasValue)
                return _cachedTotalMemoryMB.Value;

            try
            {
                MEMORYSTATUSEX memInfo = new MEMORYSTATUSEX();
                memInfo.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                
                if (GlobalMemoryStatusEx(ref memInfo))
                {
                    _cachedTotalMemoryMB = (long)(memInfo.ullTotalPhys / 1024 / 1024);
                    return _cachedTotalMemoryMB.Value;
                }
            }
            catch { }

            // Fallback / Dự phòng
            return 16384; // 16 GB default
        }

        /// <summary>
        /// Get current RAM usage / Lấy thông tin sử dụng RAM hiện tại
        /// </summary>
        public RamInfo GetRamInfo()
        {
            long totalMemoryMB = GetTotalPhysicalMemoryMB();

            // Get available memory using Windows API / Lấy bộ nhớ khả dụng
            long availableMB = 0;
            try
            {
                MEMORYSTATUSEX memInfo = new MEMORYSTATUSEX();
                memInfo.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                
                if (GlobalMemoryStatusEx(ref memInfo))
                {
                    availableMB = (long)(memInfo.ullAvailPhys / 1024 / 1024);
                }
            }
            catch
            {
                // Fallback to PerformanceCounter / Dự phòng dùng PerformanceCounter
                try
                {
                    var availableCounter = new PerformanceCounter("Memory", "Available MBytes");
                    availableMB = (long)availableCounter.NextValue();
                }
                catch { availableMB = totalMemoryMB / 2; }
            }

            long usedMemoryMB = totalMemoryMB - availableMB;
            int usagePercentage = (int)((usedMemoryMB * 100) / totalMemoryMB);

            return new RamInfo
            {
                TotalMemoryMB = totalMemoryMB,
                AvailableMemoryMB = availableMB,
                UsedMemoryMB = usedMemoryMB,
                UsagePercentage = usagePercentage
            };
        }

        /// <summary>
        /// Optimize memory with progress callback / Tối ưu bộ nhớ với callback tiến trình
        /// </summary>
        public async Task<(long MemoryFreedMB, int ProcessesOptimized)> OptimizeMemoryAsync(
            IProgress<(int current, int total, string processName)>? progress = null)
        {
            return await Task.Run(() =>
            {
                int processesOptimized = 0;
                long memoryBefore = GetRamInfo().UsedMemoryMB;

                try
                {
                    // Get all processes / Lấy tất cả tiến trình
                    var processes = Process.GetProcesses();
                    int total = processes.Length;
                    int current = 0;

                    foreach (var process in processes)
                    {
                        current++;
                        try
                        {
                            // Skip system processes / Bỏ qua tiến trình hệ thống
                            string procName = process.ProcessName.ToLower();
                            if (procName.Contains("system") || 
                                procName.Contains("csrss") ||
                                procName.Contains("smss") ||
                                procName.Contains("wininit") ||
                                procName.Contains("services") ||
                                procName.Contains("lsass"))
                            {
                                progress?.Report((current, total, $"Skipping {process.ProcessName}"));
                                continue;
                            }

                            // Report progress / Báo cáo tiến trình
                            progress?.Report((current, total, process.ProcessName));

                            // Empty working set / Làm trống working set
                            EmptyWorkingSet(process.Handle);
                            processesOptimized++;
                        }
                        catch
                        {
                            // Skip processes we can't access / Bỏ qua tiến trình không truy cập được
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }

                    // Force garbage collection / Ép buộc thu gom rác
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
                catch (Exception)
                {
                    // Handle errors silently / Xử lý lỗi âm thầm
                }

                long memoryAfter = GetRamInfo().UsedMemoryMB;
                long memoryFreed = Math.Max(0, memoryBefore - memoryAfter);

                return (memoryFreed, processesOptimized);
            });
        }
    }

    /// <summary>
    /// RAM information model / Mô hình thông tin RAM
    /// </summary>
    public class RamInfo
    {
        public long TotalMemoryMB { get; set; }
        public long AvailableMemoryMB { get; set; }
        public long UsedMemoryMB { get; set; }
        public int UsagePercentage { get; set; }
    }
}
