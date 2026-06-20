using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AVASec.Optimization.Services
{
    /// <summary>
    /// Professional Disk Cleaner Service
    /// Provides deep cleaning for System, Browsers, Applications, and Documents
    /// </summary>
    public class DiskCleanerService
    {
        // P/Invoke for Recycle Bin
        [DllImport("shell32.dll")]
        static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);

        const uint SHERB_NOCONFIRMATION = 0x00000001;
        const uint SHERB_NOPROGRESSUI = 0x00000002;
        const uint SHERB_NOSOUND = 0x00000004;

        public async Task<(bool Success, long BytesFreed, int FilesRemoved)> CleanLocations(List<CleanupLocation> locations)
        {
            long bytesFreed = 0;
            int filesRemoved = 0;

            await Task.Run(() =>
            {
                foreach (var loc in locations)
                {
                    if (loc.Id == "sys_recycle_bin")
                    {
                        try { SHEmptyRecycleBin(IntPtr.Zero, null, SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND); } catch { }
                        continue;
                    }

                    if (loc.Path != null && Directory.Exists(loc.Path))
                    {
                        try {
                            var files = Directory.GetFiles(loc.Path, "*", SearchOption.AllDirectories);
                            foreach (var file in files)
                            {
                                try {
                                    var info = new FileInfo(file);
                                    long size = info.Length;
                                    File.Delete(file);
                                    bytesFreed += size;
                                    filesRemoved++;
                                } catch { }
                            }
                            // Try to remove empty dirs? Optional.
                        } catch { } // Best effort
                    }
                    else if (loc.Path != null && File.Exists(loc.Path))
                    {
                         try { 
                            var info = new FileInfo(loc.Path);
                            long size = info.Length;
                            File.Delete(loc.Path); 
                            bytesFreed += size;
                            filesRemoved++;
                         } catch { }
                    }
                }
            });

            return (true, bytesFreed, filesRemoved);
        }

        /// <summary>
        /// Get all cleanable locations categorized
        /// </summary>
        public List<CleanupLocation> GetCleanableLocations()
        {
            var locations = new List<CleanupLocation>();

            // 1. SYSTEM CLEANUP
            AddSystemLocations(locations);

            // 2. BROWSER CLEANUP
            AddBrowserLocations(locations);

            // 3. APPLICATION CLEANUP
            AddApplicationLocations(locations);

            // 4. DOCUMENT & HISTORY
            AddDocumentLocations(locations);

            // 5. LARGE FILES (Files > 50MB in Downloads/Documents)
            AddLargeFileLocations(locations);

            // 6. OPTIMIZATION & REPAIR
            AddOptimizationLocations(locations);

            return locations;
        }

        public async Task<(long BytesFreed, int FilesDeleted)> CleanSystemTempAsync()
        {
            var locations = new List<string> { "sys_temp_win", "sys_temp_user" };
            var result = await CleanSelectedLocationsAsync(locations);
            return (result.BytesCleaned, result.FilesDeleted);
        }

        private void AddSystemLocations(List<CleanupLocation> locations)
        {
            // Windows Temp
            AddLocation(locations, "sys_temp_win", "Windows Temp / Thư mục Temp Windows", "System Temporary Files / Tệp tạm hệ thống", "System", 
                Path.GetTempPath());

            // User Temp
            AddLocation(locations, "sys_temp_user", "User Temp / Thư mục Temp người dùng", "User Temporary Files / Tệp tạm người dùng", "System",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"));

            // Recycle Bin
            locations.Add(new CleanupLocation
            {
                Id = "sys_recycle_bin",
                Name = "Thùng rác / Recycle Bin",
                Description = "Các tệp đã xóa / Deleted files",
                Category = "System",
                Path = "Recycle Bin",
                EstimatedSize = 0, // Hard to calc accurately without scanning all drives, but could iterate $Recycle.Bin
                IsSafe = true
            });

            // Windows Error Reporting
            AddLocation(locations, "sys_wer", "Windows Error Reporting / Báo cáo lỗi Windows", "Crash reports / Báo cáo sự cố", "System",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "WER"));

            // Windows Prefetch
            AddLocation(locations, "sys_prefetch", "Windows Prefetch / Bộ nhớ đệm khởi động", "Startup cache / Bộ nhớ đệm khởi động", "System",
                Path.Combine(Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows", "Prefetch"));

            // Memory Dumps
            string sysRoot = Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows";
            AddLocation(locations, "sys_minidump", "Memory Dumps / File Dump bộ nhớ", "System crash dumps / File lỗi hệ thống", "System",
                Path.Combine(sysRoot, "Minidump"));
            
            // Windows Defender History
            AddLocation(locations, "sys_defender", "Windows Defender History / Lịch sử Windows Defender", "Scan history / Lịch sử quét", "System",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Windows Defender", "Scans", "History", "Results"));

            // DNS Cache (Virtual location)
            locations.Add(new CleanupLocation
            {
                Id = "sys_dns",
                Name = "DNS Cache / Bộ nhớ đệm DNS",
                Description = "Cache địa chỉ mạng / Network address cache",
                Category = "System",
                Path = "Command",
                EstimatedSize = 0,
                IsSafe = true
            });

             // Event Logs (Virtual location)
            locations.Add(new CleanupLocation
            {
                Id = "sys_event_logs",
                Name = "Windows Event Logs / Nhật ký sự kiện Windows",
                Description = "Nhật ký hệ thống / System logs",
                Category = "System",
                Path = "System Logs",
                EstimatedSize = 0,
                IsSafe = true
            });
        }

        private void AddBrowserLocations(List<CleanupLocation> locations)
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Chrome
            string chromeUser = Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default");
            if (Directory.Exists(chromeUser))
            {
                AddLocation(locations, "chrome_cache", "Google Chrome Cache", "Internet Cache / Cache Internet", "Browsers", Path.Combine(chromeUser, "Cache"));
                AddLocation(locations, "chrome_cookies", "Google Chrome Cookies", "Website Cookies (Warning: Logs you out) / Cookies (Lưu ý: Sẽ đăng xuất bạn)", "Browsers", Path.Combine(chromeUser, "Network"), isSafe: false); // Cookies often in Network folder now or "Cookies" file
            }

            // Edge
            string edgeUser = Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default");
            if (Directory.Exists(edgeUser))
            {
                AddLocation(locations, "edge_cache", "Microsoft Edge Cache", "Internet Cache / Cache Internet", "Browsers", Path.Combine(edgeUser, "Cache"));
                AddLocation(locations, "edge_cookies", "Microsoft Edge Cookies", "Website Cookies (Warning: Logs you out) / Cookies (Lưu ý: Sẽ đăng xuất bạn)", "Browsers", Path.Combine(edgeUser, "Network"), isSafe: false);
            }

            // Firefox
            string firefoxProfiles = Path.Combine(appData, "Mozilla", "Firefox", "Profiles");
            if (Directory.Exists(firefoxProfiles))
            {
                // Just scan first profile for this demo
                var profiles = Directory.GetDirectories(firefoxProfiles);
                foreach(var profile in profiles)
                {
                    string profileName = Path.GetFileName(profile);
                    // Cache is usually in LocalAppData, not Roaming
                    string firefoxLocal = Path.Combine(localAppData, "Mozilla", "Firefox", "Profiles", profileName);
                    
                    if (Directory.Exists(firefoxLocal))
                    {
                        AddLocation(locations, $"firefox_cache_{profileName}", $"Firefox Cache ({profileName})", "Internet Cache / Cache Internet", "Browsers", Path.Combine(firefoxLocal, "cache2"));
                    }
                }
            }

            // Opera
            string operaCache = Path.Combine(localAppData, "Opera Software", "Opera Stable", "Cache");
            if (Directory.Exists(operaCache))
                AddLocation(locations, "opera_cache", "Opera Cache", "Internet Cache / Cache Internet", "Browsers", operaCache);
            
            // Brave
            string braveUser = Path.Combine(localAppData, "BraveSoftware", "Brave-Browser", "User Data", "Default");
            if (Directory.Exists(braveUser))
                 AddLocation(locations, "brave_cache", "Brave Cache", "Internet Cache / Cache Internet", "Browsers", Path.Combine(braveUser, "Cache"));
        }

        private void AddApplicationLocations(List<CleanupLocation> locations)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Discord
            AddLocation(locations, "app_discord_cache", "Discord Cache", "Image & Message Cache / Cache hình ảnh & tin nhắn", "Applications", 
                Path.Combine(appData, "discord", "Cache"));
            AddLocation(locations, "app_discord_code", "Discord Code Cache", "Internal Cache / Cache nội bộ", "Applications", 
                Path.Combine(appData, "discord", "Code Cache"));

            // VS Code
            AddLocation(locations, "app_vscode_cache", "VS Code Cache", "Editor Cache / Cache trình soạn thảo", "Applications", 
                Path.Combine(appData, "Code", "Cache"));
            AddLocation(locations, "app_vscode_cacheddata", "VS Code Cached Data", "Internal Cache / Cache nội bộ", "Applications", 
                Path.Combine(appData, "Code", "CachedData"));

            // Spotify
            AddLocation(locations, "app_spotify", "Spotify Cache", "Music & Image Cache / Cache nhạc & hình ảnh", "Applications", 
                Path.Combine(localAppData, "Spotify", "Storage"));

            // Steam
            // Steam path varies, assume default for now or check Registry (omitted for simplicity)
            // Common: C:\Program Files (x86)\Steam\appcache
        }

        private void AddDocumentLocations(List<CleanupLocation> locations)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Windows Recent
            AddLocation(locations, "doc_recent", "Recent Documents / Tài liệu gần đây", "Shortcut history of opened files / Lịch sử lối tắt tệp đã mở", "Documents", 
                Path.Combine(appData, "Microsoft", "Windows", "Recent"));

            // Office Unsaved
            AddLocation(locations, "doc_office_unsaved", "Office Unsaved Files / Tệp Office chưa lưu", "Leftover unsaved documents / Các tài liệu chưa lưu còn sót lại", "Documents", 
                Path.Combine(localAppData, "Microsoft", "Office", "UnsavedFiles"));

            // Adobe Reader
            AddLocation(locations, "doc_adobe", "Adobe Reader Cache / Cache Adobe Reader", "PDF Thumbnail Cache / Cache hình thu nhỏ PDF", "Documents", 
                Path.Combine(localAppData, "Adobe", "Acrobat", "DC", "Cache")); // Version varies (DC, 2015, etc)

            // Office Temp Files (Virtual/Search based)
            locations.Add(new CleanupLocation
            {
                Id = "doc_office_temp",
                Name = "Office Temporary Files / Tệp tạm Office",
                Description = "Temporary files (~$*) in Documents / Các tệp tạm (~$*) trong Documents",
                Category = "Documents",
                Path = "My Documents",
                EstimatedSize = CalculateOfficeTempSize(),
                IsSafe = true
            });
        }

        private void AddLargeFileLocations(List<CleanupLocation> locations)
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloads = Path.Combine(userProfile, "Downloads");
            
            // "Large Files" is virtual, we scan specific folders for files > 100MB
            locations.Add(new CleanupLocation
            {
                Id = "large_files_downloads",
                Name = "Large Files (Downloads) / File lớn (Downloads)",
                Description = "Files > 100MB in Downloads / Các file > 100MB trong Downloads",
                Category = "Large Files",
                Path = downloads,
                EstimatedSize = CalculateLargeFilesSize(downloads, 100 * 1024 * 1024),
                IsSafe = false // User must review
            });
        }

        private void AddOptimizationLocations(List<CleanupLocation> locations)
        {
            // Registry Issues (Simulation/Safe Keys)
            locations.Add(new CleanupLocation
            {
                Id = "opt_registry_mru",
                Name = "Registry MRU Lists / Lịch sử Registry (MRU)",
                Description = "Most Recently Used lists in Registry / Danh sách đã dùng gần đây",
                Category = "Registry Repair",
                Path = "Registry",
                EstimatedSize = 0,
                IsSafe = true
            });

            locations.Add(new CleanupLocation
            {
                Id = "opt_registry_missing_dlls",
                Name = "Missing Shared DLLs / Thiếu Shared DLLs",
                Description = "Invalid references to DLLs / Tham chiếu DLL không hợp lệ",
                Category = "Registry Repair",
                Path = "Registry",
                EstimatedSize = 0,
                IsSafe = true
            });

            // Windows Optimization
            locations.Add(new CleanupLocation
            {
                Id = "opt_win_delivery_optimization",
                Name = "Delivery Optimization Files / Tối ưu phân phối",
                Description = "Windows Update delivery cache / Cache phân phối cập nhật",
                Category = "Computer Optimization",
                Path = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows", "SoftwareDistribution", "DeliveryOptimization"),
                EstimatedSize = CalculateFolderSize(Path.Combine(Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows", "SoftwareDistribution", "DeliveryOptimization")),
                IsSafe = true
            });
        }

        private long CalculateLargeFilesSize(string path, long minSize)
        {
            if (!Directory.Exists(path)) return 0;
            try
            {
                return Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                    .Select(f => new FileInfo(f))
                    .Where(f => f.Length > minSize)
                    .Sum(f => f.Length);
            }
            catch { return 0; }
        }

        private void AddLocation(List<CleanupLocation> list, string id, string name, string desc, string category, string path, bool isSafe = true)
        {
            if (Directory.Exists(path))
            {
                list.Add(new CleanupLocation
                {
                    Id = id,
                    Name = name,
                    Description = desc,
                    Category = category,
                    Path = path,
                    EstimatedSize = CalculateFolderSize(path),
                    IsSafe = isSafe
                });
            }
        }

        public async Task<(long BytesCleaned, int FilesDeleted, List<string> Errors)> CleanSelectedLocationsAsync(List<string> selectedIds)
        {
            return await Task.Run(() =>
            {
                long totalBytes = 0;
                int filesDeleted = 0;
                var errors = new List<string>();

                var locations = GetCleanableLocations();
                var targets = locations.Where(l => selectedIds.Contains(l.Id)).ToList();

                foreach (var target in targets)
                {
                    try
                    {
                        // Special Handlers
                        if (target.Id == "sys_recycle_bin")
                        {
                            try 
                            { 
                                SHEmptyRecycleBin(IntPtr.Zero, null, SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND); 
                            } 
                            catch (Exception ex) { errors.Add($"Recycle Bin error: {ex.Message}"); }
                            continue;
                        }
                        
                        if (target.Id == "sys_dns")
                        {
                            RunCommand("ipconfig", "/flushdns");
                            continue;
                        }

                        if (target.Id == "sys_event_logs")
                        {
                            ClearEventLogs(errors);
                            continue;
                        }


                        if (target.Id == "doc_office_temp")
                        {
                           var res = CleanOfficeTempFiles();
                           totalBytes += res.BytesFreed;
                           filesDeleted += res.FilesDeleted;
                           errors.AddRange(res.Errors);
                           continue;
                        }

                        // Directory Cleaning
                        if (Directory.Exists(target.Path))
                        {
                            var result = CleanDirectory(target.Path);
                            totalBytes += result.BytesFreed;
                            filesDeleted += result.FilesDeleted;
                            errors.AddRange(result.Errors);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error cleaning {target.Name}: {ex.Message}");
                    }
                }

                return (totalBytes, filesDeleted, errors);
            });
        }

        private (long BytesFreed, int FilesDeleted, List<string> Errors) CleanDirectory(string path)
        {
            long bytesFreed = 0;
            int filesDeleted = 0;
            var errors = new List<string>();

            if (!Directory.Exists(path)) return (0, 0, errors);

            try
            {
                // Use safe enumeration to avoid access denied crashes
                var files = GetFilesSafe(path);
                foreach (var file in files)
                {
                    try
                    {
                        var info = new FileInfo(file);
                        
                        // Security check: Don't delete system critical files if path is somehow root
                        if (info.DirectoryName != null && info.DirectoryName.Length < 4) continue; 

                        long size = info.Length;
                        if (!IsFileLocked(info))
                        {
                            File.Delete(file);
                            bytesFreed += size;
                            filesDeleted++;
                        }
                    }
                    catch (Exception ex) { errors.Add($"Failed to delete {Path.GetFileName(file)}: {ex.Message}"); }
                }
            }
            catch (Exception ex) { errors.Add($"Access denied {path}: {ex.Message}"); }

            return (bytesFreed, filesDeleted, errors);
        }

        private IEnumerable<string> GetFilesSafe(string rootPath)
        {
            if (!Directory.Exists(rootPath)) yield break;

            var pending = new Stack<string>();
            pending.Push(rootPath);

            while (pending.Count > 0)
            {
                var path = pending.Pop();
                string[]? next = null;
                
                try
                {
                    next = Directory.GetFiles(path);
                }
                catch (UnauthorizedAccessException) { }
                catch (DirectoryNotFoundException) { }
                catch (IOException) { }

                if (next != null)
                {
                    foreach (var file in next)
                        yield return file;
                }

                try
                {
                    var subDirs = Directory.GetDirectories(path);
                    foreach (var subdir in subDirs)
                        pending.Push(subdir);
                }
                catch (UnauthorizedAccessException) { }
                catch (DirectoryNotFoundException) { }
                catch (IOException) { }
            }
        }

        private long CalculateFolderSize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            try
            {
                return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                    .Select(f => new FileInfo(f))
                    .Sum(f => f.Length);
            }
            catch { return 0; }
        }

        private bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }


        public List<CleanupItem> GetCleanupItems(string locationId)
        {
            var items = new List<CleanupItem>();
            var locations = GetCleanableLocations();
            var target = locations.FirstOrDefault(l => l.Id == locationId);

            if (target == null) return items;

            // Handle virtual/special locations
            if (target.Id == "sys_event_logs") return items; // Can't list logs easily as files
            if (target.Id == "sys_dns") return items;

            if (target.Id == "doc_office_temp")
            {
                 string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                 try
                 {
                     var files = Directory.GetFiles(docsPath, "~$*.*", SearchOption.TopDirectoryOnly);
                     foreach (var file in files) AddFileItem(items, file);
                 }
                 catch {}
                 return items;
            }

            if (target.Id == "large_files_downloads")
            {
                 // Scan downloads for > 100MB
                 string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                 string downloads = Path.Combine(userProfile, "Downloads");
                 try 
                 {
                     var files = Directory.GetFiles(downloads, "*", SearchOption.TopDirectoryOnly);
                     foreach (var file in files) 
                     {
                         try {
                             if (new FileInfo(file).Length > 100 * 1024 * 1024) AddFileItem(items, file);
                         } catch {}
                     }
                 } catch {}
                 return items;
            }
            
            if (target.Category == "Registry Repair")
            {
                // Return simulation items
                items.Add(new CleanupItem { FileName = "Invalid Key: HKLM/.../Obsolete", FilePath = "Registry", Size = 0, DateModified = DateTime.Now });
                return items;
            }

            // Directory scan
            if (Directory.Exists(target.Path))
            {
                try
                {
                    var files = Directory.GetFiles(target.Path, "*", SearchOption.AllDirectories);
                    foreach (var file in files) AddFileItem(items, file);
                }
                catch {}
            }

            return items;
        }

        private void AddFileItem(List<CleanupItem> list, string filePath)
        {
            try
            {
                var info = new FileInfo(filePath);
                if (info.DirectoryName.Length < 4) return; // Safety
                
                list.Add(new CleanupItem
                {
                    FilePath = filePath,
                    FileName = info.Name,
                    Size = info.Length,
                    DateModified = info.LastWriteTime
                });
            }
            catch {}
        }

        private long CalculateOfficeTempSize()
        {
            try
            {
                string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return Directory.GetFiles(docsPath, "~$*.*", SearchOption.TopDirectoryOnly)
                    .Select(f => new FileInfo(f))
                    .Sum(f => f.Length);
            }
            catch { return 0; }
        }

        private (long BytesFreed, int FilesDeleted, List<string> Errors) CleanOfficeTempFiles()
        {
            long bytes = 0;
            int count = 0;
            var errors = new List<string>();
            string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            try
            {
               var files = Directory.GetFiles(docsPath, "~$*.*", SearchOption.TopDirectoryOnly);
               foreach (var file in files)
               {
                   try {
                       var info = new FileInfo(file);
                       long size = info.Length;
                       File.Delete(file);
                       bytes += size;
                       count++;
                   } catch (Exception ex) { errors.Add($"Failed to delete {Path.GetFileName(file)}: {ex.Message}"); }
               }
            }
            catch {}
            return (bytes, count, errors);
        }

        private void RunCommand(string fileName, string args)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            catch {}
        }

        private void ClearEventLogs(List<string> errors)
        {
             try
             {
                 // Requires admin, simple implementation via wevtutil
                 RunCommand("wevtutil", "el | Foreach-Object {wevtutil cl \"$_\"}");
             }
             catch (Exception ex)
             {
                 errors.Add($"Failed to clear event logs: {ex.Message}");
             }
        }
    }

    }

    public class CleanupLocation
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General"; 
        public string Path { get; set; } = string.Empty;
        public long EstimatedSize { get; set; }
        public bool IsSafe { get; set; } = true; 
        
        // UI Helpers
        public bool IsSelected { get; set; } = true;
        public bool IsChecked 
        { 
            get => IsSelected; 
            set => IsSelected = value; 
        }

        public string EstimatedSizeStr 
        { 
            get 
            {
                string[] suffix = { "B", "KB", "MB", "GB", "TB" };
                double dblBytes = EstimatedSize;
                int i = 0;
                while (dblBytes >= 1024 && i < suffix.Length - 1)
                {
                    dblBytes /= 1024;
                    i++;
                }
                return $"{dblBytes:0.##} {suffix[i]}";
            } 
        }

        public List<CleanupItem> Items { get; set; } = new List<CleanupItem>();
    }

    public class CleanupItem
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsChecked { get; set; } = true;

        public string SizeStr
        {
            get
            {
                if (Size == 0) return "0 B";
                string[] suffix = { "B", "KB", "MB", "GB", "TB" };
                double dblBytes = Size;
                int i = 0;
                while (dblBytes >= 1024 && i < suffix.Length - 1)
                {
                    dblBytes /= 1024;
                    i++;
                }
                return $"{dblBytes:0.##} {suffix[i]}";
            }
        }
    }


