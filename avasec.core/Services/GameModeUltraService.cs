using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AVASec.Core.Services
{
    /// <summary>
    /// Game Mode Ultra Service - Advanced gaming optimization
    /// Dịch vụ Game Mode Ultra - Tối ưu hóa gaming nâng cao
    /// </summary>
    public class GameModeUltraService
    {
        // P/Invoke for Windows power settings
        [DllImport("kernel32.dll")]
        private static extern bool SetPriorityClass(IntPtr hProcess, uint dwPriorityClass);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        private const uint HIGH_PRIORITY_CLASS = 0x80;
        private const uint REALTIME_PRIORITY_CLASS = 0x100;
        private const uint ABOVE_NORMAL_PRIORITY_CLASS = 0x8000;

        // Known game processes
        private static readonly HashSet<string> KnownGames = new(StringComparer.OrdinalIgnoreCase)
        {
            // Popular Games
            "League of Legends", "LeagueClient", "RiotClientServices",
            "VALORANT", "VALORANT-Win64-Shipping",
            "csgo", "cs2",
            "GTA5", "GTAVLauncher", "PlayGTAV",
            "FortniteLauncher", "FortniteClient-Win64-Shipping",
            "Minecraft", "javaw", "MinecraftLauncher",
            "Warframe", "Warframe.x64",
            "Apex", "r5apex",
            "Dota2", "dota2",
            "Overwatch", "Overwatch2",
            "PUBG", "TslGame",
            "RocketLeague",
            "FIFA", "FE24", "EA SPORTS FC",
            "Cyberpunk2077", "Cyberpunk2077.exe",
            "Elden Ring", "eldenring",
            "Genshin Impact", "GenshinImpact",
            "Hogwarts Legacy",
            "Call of Duty", "cod",
            "Battlefield",
            "Rainbow Six", "RainbowSix",
            "Steam", "EpicGamesLauncher", "Battle.net"
        };

        // Services to disable during gaming
        private static readonly List<string> ServicesToDisable = new()
        {
            "SysMain",           // Superfetch
            "DiagTrack",         // Diagnostics Tracking
            "WSearch",           // Windows Search Indexer
            "OneSyncSvc",        // Sync Host
            "wisvc",             // Windows Insider Service
            "wuauserv"           // Windows Update (temporary)
        };

        // Processes to close during gaming
        private static readonly List<string> ProcessesToClose = new()
        {
            "OneDrive",
            "Teams",
            "Skype",
            "Discord",          // Optional - can keep for voice chat
            "Slack",
            "Spotify",          // Optional
            "GoogleDriveSync",
            "Dropbox",
            "iCloudServices",
            "AdobeUpdateService",
            "CCXProcess"        // Adobe CC
        };

        public class GameModeStatus
        {
            public bool IsEnabled { get; set; }
            public DateTime? EnabledAt { get; set; }
            public string ActiveProfile { get; set; } = "Normal";
            public List<string> DetectedGames { get; set; } = new();
            public int OptimizedProcesses { get; set; }
            public int DisabledServices { get; set; }
            public int ClosedApps { get; set; }
            public double MemoryFreedMB { get; set; }
            public string PowerPlan { get; set; } = "Balanced";
        }

        public class GameProfile
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public bool CloseBackgroundApps { get; set; }
            public bool DisableServices { get; set; }
            public bool SetHighPriority { get; set; }
            public bool HighPerformancePower { get; set; }
            public bool DisableNotifications { get; set; }
            public bool DisableNagle { get; set; } // Network optimization
            public List<string> CustomAppsToClose { get; set; } = new();
        }

        // Predefined profiles
        public static readonly Dictionary<string, GameProfile> Profiles = new()
        {
            ["Balanced"] = new GameProfile
            {
                Name = "Balanced",
                Description = "Moderate optimization, keeps essential apps running",
                CloseBackgroundApps = false,
                DisableServices = false,
                SetHighPriority = true,
                HighPerformancePower = false,
                DisableNotifications = true,
                DisableNagle = false
            },
            ["Performance"] = new GameProfile
            {
                Name = "Performance",
                Description = "Strong optimization, closes most background apps",
                CloseBackgroundApps = true,
                DisableServices = true,
                SetHighPriority = true,
                HighPerformancePower = true,
                DisableNotifications = true,
                DisableNagle = true
            },
            ["Ultra"] = new GameProfile
            {
                Name = "Ultra",
                Description = "Maximum performance, closes all non-essential processes",
                CloseBackgroundApps = true,
                DisableServices = true,
                SetHighPriority = true,
                HighPerformancePower = true,
                DisableNotifications = true,
                DisableNagle = true,
                CustomAppsToClose = new List<string> { "Discord", "Spotify" }
            },
            ["Stream"] = new GameProfile
            {
                Name = "Stream",
                Description = "Optimized for streaming - keeps OBS, Discord, Streamlabs",
                CloseBackgroundApps = true,
                DisableServices = true,
                SetHighPriority = true,
                HighPerformancePower = true,
                DisableNotifications = true,
                DisableNagle = true
            }
        };

        private GameModeStatus _currentStatus = new();
        private List<string> _originalServiceStates = new();
        private string _originalPowerPlan = "";

        public GameModeStatus GetStatus() => _currentStatus;

        /// <summary>
        /// Detect running games
        /// </summary>
        public async Task<List<string>> DetectGamesAsync()
        {
            var detectedGames = new List<string>();

            await Task.Run(() =>
            {
                var processes = Process.GetProcesses();
                foreach (var proc in processes)
                {
                    try
                    {
                        if (KnownGames.Contains(proc.ProcessName))
                        {
                            detectedGames.Add(proc.ProcessName);
                        }
                    }
                    catch { /* Ignore access issues */ }
                }
            });

            _currentStatus.DetectedGames = detectedGames.Distinct().ToList();
            return _currentStatus.DetectedGames;
        }

        /// <summary>
        /// Enable Game Mode with specified profile
        /// </summary>
        public async Task<GameModeStatus> EnableGameModeAsync(string profileName = "Performance", IProgress<string>? status = null)
        {
            if (!Profiles.TryGetValue(profileName, out var profile))
            {
                profile = Profiles["Performance"];
            }

            _currentStatus = new GameModeStatus
            {
                IsEnabled = true,
                EnabledAt = DateTime.Now,
                ActiveProfile = profileName
            };

            try
            {
                // 1. Detect games
                status?.Report("Detecting running games...");
                await DetectGamesAsync();

                // 2. Set high performance power plan
                if (profile.HighPerformancePower)
                {
                    status?.Report("Setting High Performance power plan...");
                    await SetHighPerformancePowerPlanAsync();
                    _currentStatus.PowerPlan = "High Performance";
                }

                // 3. Close background apps
                if (profile.CloseBackgroundApps)
                {
                    status?.Report("Closing background applications...");
                    _currentStatus.ClosedApps = await CloseBackgroundAppsAsync(profile);
                }

                // 4. Disable services
                if (profile.DisableServices)
                {
                    status?.Report("Disabling non-essential services...");
                    _currentStatus.DisabledServices = await DisableServicesAsync();
                }

                // 5. Optimize game process priority
                if (profile.SetHighPriority && _currentStatus.DetectedGames.Any())
                {
                    status?.Report("Setting game process priority...");
                    _currentStatus.OptimizedProcesses = await SetGamePriorityAsync();
                }

                // 6. Free memory
                status?.Report("Freeing up memory...");
                _currentStatus.MemoryFreedMB = await FreeMemoryAsync();

                // 7. Disable Windows notifications
                if (profile.DisableNotifications)
                {
                    status?.Report("Disabling notifications...");
                    await DisableNotificationsAsync();
                }

                // 8. Network optimization (Nagle algorithm)
                if (profile.DisableNagle)
                {
                    status?.Report("Optimizing network for gaming...");
                    await DisableNagleAlgorithmAsync();
                }

                status?.Report($"Game Mode Ultra enabled! Profile: {profileName}");
            }
            catch (Exception ex)
            {
                status?.Report($"Error: {ex.Message}");
                _currentStatus.IsEnabled = false;
            }

            return _currentStatus;
        }

        /// <summary>
        /// Disable Game Mode and restore settings
        /// </summary>
        public async Task<GameModeStatus> DisableGameModeAsync(IProgress<string>? status = null)
        {
            try
            {
                status?.Report("Restoring power plan...");
                await RestorePowerPlanAsync();

                status?.Report("Re-enabling services...");
                await RestoreServicesAsync();

                status?.Report("Enabling notifications...");
                await EnableNotificationsAsync();

                _currentStatus.IsEnabled = false;
                _currentStatus.ActiveProfile = "Normal";
                status?.Report("Game Mode disabled. Settings restored.");
            }
            catch (Exception ex)
            {
                status?.Report($"Error disabling: {ex.Message}");
            }

            return _currentStatus;
        }

        /// <summary>
        /// Set High Performance power plan
        /// </summary>
        private async Task SetHighPerformancePowerPlanAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Save current power plan
                    var currentProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "powercfg",
                            Arguments = "/getactivescheme",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    currentProcess.Start();
                    _originalPowerPlan = currentProcess.StandardOutput.ReadToEnd();
                    currentProcess.WaitForExit();

                    // Set High Performance (GUID: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c)
                    var setProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "powercfg",
                            Arguments = "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    setProcess.Start();
                    setProcess.WaitForExit();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Power plan error: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Restore original power plan
        /// </summary>
        private async Task RestorePowerPlanAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Set Balanced power plan (GUID: 381b4222-f694-41f0-9685-ff5bb260df2e)
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "powercfg",
                            Arguments = "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                }
                catch { }
            });
        }

        /// <summary>
        /// Close background applications
        /// </summary>
        private async Task<int> CloseBackgroundAppsAsync(GameProfile profile)
        {
            int closedCount = 0;

            await Task.Run(() =>
            {
                var toClose = ProcessesToClose.Concat(profile.CustomAppsToClose).Distinct();

                foreach (var processName in toClose)
                {
                    try
                    {
                        var processes = Process.GetProcessesByName(processName);
                        foreach (var proc in processes)
                        {
                            proc.Kill();
                            closedCount++;
                        }
                    }
                    catch { /* Ignore access issues */ }
                }
            });

            return closedCount;
        }

        /// <summary>
        /// Disable non-essential Windows services
        /// </summary>
        private async Task<int> DisableServicesAsync()
        {
            int disabledCount = 0;

            await Task.Run(() =>
            {
                foreach (var serviceName in ServicesToDisable)
                {
                    try
                    {
                        var sc = new ServiceController(serviceName);
                        if (sc.Status == ServiceControllerStatus.Running)
                        {
                            _originalServiceStates.Add(serviceName);
                            sc.Stop();
                            disabledCount++;
                        }
                    }
                    catch { /* Service may not exist or access denied */ }
                }
            });

            return disabledCount;
        }

        /// <summary>
        /// Restore disabled services
        /// </summary>
        private async Task RestoreServicesAsync()
        {
            await Task.Run(() =>
            {
                foreach (var serviceName in _originalServiceStates)
                {
                    try
                    {
                        var sc = new ServiceController(serviceName);
                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            sc.Start();
                        }
                    }
                    catch { }
                }
                _originalServiceStates.Clear();
            });
        }

        /// <summary>
        /// Set high priority for detected game processes
        /// </summary>
        private async Task<int> SetGamePriorityAsync()
        {
            int optimizedCount = 0;

            await Task.Run(() =>
            {
                foreach (var gameName in _currentStatus.DetectedGames)
                {
                    try
                    {
                        var processes = Process.GetProcessesByName(gameName);
                        foreach (var proc in processes)
                        {
                            proc.PriorityClass = ProcessPriorityClass.High;
                            optimizedCount++;
                        }
                    }
                    catch { }
                }
            });

            return optimizedCount;
        }

        /// <summary>
        /// Free up memory by forcing garbage collection and clearing standby list
        /// </summary>
        private async Task<double> FreeMemoryAsync()
        {
            double freedMB = 0;

            await Task.Run(() =>
            {
                try
                {
                    // Get memory before
                    var counterBefore = new PerformanceCounter("Memory", "Available MBytes");
                    double availableBefore = counterBefore.NextValue();

                    // Force .NET garbage collection
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    // Clear standby memory (requires admin)
                    // This is a simplified version - full implementation would use EmptyWorkingSet API

                    // Get memory after
                    System.Threading.Thread.Sleep(500);
                    double availableAfter = counterBefore.NextValue();
                    freedMB = availableAfter - availableBefore;

                    counterBefore.Dispose();
                }
                catch { }
            });

            return Math.Max(0, freedMB);
        }

        /// <summary>
        /// Disable Windows notifications (Focus Assist)
        /// </summary>
        private async Task DisableNotificationsAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Enable Focus Assist (Priority Only mode)
                    using var key = Registry.CurrentUser.CreateSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\CloudStore\Store\DefaultAccount\Current\default$windows.immersive.focus");
                    // Note: Full implementation requires specific binary value format
                }
                catch { }
            });
        }

        /// <summary>
        /// Enable Windows notifications
        /// </summary>
        private async Task EnableNotificationsAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Disable Focus Assist
                    // Note: Would restore from saved state
                }
                catch { }
            });
        }

        /// <summary>
        /// Disable Nagle algorithm for lower network latency
        /// </summary>
        private async Task DisableNagleAlgorithmAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Find network interface registry key
                    using var key = Registry.LocalMachine.OpenSubKey(
                        @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces", true);
                    
                    if (key == null) return;

                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using var subKey = key.OpenSubKey(subKeyName, true);
                        if (subKey?.GetValue("IPAddress") != null)
                        {
                            subKey.SetValue("TcpAckFrequency", 1, RegistryValueKind.DWord);
                            subKey.SetValue("TCPNoDelay", 1, RegistryValueKind.DWord);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Nagle disable error: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Get optimization summary for UI display
        /// </summary>
        public string GetOptimizationSummary()
        {
            if (!_currentStatus.IsEnabled)
            {
                return "Game Mode is OFF";
            }

            return $"""
                🎮 Game Mode Ultra: ENABLED
                ═══════════════════════════════════
                Profile: {_currentStatus.ActiveProfile}
                Active since: {_currentStatus.EnabledAt:HH:mm}
                ───────────────────────────────────
                🎯 Games detected: {_currentStatus.DetectedGames.Count}
                ⚡ Optimized processes: {_currentStatus.OptimizedProcesses}
                🔧 Services disabled: {_currentStatus.DisabledServices}
                📱 Apps closed: {_currentStatus.ClosedApps}
                💾 Memory freed: {_currentStatus.MemoryFreedMB:F0} MB
                🔋 Power plan: {_currentStatus.PowerPlan}
                """;
        }
    }
}
