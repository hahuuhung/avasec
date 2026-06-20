using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AVASec.Core.Services
{
    /// <summary>
    /// Privacy Guardian Service - Anti-tracking, DNS filtering, and privacy protection
    /// Dịch vụ Bảo vệ Quyền riêng tư - Chống theo dõi, lọc DNS và bảo vệ riêng tư
    /// </summary>
    public class PrivacyGuardianService
    {
        // Hosts file path
        private readonly string _hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");
        private readonly string _backupPath;

        // Known tracking domains (subset for demo)
        private static readonly List<string> TrackingDomains = new()
        {
            // Analytics & Tracking
            "google-analytics.com",
            "googletagmanager.com",
            "doubleclick.net",
            "facebook.com/tr",
            "connect.facebook.net",
            "pixel.facebook.com",
            "analytics.twitter.com",
            "t.co",
            "ads.linkedin.com",
            
            // Advertising Networks
            "googlesyndication.com",
            "googleadservices.com",
            "adnxs.com",
            "adsrvr.org",
            "rubiconproject.com",
            "pubmatic.com",
            "criteo.com",
            "outbrain.com",
            "taboola.com",
            
            // Telemetry
            "telemetry.microsoft.com",
            "vortex.data.microsoft.com",
            "settings-win.data.microsoft.com",
            "watson.microsoft.com",
            "data.microsoft.com",
            
            // Social Trackers
            "platform.twitter.com",
            "platform.linkedin.com",
            "widgets.pinterest.com",
            "connect.facebook.net",
            
            // Fingerprinting
            "amplitude.com",
            "mixpanel.com",
            "segment.io",
            "hotjar.com",
            "fullstory.com",
            "mouseflow.com",
            "crazyegg.com"
        };

        // Privacy risks in browser extensions
        private static readonly Dictionary<string, string> RiskyExtensionPatterns = new()
        {
            { "hola", "VPN with data selling concerns" },
            { "webcompanion", "Bundled adware" },
            { "superfish", "SSL MITM attack vector" },
            { "zenmate", "Free VPN with logging" },
            { "browsefox", "Ad injection" },
            { "crossrider", "Ad injection platform" },
            { "ask toolbar", "Search hijacker" },
            { "babylon", "Search hijacker" },
            { "conduit", "Search hijacker & data collection" },
            { "mindspark", "Search hijacker" }
        };

        public class PrivacyReport
        {
            public DateTime ScanTime { get; set; } = DateTime.Now;
            public int TrackersBlocked { get; set; }
            public int TrackerDomainsTotal { get; set; }
            public List<string> BlockedDomains { get; set; } = new();
            public List<BrowserPrivacyIssue> BrowserIssues { get; set; } = new();
            public List<TelemetryStatus> TelemetryStatus { get; set; } = new();
            public HostsFileStatus HostsStatus { get; set; } = new();
            public int PrivacyScore { get; set; }
            public string Grade { get; set; } = "N/A";
        }

        public class BrowserPrivacyIssue
        {
            public string Browser { get; set; } = "";
            public string Issue { get; set; } = "";
            public string Severity { get; set; } = "Low";
            public string Recommendation { get; set; } = "";
        }

        public class TelemetryStatus
        {
            public string Name { get; set; } = "";
            public bool IsEnabled { get; set; }
            public string RegistryPath { get; set; } = "";
            public bool CanDisable { get; set; }
        }

        public class HostsFileStatus
        {
            public bool Exists { get; set; }
            public bool IsWritable { get; set; }
            public int TotalEntries { get; set; }
            public int BlockingEntries { get; set; }
            public bool HasAVASecRules { get; set; }
        }

        public PrivacyGuardianService()
        {
            _backupPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "avasec", "Backups", "hosts_backup.txt"
            );
            Directory.CreateDirectory(Path.GetDirectoryName(_backupPath)!);
        }

        /// <summary>
        /// Run full privacy scan
        /// </summary>
        public async Task<PrivacyReport> RunPrivacyScanAsync(IProgress<string>? status = null)
        {
            var report = new PrivacyReport
            {
                TrackerDomainsTotal = TrackingDomains.Count
            };

            status?.Report("Checking hosts file...");
            report.HostsStatus = await CheckHostsFileAsync();
            report.BlockedDomains = await GetBlockedDomainsAsync();
            report.TrackersBlocked = report.BlockedDomains.Count;

            status?.Report("Scanning browser extensions...");
            report.BrowserIssues = await ScanBrowserExtensionsAsync();

            status?.Report("Checking telemetry settings...");
            report.TelemetryStatus = await CheckTelemetrySettingsAsync();

            // Calculate privacy score
            report.PrivacyScore = CalculatePrivacyScore(report);
            report.Grade = GetPrivacyGrade(report.PrivacyScore);

            status?.Report("Privacy scan complete!");
            return report;
        }

        /// <summary>
        /// Check hosts file status
        /// </summary>
        private async Task<HostsFileStatus> CheckHostsFileAsync()
        {
            var status = new HostsFileStatus();

            try
            {
                status.Exists = File.Exists(_hostsPath);
                if (!status.Exists) return status;

                // Check if writable
                try
                {
                    await using var fs = new FileStream(_hostsPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    status.IsWritable = true;
                }
                catch
                {
                    status.IsWritable = false;
                }

                // Read and analyze entries
                var lines = await File.ReadAllLinesAsync(_hostsPath);
                status.TotalEntries = lines.Count(l => !string.IsNullOrWhiteSpace(l) && !l.TrimStart().StartsWith("#"));
                status.BlockingEntries = lines.Count(l => l.Contains("0.0.0.0") || l.Contains("127.0.0.1"));
                status.HasAVASecRules = lines.Any(l => l.Contains("# AVA Security Privacy Guard"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Hosts check error: {ex.Message}");
            }

            return status;
        }

        /// <summary>
        /// Get list of domains currently blocked in hosts file
        /// </summary>
        private async Task<List<string>> GetBlockedDomainsAsync()
        {
            var blocked = new List<string>();

            try
            {
                if (!File.Exists(_hostsPath)) return blocked;

                var lines = await File.ReadAllLinesAsync(_hostsPath);
                var regex = new Regex(@"^(0\.0\.0\.0|127\.0\.0\.1)\s+(.+)$", RegexOptions.Compiled);

                foreach (var line in lines)
                {
                    var match = regex.Match(line.Trim());
                    if (match.Success)
                    {
                        blocked.Add(match.Groups[2].Value.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Get blocked domains error: {ex.Message}");
            }

            return blocked;
        }

        /// <summary>
        /// Add tracking domains to hosts file (block them)
        /// </summary>
        public async Task<int> EnableTrackingProtectionAsync(IProgress<string>? status = null)
        {
            int addedCount = 0;

            try
            {
                status?.Report("Backing up hosts file...");
                await BackupHostsFileAsync();

                status?.Report("Reading current hosts file...");
                var existingContent = File.Exists(_hostsPath) ? await File.ReadAllTextAsync(_hostsPath) : "";
                var existingDomains = await GetBlockedDomainsAsync();

                var newEntries = new List<string>();
                if (!existingContent.Contains("# AVA Security Privacy Guard"))
                {
                    newEntries.Add("\n# ===================================");
                    newEntries.Add("# AVA Security Privacy Guard - Anti-Tracking");
                    newEntries.Add("# ===================================");
                }

                foreach (var domain in TrackingDomains)
                {
                    if (!existingDomains.Contains(domain, StringComparer.OrdinalIgnoreCase))
                    {
                        newEntries.Add($"0.0.0.0 {domain}");
                        newEntries.Add($"0.0.0.0 www.{domain}");
                        addedCount++;
                    }
                }

                if (newEntries.Count > 0)
                {
                    status?.Report($"Adding {addedCount} tracking domains to block list...");
                    await File.AppendAllLinesAsync(_hostsPath, newEntries);
                }

                status?.Report($"Tracking protection enabled. {addedCount} domains blocked.");
                await FlushDnsCache();
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception("Administrator privileges required to modify hosts file.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to enable tracking protection: {ex.Message}");
            }

            return addedCount;
        }

        /// <summary>
        /// Remove AVA Security tracking protection rules from hosts file
        /// </summary>
        public async Task<int> DisableTrackingProtectionAsync()
        {
            int removedCount = 0;

            try
            {
                if (!File.Exists(_hostsPath)) return 0;

                var lines = await File.ReadAllLinesAsync(_hostsPath);
                var newLines = new List<string>();
                bool inAVASecSection = false;

                foreach (var line in lines)
                {
                    if (line.Contains("# AVA Security Privacy Guard"))
                    {
                        inAVASecSection = true;
                        continue;
                    }

                    if (inAVASecSection)
                    {
                        if (line.StartsWith("#") && !line.Contains("==="))
                        {
                            inAVASecSection = false;
                            newLines.Add(line);
                        }
                        else
                        {
                            removedCount++;
                        }
                    }
                    else
                    {
                        newLines.Add(line);
                    }
                }

                await File.WriteAllLinesAsync(_hostsPath, newLines);
                await FlushDnsCache();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to disable tracking protection: {ex.Message}");
            }

            return removedCount;
        }

        /// <summary>
        /// Backup hosts file
        /// </summary>
        private async Task BackupHostsFileAsync()
        {
            if (File.Exists(_hostsPath))
            {
                await Task.Run(() => File.Copy(_hostsPath, _backupPath, true));
            }
        }

        /// <summary>
        /// Restore hosts file from backup
        /// </summary>
        public async Task RestoreHostsFileAsync()
        {
            if (File.Exists(_backupPath))
            {
                await Task.Run(() => File.Copy(_backupPath, _hostsPath, true));
                await FlushDnsCache();
            }
            else
            {
                throw new Exception("No backup file found.");
            }
        }

        /// <summary>
        /// Flush DNS cache after modifying hosts
        /// </summary>
        private async Task FlushDnsCache()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ipconfig",
                        Arguments = "/flushdns",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                await process.WaitForExitAsync();
            }
            catch { /* Ignore DNS flush errors */ }
        }

        /// <summary>
        /// Scan browser extensions for privacy risks
        /// </summary>
        private async Task<List<BrowserPrivacyIssue>> ScanBrowserExtensionsAsync()
        {
            var issues = new List<BrowserPrivacyIssue>();

            // Check Chrome extensions
            var chromeExtPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Google", "Chrome", "User Data", "Default", "Extensions"
            );
            await ScanExtensionDirectoryAsync(chromeExtPath, "Chrome", issues);

            // Check Edge extensions
            var edgeExtPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft", "Edge", "User Data", "Default", "Extensions"
            );
            await ScanExtensionDirectoryAsync(edgeExtPath, "Edge", issues);

            // Check Firefox extensions
            var firefoxPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Mozilla", "Firefox", "Profiles"
            );
            if (Directory.Exists(firefoxPath))
            {
                foreach (var profile in Directory.GetDirectories(firefoxPath))
                {
                    var extPath = Path.Combine(profile, "extensions");
                    await ScanExtensionDirectoryAsync(extPath, "Firefox", issues);
                }
            }

            return issues;
        }

        private async Task ScanExtensionDirectoryAsync(string path, string browser, List<BrowserPrivacyIssue> issues)
        {
            if (!Directory.Exists(path)) return;

            await Task.Run(() =>
            {
                try
                {
                    foreach (var extDir in Directory.GetDirectories(path))
                    {
                        var extName = Path.GetFileName(extDir).ToLower();
                        
                        // Check for manifest.json to get extension name
                        var manifestFiles = Directory.GetFiles(extDir, "manifest.json", SearchOption.AllDirectories);
                        foreach (var manifest in manifestFiles)
                        {
                            try
                            {
                                var content = File.ReadAllText(manifest).ToLower();
                                
                                foreach (var (pattern, reason) in RiskyExtensionPatterns)
                                {
                                    if (content.Contains(pattern) || extName.Contains(pattern))
                                    {
                                        issues.Add(new BrowserPrivacyIssue
                                        {
                                            Browser = browser,
                                            Issue = $"Risky extension detected: {pattern}",
                                            Severity = "High",
                                            Recommendation = $"Remove this extension. Reason: {reason}"
                                        });
                                        break;
                                    }
                                }
                            }
                            catch { /* Ignore read errors */ }
                        }
                    }
                }
                catch { /* Ignore directory access errors */ }
            });
        }

        /// <summary>
        /// Check Windows telemetry settings
        /// </summary>
        private async Task<List<TelemetryStatus>> CheckTelemetrySettingsAsync()
        {
            var telemetryList = new List<TelemetryStatus>();

            await Task.Run(() =>
            {
                // Windows Telemetry
                telemetryList.Add(CheckRegistryTelemetry(
                    "Windows Telemetry",
                    @"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                    "AllowTelemetry",
                    0 // 0 = Security (minimal), 1 = Basic, 2 = Enhanced, 3 = Full
                ));

                // Advertising ID
                telemetryList.Add(CheckRegistryTelemetry(
                    "Advertising ID",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
                    "Enabled",
                    0
                ));

                // Location Tracking
                telemetryList.Add(CheckRegistryTelemetry(
                    "Location Tracking",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location",
                    "Value",
                    "Deny"
                ));

                // Cortana
                telemetryList.Add(CheckRegistryTelemetry(
                    "Cortana Data Collection",
                    @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                    "AllowCortana",
                    0
                ));

                // WiFi Sense
                telemetryList.Add(CheckRegistryTelemetry(
                    "WiFi Sense",
                    @"SOFTWARE\Microsoft\WcmSvc\wifinetworkmanager\config",
                    "AutoConnectAllowedOEM",
                    0
                ));
            });

            return telemetryList;
        }

        private TelemetryStatus CheckRegistryTelemetry(string name, string path, string valueName, object disabledValue)
        {
            var status = new TelemetryStatus
            {
                Name = name,
                RegistryPath = $"HKLM\\{path}\\{valueName}",
                CanDisable = true
            };

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(path);
                if (key != null)
                {
                    var value = key.GetValue(valueName);
                    if (value != null)
                    {
                        status.IsEnabled = !value.Equals(disabledValue);
                    }
                    else
                    {
                        status.IsEnabled = true; // Default is enabled if key doesn't exist
                    }
                }
                else
                {
                    status.IsEnabled = true;
                }
            }
            catch
            {
                status.IsEnabled = true;
                status.CanDisable = false;
            }

            return status;
        }

        /// <summary>
        /// Disable Windows telemetry (requires admin)
        /// </summary>
        public async Task<int> DisableTelemetryAsync(IProgress<string>? status = null)
        {
            int disabledCount = 0;

            await Task.Run(() =>
            {
                try
                {
                    // Windows Telemetry
                    status?.Report("Disabling Windows Telemetry...");
                    using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection"))
                    {
                        key?.SetValue("AllowTelemetry", 0, RegistryValueKind.DWord);
                        disabledCount++;
                    }

                    // Advertising ID
                    status?.Report("Disabling Advertising ID...");
                    using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo"))
                    {
                        key?.SetValue("Enabled", 0, RegistryValueKind.DWord);
                        disabledCount++;
                    }

                    // Disable DiagTrack service
                    status?.Report("Configuring DiagTrack service...");
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "sc",
                            Arguments = "config DiagTrack start= disabled",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                    disabledCount++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Disable telemetry error: {ex.Message}");
                }
            });

            status?.Report($"Disabled {disabledCount} telemetry settings.");
            return disabledCount;
        }

        /// <summary>
        /// Calculate privacy score
        /// </summary>
        private int CalculatePrivacyScore(PrivacyReport report)
        {
            int score = 100;

            // Deduct for unblocked trackers
            int unblockedTrackers = report.TrackerDomainsTotal - report.TrackersBlocked;
            score -= unblockedTrackers * 2;

            // Deduct for browser issues
            score -= report.BrowserIssues.Count(i => i.Severity == "High") * 15;
            score -= report.BrowserIssues.Count(i => i.Severity == "Medium") * 10;
            score -= report.BrowserIssues.Count(i => i.Severity == "Low") * 5;

            // Deduct for enabled telemetry
            score -= report.TelemetryStatus.Count(t => t.IsEnabled) * 5;

            // Bonus for having AVA Security protection
            if (report.HostsStatus.HasAVASecRules) score += 10;

            return Math.Max(0, Math.Min(100, score));
        }

        /// <summary>
        /// Get privacy grade
        /// </summary>
        private string GetPrivacyGrade(int score)
        {
            return score switch
            {
                >= 90 => "A+ (Excellent Privacy)",
                >= 80 => "A (Strong Privacy)",
                >= 70 => "B (Good Privacy)",
                >= 60 => "C (Moderate Privacy)",
                >= 50 => "D (Weak Privacy)",
                _ => "F (Poor Privacy - Action Required)"
            };
        }
    }

    // File.CopyAsync extension method
    public static class FileExtensions
    {
        public static async Task CopyAsync(string sourceFile, string destFile, bool overwrite = false)
        {
            using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
            using var destStream = new FileStream(destFile, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
            await sourceStream.CopyToAsync(destStream);
        }
    }
}
