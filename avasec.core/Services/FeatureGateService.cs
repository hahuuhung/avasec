using System;
using System.Collections.Generic;

namespace AVASec.Core.Services
{
    /// <summary>
    /// Feature gating service for license-based access control
    /// Dịch vụ kiểm soát quyền truy cập tính năng dựa trên giấy phép
    /// </summary>
    public class FeatureGateService
    {
        /// <summary>
        /// License tier enumeration / Các tầng giấy phép
        /// </summary>
        public enum LicenseTier
        {
            Free,       // Basic features only / Chỉ tính năng cơ bản
            Trial,      // All features for 14 days / Tất cả tính năng trong 14 ngày
            Ultra,      // Full features yearly / Đầy đủ tính năng theo năm
            Lifetime    // Full features forever / Đầy đủ tính năng vĩnh viễn
        }

        /// <summary>
        /// Feature identifiers / Mã định danh tính năng
        /// </summary>
        public static class Features
        {
            // Free tier features / Tính năng miễn phí
            public const string SystemMonitor = "SystemMonitor";
            public const string BasicDiskCleanup = "BasicDiskCleanup";
            public const string BasicScan = "BasicScan";
            public const string Settings = "Settings";

            // Pro tier features / Tính năng Pro
            public const string FullDiskCleanup = "FullDiskCleanup";
            public const string VirusScanner = "VirusScanner";
            public const string SystemBooster = "SystemBooster";
            public const string GameBooster = "GameBooster";
            public const string RegistryTweaks = "RegistryTweaks";
            public const string StartupManager = "StartupManager";
            public const string PrivacyGuardian = "PrivacyGuardian";
            public const string FileRecovery = "FileRecovery";
            public const string ProcessManager = "ProcessManager";
            public const string Benchmark = "Benchmark";
            public const string Windows11Tweaks = "Windows11Tweaks";
            public const string SpeedBooster = "SpeedBooster";
            public const string Toolbox = "Toolbox";
        }

        // Features available in Free tier / Tính năng miễn phí
        private static readonly HashSet<string> _freeFeatures = new()
        {
            Features.SystemMonitor,
            Features.BasicDiskCleanup,
            Features.BasicScan,
            Features.Settings
        };

        // Features requiring Pro (Ultra/Lifetime/Trial) / Tính năng yêu cầu Pro
        private static readonly HashSet<string> _proFeatures = new()
        {
            Features.FullDiskCleanup,
            Features.VirusScanner,
            Features.SystemBooster,
            Features.GameBooster,
            Features.RegistryTweaks,
            Features.StartupManager,
            Features.PrivacyGuardian,
            Features.FileRecovery,
            Features.ProcessManager,
            Features.Benchmark,
            Features.Windows11Tweaks,
            Features.SpeedBooster,
            Features.Toolbox
        };

        private LicenseTier _currentTier = LicenseTier.Free;
        private int _remainingDays = 0;
        private string _licenseType = "Free";

        /// <summary>
        /// Current license tier / Tầng giấy phép hiện tại
        /// </summary>
        public LicenseTier CurrentTier => _currentTier;

        /// <summary>
        /// Remaining days for current license / Số ngày còn lại
        /// </summary>
        public int RemainingDays => _remainingDays;

        /// <summary>
        /// Whether the user has a Pro license / Người dùng có giấy phép Pro không
        /// </summary>
        public bool IsPro => _currentTier != LicenseTier.Free;

        /// <summary>
        /// Display name for current tier / Tên hiển thị của tầng hiện tại
        /// </summary>
        public string TierDisplayName => _currentTier switch
        {
            LicenseTier.Free => "Free",
            LicenseTier.Trial => "Trial",
            LicenseTier.Ultra => "Ultra",
            LicenseTier.Lifetime => "Lifetime",
            _ => "Free"
        };

        /// <summary>
        /// Set the current license tier based on license data
        /// Đặt tầng giấy phép dựa trên dữ liệu license
        /// </summary>
        public void SetLicense(string? licenseType, bool isValid, int remainingDays)
        {
            _remainingDays = remainingDays;
            _licenseType = licenseType ?? "Free";

            if (!isValid || string.IsNullOrEmpty(licenseType))
            {
                _currentTier = LicenseTier.Free;
                return;
            }

            _currentTier = licenseType switch
            {
                "Trial" => LicenseTier.Trial,
                "Monthly" => LicenseTier.Ultra,
                "Yearly" => LicenseTier.Ultra,
                "Ultimate" => LicenseTier.Lifetime,
                "Lifetime" => LicenseTier.Lifetime,
                _ => LicenseTier.Free
            };
        }

        /// <summary>
        /// Check if a feature is accessible with current license
        /// Kiểm tra tính năng có thể truy cập với giấy phép hiện tại
        /// </summary>
        public bool CanAccess(string featureId)
        {
            // Free features are always accessible / Tính năng miễn phí luôn truy cập được
            if (_freeFeatures.Contains(featureId))
                return true;

            // Pro features require a valid Pro license / Tính năng Pro yêu cầu giấy phép Pro hợp lệ
            if (_proFeatures.Contains(featureId))
                return IsPro;

            // Unknown features default to accessible / Tính năng không xác định mặc định cho truy cập
            return true;
        }

        /// <summary>
        /// Check if a feature is a Pro feature / Kiểm tra tính năng có phải Pro không
        /// </summary>
        public bool IsProFeature(string featureId)
        {
            return _proFeatures.Contains(featureId);
        }

        /// <summary>
        /// Get the upgrade message for a locked feature
        /// Lấy thông báo nâng cấp cho tính năng bị khoá
        /// </summary>
        public string GetUpgradeMessage(string featureId)
        {
            return $"Tính năng này yêu cầu giấy phép Ultra hoặc Lifetime.\n" +
                   $"This feature requires an Ultra or Lifetime license.\n\n" +
                   $"Nâng cấp ngay để mở khoá tất cả tính năng Pro!\n" +
                   $"Upgrade now to unlock all Pro features!";
        }
    }
}
