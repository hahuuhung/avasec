using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace AVASec.Core.Services
{
    /// <summary>
    /// Performance Benchmark Service - Test and compare system performance
    /// Dịch vụ Đánh giá Hiệu năng - Kiểm tra và so sánh hiệu năng hệ thống
    /// </summary>
    public class BenchmarkService
    {
        public class BenchmarkResult
        {
            public DateTime TestTime { get; set; } = DateTime.Now;
            public CPUBenchmark CPU { get; set; } = new();
            public MemoryBenchmark Memory { get; set; } = new();
            public DiskBenchmark Disk { get; set; } = new();
            public int OverallScore { get; set; }
            public string Grade { get; set; } = "N/A";
        }

        public class CPUBenchmark
        {
            public string ProcessorName { get; set; } = "Unknown";
            public int Cores { get; set; }
            public int Threads { get; set; }
            public double ClockSpeedGHz { get; set; }
            public int SingleCoreScore { get; set; }
            public int MultiCoreScore { get; set; }
            public double TestDurationMs { get; set; }
        }

        public class MemoryBenchmark
        {
            public long TotalMemoryMB { get; set; }
            public long AvailableMemoryMB { get; set; }
            public double ReadSpeedMBps { get; set; }
            public double WriteSpeedMBps { get; set; }
            public double LatencyNs { get; set; }
            public int Score { get; set; }
        }

        public class DiskBenchmark
        {
            public string DriveName { get; set; } = "C:";
            public string DriveType { get; set; } = "Unknown";
            public long TotalSpaceGB { get; set; }
            public long FreeSpaceGB { get; set; }
            public double SequentialReadMBps { get; set; }
            public double SequentialWriteMBps { get; set; }
            public double Random4KReadMBps { get; set; }
            public double Random4KWriteMBps { get; set; }
            public int Score { get; set; }
        }

        private readonly string _tempPath;

        public BenchmarkService()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), "AVASecBenchmark");
            Directory.CreateDirectory(_tempPath);
        }

        /// <summary>
        /// Run full system benchmark
        /// </summary>
        public async Task<BenchmarkResult> RunFullBenchmarkAsync(IProgress<string>? status = null, IProgress<int>? progress = null)
        {
            var result = new BenchmarkResult();

            // CPU Benchmark (40% of progress)
            status?.Report("Testing CPU performance...");
            result.CPU = await RunCPUBenchmarkAsync(progress, 0, 40);

            // Memory Benchmark (30% of progress)
            status?.Report("Testing Memory performance...");
            result.Memory = await RunMemoryBenchmarkAsync(progress, 40, 30);

            // Disk Benchmark (30% of progress)
            status?.Report("Testing Disk performance...");
            result.Disk = await RunDiskBenchmarkAsync(progress, 70, 30);

            // Calculate Overall Score
            result.OverallScore = CalculateOverallScore(result);
            result.Grade = GetGrade(result.OverallScore);

            status?.Report("Benchmark complete!");
            progress?.Report(100);

            return result;
        }

        /// <summary>
        /// CPU Benchmark - Single and Multi-core tests
        /// </summary>
        private async Task<CPUBenchmark> RunCPUBenchmarkAsync(IProgress<int>? progress, int startProgress, int range)
        {
            var benchmark = new CPUBenchmark();

            try
            {
                // Get CPU Info
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    benchmark.ProcessorName = obj["Name"]?.ToString() ?? "Unknown";
                    benchmark.Cores = Convert.ToInt32(obj["NumberOfCores"] ?? 0);
                    benchmark.Threads = Convert.ToInt32(obj["NumberOfLogicalProcessors"] ?? 0);
                    benchmark.ClockSpeedGHz = Convert.ToDouble(obj["MaxClockSpeed"] ?? 0) / 1000;
                    break;
                }

                // Single-core test (prime calculation)
                progress?.Report(startProgress + (range / 4));
                var singleStart = Stopwatch.StartNew();
                var primeCount = await Task.Run(() => CountPrimes(1, 100000));
                singleStart.Stop();

                // Calculate single-core score (normalize to ~1000 for a typical modern CPU)
                benchmark.SingleCoreScore = (int)(100000.0 / singleStart.ElapsedMilliseconds * 100);

                // Multi-core test
                progress?.Report(startProgress + (range / 2));
                var multiStart = Stopwatch.StartNew();
                var tasks = new List<Task<int>>();
                int chunkSize = 100000 / benchmark.Threads;

                for (int i = 0; i < benchmark.Threads; i++)
                {
                    int start = i * chunkSize + 1;
                    int end = (i + 1) * chunkSize;
                    tasks.Add(Task.Run(() => CountPrimes(start, end)));
                }

                await Task.WhenAll(tasks);
                multiStart.Stop();

                // Multi-core score with scaling factor for thread count
                benchmark.MultiCoreScore = (int)(100000.0 / multiStart.ElapsedMilliseconds * 100 * Math.Sqrt(benchmark.Threads));
                benchmark.TestDurationMs = singleStart.ElapsedMilliseconds + multiStart.ElapsedMilliseconds;

                progress?.Report(startProgress + range);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CPU Benchmark error: {ex.Message}");
            }

            return benchmark;
        }

        /// <summary>
        /// Memory Benchmark - Read/Write speed and latency
        /// </summary>
        private async Task<MemoryBenchmark> RunMemoryBenchmarkAsync(IProgress<int>? progress, int startProgress, int range)
        {
            var benchmark = new MemoryBenchmark();

            try
            {
                // Get Memory Info
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    benchmark.TotalMemoryMB = Convert.ToInt64(obj["TotalPhysicalMemory"] ?? 0) / (1024 * 1024);
                    break;
                }

                using var perfCounter = new PerformanceCounter("Memory", "Available MBytes");
                benchmark.AvailableMemoryMB = (long)perfCounter.NextValue();

                progress?.Report(startProgress + (range / 3));

                // Memory speed test (allocate and copy large array)
                const int testSizeMB = 100;
                byte[] sourceArray = new byte[testSizeMB * 1024 * 1024];
                byte[] destArray = new byte[testSizeMB * 1024 * 1024];

                // Fill with random data
                new Random().NextBytes(sourceArray);

                // Write speed test
                var writeStart = Stopwatch.StartNew();
                await Task.Run(() => Array.Copy(sourceArray, destArray, sourceArray.Length));
                writeStart.Stop();
                benchmark.WriteSpeedMBps = testSizeMB * 1000.0 / writeStart.ElapsedMilliseconds;

                progress?.Report(startProgress + (range * 2 / 3));

                // Read speed test
                long sum = 0;
                var readStart = Stopwatch.StartNew();
                await Task.Run(() =>
                {
                    for (int i = 0; i < destArray.Length; i += 4096)
                    {
                        sum += destArray[i];
                    }
                });
                readStart.Stop();
                benchmark.ReadSpeedMBps = testSizeMB * 1000.0 / readStart.ElapsedMilliseconds;

                // Latency test (random access)
                var random = new Random();
                var latencyStart = Stopwatch.StartNew();
                for (int i = 0; i < 10000; i++)
                {
                    sum += destArray[random.Next(destArray.Length)];
                }
                latencyStart.Stop();
                benchmark.LatencyNs = latencyStart.ElapsedTicks * 1000000000.0 / Stopwatch.Frequency / 10000;

                // Score calculation
                benchmark.Score = (int)((benchmark.ReadSpeedMBps + benchmark.WriteSpeedMBps) * 10 / 2);

                // Prevent compiler optimization
                GC.KeepAlive(sum);

                progress?.Report(startProgress + range);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Memory Benchmark error: {ex.Message}");
            }

            return benchmark;
        }

        /// <summary>
        /// Disk Benchmark - Sequential and Random I/O
        /// </summary>
        private async Task<DiskBenchmark> RunDiskBenchmarkAsync(IProgress<int>? progress, int startProgress, int range)
        {
            var benchmark = new DiskBenchmark();

            try
            {
                var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == "C:\\" && d.IsReady);
                if (drive != null)
                {
                    benchmark.DriveName = drive.Name;
                    benchmark.DriveType = drive.DriveType.ToString();
                    benchmark.TotalSpaceGB = drive.TotalSize / (1024 * 1024 * 1024);
                    benchmark.FreeSpaceGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                }

                string testFile = Path.Combine(_tempPath, "benchmark_test.tmp");
                const int testSizeMB = 100;
                byte[] buffer = new byte[testSizeMB * 1024 * 1024];
                new Random().NextBytes(buffer);

                progress?.Report(startProgress + (range / 4));

                // Sequential Write Test
                var writeStart = Stopwatch.StartNew();
                await File.WriteAllBytesAsync(testFile, buffer);
                await using (var fs = new FileStream(testFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                {
                    await fs.WriteAsync(buffer, 0, buffer.Length);
                }
                writeStart.Stop();
                benchmark.SequentialWriteMBps = testSizeMB * 1000.0 / writeStart.ElapsedMilliseconds;

                progress?.Report(startProgress + (range / 2));

                // Sequential Read Test
                var readStart = Stopwatch.StartNew();
                await using (var fs = new FileStream(testFile, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.SequentialScan))
                {
                    await fs.ReadAsync(buffer, 0, buffer.Length);
                }
                readStart.Stop();
                benchmark.SequentialReadMBps = testSizeMB * 1000.0 / readStart.ElapsedMilliseconds;

                progress?.Report(startProgress + (range * 3 / 4));

                // 4K Random Read/Write Test
                const int blockSize = 4096;
                const int iterations = 1000;
                var random = new Random();
                var smallBuffer = new byte[blockSize];

                await using (var fs = new FileStream(testFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None, blockSize))
                {
                    // Random Read
                    var random4KReadStart = Stopwatch.StartNew();
                    for (int i = 0; i < iterations; i++)
                    {
                        long position = random.NextInt64(0, fs.Length - blockSize);
                        fs.Seek(position, SeekOrigin.Begin);
                        await fs.ReadAsync(smallBuffer, 0, blockSize);
                    }
                    random4KReadStart.Stop();
                    benchmark.Random4KReadMBps = (iterations * blockSize / 1024.0 / 1024.0) * 1000 / random4KReadStart.ElapsedMilliseconds;

                    // Random Write
                    var random4KWriteStart = Stopwatch.StartNew();
                    for (int i = 0; i < iterations; i++)
                    {
                        long position = random.NextInt64(0, fs.Length - blockSize);
                        fs.Seek(position, SeekOrigin.Begin);
                        await fs.WriteAsync(smallBuffer, 0, blockSize);
                    }
                    random4KWriteStart.Stop();
                    benchmark.Random4KWriteMBps = (iterations * blockSize / 1024.0 / 1024.0) * 1000 / random4KWriteStart.ElapsedMilliseconds;
                }

                // Cleanup
                File.Delete(testFile);

                // Score calculation (weighted towards random I/O for SSD detection)
                benchmark.Score = (int)(
                    (benchmark.SequentialReadMBps + benchmark.SequentialWriteMBps) * 5 +
                    (benchmark.Random4KReadMBps + benchmark.Random4KWriteMBps) * 50
                );

                progress?.Report(startProgress + range);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Disk Benchmark error: {ex.Message}");
            }

            return benchmark;
        }

        /// <summary>
        /// Calculate overall system score
        /// </summary>
        private int CalculateOverallScore(BenchmarkResult result)
        {
            // Weighted average: CPU 40%, Memory 30%, Disk 30%
            return (int)(
                (result.CPU.SingleCoreScore + result.CPU.MultiCoreScore) / 2 * 0.4 +
                result.Memory.Score * 0.3 +
                result.Disk.Score * 0.3
            );
        }

        /// <summary>
        /// Get grade letter based on score
        /// </summary>
        private string GetGrade(int score)
        {
            return score switch
            {
                >= 5000 => "S+ (Excellent)",
                >= 4000 => "A (Very Good)",
                >= 3000 => "B (Good)",
                >= 2000 => "C (Average)",
                >= 1000 => "D (Below Average)",
                _ => "F (Poor)"
            };
        }

        /// <summary>
        /// Count prime numbers in range (CPU test)
        /// </summary>
        private int CountPrimes(int start, int end)
        {
            int count = 0;
            for (int n = start; n <= end; n++)
            {
                if (IsPrime(n)) count++;
            }
            return count;
        }

        private bool IsPrime(int n)
        {
            if (n < 2) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;

            int sqrt = (int)Math.Sqrt(n);
            for (int i = 3; i <= sqrt; i += 2)
            {
                if (n % i == 0) return false;
            }
            return true;
        }

        /// <summary>
        /// Compare two benchmark results
        /// </summary>
        public string CompareResults(BenchmarkResult before, BenchmarkResult after)
        {
            int cpuDiff = after.CPU.MultiCoreScore - before.CPU.MultiCoreScore;
            int memDiff = after.Memory.Score - before.Memory.Score;
            int diskDiff = after.Disk.Score - before.Disk.Score;
            int overallDiff = after.OverallScore - before.OverallScore;

            string GetTrend(int diff) => diff > 0 ? $"+{diff} ↑" : diff < 0 ? $"{diff} ↓" : "= unchanged";

            return $"""
                Performance Comparison:
                ═══════════════════════════════════════
                CPU:      {GetTrend(cpuDiff)}
                Memory:   {GetTrend(memDiff)}
                Disk:     {GetTrend(diskDiff)}
                ───────────────────────────────────────
                Overall:  {GetTrend(overallDiff)} ({before.Grade} → {after.Grade})
                """;
        }
    }
}
