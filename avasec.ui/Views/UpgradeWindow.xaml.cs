using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using AVASec.Core.Models;
using AVASec.Core.Services;
using AVASec.Database;
using AVASec.UI.Services;

namespace AVASec.UI.Views
{
    /// <summary>
    /// Upgrade window for license activation and plan comparison
    /// Cửa sổ nâng cấp để kích hoạt giấy phép và so sánh các gói
    /// </summary>
    public partial class UpgradeWindow : Window
    {
        private readonly FeatureGateService _featureGate;
        private readonly int _userId;

        public UpgradeWindow(FeatureGateService featureGate, int userId = 0)
        {
            InitializeComponent();
            _featureGate = featureGate;
            _userId = userId;

            // Allow dragging the window / Cho phép kéo cửa sổ
            MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };

            LoadCurrentStatus();
        }

        /// <summary>
        /// Load and display current license status / Tải và hiển thị trạng thái giấy phép hiện tại
        /// </summary>
        private void LoadCurrentStatus()
        {
            CurrentTierText.Text = $"Gói hiện tại / Current Plan: {_featureGate.TierDisplayName}";

            if (_featureGate.IsPro)
            {
                if (_featureGate.CurrentTier == FeatureGateService.LicenseTier.Lifetime)
                {
                    RemainingDaysText.Text = "♾️ Vĩnh viễn / Lifetime";
                }
                else
                {
                    RemainingDaysText.Text = $"📅 Còn {_featureGate.RemainingDays} ngày / {_featureGate.RemainingDays} days remaining";
                }
            }
            else
            {
                RemainingDaysText.Text = "⚠️ Bạn đang dùng gói miễn phí / You are using the Free plan";
            }
        }

        /// <summary>
        /// Activate a license key / Kích hoạt mã giấy phép
        /// </summary>
        private async void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            string key = LicenseKeyInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(key))
            {
                MessageBox.Show(
                    "Vui lòng nhập mã kích hoạt.\nPlease enter a license key.",
                    "Thiếu mã / Missing Key",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // AVA-, SA-, TRIAL- keys from website pool
            if (!IsValidKeyFormat(key))
            {
                MessageBox.Show(
                    "Mã không hợp lệ. AVA-XXXX-XXXX-XXXX-XXXX\nInvalid key format.",
                    "Lỗi / Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var webLicense = App.ServiceProvider.GetRequiredService<WebLicenseService>();
                var webResult = await webLicense.ActivateKeyAsync(key, _userId);

                if (webResult.Success && webResult.License != null)
                {
                    _featureGate.SetLicense(
                        webResult.License.LicenseType,
                        webResult.License.IsActive && webResult.License.ExpiryDate > DateTime.Now,
                        Math.Max(0, (webResult.License.ExpiryDate - DateTime.Now).Days));
                    LoadCurrentStatus();
                    MessageBox.Show(
                        $"🎉 Kích hoạt thành công!\nActivation successful!\n\n" +
                        $"Gói / Plan: {webResult.License.LicenseType}\n" +
                        $"Hết hạn / Expires: {webResult.License.ExpiryDate:yyyy-MM-dd}",
                        "Thành công / Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                    return;
                }

                // Fallback: local SQLite (dev / legacy keys)
                using var context = new AVASecContext();
                var license = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .FirstOrDefaultAsync(context.Licenses, l => l.LicenseKey == key);

                if (license != null && license.IsValid())
                {
                    LicensePortalSync.ApplyToFeatureGate(_featureGate, license);
                    LoadCurrentStatus();
                    MessageBox.Show(
                        $"🎉 Kích hoạt local thành công!\nLocal activation OK!\n\nGói: {license.LicenseType}",
                        "Thành công / Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                    return;
                }

                MessageBox.Show(
                    webResult.Message + "\n\nMua key tại website / Buy key on website:\n" + PortalConfig.PortalUrl,
                    "Không hợp lệ / Invalid",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi kích hoạt: {ex.Message}\nActivation error: {ex.Message}",
                    "Lỗi / Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Open buy online page / Mở trang mua hàng online
        /// </summary>
        private static bool IsValidKeyFormat(string key)
        {
            var prefixes = new[] { "AVA-", "SA-", "TRIAL-" };
            return prefixes.Any(key.StartsWith) && key.Length >= 15;
        }

        private void BuyOnlineButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = PortalConfig.PortalUrl,
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show(
                    $"Vui lòng truy cập:\n{PortalConfig.PortalUrl}",
                    "Mua hàng / Purchase",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Close button click / Nhấn nút đóng
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
