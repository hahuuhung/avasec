using System;
using System.Windows;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;
using AVASec.Core.Services;
using System.Windows.Controls;
using AVASec.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AVASec.UI.Views
{
    public partial class SettingsView : Window
    {
        private readonly ISettingsService? _settingsService;
        private readonly AVASec.Optimization.Services.ISystemTweaksService? _tweaksService;
        private AppSettings? _currentSettings;

        // Constructor for DI
        public SettingsView(ISettingsService settingsService, AVASec.Optimization.Services.ISystemTweaksService tweaksService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _tweaksService = tweaksService;
            LoadSettings();
            LoadAboutInfo();
        }

        // Fallback constructor for XAML designer
#pragma warning disable CS8618
        public SettingsView()
        {
            InitializeComponent();
        }
#pragma warning restore CS8618

        private void LoadSettings()
        {
            if (_settingsService == null) return;

            _currentSettings = _settingsService.LoadSettings();

            if (_currentSettings != null)
            {
                AutoStartCheck.IsChecked = _currentSettings.AutoStart;
                MinimizeToTrayCheck.IsChecked = _currentSettings.MinimizeToTray;
                RealTimeProtectionCheck.IsChecked = _currentSettings.RealTimeProtection;
                DarkModeCheck.IsChecked = _currentSettings.IsDarkMode;
            }
            
            // Load Language
            var currentLang = LanguageService.Instance.CurrentLanguage;
            if (LanguageComboBox != null && LanguageComboBox.Items != null)
            {
                foreach (ComboBoxItem item in LanguageComboBox.Items)
                {
                    if (item != null && item.Tag != null && item.Tag.ToString() == currentLang)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }

            bool winUpdateEnabled = _tweaksService?.IsWindowsUpdateEnabled() ?? true;
            UpdateWindowsUpdateToggleUI(winUpdateEnabled);

            if (FontScaleComboBox != null)
            {
                foreach (ComboBoxItem item in FontScaleComboBox.Items)
                {
                    if (item.Tag?.ToString() == _currentSettings.FontScale.ToString("0.0"))
                    {
                        FontScaleComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            if (HighContrastCheck != null)
            {
                HighContrastCheck.IsChecked = _currentSettings.HighContrast;
            }
        }

        /// <summary>
        /// Load About section info / Tải thông tin phần Giới thiệu
        /// </summary>
        private void LoadAboutInfo()
        {
            try
            {
                SettingsVersionText.Text = $"v{AppVersionInfo.Version}";
                
                var featureGate = App.ServiceProvider.GetService<FeatureGateService>();
                if (featureGate != null)
                {
                    SettingsLicenseText.Text = featureGate.TierDisplayName;
                }
            }
            catch { /* Ignore if services not available / Bỏ qua nếu dịch vụ không có */ }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                string? lang = item.Tag.ToString();
                if (lang != null)
                {
                    LanguageService.Instance.SetLanguage(lang);
                }
            }
        }

        private void UpdateWindowsUpdateToggleUI(bool enabled)
        {
            WindowsUpdateToggle.IsChecked = enabled;
            WindowsUpdateToggle.Content = enabled ? "ON" : "OFF";
            WindowsUpdateToggle.Background = enabled ? (System.Windows.Media.Brush)Application.Current.Resources["SuccessBrush"] : (System.Windows.Media.Brush)Application.Current.Resources["DangerBrush"];
        }

        private void WindowsUpdateToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isEnabled = WindowsUpdateToggle.IsChecked ?? false;
            
            if (_tweaksService != null)
            {
                bool success = _tweaksService.SetWindowsUpdateState(isEnabled);
                if (success)
                {
                    UpdateWindowsUpdateToggleUI(isEnabled);
                    MessageBox.Show(isEnabled ? "Windows Update đã được BẬT." : "Windows Update đã được TẮT.", "System Tools", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể thay đổi trạng thái Windows Update. Vui lòng chạy ứng dụng với quyền Admin.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Revert UI
                    UpdateWindowsUpdateToggleUI(!isEnabled);
                }
            }
        }

        private void FixPrinter_Click(object sender, RoutedEventArgs e)
        {
            if (_tweaksService != null)
            {
                bool success = _tweaksService.FixNetworkPrinterError();
                if (success)
                {
                    MessageBox.Show("Đã áp dụng sửa lỗi máy in thành công!\n(RpcAuthnLevelPrivacyEnabled = 0)", "Printer Fix", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể sửa lỗi registry. Vui lòng chạy ứng dụng với quyền Admin.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSettings == null) _currentSettings = new AppSettings();

            _currentSettings.AutoStart = AutoStartCheck.IsChecked ?? false;
            _currentSettings.MinimizeToTray = MinimizeToTrayCheck.IsChecked ?? false;
            _currentSettings.RealTimeProtection = RealTimeProtectionCheck.IsChecked ?? false;
            _currentSettings.WindowsUpdateEnabled = WindowsUpdateToggle.IsChecked ?? false;
            _currentSettings.IsDarkMode = DarkModeCheck.IsChecked ?? false;
            _currentSettings.FontScale = GetSelectedFontScale();
            _currentSettings.HighContrast = HighContrastCheck.IsChecked ?? false;
            
            if (LanguageComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                _currentSettings.Language = item.Tag.ToString() ?? "vi-EN";
            }

            if (_settingsService != null)
            {
                _settingsService.SaveSettings(_currentSettings);
                
                // --- STARTUP LOGIC ---
                try
                {
                    string appPath = Environment.ProcessPath ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
                    string appName = "AVASecurity";
                    
                    using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        if (key != null)
                        {
                            if (_currentSettings.AutoStart)
                            {
                                key.SetValue(appName, $"\"{appPath}\"");
                            }
                            else
                            {
                                if (key.GetValue(appName) != null)
                                    key.DeleteValue(appName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error managing startup registry: {ex.Message}");
                }
                // --- END STARTUP LOGIC ---
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Warning: _settingsService is null in Save_Click");
            }
            
            MessageBox.Show("Đã lưu cài đặt thành công!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            AccessibilityService.Instance.Apply(_currentSettings);
            this.Close();
        }

        private double GetSelectedFontScale()
        {
            if (FontScaleComboBox?.SelectedItem is ComboBoxItem item &&
                double.TryParse(item.Tag?.ToString(), out var scale))
            {
                return scale;
            }

            return 1.0;
        }

        private void FontScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || _currentSettings == null)
            {
                return;
            }

            _currentSettings.FontScale = GetSelectedFontScale();
            AccessibilityService.Instance.Apply(_currentSettings);
        }

        private void HighContrastCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded || _currentSettings == null || HighContrastCheck == null)
            {
                return;
            }

            _currentSettings.HighContrast = HighContrastCheck.IsChecked ?? false;
            AccessibilityService.Instance.Apply(_currentSettings);
        }

        private void ReplayOnboarding_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsService == null)
            {
                return;
            }

            var settings = _settingsService.LoadSettings();
            settings.HasCompletedOnboarding = false;
            _settingsService.SaveSettings(settings);

            var onboarding = new OnboardingWindow(_settingsService);
            onboarding.Owner = this;
            onboarding.ShowDialog();
        }

        private void DarkModeCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DarkModeCheck != null && IsLoaded)
            {
                ThemeService.Instance.SetTheme(DarkModeCheck.IsChecked ?? false);
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // Signal to open Login
            this.Tag = "LOGIN_REQUEST"; 
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Open About window / Mở cửa sổ Giới thiệu
        /// </summary>
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var featureGate = App.ServiceProvider.GetService<FeatureGateService>();
                var aboutWindow = new AboutWindow(featureGate);
                aboutWindow.Owner = this;
                aboutWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi / Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Open Upgrade window / Mở cửa sổ Nâng cấp
        /// </summary>
        private void UpgradeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var featureGate = App.ServiceProvider.GetService<FeatureGateService>() ?? new FeatureGateService();
                var upgradeWindow = new UpgradeWindow(featureGate);
                upgradeWindow.Owner = this;
                if (upgradeWindow.ShowDialog() == true)
                {
                    // Refresh license display / Cập nhật hiển thị giấy phép
                    LoadAboutInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi / Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            Login_Click(sender, e);
        }

        private void ChatToggle_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Support Chat is available on the main Dashboard.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Toolbox_Click(object sender, RoutedEventArgs e)
        {
            try { new ToolboxWindow { Owner = this }.ShowDialog(); }
            catch {}
        }
    }
}
