using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AVASec.Antivirus.Models;

namespace AVASec.Antivirus.Services
{
    /// <summary>
    /// File scanner service / Dịch vụ quét file
    /// Scans files for malware signatures / Quét file để tìm chữ ký malware
    /// </summary>
    public class FileScannerService
    {
        private readonly List<VirusSignature> _virusSignatures;
        private readonly HashSet<string> _knownMalwareHashes;
        private readonly AIDetectionService _aiService;

        public FileScannerService()
        {
            _virusSignatures = LoadVirusSignatures();
            _knownMalwareHashes = LoadMalwareHashes();
            _aiService = new AIDetectionService();
        }

        /// <summary>
        /// Scan file for threats / Quét file để tìm mối đe dọa
        /// </summary>
        public async Task<ScanResult> ScanFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var result = new ScanResult
            {
                FilePath = filePath,
                ScanTime = DateTime.Now
            };

            try
            {
                if (!File.Exists(filePath))
                {
                    result.Status = "File not found / File không tồn tại";
                    return result;
                }


                var fileInfo = new FileInfo(filePath);
                result.FileSize = fileInfo.Length;

                // Skip files larger than 100MB / Bỏ qua file lớn hơn 100MB
                if (fileInfo.Length > 100 * 1024 * 1024)
                {
                    result.Status = "File too large, skipped / File quá lớn, đã bỏ qua";
                    return result;
                }

                // Step 1: Hash-based detection / Bước 1: Phát hiện dựa trên hash
                string fileHash = await CalculateFileHashAsync(filePath);
                result.FileHash = fileHash;

                if (_knownMalwareHashes.Contains(fileHash))
                {
                    result.IsThreat = true;
                    result.ThreatName = "Known malware (hash match) / Malware đã biết (khớp hash)";
                    result.ThreatLevel = "High / Cao";
                    result.Status = "Threat detected / Phát hiện mối đe dọa";
                    return result;
                }

                // Step 2: Signature-based detection / Bước 2: Phát hiện dựa trên chữ ký
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                
                foreach (var signature in _virusSignatures)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (ContainsSignature(fileBytes, signature.Pattern))
                    {
                        result.IsThreat = true;
                        result.ThreatName = signature.Name;
                        result.ThreatLevel = signature.Severity;
                        result.Status = "Threat detected / Phát hiện mối đe dọa";
                        return result;
                    }
                }

                // Step 3: AI-Powered Deep Analysis (Advanced) / Bước 3: Phân tích sâu bằng AI (Nâng cao)
                var aiResult = await _aiService.AnalyzeFileAsync(filePath);
                if (aiResult.ThreatLevel >= AIDetectionService.AIThreatLevel.High)
                {
                    result.IsThreat = true;
                    result.ThreatName = $"AI Detection: {string.Join(", ", aiResult.DetectedPatterns.Take(2))}";
                    result.ThreatLevel = aiResult.ThreatLevel.ToString();
                    result.Status = "Suspicious activity detected by AI";
                    return result;
                }

                // Clean / Sạch — local signatures + AI only (no cloud lookup)
                result.Status = "Clean / Sạch";
                return result;
            }
            catch (Exception ex)
            {
                result.Status = $"Scan error / Lỗi quét: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Scan directory / Quét thư mục
        /// </summary>
        /// <summary>
        /// Scan directory / Quét thư mục
        /// Supports multi-threading and background scanning / Hỗ trợ đa luồng và quét nền
        /// </summary>
        /// <param name="directoryPath">Path to directory / Đường dẫn thư mục</param>
        /// <param name="progress">Progress reporter / Báo cáo tiến độ</param>
        /// <param name="cancellationToken">Token to cancel operation / Token để hủy thao tác</param>
        /// <param name="mode">Scan mode details / Chế độ quét chi tiết</param>
        /// <param name="isBackground">Is running in background / Có chạy ngầm không</param>
        public async Task<List<ScanResult>> ScanDirectoryAsync(string directoryPath, 
            IProgress<ScanProgress>? progress = null, 
            CancellationToken cancellationToken = default,
            bool isBackground = false)
        {
            var results = new System.Collections.Concurrent.ConcurrentBag<ScanResult>();
            int scannedFiles = 0;
            int totalFiles = 0;

            try
            {
                // Count files first for progress tracking (optional, can be skipped for speed)
                // Đếm file trước để theo dõi tiến độ (tùy chọn, có thể bỏ qua để tăng tốc)
                // Using a heuristic or counting safely if needed. For now, we estimate or update total dynamically.
                // Estimating total files might be slow for deep directories. Let's just track scanned count.
                
                // Get all files safely and count them for progress / Lấy tất cả file và đếm để theo dõi tiến độ
                var files = GetFilesSafe(directoryPath).ToList();
                totalFiles = files.Count;
                
                // Configure parallelism / Cấu hình song song
                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = isBackground 
                        ? Math.Max(2, Environment.ProcessorCount / 4) 
                        : Math.Max(2, (int)(Environment.ProcessorCount * 0.5))
                };

                await Parallel.ForEachAsync(files, parallelOptions, async (file, ct) =>
                {
                    if (IsExcludedPath(file)) return;

                    var result = await ScanFileAsync(file, ct);
                    results.Add(result);

                    int currentCount = Interlocked.Increment(ref scannedFiles);

                    // Report progress / Báo cáo tiến độ
                    if (progress != null)
                    {
                        progress.Report(new ScanProgress
                        {
                           TotalFiles = totalFiles,
                           ScannedFiles = currentCount,
                           CurrentFile = file,
                           ThreatsFound = results.Count(r => r.IsThreat)
                        });
                    }
                });
            }
            catch (Exception) { }

            return results.ToList();
        }

        /// <summary>
        /// Safely enumerate files (skipping access denied) / Liệt kê file an toàn (bỏ qua lỗi truy cập)
        /// </summary>
        private IEnumerable<string> GetFilesSafe(string rootPath)
        {
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
                catch (IOException) { } // Path too long, etc.

                if (next != null)
                {
                    foreach (var file in next)
                        yield return file;
                }

                try
                {
                    // Push sub-directories
                    var subDirs = Directory.GetDirectories(path);
                    foreach (var subdir in subDirs)
                        pending.Push(subdir);
                }
                catch (UnauthorizedAccessException) { }
                catch (DirectoryNotFoundException) { }
                catch (IOException) { }
            }
        }

        /// <summary>
        /// Calculate file hash / Tính hash file
        /// </summary>
        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = await Task.Run(() => sha256.ComputeHash(stream));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Check if file contains signature / Kiểm tra file có chứa chữ ký
        /// </summary>
        private bool ContainsSignature(byte[] fileBytes, byte[] signature)
        {
            for (int i = 0; i <= fileBytes.Length - signature.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < signature.Length; j++)
                {
                    if (fileBytes[i + j] != signature[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return true;
            }
            return false;
        }

        /// <summary>
        /// Heuristic analysis / Phân tích heuristic
        /// </summary>


        private bool IsFileSuspicious(string filePath, byte[] fileBytes)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            
            // Check for double extensions / Kiểm tra extension kép
            string fileName = Path.GetFileName(filePath);
            if (fileName.Count(c => c == '.') > 1 && 
                (extension == ".exe" || extension == ".bat" || extension == ".cmd"))
            {
                return true;
            }

            // Check for suspicious patterns / Kiểm tra pattern đáng ngờ
            if (extension == ".exe" || extension == ".dll")
            {
                string content = Encoding.ASCII.GetString(fileBytes).ToLower();
                var suspiciousKeywords = new[] { "keylogger", "backdoor", "trojan", "ransomware" };
                
                foreach (var keyword in suspiciousKeywords)
                {
                    if (content.Contains(keyword))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if path should be excluded / Kiểm tra đường dẫn nên bỏ qua
        /// </summary>
        private bool IsExcludedPath(string path)
        {
            var excludedFolders = new[] { "Windows", "Program Files", "$Recycle.Bin", "System Volume Information" };
            return excludedFolders.Any(folder => path.Contains(folder));
        }

        /// <summary>
        /// Load virus signatures from JSON database / Tải chữ ký virus từ database JSON
        /// </summary>
        private List<VirusSignature> LoadVirusSignatures()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dbPath = Path.Combine(appData, "avasec", "VirusDatabase", "virus_signatures.json");
                
                if (!File.Exists(dbPath))
                {
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    dbPath = Path.Combine(appDir, "Data", "virus_signatures.json");
                }

                if (File.Exists(dbPath))
                {
                    string json = File.ReadAllText(dbPath);
                    var database = JsonSerializer.Deserialize<VirusDatabase>(json);
                    
                    if (database?.Signatures != null)
                    {
                        try 
                        {
                            return database.Signatures.Select(s => new VirusSignature 
                            { 
                                Name = s.Name, 
                                Pattern = Convert.FromBase64String(s.Pattern), 
                                Severity = s.Severity 
                            }).ToList();
                        }
                        catch { /* Ignore invalid patterns */ }
                        return new List<VirusSignature>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading signatures: {ex.Message}");
            }

            return new List<VirusSignature>
            {
                new VirusSignature 
                { 
                    Name = "EICAR Test File", 
                    Pattern = Encoding.ASCII.GetBytes("X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*"),
                    Severity = "Low"
                }
            };
        }

        /// <summary>
        /// Load known malware hashes from JSON database / Tải hash malware từ database JSON
        /// </summary>
        private HashSet<string> LoadMalwareHashes()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dbPath = Path.Combine(appData, "avasec", "VirusDatabase", "virus_signatures.json");
                
                if (!File.Exists(dbPath))
                {
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    dbPath = Path.Combine(appDir, "Data", "virus_signatures.json");
                }

                if (File.Exists(dbPath))
                {
                    string json = File.ReadAllText(dbPath);
                    var database = JsonSerializer.Deserialize<VirusDatabase>(json);
                    
                    if (database?.MalwareHashes != null)
                    {
                        return new HashSet<string>(database.MalwareHashes.Select(h => h.Hash));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading malware hashes: {ex.Message}");
            }

            return new HashSet<string>
            {
                "44d88612fea8a8f36de82e1278abb02f"
            };
        }
    }

    /// <summary>
    /// Virus signature model / Mô hình chữ ký virus
    /// </summary>
    public class VirusSignature
    {
        public string Name { get; set; } = string.Empty;
        public byte[] Pattern { get; set; } = Array.Empty<byte>();
        public string Severity { get; set; } = "Medium";
    }

    /// <summary>
    /// Scan result model / Mô hình kết quả quét
    /// </summary>
    public class ScanProgress
    {
        public int TotalFiles { get; set; }
        public int ScannedFiles { get; set; }
        public string CurrentFile { get; set; } = string.Empty;
        public int ThreatsFound { get; set; }
    }
    public class ScanResult
    {
        public string FilePath { get; set; } = string.Empty;
        public DateTime ScanTime { get; set; }
        public long FileSize { get; set; }
        public string FileHash { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsThreat { get; set; }
        public string ThreatName { get; set; } = string.Empty;
        public string ThreatLevel { get; set; } = "Low";
    }
}
