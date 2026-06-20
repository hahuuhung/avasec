using System.Windows;
using System.Windows.Media;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;

namespace AVASec.UI.Views
{
    public partial class OnboardingWindow : Window
    {
        private readonly ISettingsService _settingsService;
        private int _step;
        private AppSettings _settings;

        public bool RunSmartScanAfterClose { get; private set; }

        public OnboardingWindow(ISettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _settings = settingsService.LoadSettings();
            ShowStep(0);
        }

        private void ShowStep(int step)
        {
            _step = step;
            BackButton.Visibility = step > 0 ? Visibility.Visible : Visibility.Collapsed;
            NextButton.Content = step == 2 ? "Bắt đầu / Start" : "Tiếp / Next →";

            Dot1.Fill = step == 0 ? Brushes.Cyan : new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55));
            Dot2.Fill = step == 1 ? Brushes.Cyan : new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55));
            Dot3.Fill = step == 2 ? Brushes.Cyan : new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55));

            switch (step)
            {
                case 0:
                    StepTitle.Text = "Bước 1: Quét thông minh / Smart Scan";
                    StepDescription.Text =
                        "AVA Mind AI quét virus, dọn rác tạm và tối ưu RAM trong một lần bấm.\n" +
                        "AVA Mind AI scans threats, cleans temp files, and optimizes RAM in one click.";
                    StepHint.Text = "💡 Nhấn \"Smart Scan\" trên Dashboard sau khi hoàn tất.";
                    break;
                case 1:
                    StepTitle.Text = "Bước 2: Bảo vệ quyền riêng tư / Privacy Guardian";
                    StepDescription.Text =
                        "Dọn dấu vết trình duyệt, cookie và file nhạy cảm — hoạt động 100% offline.\n" +
                        "Clean browser traces, cookies, and sensitive files — 100% offline.";
                    StepHint.Text = "🔒 Dữ liệu của bạn không rời khỏi máy tính.";
                    break;
                case 2:
                    StepTitle.Text = "Bước 3: Kích hoạt & bảo vệ / Activate & Protect";
                    StepDescription.Text =
                        "Đăng ký tại cổng web để nhận key dùng thử 14 ngày, hoặc dùng miễn phí ngay.\n" +
                        "Register on the web portal for a 14-day trial key, or continue with Free tier.";
                    StepHint.Text = "🌐 Cài đặt → Nâng cấp / Upgrade để nhập key bất cứ lúc nào.";
                    break;
            }
        }

        private void Complete(bool runScan)
        {
            _settings.HasCompletedOnboarding = true;
            _settingsService.SaveSettings(_settings);
            RunSmartScanAfterClose = runScan;
            DialogResult = true;
            Close();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (_step < 2)
            {
                ShowStep(_step + 1);
                return;
            }

            Complete(runScan: true);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (_step > 0)
            {
                ShowStep(_step - 1);
            }
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            Complete(runScan: false);
        }
    }
}
