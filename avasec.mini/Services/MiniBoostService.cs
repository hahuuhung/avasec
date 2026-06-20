using System.Diagnostics;
using System.Runtime.InteropServices;
using AVASec.Mini.Models;

namespace AVASec.Mini.Services;

public sealed class MiniBoostService
{
    private readonly MiniCleanService _clean = new();

    [DllImport("psapi.dll")] private static extern bool EmptyWorkingSet(IntPtr hwProc);
    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx { public uint Length; public uint MemoryLoad; public ulong TotalPhys; public ulong AvailPhys; }
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

    public async Task<BoostResult> QuickBoostAsync(IProgress<string>? progress = null, CancellationToken token = default)
    {
        long ramBefore = GetUsedRamMb();
        progress?.Report("Toi uu RAM / Optimizing RAM...");
        int trimmed = await Task.Run(() => TrimWorkingSets(), token);
        progress?.Report("Don rac / Cleaning junk...");
        long cleaned = await _clean.CleanJunkAsync(progress, token);
        long ramAfter = GetUsedRamMb();
        long ramFreed = Math.Max(0, ramBefore - ramAfter);
        return new BoostResult
        {
            BytesCleaned = cleaned,
            RamFreedMb = ramFreed,
            ProcessesTrimmed = trimmed,
            MessageVi = $"Giai phong ~{MiniCleanService.FormatBytes(cleaned)} rac, ~{ramFreed} MB RAM.",
            MessageEn = $"Freed ~{MiniCleanService.FormatBytes(cleaned)} junk, ~{ramFreed} MB RAM."
        };
    }

    private static int TrimWorkingSets()
    {
        int count = 0;
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    var n = process.ProcessName.ToLowerInvariant();
                    if (n.Contains("system") || n.Contains("csrss") || n.Contains("winlogon") || n.Contains("dwm")) continue;
                    if (EmptyWorkingSet(process.Handle)) count++;
                }
                catch { }
            }
        }
        return count;
    }

    private static long GetUsedRamMb()
    {
        var mem = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref mem)) return 0;
        return (long)(mem.TotalPhys / 1024 / 1024 - mem.AvailPhys / 1024 / 1024);
    }
}