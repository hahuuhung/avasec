using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AVASec.Mini.Models;

namespace AVASec.Mini.Services;

public sealed class MiniMonitorService : IDisposable
{
    private PerformanceCounter? _cpuCounter;
    private bool _disposed;
    private long _prevIdle;
    private long _prevKernel;
    private long _prevUser;
    private bool _hasCpuSample;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;

        public MemoryStatusEx() => Length = (uint)Marshal.SizeOf<MemoryStatusEx>();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemFileTime
    {
        public uint LowDateTime;
        public uint HighDateTime;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx lpBuffer);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(out SystemFileTime idle, out SystemFileTime kernel, out SystemFileTime user);

    public MiniMonitorService()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _cpuCounter.NextValue();
        }
        catch
        {
            _cpuCounter = null;
        }

        PrimeCpuSample();
    }

    public SystemSnapshot Capture()
    {
        float cpu = ReadCpuPercent();
        float ramPercent = 0, ramUsed = 0, ramTotal = 0;

        try
        {
            var mem = new MemoryStatusEx();
            if (GlobalMemoryStatusEx(mem))
            {
                ramTotal = mem.TotalPhys / 1024f / 1024f;
                ramUsed = ramTotal - mem.AvailPhys / 1024f / 1024f;
                ramPercent = mem.MemoryLoad;
            }
        }
        catch { }

        float diskPercent = 0, diskFreeGb = 0;
        try
        {
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name.StartsWith("C", StringComparison.OrdinalIgnoreCase))
                ?? DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Fixed);
            if (drive != null)
            {
                diskPercent = (float)(100.0 - drive.AvailableFreeSpace * 100.0 / drive.TotalSize);
                diskFreeGb = drive.AvailableFreeSpace / 1024f / 1024f / 1024f;
            }
        }
        catch { }

        return new SystemSnapshot
        {
            CpuPercent = Math.Clamp(cpu, 0, 100),
            RamPercent = Math.Clamp(ramPercent, 0, 100),
            RamUsedMb = ramUsed,
            RamTotalMb = ramTotal,
            DiskPercent = Math.Clamp(diskPercent, 0, 100),
            DiskFreeGb = diskFreeGb
        };
    }

    private void PrimeCpuSample()
    {
        if (!GetSystemTimes(out var idle, out var kernel, out var user))
            return;

        _prevIdle = ToUInt64(idle);
        _prevKernel = ToUInt64(kernel);
        _prevUser = ToUInt64(user);
        _hasCpuSample = true;
    }

    private float ReadCpuPercent()
    {
        var fromTimes = ReadCpuFromSystemTimes();
        if (fromTimes > 0)
            return fromTimes;

        if (_cpuCounter != null)
        {
            try { return Math.Clamp(_cpuCounter.NextValue(), 0, 100); }
            catch { _cpuCounter = null; }
        }

        return fromTimes;
    }

    private float ReadCpuFromSystemTimes()
    {
        if (!GetSystemTimes(out var idle, out var kernel, out var user))
            return 0;

        long idleNow = ToUInt64(idle);
        long kernelNow = ToUInt64(kernel);
        long userNow = ToUInt64(user);

        if (!_hasCpuSample)
        {
            _prevIdle = idleNow;
            _prevKernel = kernelNow;
            _prevUser = userNow;
            _hasCpuSample = true;
            return 0;
        }

        long idleDelta = idleNow - _prevIdle;
        long totalDelta = (kernelNow - _prevKernel) + (userNow - _prevUser);
        _prevIdle = idleNow;
        _prevKernel = kernelNow;
        _prevUser = userNow;

        if (totalDelta <= 0)
            return 0;

        return (float)(100.0 * (totalDelta - idleDelta) / totalDelta);
    }

    private static long ToUInt64(SystemFileTime time) =>
        ((long)time.HighDateTime << 32) | (uint)time.LowDateTime;

    public void Dispose()
    {
        if (_disposed) return;
        _cpuCounter?.Dispose();
        _disposed = true;
    }
}