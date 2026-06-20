using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AVASec.Optimization.Services;

namespace AVASec.UI.Views
{
    public partial class SystemBoosterWindow : Window
    {
        private readonly SystemTweaksService _tweaksService;
        private readonly DiskCleanerService _diskCleaner;

        public SystemBoosterWindow()
        {
            InitializeComponent();
            _tweaksService = new SystemTweaksService();
            _diskCleaner = new DiskCleanerService();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Category_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton btn && DesktopPanel != null)
            {
                // Hide all panels
                DesktopPanel.Visibility = Visibility.Collapsed;
                NetworkPanel.Visibility = Visibility.Collapsed;
                PowerPanel.Visibility = Visibility.Collapsed;
                CleanupPanel.Visibility = Visibility.Collapsed;
                if (BenchmarkPanel != null) BenchmarkPanel.Visibility = Visibility.Collapsed;

                try
                {
                    // Show selected panel and update header text using localized resources
                    switch (btn.Tag.ToString())
                    {
                        case "💻": 
                            DesktopPanel.Visibility = Visibility.Visible; 
                            ActiveTabTitle.Text = (string)TryFindResource("Lang.Tools.SystemBooster.Header.Desktop") ?? "Desktop Optimization";
                            ActiveTabDesc.Text = (string)TryFindResource("Lang.Tools.SystemBooster.Header.DesktopDesc");
                            break;
                        case "🌐": 
                            NetworkPanel.Visibility = Visibility.Visible; 
                            ActiveTabTitle.Text = (string)TryFindResource("Lang.Tools.SystemBooster.Cat.Network") ?? "Network Settings";
                            ActiveTabDesc.Text = (string)TryFindResource("Lang.Tools.SystemBooster.Header.NetworkDesc");
                            break;
                        case "⚡": 
                            PowerPanel.Visibility = Visibility.Visible; 
                            ActiveTabTitle.Text = (string)TryFindResource("Lang.Tools.SystemBooster.Cat.Power") ?? "Power Management";
                            ActiveTabDesc.Text = (string)TryFindResource("Lang.Tools.SystemBooster.Header.PowerDesc");
                            break;
                        case "🧹": 
                            CleanupPanel.Visibility = Visibility.Visible; 
                            ActiveTabTitle.Text = (string)TryFindResource("Lang.Tools.SystemBooster.Cat.Stability") ?? "System Cleanup";
                            ActiveTabDesc.Text = (string)TryFindResource("Lang.Tools.SystemBooster.Header.StabilityDesc");
                            break;
                        case "📊": 
                            if (BenchmarkPanel != null) BenchmarkPanel.Visibility = Visibility.Visible; 
                            ActiveTabTitle.Text = "Performance Benchmark / Đánh giá Hiệu năng";
                            ActiveTabDesc.Text = "Verify and measure performance changes before & after system optimizations. / Đo lường và so sánh hiệu năng hệ thống trước và sau tối ưu.";
                            break;
                    }
                }
                catch { }
            }
        }

        private async void BoostButton_Click(object sender, RoutedEventArgs e)
        {
            BoostBtnV2.IsEnabled = false;
            StatusPanel.Visibility = Visibility.Visible;
            BoostProgressV2.Value = 0;
            ProgressPercentText.Text = "0%";
            StatusTextV2.Text = "Analyzing system...";

            try
            {
                int totalSteps = 7;
                int completedSteps = 0;

                // 1. Network Optimization
                if (CheckNetworkV2.IsChecked == true)
                {
                    StatusTextV2.Text = "Optimizing Network Settings (TCP/IP)...";
                    await Task.Run(() => _tweaksService.OptimizeNetwork());
                    completedSteps++;
                    UpdateProgress(completedSteps, totalSteps);
                    await Task.Delay(400); 
                }

                // 2. Visual Effects
                if (CheckVisualsV2.IsChecked == true)
                {
                    StatusTextV2.Text = "Adjusting Visual Effects & Animations...";
                    await Task.Run(() => _tweaksService.DisableVisualEffects());
                    completedSteps++;
                    UpdateProgress(completedSteps, totalSteps);
                    await Task.Delay(400);
                }

                // 3. Telemetry
                if (CheckTelemetryV2.IsChecked == true)
                {
                    StatusTextV2.Text = "Disabling Diagnostic Telemetry...";
                    await Task.Run(() => _tweaksService.DisableTelemetry());
                    completedSteps++;
                    UpdateProgress(completedSteps, totalSteps);
                    await Task.Delay(400);
                }

                // 4. Clean Temp Files
                if (CheckTempFilesV2.IsChecked == true)
                {
                    StatusTextV2.Text = "Cleaning System Temporary Files...";
                    await _diskCleaner.CleanSystemTempAsync();
                    completedSteps++;
                    UpdateProgress(completedSteps, totalSteps);
                    await Task.Delay(400);
                }

                // 5. Disable Sleep
                if (CheckSleepV2.IsChecked == true)
                {
                    StatusTextV2.Text = "Applying Power Policy: Disable Sleep...";
                    await Task.Run(() => _tweaksService.DisableSleep());
                    completedSteps++;
                    UpdateProgress(completedSteps, totalSteps);
                    await Task.Delay(400);
                }

                // 6. Disable Hibernation
                if (CheckHiberV2.IsChecked == true)
                {
                    StatusTextV2.Text = "Applying Power Policy: Disable Hibernation...";
                    await Task.Run(() => _tweaksService.DisableHibernation());
                    completedSteps++;
                    UpdateProgress(completedSteps, totalSteps);
                    await Task.Delay(400);
                }

                // 7. High Performance Plan
                if (CheckHighPerfV2.IsChecked == true)
                {
                    StatusTextV2.Text = "Switching to High Performance Power Plan...";
                    await Task.Run(() => _tweaksService.SetHighPerformancePlan());
                    completedSteps++;
                    UpdateProgress(completedSteps, totalSteps);
                }

                StatusTextV2.Text = "Optimization Complete! Your system is now optimized.";
                var response = MessageBox.Show(
                    "System Boost Completed Successfully!\nWould you like to run a Performance Benchmark to verify the improvements?\n\nTối ưu hóa hệ thống thành công!\nBạn có muốn chạy Đánh giá Hiệu năng để kiểm tra kết quả cải thiện?",
                    "System Booster", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);
                
                if (response == MessageBoxResult.Yes && BenchmarkTabBtn != null)
                {
                    BenchmarkTabBtn.IsChecked = true;
                }
                else
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                StatusTextV2.Text = "Error: " + ex.Message;
                MessageBox.Show($"Optimization Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BoostBtnV2.IsEnabled = true;
            }
        }

        private void UpdateProgress(int current, int total)
        {
            double progress = (double)current / total * 100;
            BoostProgressV2.Value = progress;
            ProgressPercentText.Text = $"{(int)progress}%";
        }
    }
}
