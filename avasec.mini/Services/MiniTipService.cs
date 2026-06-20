using AVASec.Mini.Models;

namespace AVASec.Mini.Services;

public static class MiniTipService
{
    public static string GetTip(SystemSnapshot s)
    {
        if (s.RamPercent >= 85) return "RAM cao — bấm Tăng tốc / High RAM — tap Boost.";
        if (s.CpuPercent >= 85) return "CPU cao — đóng app nền / High CPU — close background apps.";
        if (s.DiskPercent >= 90) return "Ổ đĩa đầy — Dọn rác / Disk full — tap Clean.";
        if (s.RamPercent < 55 && s.CpuPercent < 40) return "Hệ thống ổn định / System healthy.";
        return "Mẹo: thử Tăng tốc mỗi giờ / Tip: try Quick Boost hourly.";
    }
}