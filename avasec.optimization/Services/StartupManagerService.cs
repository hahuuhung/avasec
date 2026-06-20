using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AVASec.Optimization.Services
{
    /// <summary>
    /// Startup manager service / Dịch vụ quản lý khởi động
    /// Manages programs that run at Windows startup / Quản lý chương trình chạy khi khởi động Windows
    /// </summary>
    public class StartupManagerService
    {
        /// <summary>
        /// Get list of startup programs / Lấy danh sách chương trình khởi động
        /// </summary>
        public List<StartupItem> GetStartupPrograms()
        {
            var startupItems = new List<StartupItem>();

            // Check Registry Run key / Kiểm tra khóa Registry Run
            AddStartupFromRegistry(startupItems, Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Current User", true);
            AddStartupFromRegistry(startupItems, Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "All Users", true);

            // Check Backup Registry (Disabled items)
            AddStartupFromRegistry(startupItems, Registry.CurrentUser, @"SOFTWARE\AVASecurity\StartupBackup", "Current User (Disabled)", false);
            AddStartupFromRegistry(startupItems, Registry.LocalMachine, @"SOFTWARE\AVASecurity\StartupBackup", "All Users (Disabled)", false);

            // Check Startup folder / Kiểm tra thư mục Startup
            AddStartupFromFolder(startupItems, Environment.SpecialFolder.Startup, "Current User Startup Folder");
            AddStartupFromFolder(startupItems, Environment.SpecialFolder.CommonStartup, "All Users Startup Folder");

            // Populate Usage Stats
            var stats = GetUsageStatistics();
            foreach (var item in startupItems)
            {
                if (stats.TryGetValue(item.Name.ToLower(), out var count))
                {
                    item.UsageCount = count;
                }
            }

            return startupItems;
        }

        /// <summary>
        /// Add startup items from Registry
        /// </summary>
        private void AddStartupFromRegistry(List<StartupItem> items, RegistryKey rootKey, string subKeyPath, string location, bool isEnabled)
        {
            try
            {
                using (var key = rootKey.OpenSubKey(subKeyPath, false))
                {
                    if (key == null) return;
                    
                    foreach (var valueName in key.GetValueNames())
                    {
                        var value = key.GetValue(valueName)?.ToString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            items.Add(new StartupItem
                            {
                                Name = valueName,
                                Command = value,
                                Location = location,
                                IsEnabled = isEnabled,
                                Type = "Registry"
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently handle registry access errors / Xử lý âm thầm lỗi truy cập registry
            }
        }

        /// <summary>
        /// Add startup items from folder / Thêm mục khởi động từ thư mục
        /// </summary>
        private void AddStartupFromFolder(List<StartupItem> items, Environment.SpecialFolder folder, string location)
        {
            try
            {
                string folderPath = Environment.GetFolderPath(folder);
                if (Directory.Exists(folderPath))
                {
                    var files = Directory.GetFiles(folderPath);
                    foreach (var file in files)
                    {
                        items.Add(new StartupItem
                        {
                            Name = Path.GetFileNameWithoutExtension(file),
                            Command = file,
                            Location = location,
                            IsEnabled = true,
                            Type = "Folder"
                        });
                    }
                }
            }
            catch (Exception)
            {
                // Silently handle folder access errors / Xử lý âm thầm lỗi truy cập thư mục
            }
        }

        /// <summary>
        /// Disable a startup item / Vô hiệu hóa mục khởi động
        /// </summary>
        public async Task<(bool Success, string Message)> DisableStartupItemAsync(StartupItem item)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (item.Type == "Registry")
                    {
                        var rootKey = item.Location.Contains("Current User") ? Registry.CurrentUser : Registry.LocalMachine;
                        string runPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                        string backupPath = @"SOFTWARE\AVASecurity\StartupBackup";

                        using (var runKey = rootKey.OpenSubKey(runPath, true))
                        using (var backupKey = rootKey.CreateSubKey(backupPath, true))
                        {
                            if (runKey != null && backupKey != null)
                            {
                                var val = runKey.GetValue(item.Name);
                                if (val != null)
                                {
                                    backupKey.SetValue(item.Name, val);
                                    runKey.DeleteValue(item.Name, false);
                                    item.IsEnabled = false;
                                    return (true, $"Program '{item.Name}' put to sleep / Đã tạm tắt '{item.Name}'");
                                }
                            }
                        }
                    }
                    else if (item.Type == "Folder")
                    {
                        string sourceFile = item.Command;
                        string dir = Path.GetDirectoryName(sourceFile)!;
                        string fileName = Path.GetFileName(sourceFile);
                        string disabledDir = Path.Combine(dir, "AVASecurity_Disabled");
                        
                        if (!Directory.Exists(disabledDir)) Directory.CreateDirectory(disabledDir);
                        
                        string destFile = Path.Combine(disabledDir, fileName);
                        if (File.Exists(destFile)) File.Delete(destFile);
                        File.Move(sourceFile, destFile);
                        
                        item.IsEnabled = false;
                        return (true, $"Program '{item.Name}' moved to disabled folder / Đã tạm tắt '{item.Name}'");
                    }

                    return (false, "Could not disable item / Không thể tắt mục khởi động");
                }
                catch (Exception ex)
                {
                    return (false, $"Error / Lỗi: {ex.Message}");
                }
            });
        }

        public async Task<(bool Success, string Message)> EnableStartupItemAsync(StartupItem item)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (item.Type == "Registry")
                    {
                        var rootKey = item.Location.Contains("Current User") ? Registry.CurrentUser : Registry.LocalMachine;
                        string runPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                        string backupPath = @"SOFTWARE\AVASecurity\StartupBackup";

                        using (var runKey = rootKey.CreateSubKey(runPath, true))
                        using (var backupKey = rootKey.OpenSubKey(backupPath, true))
                        {
                            if (runKey != null && backupKey != null)
                            {
                                var val = backupKey.GetValue(item.Name);
                                if (val != null)
                                {
                                    runKey.SetValue(item.Name, val);
                                    backupKey.DeleteValue(item.Name, false);
                                    item.IsEnabled = true;
                                    return (true, $"Program '{item.Name}' awakened / Đã bật lại '{item.Name}'");
                                }
                            }
                        }
                    }
                    else if (item.Type == "Folder")
                    {
                        string disabledFile = item.Command;
                        // If it's folder type and disabled, Command should point to the file in AVASecurity_Disabled
                        if (disabledFile.Contains("AVASecurity_Disabled"))
                        {
                            string targetDir = Path.GetDirectoryName(Path.GetDirectoryName(disabledFile)!)!;
                            string fileName = Path.GetFileName(disabledFile);
                            string targetFile = Path.Combine(targetDir, fileName);
                            
                            if (File.Exists(targetFile)) File.Delete(targetFile);
                            File.Move(disabledFile, targetFile);
                            
                            item.IsEnabled = true;
                            return (true, $"Program '{item.Name}' restored / Đã bật lại '{item.Name}'");
                        }
                    }

                    return (false, "Could not enable item / Không thể bật lại mục khởi động");
                }
                catch (Exception ex)
                {
                    return (false, $"Error / Lỗi: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Get startup impact / Lấy tác động khởi động
        /// </summary>
        public string GetStartupImpact(StartupItem item)
        {
            // Simple heuristic based on file location / Heuristic đơn giản dựa trên vị trí file
            if (item.Command.Contains("System32") || item.Command.Contains("Windows"))
                return "High / Cao";
            else if (item.Command.Contains("Program Files"))
                return "Medium / Trung bình";
            else
                return "Low / Thấp";
        }

        /// <summary>
        /// Check if AVA Security is set to run at startup / Kiểm tra AVA Security có chạy khi khởi động
        /// </summary>
        public bool IsAVASecurityInStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                if (key != null)
                {
                    var value = key.GetValue("AVASecurity");
                    return value != null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking AVA Security startup / Lỗi kiểm tra khởi động: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Enable AVA Security to run at Windows startup / Bật AVA Security chạy khi khởi động Windows
        /// </summary>
        public (bool Success, string Message) EnableAVASecurityStartup()
        {
            try
            {
                // Get application path / Lấy đường dẫn ứng dụng
                string appPath = Assembly.GetExecutingAssembly().Location;
                
                // For .NET apps, use the exe path / Với ứng dụng .NET, dùng đường dẫn exe
                if (appPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    appPath = appPath.Replace(".dll", ".exe");
                }

                // Add to Registry / Thêm vào Registry
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    // Add with --minimized argument / Thêm với tham số --minimized
                    key.SetValue("AVASecurity", $"\"{appPath}\" --minimized");
                    return (true, "AVA Security will now start with Windows / AVA Security sẽ chạy cùng Windows");
                }
                
                return (false, "Could not access Registry / Không thể truy cập Registry");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to enable startup / Lỗi bật khởi động: {ex.Message}");
            }
        }

        /// <summary>
        /// Disable AVA Security from running at Windows startup / Tắt AVA Security chạy khi khởi động Windows
        /// </summary>
        public (bool Success, string Message) DisableAVASecurityStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    var value = key.GetValue("AVASecurity");
                    if (value != null)
                    {
                        key.DeleteValue("AVASecurity");
                        return (true, "AVA Security removed from startup / Đã xóa AVA Security khỏi khởi động");
                    }
                    return (false, "AVA Security is not in startup / AVA Security không có trong khởi động");
                }
                
                return (false, "Could not access Registry / Không thể truy cập Registry");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to disable startup / Lỗi tắt khởi động: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggle AVA Security startup / Bật/tắt AVA Security khởi động
        /// </summary>
        public (bool Success, string Message) ToggleAVASecurityStartup()
        {
            if (IsAVASecurityInStartup())
            {
                return DisableAVASecurityStartup();
            }
            else
            {
                return EnableAVASecurityStartup();
            }
        }

        public void DisableStartupProgram(string programName)
        {
            // Forward to finding the item first for better handling
            var items = GetStartupPrograms();
            var item = items.FirstOrDefault(i => i.Name.Equals(programName, StringComparison.OrdinalIgnoreCase) && i.IsEnabled);
            if (item != null)
            {
                DisableStartupItemAsync(item).Wait();
            }
        }

        public void EnableStartupProgram(string programName)
        {
            var items = GetStartupPrograms();
            var item = items.FirstOrDefault(i => i.Name.Equals(programName, StringComparison.OrdinalIgnoreCase) && !i.IsEnabled);
            if (item != null)
            {
                EnableStartupItemAsync(item).Wait();
            }
        }

        private Dictionary<string, int> GetUsageStatistics()
        {
            var stats = new Dictionary<string, int>();
            try
            {
                // UserAssist handles execution telemetry
                string userAssistKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist\{CEBFF5CD-ACE2-4F4F-9178-9926F41749EA}\Count";
                using (var key = Registry.CurrentUser.OpenSubKey(userAssistKey))
                {
                    if (key != null)
                    {
                        foreach (var valName in key.GetValueNames())
                        {
                            // ROT13 Decode
                            string decoded = new string(valName.Select(c => 
                                (c >= 'a' && c <= 'm') || (c >= 'A' && c <= 'M') ? (char)(c + 13) :
                                (c >= 'n' && c <= 'z') || (c >= 'N' && c <= 'Z') ? (char)(c - 13) : c).ToArray());
                            
                            string fileName = Path.GetFileNameWithoutExtension(decoded);
                            byte[] data = (byte[])key.GetValue(valName)!;
                            
                            if (data.Length >= 8)
                            {
                                int count = BitConverter.ToInt32(data, 4);
                                if (!stats.ContainsKey(fileName.ToLower()))
                                    stats[fileName.ToLower()] = count;
                            }
                        }
                    }
                }
            }
            catch { }
            return stats;
        }
    }

    /// <summary>
    /// Startup item model / Mô hình mục khởi động
    /// </summary>
    public class StartupItem
    {
        public string Name { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public string Type { get; set; } = string.Empty; // Registry or Folder
        public string Impact { get; set; } = "Medium"; // High, Medium, Low

        public int UsageCount { get; set; } = 0;
        public DateTime? LastRun { get; set; }
    }
}
