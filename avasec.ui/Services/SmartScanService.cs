using System.IO;
using AVASec.Antivirus.Services;
using AVASec.Core.Services;
using AVASec.Optimization.Services;

namespace AVASec.UI.Services;

/// <summary>
/// Smart Scan pipeline: quick scan + AI heuristics + temp cleanup + RAM trim.
/// Quét thông minh: quét nhanh + AI + dọn rác + tối ưu RAM.
/// </summary>
public class SmartScanService
{
    private readonly FileScannerService _fileScanner;
    private readonly AIDetectionService _aiDetection;
    private readonly DiskCleanerService _diskCleaner;
    private readonly RamOptimizerService _ramOptimizer;
    private readonly BenchmarkService _benchmark;

    public SmartScanService(
        FileScannerService fileScanner,
        DiskCleanerService diskCleaner,
        RamOptimizerService ramOptimizer)
    {
        _fileScanner = fileScanner;
        _aiDetection = new AIDetectionService();
        _diskCleaner = diskCleaner;
        _ramOptimizer = ramOptimizer;
        _benchmark = new BenchmarkService();
    }

    public async Task<SmartScanResult> RunAsync(
        SmartScanOptions options,
        IProgress<SmartScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var result = new SmartScanResult { StartedAt = DateTime.Now };
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            Report(progress, "Scanning threats / Quét mối đe dọa", 10);
            if (options.RunThreatScan)
            {
                var scanPath = options.ScanPath ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var scanResults = await _fileScanner.ScanDirectoryAsync(scanPath, cancellationToken: cancellationToken);
                var threats = scanResults.Where(r => r.IsThreat).ToList();
                result.FilesScanned = scanResults.Count;
                result.ThreatsFound = threats.Count;
                result.Threats.AddRange(threats.Select(t => t.FilePath));
            }

            Report(progress, "AI heuristic analysis / Phân tích AI heuristic", 35);
            if (options.RunAiPass && result.Threats.Count == 0)
            {
                var downloads = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (Directory.Exists(downloads))
                {
                    var aiResults = await _aiDetection.AnalyzeDirectoryAsync(downloads);
                    var suspicious = aiResults.Where(r =>
                        r.ThreatLevel >= AIDetectionService.AIThreatLevel.Medium).ToList();
                    result.AiSuspiciousCount = suspicious.Count;
                    result.Threats.AddRange(suspicious.Select(s => s.FilePath));
                }
            }

            Report(progress, "Cleaning temp files / Dọn file tạm", 55);
            if (options.RunCleanup)
            {
                var cleanResult = await _diskCleaner.CleanSystemTempAsync();
                result.BytesFreed = cleanResult.BytesFreed;
                result.ItemsCleaned = cleanResult.FilesDeleted;
            }

            Report(progress, "Optimizing RAM / Tối ưu RAM", 75);
            if (options.RunRamTrim)
            {
                var ramResult = await _ramOptimizer.OptimizeMemoryAsync();
                result.MemoryFreedMb = ramResult.MemoryFreedMB;
            }

            Report(progress, "Benchmark score / Đánh giá hiệu năng", 90);
            if (options.RunBenchmark)
            {
                var bench = await _benchmark.RunFullBenchmarkAsync();
                result.BenchmarkGrade = bench.Grade;
                result.BenchmarkScore = bench.OverallScore;
            }

            Report(progress, "Complete / Hoàn tất", 100);
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }
        finally
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
            result.CompletedAt = DateTime.Now;
        }

        return result;
    }

    private static void Report(IProgress<SmartScanProgress>? progress, string step, int percent)
    {
        progress?.Report(new SmartScanProgress { Step = step, Percent = percent });
    }
}

public sealed class SmartScanOptions
{
    public bool RunThreatScan { get; set; } = true;
    public bool RunAiPass { get; set; } = true;
    public bool RunCleanup { get; set; } = true;
    public bool RunRamTrim { get; set; } = true;
    public bool RunBenchmark { get; set; } = true;
    public string? ScanPath { get; set; }
}

public sealed class SmartScanProgress
{
    public string Step { get; init; } = string.Empty;
    public int Percent { get; init; }
}

public sealed class SmartScanResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public int FilesScanned { get; set; }
    public int ThreatsFound { get; set; }
    public int AiSuspiciousCount { get; set; }
    public long BytesFreed { get; set; }
    public int ItemsCleaned { get; set; }
    public long MemoryFreedMb { get; set; }
    public string BenchmarkGrade { get; set; } = "N/A";
    public double BenchmarkScore { get; set; }
    public List<string> Threats { get; set; } = new();
}
