using System; // Verified
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using AVASec.Plugin.Core.Interfaces;

namespace AVASec.Plugins.SystemSweeper
{
    public partial class SweeperWindow : Window
    {
        private readonly IPluginContext? _context;

        public SweeperWindow(IPluginContext? context)
        {
            InitializeComponent();
            _context = context;
        }

        private async void Clean_Click(object sender, RoutedEventArgs e)
        {
            CleanBtn.IsEnabled = false;
            CleanProgress.Visibility = Visibility.Visible;
            CleanProgress.IsIndeterminate = true;
            TotalSizeText.Text = "Status: Cleaning...";

            int itemsCleaned = 0;
            long bytesFreed = 0;

            await Task.Run(async () =>
            {
                // 1. Clean Temp Files / Dọn dẹp tệp tạm
                if (CheckTemp.IsChecked == true)
                {
                    string tempPath = Path.GetTempPath();
                    try
                    {
                        var files = Directory.GetFiles(tempPath);
                        
                        // Use Parallel.ForEach to speed up deletion of many small files
                        // Sử dụng Parallel.ForEach để tăng tốc độ xóa nhiều file nhỏ
                        Parallel.ForEach(files, file => 
                        {
                            try 
                            { 
                                FileInfo fi = new FileInfo(file);
                                long len = fi.Length;
                                File.Delete(file); 
                                
                                // Atomic updates for thread safety / Cập nhật nguyên tử để đảm bảo an toàn luồng
                                System.Threading.Interlocked.Add(ref bytesFreed, len);
                                System.Threading.Interlocked.Increment(ref itemsCleaned);
                            } catch { }
                        });
                    }
                    catch { }
                }

                await Task.Delay(500); // UI feel / Giả lập độ trễ để người dùng cảm nhận được tiến trình

                // 2. Browser Cache Cleaning / Dọn dẹp cache trình duyệt
                if (CheckPrefetch.IsChecked == true) // Reusing 'Prefetch' checkbox for 'Browser Cache' for now, or assume it means 'System & Browser Junk'
                {
                    // Chrome
                    string chromePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data\Default\Cache\Cache_Data");
                    CleanDirectory(chromePath, ref itemsCleaned, ref bytesFreed);

                    // Edge
                    string edgePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\User Data\Default\Cache\Cache_Data");
                    CleanDirectory(edgePath, ref itemsCleaned, ref bytesFreed);
                    
                    // Windows Prefetch (Require Admin)
                    string prefetchPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Prefetch");
                    CleanDirectory(prefetchPath, ref itemsCleaned, ref bytesFreed);
                }

                if (CheckLogs.IsChecked == true) 
                { 
                    // Windows Temp
                    string winTemp = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Temp");
                    CleanDirectory(winTemp, ref itemsCleaned, ref bytesFreed);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    CleanProgress.Visibility = Visibility.Hidden;
                    CleanBtn.IsEnabled = true;
                    string size = FormatSize(bytesFreed);
                    TotalSizeText.Text = $"Cleaned {itemsCleaned} files. Recovered {size}.";
                    _context?.Notify("System Sweeper", $"Cleanup complete. Recovered {size}.", "Success");
                });
            });
        }

        private string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / 1024.0 / 1024.0:F1} MB";
        }

        private void CleanDirectory(string path, ref int itemsCleaned, ref long bytesFreed)
        {
            if (!Directory.Exists(path)) return;
            
            try
            {
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(file);
                        long len = fi.Length;
                        File.Delete(file);
                        System.Threading.Interlocked.Add(ref bytesFreed, len);
                        System.Threading.Interlocked.Increment(ref itemsCleaned);
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
