using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace AVASec.Optimization.Services
{
    /// <summary>
    /// Registry Tweaks Service / Dịch vụ tối ưu Registry
    /// Manages Windows registry tweaks for optimization / Quản lý các tweaks Registry để tối ưu Windows
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class RegistryTweaksService
    {
        /// <summary>
        /// Get all available registry tweaks / Lấy tất cả tweaks Registry có sẵn
        /// </summary>
        public List<RegistryTweak> GetAvailableTweaks()
        {
            return new List<RegistryTweak>
            {
                new RegistryTweak
                {
                    Id = "DisableTelemetry",
                    Name = "🔒 Disable Windows Telemetry",
                    NameVi = "Tắt thu thập dữ liệu Windows",
                    Description = "Disable data collection and telemetry",
                    DescriptionVi = "Tắt thu thập dữ liệu và theo dõi",
                    Category = "Bảo mật / Privacy",
                    RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                    ValueName = "AllowTelemetry",
                    EnabledValue = 0,
                    DisabledValue = 1,
                    ValueType = RegistryValueKind.DWord,
                    RequiresAdmin = true
                },
                new RegistryTweak
                {
                    Id = "DisableCortana",
                    Name = "🎙️ Disable Cortana",
                    NameVi = "Tắt Cortana",
                    Description = "Disable Cortana assistant",
                    DescriptionVi = "Tắt trợ lý Cortana",
                    Category = "Bảo mật / Privacy",
                    RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                    ValueName = "AllowCortana",
                    EnabledValue = 0,
                    DisabledValue = 1,
                    ValueType = RegistryValueKind.DWord,
                    RequiresAdmin = true
                },
                new RegistryTweak
                {
                    Id = "DisableAutoRestart",
                    Name = "🔄 Disable Auto Restart",
                    NameVi = "Tắt tự động khởi động lại",
                    Description = "Prevent automatic restart after Windows Update",
                    DescriptionVi = "Ngăn tự động khởi động lại sau cập nhật",
                    Category = "Hệ thống / System",
                    RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                    ValueName = "NoAutoRebootWithLoggedOnUsers",
                    EnabledValue = 1,
                    DisabledValue = 0,
                    ValueType = RegistryValueKind.DWord,
                    RequiresAdmin = true
                },
                new RegistryTweak
                {
                    Id = "DisableDefenderNotifications",
                    Name = "🛡️ Disable Defender Notifications",
                    NameVi = "Tắt thông báo Defender",
                    Description = "Disable Windows Defender notifications",
                    DescriptionVi = "Tắt thông báo Windows Defender",
                    Category = "Hệ thống / System",
                    RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications",
                    ValueName = "DisableNotifications",
                    EnabledValue = 1,
                    DisabledValue = 0,
                    ValueType = RegistryValueKind.DWord,
                    RequiresAdmin = true
                },
                new RegistryTweak
                {
                    Id = "DisableStartupDelay",
                    Name = "⚡ Disable Startup Delay",
                    NameVi = "Tắt độ trễ khởi động",
                    Description = "Remove startup programs delay",
                    DescriptionVi = "Xóa độ trễ chương trình khởi động",
                    Category = "Hiệu suất / Performance",
                    RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Serialize",
                    ValueName = "StartupDelayInMSec",
                    EnabledValue = 0,
                    DisabledValue = null,
                    ValueType = RegistryValueKind.DWord,
                    RequiresAdmin = false
                },
                new RegistryTweak
                {
                    Id = "DisableAnimations",
                    Name = "🎨 Disable Animations",
                    NameVi = "Tắt hiệu ứng động",
                    Description = "Disable window animations for better performance",
                    DescriptionVi = "Tắt hiệu ứng cửa sổ để tăng hiệu suất",
                    Category = "Hiệu suất / Performance",
                    RegistryPath = @"Control Panel\Desktop\WindowMetrics",
                    ValueName = "MinAnimate",
                    EnabledValue = 0,
                    DisabledValue = 1,
                    ValueType = RegistryValueKind.String,
                    RequiresAdmin = false
                },
                new RegistryTweak
                {
                    Id = "EnableGameMode",
                    Name = "🎮 Enable Game Mode",
                    NameVi = "Bật chế độ Game",
                    Description = "Enable Windows Game Mode for better gaming performance",
                    DescriptionVi = "Bật chế độ Game để tăng hiệu suất chơi game",
                    Category = "Hiệu suất / Performance",
                    RegistryPath = @"SOFTWARE\Microsoft\GameBar",
                    ValueName = "AutoGameModeEnabled",
                    EnabledValue = 1,
                    DisabledValue = 0,
                    ValueType = RegistryValueKind.DWord,
                    RequiresAdmin = false
                },
                new RegistryTweak
                {
                    Id = "DisableTransparency",
                    Name = "💎 Disable Transparency",
                    NameVi = "Tắt hiệu ứng trong suốt",
                    Description = "Disable transparency effects",
                    DescriptionVi = "Tắt hiệu ứng trong suốt",
                    Category = "Hiệu suất / Performance",
                    RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    ValueName = "EnableTransparency",
                    EnabledValue = 0,
                    DisabledValue = 1,
                    ValueType = RegistryValueKind.DWord,
                    RequiresAdmin = false
                },
                new RegistryTweak
                {
                    Id = "DisableBingSearch",
                    Name = "🔍 Disable Bing Search",
                    NameVi = "Tắt tìm kiếm Bing",
                    Description = "Remove web search results from Start Menu",
                    DescriptionVi = "Xóa kết quả tìm kiếm web khỏi Start Menu",
                    Category = "Windows 11",
                    RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\Explorer",
                    ValueName = "DisableSearchBoxSuggestions",
                    EnabledValue = 1,
                    DisabledValue = 0,
                    ValueType = RegistryValueKind.DWord,
                    RequiresAdmin = true
                },
                new RegistryTweak
                {
                    Id = "ClassicContextMenu",
                    Name = "🖱️ Classic Context Menu",
                    NameVi = "Menu chuột phải cổ điển",
                    Description = "Restore Windows 10 style context menu",
                    DescriptionVi = "Khôi phục menu chuột phải kiểu Windows 10",
                    Category = "Windows 11",
                    RegistryPath = @"SOFTWARE\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32",
                    ValueName = "", // Default value
                    EnabledValue = "",
                    DisabledValue = null, // Deleting key restores default behavior
                    ValueType = RegistryValueKind.String,
                    RequiresAdmin = false
                },
                new RegistryTweak
                {
                    Id = "DisableLockScreen",
                    Name = "🔒 Disable Lock Screen",
                    NameVi = "Tắt màn hình khóa",
                    Description = "Skip the lock screen at startup",
                    DescriptionVi = "Bỏ qua màn hình khóa khi khởi động",
                    Category = "Hệ thống / System",
                    RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\Personalization",
                    ValueName = "NoLockScreen",
                    EnabledValue = 1,
                    DisabledValue = 0,
                    ValueType = RegistryValueKind.DWord,
                    RequiresAdmin = true
                },
                new RegistryTweak
                {
                    Id = "DisableMouseAccel",
                    Name = "🖱️ Disable Mouse Acceleration",
                    NameVi = "Tắt gia tốc chuột",
                    Description = "Disable 'Enhance pointer precision' for gaming",
                    DescriptionVi = "Tắt 'Enhance pointer precision' để chơi game tốt hơn",
                    Category = "Chơi game / Gaming",
                    RegistryPath = @"Control Panel\Mouse",
                    ValueName = "MouseSpeed",
                    EnabledValue = "0",
                    DisabledValue = "1",
                    ValueType = RegistryValueKind.String,
                    RequiresAdmin = false
                },
                new RegistryTweak
                {
                    Id = "NetworkThrottling",
                    Name = "🌐 Optimize Network",
                    NameVi = "Tối ưu hóa mạng",
                    Description = "Disable network throttling for gaming",
                    DescriptionVi = "Tắt giới hạn băng thông mạng (tốt cho game)",
                    Category = "Chơi game / Gaming",
                    RegistryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                    ValueName = "NetworkThrottlingIndex",
                    EnabledValue = unchecked((int)0xFFFFFFFF),
                    DisabledValue = 10, // Default value is usually 10
                    ValueType = RegistryValueKind.DWord,
                    RequiresAdmin = true
                },
                new RegistryTweak
                {
                    Id = "DisableStickyKeys",
                    Name = "⌨️ Disable Sticky Keys",
                    NameVi = "Tắt Sticky Keys",
                    Description = "Disable Sticky Keys shortcut (Shift 5x)",
                    DescriptionVi = "Tắt phím tắt Sticky Keys (nhấn Shift 5 lần)",
                    Category = "Chơi game / Gaming",
                    RegistryPath = @"Control Panel\Accessibility\StickyKeys",
                    ValueName = "Flags",
                    EnabledValue = "506", // Flags for Off
                    DisabledValue = "510", // Flags for On
                    ValueType = RegistryValueKind.String,
                    RequiresAdmin = false
                }
            };
        }

        /// <summary>
        /// Check if a tweak is currently enabled / Kiểm tra tweak có đang bật
        /// </summary>
        public bool IsTweakEnabled(RegistryTweak tweak)
        {
            try
            {
                RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
                
                using (var key = baseKey.OpenSubKey(tweak.RegistryPath, false))
                {
                    if (key == null)
                        return false;

                    var value = key.GetValue(tweak.ValueName);
                    if (value == null)
                        return false;

                    // Compare with enabled value
                    if (tweak.ValueType == RegistryValueKind.DWord)
                    {
                        int intValue = Convert.ToInt32(value);
                        return intValue == Convert.ToInt32(tweak.EnabledValue);
                    }
                    else if (tweak.ValueType == RegistryValueKind.String)
                    {
                        return value.ToString() == tweak.EnabledValue?.ToString();
                    }

                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Apply a registry tweak / Áp dụng tweak Registry
        /// </summary>
        public (bool Success, string Message) ApplyTweak(RegistryTweak tweak, bool enable)
        {
            try
            {
                RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
                
                using (var key = baseKey.CreateSubKey(tweak.RegistryPath, true))
                {
                    if (key == null)
                    {
                        return (false, "Không thể truy cập registry / Cannot access registry");
                    }

                    var valueToSet = enable ? tweak.EnabledValue : tweak.DisabledValue;

                    if (valueToSet == null)
                    {
                        // Delete the value
                        key.DeleteValue(tweak.ValueName, false);
                    }
                    else
                    {
                        key.SetValue(tweak.ValueName, valueToSet, tweak.ValueType);
                    }

                    string action = enable ? "enabled" : "disabled";
                    string actionVi = enable ? "đã bật" : "đã tắt";
                    return (true, $"Tweak {actionVi} thành công / Tweak {action} successfully");
                }
            }
            catch (UnauthorizedAccessException)
            {
                return (false, "Truy cập bị từ chối. Chạy với quyền Admin. / Access denied. Run as Administrator");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi / Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get only Windows 11 specific tweaks / Lấy các tweak dành riêng cho Windows 11
        /// </summary>
        public List<RegistryTweak> GetWindows11Tweaks()
        {
            // Check if running on Windows 11 (Build >= 22000)
            if (System.Environment.OSVersion.Version.Major >= 10 && System.Environment.OSVersion.Version.Build >= 22000)
            {
                return GetAvailableTweaks().FindAll(t => t.Category == "Windows 11" || t.Category == "Chơi game / Gaming");
            }
            
            // If not Windows 11, return empty list (or just Gaming if appropriate, but user said check for Win11 to show menu)
            return new List<RegistryTweak>();
        }

        /// <summary>
        /// Get general tweaks (excluding Windows 11 specific ones) / Lấy các tweak chung (loại trừ Windows 11)
        /// </summary>
        public List<RegistryTweak> GetGeneralTweaks()
        {
            return GetAvailableTweaks().FindAll(t => t.Category != "Windows 11" && t.Category != "Chơi game / Gaming");
        }

        /// <summary>
        /// Toggle a tweak / Bật/tắt tweak
        /// </summary>
        public (bool Success, string Message) ToggleTweak(RegistryTweak tweak)
        {
            bool isEnabled = IsTweakEnabled(tweak);
            return ApplyTweak(tweak, !isEnabled);
        }
    }

    /// <summary>
    /// Registry tweak model / Mô hình tweak Registry
    /// </summary>
    public class RegistryTweak
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameVi { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DescriptionVi { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string RegistryPath { get; set; } = string.Empty;
        public string ValueName { get; set; } = string.Empty;
        public object? EnabledValue { get; set; }
        public object? DisabledValue { get; set; }
        public RegistryValueKind ValueType { get; set; }
        public bool RequiresAdmin { get; set; }
    }
}
