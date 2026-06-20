using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AVASec.Antivirus.Services;
using AVASec.Core.Models; // Assuming ThreatInfo is here, or similar

namespace AVASec.UI.Views
{
    public partial class VirusScannerWindow : Window
    {
        private readonly FileScannerService _scannerService;
        private readonly QuarantineService _quarantineService;
        private CancellationTokenSource? _cts;
        private bool _isScanning = false;

        public VirusScannerWindow()
        {
            InitializeComponent();
            _scannerService = new FileScannerService();
            // Resolve QuarantineService manually since this window is often created via new() without DI scope
            _quarantineService = App.ServiceProvider.GetRequiredService<QuarantineService>();
        }

        private void OpenQuarantine_Click(object sender, RoutedEventArgs e)
        {
            var window = App.ServiceProvider.GetRequiredService<QuarantineWindow>();
            window.Owner = this;
            window.ShowDialog();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isScanning) return;

            string scanPath = "";
            bool isQuick = RadioQuick.IsChecked == true;
            bool isFull = RadioFull.IsChecked == true;
            bool isBackground = CheckBackground.IsChecked == true;

            if (isQuick)
            {
                scanPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
            else if (isFull)
            {
                scanPath = "C:\\"; // Require Admin usually
            }
            else
            {
                // Custom
                var dialog = new Microsoft.Win32.OpenFolderDialog();
                if (dialog.ShowDialog() == true)
                {
                    scanPath = dialog.FolderName;
                }
                else
                {
                    return;
                }
            }

            if (string.IsNullOrEmpty(scanPath) || !System.IO.Directory.Exists(scanPath))
            {
                MessageBox.Show("Invalid scan path selected. / Đường dẫn quét không hợp lệ.", "Error / Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // UI State
            _isScanning = true;
            StartButton.Visibility = Visibility.Collapsed;
            StopButton.Visibility = Visibility.Visible;
            ScanningPanel.Visibility = Visibility.Visible;
            ResultsPanel.Visibility = Visibility.Collapsed;
            FilesScannedText.Text = "0";
            ThreatsFoundText.Text = "0";
            ProgressPercentText.Text = "0%";
            StartSpinner();

            _cts = new CancellationTokenSource();

            try
            {
                // If background scan is checked, we can optionally minimize or just notify
                // Nếu chọn quét nền, có thể thu nhỏ hoặc thông báo
                if (isBackground)
                {
                    CurrentStatusText.Text = "Đang chạy chế độ nền (CPU thấp)...";
                }

                var progress = new Progress<ScanProgress>(p =>
                {
                    FilesScannedText.Text = p.ScannedFiles.ToString();
                    CurrentFileText.Text = p.CurrentFile;
                    ThreatsFoundText.Text = p.ThreatsFound.ToString();
                    
                    if (p.TotalFiles > 0)
                    {
                        double percentage = (double)p.ScannedFiles / p.TotalFiles * 100;
                        ProgressPercentText.Text = $"{(int)percentage}%";
                        
                        // Stage Logic Simulation
                        UpdateScanStages(percentage);
                    }

                    if (!isBackground) 
                    {
                        CurrentStatusText.Text = "ACTIVE PROTECTION: SCANNING...";
                    }
                });

                // Run Scan with Background Flag
                var results = await _scannerService.ScanDirectoryAsync(scanPath, progress, _cts.Token, isBackground);

                // Finished
                ScanFinished(results);
            }
            catch (OperationCanceledException)
            {
                CurrentStatusText.Text = "Đã hủy quét";
                StopSpinner();
                _isScanning = false;
                StartButton.Visibility = Visibility.Visible;
                StopButton.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi trong quá trình quét: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                _isScanning = false;
                StartButton.Visibility = Visibility.Visible;
                StopButton.Visibility = Visibility.Collapsed;
                StopSpinner();
            }
        }

        private void ScanFinished(List<ScanResult> results)
        {
            _isScanning = false;
            StopSpinner();

            ScanningPanel.Visibility = Visibility.Collapsed;
            ResultsPanel.Visibility = Visibility.Visible;
            StartButton.Visibility = Visibility.Collapsed;
            StopButton.Visibility = Visibility.Collapsed;
            CloseResultButton.Visibility = Visibility.Visible;

            var threats = results.Where(r => r.IsThreat).ToList();
            ResultThreatCount.Text = $"{threats.Count} THREATS FOUND";
            
            if (threats.Count > 0)
            {
                ThreatsList.ItemsSource = threats;
                NoThreatsText.Visibility = Visibility.Collapsed;
            }
            else
            {
                ThreatsList.ItemsSource = null;
                NoThreatsText.Visibility = Visibility.Visible;
            }

            // Finalize stages
            Stage4Status.Text = "Complete";
            Stage4Icon.Fill = (Brush)FindResource("SuccessBrush");
        }

        private void UpdateScanStages(double percentage)
        {
            if (percentage > 5) { Stage1Status.Text = "Verified"; Stage1Icon.Fill = (Brush)FindResource("SuccessBrush"); }
            if (percentage > 25) { Stage2Status.Text = "Scanning..."; Stage2Icon.Fill = (Brush)FindResource("CyanBrush"); }
            if (percentage > 50) { Stage2Status.Text = "Verified"; Stage2Icon.Fill = (Brush)FindResource("SuccessBrush"); }
            if (percentage > 60) { Stage3Status.Text = "Analyzing..."; Stage3Icon.Fill = (Brush)FindResource("CyanBrush"); }
            if (percentage > 85) { Stage3Status.Text = "Verified"; Stage3Icon.Fill = (Brush)FindResource("SuccessBrush"); }
            if (percentage > 90) { Stage4Status.Text = "Cloud Auth..."; Stage4Icon.Fill = (Brush)FindResource("CyanBrush"); }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isScanning)
            {
                var result = MessageBox.Show("Quá trình quét đang diễn ra. Bạn có chắc muốn dừng?", 
                    "Dừng quét", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _cts?.Cancel();
                    Close();
                }
            }
            else
            {
                Close();
            }
        }

        private async void QuarantineItem_Click(object sender, RoutedEventArgs e)
        {
             if (sender is Button btn && btn.Tag is ScanResult threat)
             {
                 try 
                 {
                     var result = await _quarantineService.QuarantineFileAsync(threat.FilePath, threat.ThreatName, 0); // 0 for now as ScanId
                     
                     if (result.Success)
                     {
                         MessageBox.Show("File quarantined successfully.\nFile đã được cách ly thành công.", "Success / Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                         // Refresh list removing the quarantined item
                         var list = ThreatsList.ItemsSource as List<ScanResult>;
                         if (list != null)
                         {
                             list.Remove(threat);
                             ThreatsList.Items.Refresh(); // Or use ObservableCollection
                         }
                     }
                     else
                     {
                         MessageBox.Show($"Failed to quarantine: {result.Message}\nThất bại: {result.Message}", "Error / Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                     }
                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                 }
             }
        }
        private void StartSpinner()
        {
            var animation = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromSeconds(2)));
            animation.RepeatBehavior = RepeatBehavior.Forever;
            SpinnerTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void StopSpinner()
        {
            SpinnerTransform.BeginAnimation(RotateTransform.AngleProperty, null);
        }

        // Window Controls
        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TrayButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                MessageBox.Show("Please use the main Dashboard for user login.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); 
            }
            catch {}
        }

        private void ChatToggle_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Support Chat is available on the main Dashboard.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
             try 
             {
                 if (Application.Current is App app && App.ServiceProvider != null)
                 {
                     var win = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<SettingsView>(App.ServiceProvider);
                     win.Owner = this;
                     win.ShowDialog();
                 }
                 else
                 {
                     new SettingsView().ShowDialog();
                 }
             }
             catch (Exception ex)
             {
                 MessageBox.Show($"Error opening Settings: {ex.Message}");
             }
        }

        private void Toolbox_Click(object sender, RoutedEventArgs e)
        {
            try { new ToolboxWindow { Owner = this }.ShowDialog(); }
            catch {}
        }
    }
}
