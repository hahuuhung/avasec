using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;

namespace AVASec.Antivirus.Services
{
    /// <summary>
    /// AI-Powered Malware Detection Service
    /// Uses machine learning patterns, behavioral analysis, and heuristics
    /// Dịch vụ Phát hiện Malware bằng AI
    /// </summary>
    public class AIDetectionService
    {
        // AI Detection Configuration
        private const double SUSPICION_THRESHOLD = 0.7; // 70% confidence = suspicious
        private const double MALWARE_THRESHOLD = 0.85; // 85% confidence = malware
        
        private readonly AhoCorasick _patternMatcher;

        public AIDetectionService()
        {
            _patternMatcher = new AhoCorasick(SuspiciousPatterns);
        }
        
        // Known malicious patterns (simulated AI signatures)
        private static readonly List<string> SuspiciousPatterns = new()
        {
            // Process injection patterns
            "WriteProcessMemory",
            "VirtualAllocEx", 
            "CreateRemoteThread",
            
            // Keylogger patterns
            "GetAsyncKeyState",
            "SetWindowsHookEx",
            
            // Ransomware patterns
            "CryptEncrypt",
            "CryptGenKey",
            ".encrypted",
            ".locked",
            
            // Persistence patterns
            "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run",
            "schtasks /create",
            
            // Network exfiltration
            "WebClient.DownloadString",
            "Invoke-WebRequest",
            
            // Code obfuscation
            "FromBase64String",
            "Invoke-Expression",
            "-EncodedCommand"
        };

        // High-risk file extensions
        private static readonly HashSet<string> HighRiskExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".dll", ".scr", ".bat", ".cmd", ".ps1", ".vbs", ".js",
            ".jar", ".msi", ".com", ".pif", ".hta", ".wsf"
        };

        // Detection result structure
        public class AIDetectionResult
        {
            public string FilePath { get; set; } = string.Empty;
            public double ConfidenceScore { get; set; }
            public AIThreatLevel ThreatLevel { get; set; }
            public List<string> DetectedPatterns { get; set; } = new();
            public string Recommendation { get; set; } = string.Empty;
            public DateTime ScanTime { get; set; } = DateTime.Now;
        }

        public enum AIThreatLevel
        {
            Safe = 0,
            Low = 1,
            Medium = 2,
            High = 3,
            Critical = 4
        }

        /// <summary>
        /// Analyze file using AI-powered detection
        /// </summary>
        public async Task<AIDetectionResult> AnalyzeFileAsync(string filePath)
        {
            var result = new AIDetectionResult
            {
                FilePath = filePath,
                ScanTime = DateTime.Now
            };

            if (!File.Exists(filePath))
            {
                result.ThreatLevel = AIThreatLevel.Safe;
                result.Recommendation = "File does not exist";
                return result;
            }

            try
            {
                double score = 0;
                var patterns = new List<string>();

                // 1. Extension Analysis (10% weight)
                var extension = Path.GetExtension(filePath);
                if (HighRiskExtensions.Contains(extension))
                {
                    score += 0.1;
                    patterns.Add($"High-risk extension: {extension}");
                }

                // 2. File Size Analysis (5% weight)
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length < 10000 && HighRiskExtensions.Contains(extension))
                {
                    // Small executables are often droppers
                    score += 0.05;
                    patterns.Add("Small executable (potential dropper)");
                }

                // 3. Content Pattern Analysis (50% weight)
                if (fileInfo.Length < 5 * 1024 * 1024) // Only scan files < 5MB for content
                {
                    var contentScore = await AnalyzeContentAsync(filePath, patterns);
                    score += contentScore * 0.5;
                }

                // 4. Entropy Analysis (15% weight) - High entropy = packed/encrypted
                var entropy = await CalculateEntropyAsync(filePath);
                if (entropy > 7.5) // Max entropy is 8
                {
                    score += 0.15;
                    patterns.Add($"High entropy ({entropy:F2}) - possible packing/encryption");
                }
                else if (entropy > 6.5)
                {
                    score += 0.08;
                    patterns.Add($"Moderate entropy ({entropy:F2})");
                }

                // 5. Digital Signature Check (10% weight)
                if (HighRiskExtensions.Contains(extension))
                {
                    var isSigned = await CheckDigitalSignatureAsync(filePath);
                    if (!isSigned)
                    {
                        score += 0.1;
                        patterns.Add("Unsigned executable");
                    }
                }

                // 6. PE Header Analysis (25% weight)
                if (extension == ".exe" || extension == ".dll")
                {
                    var peScore = await AnalyzePEHeaderAsync(filePath, patterns);
                    score += peScore * 0.25;
                }

                // 7. Hidden/System attributes (10% weight)
                if ((fileInfo.Attributes & FileAttributes.Hidden) != 0 ||
                    (fileInfo.Attributes & FileAttributes.System) != 0)
                {
                    score += 0.1;
                    patterns.Add("Hidden or system file");
                }

                // Normalize score to 0-1
                result.ConfidenceScore = Math.Min(score, 1.0);
                result.DetectedPatterns = patterns;

                // Determine threat level
                if (score >= MALWARE_THRESHOLD)
                {
                    result.ThreatLevel = AIThreatLevel.Critical;
                    result.Recommendation = "Quarantine immediately. High probability of malware.";
                }
                else if (score >= SUSPICION_THRESHOLD)
                {
                    result.ThreatLevel = AIThreatLevel.High;
                    result.Recommendation = "Suspicious file. Manual review recommended.";
                }
                else if (score >= 0.5)
                {
                    result.ThreatLevel = AIThreatLevel.Medium;
                    result.Recommendation = "Moderate risk. Monitor behavior.";
                }
                else if (score >= 0.3)
                {
                    result.ThreatLevel = AIThreatLevel.Low;
                    result.Recommendation = "Low risk. Normal operation.";
                }
                else
                {
                    result.ThreatLevel = AIThreatLevel.Safe;
                    result.Recommendation = "No threats detected.";
                }
            }
            catch (Exception ex)
            {
                result.ThreatLevel = AIThreatLevel.Low;
                result.Recommendation = $"Error during analysis: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Analyze file content for suspicious patterns using optimized streaming matching
        /// </summary>
        private async Task<double> AnalyzeContentAsync(string filePath, List<string> patterns)
        {
            double score = 0;
            int matchCount = 0;

            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                var buffer = new byte[8192];
                int bytesRead;
                
                var foundPatterns = new HashSet<string>();

                while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    var chunkMatches = _patternMatcher.Search(buffer, bytesRead);
                    foreach (var m in chunkMatches)
                    {
                        if (foundPatterns.Add(m))
                        {
                            matchCount++;
                            patterns.Add($"Malicious String: {m}");
                        }
                    }

                    if (matchCount > 10) break; // Cap the scoring
                }

                if (matchCount > 0)
                {
                    score = Math.Min((matchCount * 0.12), 1.0);
                }
            }
            catch
            {
                // Access error
            }

            return score;
        }

        /// <summary>
        /// Analyze PE Header for suspicious characteristics
        /// </summary>
        private async Task<double> AnalyzePEHeaderAsync(string filePath, List<string> patterns)
        {
            double score = 0;
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var reader = new BinaryReader(fs);

                // DOS Header
                if (reader.ReadUInt16() != 0x5A4D) return 0; // Not MZ

                fs.Seek(0x3C, SeekOrigin.Begin);
                int peHeaderOffset = reader.ReadInt32();
                fs.Seek(peHeaderOffset, SeekOrigin.Begin);

                // PE Signature
                if (reader.ReadUInt32() != 0x00004550) return 0; // Not PE

                // COFF Header
                fs.Seek(peHeaderOffset + 4, SeekOrigin.Begin);
                ushort machine = reader.ReadUInt16();
                ushort numberOfSections = reader.ReadUInt16();
                
                // Typical malware has fewer or non-standard sections
                if (numberOfSections < 3)
                {
                    score += 0.2;
                    patterns.Add("Low section count (suspicious)");
                }

                // Optimization Header
                fs.Seek(peHeaderOffset + 24, SeekOrigin.Begin);
                ushort magic = reader.ReadUInt16();
                int addressOfEntryPoint = reader.ReadInt32();

                // Check for Entry Point in unusual section
                if (addressOfEntryPoint == 0)
                {
                    score += 0.3;
                    patterns.Add("Missing entry point (possible resource-only or malicious)");
                }

                // Check sections for suspicious flags (Writable + Executable)
                // This usually indicates self-modifying code or packers
                fs.Seek(peHeaderOffset + 24 + (magic == 0x10B ? 96 : 112) + 128, SeekOrigin.Begin); // Skip Optional Header directories
                
                for (int i = 0; i < numberOfSections; i++)
                {
                    byte[] sectionNameBytes = reader.ReadBytes(8);
                    string sectionName = Encoding.ASCII.GetString(sectionNameBytes).TrimEnd('\0');
                    fs.Seek(28, SeekOrigin.Current); // Skip size and RVA data
                    uint characteristics = reader.ReadUInt32();

                    // 0x80000000 = Writable, 0x20000000 = Executable
                    if ((characteristics & 0xA0000000) == 0xA0000000)
                    {
                        score += 0.4;
                        patterns.Add($"Suspicious section flags in {sectionName} (W+X)");
                    }

                    var maliciousSectionNames = new[] { ".aspack", ".pdata", "UPX", ".reloc" };
                    if (maliciousSectionNames.Any(n => sectionName.Contains(n)))
                    {
                        score += 0.2;
                        patterns.Add($"Known packer/obfuscator section: {sectionName}");
                    }
                }
            }
            catch { }
            return score;
        }

        /// <summary>
        /// Aho-Corasick Implementation for high-performance multi-pattern matching
        /// </summary>
        private class AhoCorasick
        {
            private class Node
            {
                public Dictionary<byte, Node> Children = new();
                public Node Failure = null!;
                public List<string> Matches = new();
            }

            private readonly Node _root = new();

            public AhoCorasick(IEnumerable<string> patterns)
            {
                foreach (var p in patterns)
                {
                    var current = _root;
                    foreach (var b in Encoding.ASCII.GetBytes(p))
                    {
                        if (!current.Children.ContainsKey(b)) current.Children[b] = new Node();
                        current = current.Children[b];
                    }
                    current.Matches.Add(p);
                }

                // Build failure links
                var queue = new Queue<Node>();
                foreach (var node in _root.Children.Values)
                {
                    node.Failure = _root;
                    queue.Enqueue(node);
                }

                while (queue.Count > 0)
                {
                    var r = queue.Dequeue();
                    foreach (var entry in r.Children)
                    {
                        var u = entry.Value;
                        var v = r.Failure;
                        while (v != _root && !v.Children.ContainsKey(entry.Key)) v = v.Failure;
                        u.Failure = v.Children.ContainsKey(entry.Key) ? v.Children[entry.Key] : _root;
                        u.Matches.AddRange(u.Failure.Matches);
                        queue.Enqueue(u);
                    }
                }
            }

            public List<string> Search(byte[] data, int length)
            {
                var matches = new List<string>();
                var current = _root;
                for (int i = 0; i < length; i++)
                {
                    byte b = data[i];
                    while (current != _root && !current.Children.ContainsKey(b)) current = current.Failure;
                    current = current.Children.ContainsKey(b) ? current.Children[b] : _root;
                    if (current.Matches.Count > 0) matches.AddRange(current.Matches);
                }
                return matches;
            }
        }

        /// <summary>
        /// Calculate file entropy (randomness)
        /// Higher entropy = more likely packed/encrypted
        /// </summary>
        private async Task<double> CalculateEntropyAsync(string filePath)
        {
            try
            {
                var bytes = await File.ReadAllBytesAsync(filePath);
                if (bytes.Length == 0) return 0;

                // Limit sample size for large files
                if (bytes.Length > 1024 * 1024)
                {
                    bytes = bytes.Take(1024 * 1024).ToArray();
                }

                var frequency = new int[256];
                foreach (var b in bytes)
                {
                    frequency[b]++;
                }

                double entropy = 0;
                foreach (var count in frequency)
                {
                    if (count > 0)
                    {
                        double p = (double)count / bytes.Length;
                        entropy -= p * Math.Log(p, 2);
                    }
                }

                return entropy;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Check if file has valid digital signature
        /// </summary>
        private async Task<bool> CheckDigitalSignatureAsync(string filePath)
        {
            try
            {
                // Use signtool or AuthenticodeVerifier in real implementation
                // This is a simplified check
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = $"-Command \"(Get-AuthenticodeSignature '{filePath}').Status -eq 'Valid'\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return output.Trim().Equals("True", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false; // Assume unsigned if check fails
            }
        }

        /// <summary>
        /// Batch analyze multiple files
        /// </summary>
        public async Task<List<AIDetectionResult>> AnalyzeDirectoryAsync(string directoryPath, IProgress<int>? progress = null)
        {
            var results = new List<AIDetectionResult>();

            if (!Directory.Exists(directoryPath))
                return results;

            var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                .Where(f => HighRiskExtensions.Contains(Path.GetExtension(f)))
                .ToList();

            int processed = 0;
            foreach (var file in files)
            {
                var result = await AnalyzeFileAsync(file);
                results.Add(result);

                processed++;
                progress?.Report((processed * 100) / files.Count);
            }

            return results;
        }

        /// <summary>
        /// Get threat summary for reporting
        /// </summary>
        public string GetThreatSummary(List<AIDetectionResult> results)
        {
            var critical = results.Count(r => r.ThreatLevel == AIThreatLevel.Critical);
            var high = results.Count(r => r.ThreatLevel == AIThreatLevel.High);
            var medium = results.Count(r => r.ThreatLevel == AIThreatLevel.Medium);
            var safe = results.Count(r => r.ThreatLevel == AIThreatLevel.Safe);

            return $"🔴 Critical: {critical} | 🟠 High: {high} | 🟡 Medium: {medium} | 🟢 Safe: {safe}";
        }
    }
}
