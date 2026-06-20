using System;
using System.Collections.Generic;

namespace AVASec.Core.Models
{
    public class AppSettings
    {
        // System Settings
        public bool AutoStart { get; set; } = true;
        public bool RealTimeProtection { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;

        // UI Settings
        public string Language { get; set; } = "vi-EN";
        public bool IsDarkMode { get; set; } = false;

        // Cleaning Settings
        public bool CleanRecycleBin { get; set; } = true;
        public bool CleanTempFiles { get; set; } = true;
        public bool CleanBrowserCache { get; set; } = true;

        // Advanced Tools
        public bool WindowsUpdateEnabled { get; set; } = true; // Default true (normal)
        public List<string> WhitelistedPaths { get; set; } = new List<string>();

        // Schedule
        public DateTime? LastScanDate { get; set; }
        public int AutoScanIntervalHours { get; set; } = 24;

        // Onboarding & Accessibility
        public bool HasCompletedOnboarding { get; set; }
        public double FontScale { get; set; } = 1.0;
        public bool HighContrast { get; set; }
    }
}
