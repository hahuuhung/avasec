using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AVASec.Antivirus.Models;

namespace AVASec.Antivirus.Services
{
    /// <summary>
    /// Virus database update service / Dịch vụ cập nhật cơ sở dữ liệu virus
    /// Handles downloading and updating virus signatures / Xử lý tải xuống và cập nhật chữ ký virus
    /// </summary>
    public class VirusDatabaseUpdateService
    {
        private readonly string _databasePath;
        private readonly string _updateUrl;
        private static readonly HttpClient _httpClient = new HttpClient();

        public VirusDatabaseUpdateService()
        {
            // Set local database path / Đặt đường dẫn database local
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string sysAntiPath = Path.Combine(appData, "avasec", "VirusDatabase");
            Directory.CreateDirectory(sysAntiPath);
            
            _databasePath = Path.Combine(sysAntiPath, "virus_signatures.json");
            
            // In production, this would be a real update server / Trong thực tế, đây sẽ là server cập nhật thật
            // For demo, we'll use local file / Để demo, sử dụng file local
            _updateUrl = "https://api.sysanti.example.com/virus-database/latest.json";
        }

        /// <summary>
        /// Get current virus database / Lấy cơ sở dữ liệu virus hiện tại
        /// </summary>
        public async Task<VirusDatabase?> GetCurrentDatabaseAsync()
        {
            try
            {
                // Check if local database exists / Kiểm tra database local có tồn tại
                if (!File.Exists(_databasePath))
                {
                    // Copy from installation folder / Sao chép từ thư mục cài đặt
                    await InitializeDatabaseAsync();
                }

                string json = await File.ReadAllTextAsync(_databasePath);
                var database = JsonSerializer.Deserialize<VirusDatabase>(json);
                return database;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading virus database / Lỗi tải database virus: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Initialize database from bundled file / Khởi tạo database từ file đi kèm
        /// </summary>
        private async Task InitializeDatabaseAsync()
        {
            // Get application directory / Lấy thư mục ứng dụng
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string bundledDbPath = Path.Combine(appDir, "Data", "virus_signatures.json");

            if (File.Exists(bundledDbPath))
            {
                // Copy bundled database to user folder / Sao chép database đi kèm vào thư mục người dùng
                File.Copy(bundledDbPath, _databasePath, overwrite: true);
            }
            else
            {
                // Create default database / Tạo database mặc định
                var defaultDb = new VirusDatabase
                {
                    Version = "1.0.0",
                    LastUpdated = DateTime.UtcNow,
                    Signatures = new System.Collections.Generic.List<SignatureEntry>
                    {
                        new SignatureEntry
                        {
                            Name = "EICAR Test File",
                            Pattern = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*")),
                            Severity = "Low",
                            Description = "EICAR standard test file"
                        }
                    }
                };

                string json = JsonSerializer.Serialize(defaultDb, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_databasePath, json);
            }
        }

        /// <summary>
        /// Check if update is available / Kiểm tra có bản cập nhật không
        /// </summary>
        public async Task<(bool Available, string NewVersion)> CheckForUpdatesAsync()
        {
            try
            {
                // Get current version / Lấy phiên bản hiện tại
                var currentDb = await GetCurrentDatabaseAsync();
                if (currentDb == null)
                    return (false, string.Empty);

                // In production: download version info from server / Trong thực tế: tải thông tin phiên bản từ server
                // For demo: simulate update check / Để demo: giả lập kiểm tra cập nhật
                
                // Simulate: always has newer version for demo / Giả lập: luôn có phiên bản mới để demo
                string newVersion = IncrementVersion(currentDb.Version);
                
                return (true, newVersion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for updates / Lỗi kiểm tra cập nhật: {ex.Message}");
                return (false, string.Empty);
            }
        }

        /// <summary>
        /// Download and install update / Tải xuống và cài đặt cập nhật
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateDatabaseAsync()
        {
            try
            {
                // In production: download from update server / Trong thực tế: tải từ server cập nhật
                // For demo: use bundled file or simulate update / Để demo: dùng file đi kèm hoặc giả lập
                
                var currentDb = await GetCurrentDatabaseAsync();
                if (currentDb == null)
                {
                    await InitializeDatabaseAsync();
                    return (true, "Database initialized / Database đã được khởi tạo");
                }

                // Simulate downloading updated database / Giả lập tải database cập nhật
                currentDb.Version = IncrementVersion(currentDb.Version);
                currentDb.LastUpdated = DateTime.UtcNow;
                
                // Add some new signatures to demonstrate update / Thêm chữ ký mới để demo cập nhật
                currentDb.Signatures.Add(new SignatureEntry
                {
                    Name = $"New Threat Detected {DateTime.Now:HHmmss}",
                    Pattern = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("NEWSIGNATURE")),
                    Severity = "Medium",
                    Description = "Recently discovered threat pattern"
                });

                // Save updated database / Lưu database đã cập nhật
                string json = JsonSerializer.Serialize(currentDb, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_databasePath, json);

                return (true, $"Database updated to version {currentDb.Version} / Database đã cập nhật lên phiên bản {currentDb.Version}");
            }
            catch (Exception ex)
            {
                return (false, $"Update failed / Cập nhật thất bại: {ex.Message}");
            }
        }

        /// <summary>
        /// Get database information / Lấy thông tin database
        /// </summary>
        public async Task<(string Version, DateTime LastUpdated, int SignatureCount)> GetDatabaseInfoAsync()
        {
            var db = await GetCurrentDatabaseAsync();
            if (db == null)
                return ("Unknown", DateTime.MinValue, 0);

            return (db.Version, db.LastUpdated, db.Signatures.Count + db.MalwareHashes.Count);
        }

        /// <summary>
        /// Increment version number / Tăng số phiên bản
        /// </summary>
        private string IncrementVersion(string version)
        {
            var parts = version.Split('.');
            if (parts.Length == 3 && int.TryParse(parts[2], out int patch))
            {
                return $"{parts[0]}.{parts[1]}.{patch + 1}";
            }
            return "1.0.1";
        }
    }
}
