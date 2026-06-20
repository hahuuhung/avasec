using System.Windows;
using System.Windows.Input;
using AVASec.Core.Models;
using AVASec.Core.Services;

namespace AVASec.UI.Views
{
    /// <summary>
    /// About window showing version, license and contact info
    /// Cửa sổ Giới thiệu hiển thị thông tin phiên bản, giấy phép và liên hệ
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow(FeatureGateService? featureGate = null)
        {
            InitializeComponent();

            // Allow dragging / Cho phép kéo
            MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };

            // Set version info / Đặt thông tin phiên bản
            VersionText.Text = $"v{AppVersionInfo.Version}";
            BuildDateText.Text = AppVersionInfo.BuildDate;

            // Set license info / Đặt thông tin giấy phép
            if (featureGate != null)
            {
                LicenseTierText.Text = featureGate.TierDisplayName;

                if (featureGate.CurrentTier == FeatureGateService.LicenseTier.Lifetime)
                {
                    LicenseExpiryText.Text = "♾️ Vĩnh viễn / Lifetime";
                    LicenseRemainingText.Text = "♾️";
                }
                else if (featureGate.IsPro)
                {
                    LicenseRemainingText.Text = $"{featureGate.RemainingDays} ngày / days";
                }
                else
                {
                    LicenseTierText.Text = "Free";
                    LicenseExpiryText.Text = "N/A";
                    LicenseRemainingText.Text = "Nâng cấp để mở khoá / Upgrade to unlock";
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
