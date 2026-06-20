using System;
using System.Windows;
using System.Windows.Input;

namespace AVASec.UI.Views
{
    /// <summary>
    /// Toolbox Window - All System Tools / Cửa sổ Toolbox - Tất cả Công cụ Hệ thống
    /// </summary>
    public partial class ToolboxWindow : Window
    {
        public ToolboxWindow()
        {
            InitializeComponent();
        }

        private void Tool_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string toolName)
            {
                try
                {
                    switch (toolName)
                    {
                        case "StartupManager":
                            var startupWindow = new StartupManagerWindow();
                            startupWindow.ShowDialog();
                            break;

                        case "DiskCleaner":
                            var diskCleanupWindow = new DiskCleanupWindow();
                            diskCleanupWindow.ShowDialog();
                            break;

                        case "RegistryCleaner":
                            var registryWindow = new RegistryTweaksWindow();
                            registryWindow.ShowDialog();
                            break;

                        case "MyWin10":
                            var win11Window = new Windows11TweaksWindow();
                            win11Window.ShowDialog();
                            break;

                        case "SmartRAM":
                            OpenSmartRAM();
                            break;

                        case "LargeFileFinder":
                            OpenLargeFileFinder();
                            break;

                        case "EmptyFolderScanner":
                            OpenEmptyFolderScanner();
                            break;

                        case "AutoShutdown":
                            OpenAutoShutdown();
                            break;

                        case "FileShredder":
                            OpenFileShredder();
                            break;

                        case "InternetBooster":
                            OpenInternetBooster();
                            break;

                        case "DiskDoctor":
                            OpenDiskDoctor();
                            break;

                        case "ShortcutFixer":
                            OpenShortcutFixer();
                            break;

                        default:
                            MessageBox.Show($"🚧 {toolName} đang được phát triển!\n{toolName} is under development!",
                                "Thông báo / Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Lỗi mở {toolName}:\n{ex.Message}",
                        "Lỗi / Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenSmartRAM()
        {
            var result = MessageBox.Show(
                "💾 Smart RAM sẽ tối ưu bộ nhớ của bạn.\nBạn có muốn tiếp tục không?\n\nSmart RAM will optimize your memory.\nDo you want to continue?",
                "Smart RAM", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var ramOptimizer = new AVASec.Optimization.Services.RamOptimizerService();
                var ramInfo = ramOptimizer.GetRamInfo();
                
                MessageBox.Show(
                    $"📊 Thông tin RAM hiện tại:\n\n" +
                    $"Tổng RAM: {ramInfo.TotalMemoryMB / 1024.0:F1} GB\n" +
                    $"Đã dùng: {ramInfo.UsedMemoryMB / 1024.0:F1} GB ({ramInfo.UsagePercentage}%)\n" +
                    $"Còn trống: {ramInfo.AvailableMemoryMB / 1024.0:F1} GB\n\n" +
                    "✅ Đang tối ưu RAM...",
                    "Smart RAM", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenLargeFileFinder()
        {
            MessageBox.Show(
                "📁 Large File Finder sẽ tìm các file lớn (>100MB) trên ổ đĩa.\n\n" +
                "Large File Finder will search for large files (>100MB) on disk.\n\n" +
                "🚧 Tính năng đang được phát triển!",
                "Large File Finder", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenEmptyFolderScanner()
        {
            MessageBox.Show(
                "📂 Empty Folder Scanner sẽ tìm và xóa các thư mục trống.\n\n" +
                "Empty Folder Scanner will find and delete empty folders.\n\n" +
                "🚧 Tính năng đang được phát triển!",
                "Empty Folder Scanner", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenAutoShutdown()
        {
            var result = MessageBox.Show(
                "⏻ Auto Shutdown - Lên lịch tắt máy tự động.\n\n" +
                "Bạn muốn lên lịch tắt máy sau bao lâu?\n" +
                "• Nhấn Yes: 1 giờ\n" +
                "• Nhấn No: Hủy lệnh tắt máy hiện tại",
                "Auto Shutdown", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start("shutdown", "/s /t 3600");
                    MessageBox.Show("✅ Đã lên lịch tắt máy sau 1 giờ.\nScheduled shutdown in 1 hour.",
                        "Auto Shutdown", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Lỗi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    System.Diagnostics.Process.Start("shutdown", "/a");
                    MessageBox.Show("✅ Đã hủy lệnh tắt máy.\nCancelled scheduled shutdown.",
                        "Auto Shutdown", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch { }
            }
        }

        private void OpenFileShredder()
        {
            MessageBox.Show(
                "🗑 File Shredder sẽ xóa vĩnh viễn các file, không thể khôi phục.\n\n" +
                "File Shredder will permanently delete files, unrecoverable.\n\n" +
                "🚧 Tính năng đang được phát triển!",
                "File Shredder", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void OpenInternetBooster()
        {
            MessageBox.Show(
                "🌐 Internet Booster sẽ tối ưu cài đặt mạng.\n\n" +
                "Internet Booster will optimize network settings.\n\n" +
                "🚧 Tính năng đang được phát triển!",
                "Internet Booster", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenDiskDoctor()
        {
            var result = MessageBox.Show(
                "💿 Disk Doctor sẽ kiểm tra và sửa lỗi ổ đĩa.\n\n" +
                "Disk Doctor will check and repair disk errors.\n\n" +
                "Bạn có muốn chạy kiểm tra ổ C: không?",
                "Disk Doctor", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c chkdsk C: /F",
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Lỗi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenShortcutFixer()
        {
            MessageBox.Show(
                "🔗 Shortcut Fixer sẽ tìm và sửa các shortcut hỏng.\n\n" +
                "Shortcut Fixer will find and fix broken shortcuts.\n\n" +
                "🚧 Tính năng đang được phát triển!",
                "Shortcut Fixer", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (BuiltInToolsPanel == null || PluginsPanel == null) return;

            if (sender is System.Windows.Controls.RadioButton rb && rb.Tag?.ToString() == "Plugins")
            {
                BuiltInToolsPanel.Visibility = Visibility.Collapsed;
                PluginsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                BuiltInToolsPanel.Visibility = Visibility.Visible;
                PluginsPanel.Visibility = Visibility.Collapsed;
            }
        }

        // Window Controls
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TrayButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
    }
}
