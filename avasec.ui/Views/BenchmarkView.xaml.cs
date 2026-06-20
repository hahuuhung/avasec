using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using AVASec.Core.Services;

namespace AVASec.UI.Views
{
    /// <summary>
    /// Interaction logic for BenchmarkView.xaml
    /// </summary>
    public partial class BenchmarkView : UserControl
    {
        private readonly BenchmarkService _benchmarkService;
        private BenchmarkService.BenchmarkResult? _lastResult;

        public BenchmarkView()
        {
            InitializeComponent();
            _benchmarkService = new BenchmarkService();
        }

        private async void StartBenchmark_Click(object sender, RoutedEventArgs e)
        {
            StartBenchmarkBtn.IsEnabled = false;
            CompareBtn.IsEnabled = false;

            // Store previous result for comparison
            var previousResult = _lastResult;

            // Reset UI
            ResetUI();

            try
            {
                var status = new Progress<string>(s => StatusText.Text = s);
                var progress = new Progress<int>(p =>
                {
                    ProgressBar.Width = (p / 100.0) * 150;
                });

                // Run benchmark
                _lastResult = await _benchmarkService.RunFullBenchmarkAsync(status, progress);

                // Update UI with results
                UpdateUI(_lastResult);

                // Enable compare if we have previous result
                CompareBtn.IsEnabled = previousResult != null;

                if (previousResult != null)
                {
                    var comparison = _benchmarkService.CompareResults(previousResult, _lastResult);
                    StatusText.Text = "Benchmark complete! Click Compare for details.";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
            finally
            {
                StartBenchmarkBtn.IsEnabled = true;
            }
        }

        private void ResetUI()
        {
            ScoreText.Text = "--";
            GradeText.Text = "Testing...";
            CPUScore.Text = "--";
            CPUSingleScore.Text = "--";
            CPUMultiScore.Text = "--";
            CPUName.Text = "Testing...";
            MemoryScore.Text = "--";
            MemReadSpeed.Text = "-- MB/s";
            MemWriteSpeed.Text = "-- MB/s";
            MemoryInfo.Text = "Testing...";
            DiskScore.Text = "--";
            DiskSeqRead.Text = "-- MB/s";
            DiskSeqWrite.Text = "-- MB/s";
            DiskInfo.Text = "Testing...";
            ProgressBar.Width = 0;
        }

        private void UpdateUI(BenchmarkService.BenchmarkResult result)
        {
            // Overall Score with animation
            AnimateScore(ScoreText, result.OverallScore);
            GradeText.Text = result.Grade;

            // CPU Results
            AnimateScore(CPUScore, (result.CPU.SingleCoreScore + result.CPU.MultiCoreScore) / 2);
            CPUSingleScore.Text = result.CPU.SingleCoreScore.ToString();
            CPUMultiScore.Text = result.CPU.MultiCoreScore.ToString();
            CPUName.Text = $"{result.CPU.ProcessorName.Substring(0, Math.Min(30, result.CPU.ProcessorName.Length))}...";

            // Memory Results
            AnimateScore(MemoryScore, result.Memory.Score);
            MemReadSpeed.Text = $"{result.Memory.ReadSpeedMBps:F0} MB/s";
            MemWriteSpeed.Text = $"{result.Memory.WriteSpeedMBps:F0} MB/s";
            MemoryInfo.Text = $"{result.Memory.TotalMemoryMB / 1024} GB Total | {result.Memory.AvailableMemoryMB / 1024} GB Free";

            // Disk Results
            AnimateScore(DiskScore, result.Disk.Score);
            DiskSeqRead.Text = $"{result.Disk.SequentialReadMBps:F0} MB/s";
            DiskSeqWrite.Text = $"{result.Disk.SequentialWriteMBps:F0} MB/s";
            DiskInfo.Text = $"{result.Disk.FreeSpaceGB} GB Free / {result.Disk.TotalSpaceGB} GB Total";

            // Update progress ring
            UpdateProgressRing(result.OverallScore);
        }

        private void AnimateScore(TextBlock textBlock, int targetValue)
        {
            int currentValue = 0;
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };

            timer.Tick += (s, e) =>
            {
                currentValue += Math.Max(1, (targetValue - currentValue) / 10);
                if (currentValue >= targetValue)
                {
                    currentValue = targetValue;
                    timer.Stop();
                }
                textBlock.Text = currentValue.ToString();
            };

            timer.Start();
        }

        private void UpdateProgressRing(int score)
        {
            // Normalize score to 0-100 for ring display (assuming max score ~5000)
            double normalizedScore = Math.Min(100, score / 50.0);
            double circumference = Math.PI * 180; // π * diameter
            double dashLength = (normalizedScore / 100.0) * circumference;

            ProgressRing.StrokeDashArray = new System.Windows.Media.DoubleCollection { dashLength / 10, 100 };
        }

        private void CompareResults_Click(object sender, RoutedEventArgs e)
        {
            if (_lastResult != null)
            {
                MessageBox.Show(
                    $"Benchmark Results\n\n" +
                    $"Overall Score: {_lastResult.OverallScore}\n" +
                    $"Grade: {_lastResult.Grade}\n\n" +
                    $"CPU Score: {(_lastResult.CPU.SingleCoreScore + _lastResult.CPU.MultiCoreScore) / 2}\n" +
                    $"Memory Score: {_lastResult.Memory.Score}\n" +
                    $"Disk Score: {_lastResult.Disk.Score}",
                    "Benchmark Results",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
    }
}
