using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AVASec.Core.Models;

namespace AVASec.UI.Views
{
    /// <summary>
    /// Splash screen displayed during application startup
    /// Màn hình splash hiển thị khi khởi động ứng dụng
    /// </summary>
    public partial class SplashScreen : Window
    {
        private readonly DispatcherTimer _progressTimer;
        private double _currentProgress = 0;
        private readonly string[] _statusMessages = new[]
        {
            "Đang khởi tạo cơ sở dữ liệu... / Initializing database...",
            "Đang tải cấu hình... / Loading configuration...",
            "Đang kiểm tra giấy phép... / Checking license...",
            "Đang khởi tạo dịch vụ... / Initializing services...",
            "Đang chuẩn bị giao diện... / Preparing interface...",
            "Hoàn tất! / Ready!"
        };

        public SplashScreen()
        {
            InitializeComponent();

            VersionText.Text = $"v{AppVersionInfo.Version}";

            // Animate progress bar / Hoạt ảnh thanh tiến trình
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(40)
            };
            _progressTimer.Tick += ProgressTimer_Tick;
        }

        /// <summary>
        /// Start the splash screen animation / Bắt đầu hoạt ảnh splash screen
        /// </summary>
        public void StartLoading()
        {
            _progressTimer.Start();
        }

        private void ProgressTimer_Tick(object? sender, EventArgs e)
        {
            _currentProgress += 1.2;

            if (_currentProgress > 100)
                _currentProgress = 100;

            // Update progress bar width / Cập nhật chiều rộng thanh tiến trình
            ProgressBar.Width = (300.0 * _currentProgress / 100.0);

            // Update status message / Cập nhật trạng thái
            int messageIndex = Math.Min((int)(_currentProgress / 20), _statusMessages.Length - 1);
            StatusText.Text = _statusMessages[messageIndex];

            if (_currentProgress >= 100)
            {
                _progressTimer.Stop();
            }
        }

        /// <summary>
        /// Set status message / Đặt thông báo trạng thái
        /// </summary>
        public void SetStatus(string message)
        {
            StatusText.Text = message;
        }

        /// <summary>
        /// Set progress / Đặt tiến trình
        /// </summary>
        public void SetProgress(double percent)
        {
            _currentProgress = Math.Min(percent, 100);
            ProgressBar.Width = (300.0 * _currentProgress / 100.0);
        }
    }
}
