using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using AVASec.Antivirus.Services;
using AVASec.Authentication.Services;
using AVASec.Database;
using AVASec.Optimization.Services;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;
using AVASec.Core.Services;
using AVASec.UI.Services;
using System.Drawing;
using Forms = System.Windows.Forms;


namespace AVASec.UI.Views
{
    public class NotificationItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public bool IsPromotional { get; set; }
    }

    public partial class DashboardView : Window
    {
        private int _userId;
        private readonly Controls.ChatWidget? _chatWidget;
        
        // Services from MainWindow
        private readonly AVASecContext _context;
        private readonly LicenseService _licenseService;
        private readonly DiskCleanerService _diskCleaner;
        private readonly RamOptimizerService _ramOptimizer;
        private readonly StartupManagerService _startupManager;
        private readonly FileScannerService _fileScanner;
        private readonly QuarantineService _quarantineService;
        private readonly VirusDatabaseUpdateService _virusDbUpdate;
        private readonly SystemMonitorService _systemMonitor;
        private readonly DispatcherTimer _monitorTimer;
        private readonly FeatureGateService _featureGate;
        private Forms.NotifyIcon? _trayIcon;

        // Notification Services
        private readonly AVASec.UI.Services.NotificationService _notificationService;
        private DispatcherTimer _notificationTimer;
        private System.Collections.ObjectModel.ObservableCollection<NotificationItem> _notifications;

        // Parameterless constructor for XAML / Constructor không tham số cho XAML
        public DashboardView() : this(null!, 0)
        {
        }

        public DashboardView(Controls.ChatWidget? chatWidget, int userId = 1)
        {
            InitializeComponent();
            
            _chatWidget = chatWidget;
            _userId = userId;

            // Initialize Services (from MainWindow)
            _context = new AVASecContext();
            _licenseService = new LicenseService(_context);
            _diskCleaner = new DiskCleanerService();
            _ramOptimizer = new RamOptimizerService();
            _startupManager = new StartupManagerService();
            _fileScanner = new FileScannerService();
            _quarantineService = new QuarantineService(_context);
            _virusDbUpdate = new VirusDatabaseUpdateService();
            // System Monitor Service
            _systemMonitor = new SystemMonitorService();
            
            // Feature Gate Service / Dịch vụ Kiểm soát Tính năng
            _featureGate = App.ServiceProvider.GetRequiredService<FeatureGateService>();
            
            // Initialize Notification Service
            _notificationService = new AVASec.UI.Services.NotificationService();
            _notifications = new System.Collections.ObjectModel.ObservableCollection<NotificationItem>();
            NotificationList.ItemsSource = _notifications;
             
            // Initialize System Monitor Timer
            _monitorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _monitorTimer.Tick += MonitorTimer_Tick;
            _monitorTimer.Start();

            // Initialize Notification Timer
            _notificationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30) // Check every 30s
            };
            _notificationTimer.Tick += async (s, e) => await CheckNotifications();
            _notificationTimer.Start();
            
            // Initial Load
            Loaded += DashboardView_Loaded;
            SizeChanged += DashboardView_SizeChanged;
            
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            _trayIcon = new Forms.NotifyIcon();
            _trayIcon.Text = BrandConstants.ProductNameFull;
            
            try 
            {
                // Try to load icon from resources
                var iconUri = new Uri("pack://application:,,,/Resources/app_icon.png");
                var streamInfo = Application.GetResourceStream(iconUri);
                if (streamInfo != null)
                {
                    using (var stream = streamInfo.Stream)
                    {
                        var bitmap = new Bitmap(stream);
                        _trayIcon.Icon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
                    }
                }
            }
            catch 
            {
                // Fallback to system icon if fail
                _trayIcon.Icon = SystemIcons.Shield;
            }

            _trayIcon.Visible = false;
            _trayIcon.Click += (s, e) => RestoreFromTray();
            
            // Context Menu
            var contextMenu = new Forms.ContextMenuStrip();
            contextMenu.Items.Add("Open AVA Security / Mở AVA Security", null, (s, e) => RestoreFromTray());
            contextMenu.Items.Add(new Forms.ToolStripSeparator());
            contextMenu.Items.Add("Exit / Thoát", null, (s, e) => {
                _trayIcon.Visible = false;
                Application.Current.Shutdown();
            });
            _trayIcon.ContextMenuStrip = contextMenu;
        }

        private void RestoreFromTray()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            if (_trayIcon != null) _trayIcon.Visible = false;
        }

        private void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            if (BrandTitleText != null)
            {
                BrandTitleText.Text = BrandConstants.DisplayName;
            }

            if (OfflineModeText != null)
            {
                OfflineModeText.Text = ProductMode.SidebarHintBilingual;
            }

            ApplyAdaptiveLayout(ActualWidth);
            LoadDashboard();
            CheckVirusDatabaseUpdates();
            Task.Run(async () => await CheckNotifications());

            if (LoginScreen != null)
            {
                LoginScreen.LoginSuccessful += OnLoginSuccessful;
            }
        }

        private void DashboardView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyAdaptiveLayout(e.NewSize.Width);
        }

        private void MainLayoutGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyAdaptiveLayout(e.NewSize.Width);
        }

        private void ApplyAdaptiveLayout(double width)
        {
            if (SidebarColumn == null)
            {
                return;
            }

            if (width < 768)
            {
                SidebarColumn.Width = new GridLength(72);
            }
            else if (width < 1200)
            {
                SidebarColumn.Width = new GridLength(200);
            }
            else
            {
                SidebarColumn.Width = new GridLength(240);
            }
        }

        private void OnLoginSuccessful(object? sender, int userId)
        {
            _userId = userId;
            LoginScreen.Visibility = Visibility.Collapsed;
            _ = ApplyLicenseAfterLoginAsync(userId);
            Task.Run(async () => await CheckNotifications());

            System.Windows.MessageBox.Show(TryFindResource("Lang.Msg.LoginSuccess")?.ToString() ?? "Đăng nhập thành công! / Login Successful!", 
                            TryFindResource("Lang.Msg.Welcome")?.ToString() ?? "Chào mừng / Welcome", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task ApplyLicenseAfterLoginAsync(int userId)
        {
            if (userId <= 0)
            {
                LoadDashboard();
                return;
            }

            try
            {
                var authService = App.ServiceProvider.GetRequiredService<IAuthenticationService>();
                var user = await authService.GetUserAsync(userId);
                if (user?.License != null)
                {
                    LicensePortalSync.ApplyToFeatureGate(_featureGate, user.License);
                }
            }
            catch { /* best effort */ }

            LoadDashboard();
        }

        #region System Monitoring / Giám sát Hệ thống


        private void MonitorTimer_Tick(object? sender, EventArgs e)
        {
            _systemMonitor.Refresh();
            
            // Update CPU Metric Card / Cập nhật Thẻ CPU
            if (CpuText != null && CpuProgressBorder != null && CpuStatusText != null)
            {
                var cpuPercent = _systemMonitor.CpuUsagePercent;
                CpuText.Text = $"{cpuPercent:F0}%";
                CpuProgressBorder.Width = (cpuPercent / 100.0) * 188; // Max width ~188px for metric card
                
                // Color-coded status / Trạng thái theo màu
                if (cpuPercent < 50)
                {
                    CpuStatusText.Text = "● Normal";
                    CpuStatusText.Foreground = (System.Windows.Media.Brush)FindResource("GreenBrush");
                }
                else if (cpuPercent < 80)
                {
                    CpuStatusText.Text = "● Moderate";
                    CpuStatusText.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
                }
                else
                {
                    CpuStatusText.Text = "● High Load";
                    CpuStatusText.Foreground = (System.Windows.Media.Brush)FindResource("ErrorBrush");
                }
            }

            // Update OneClick CPU Metric Card
            if (OneClickCpuText != null && OneClickCpuProgressBar != null)
            {
               var cpuPercent = _systemMonitor.CpuUsagePercent;
               OneClickCpuText.Text = $"{cpuPercent:F0}%";
               OneClickCpuProgressBar.Value = cpuPercent;
            }

            // Update RAM Metric Card / Cập nhật Thẻ RAM
            if (RamText != null && RamProgressBorder != null && RamStatusText != null && RamDetailText != null)
            {
                var ramPercent = _systemMonitor.RamUsagePercent;
                var usedGB = _systemMonitor.UsedRamMB / 1024.0;
                var totalGB = _systemMonitor.TotalRamMB / 1024.0;
                
                RamText.Text = $"{ramPercent:F0}%";
                RamDetailText.Text = $"Used {usedGB:F1}GB of {totalGB:F1}GB";
                RamProgressBorder.Width = (ramPercent / 100.0) * 188;
                
                // Color-coded status / Trạng thái theo màu
                if (ramPercent < 60)
                {
                    RamStatusText.Text = "● Normal";
                    RamStatusText.Foreground = (System.Windows.Media.Brush)FindResource("GreenBrush");
                    RamProgressBorder.Background = (System.Windows.Media.Brush)FindResource("AccentBlueBrush");
                }
                else if (ramPercent < 85)
                {
                    RamStatusText.Text = "● Moderate";
                    RamStatusText.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
                    RamProgressBorder.Background = (System.Windows.Media.Brush)FindResource("WarningBrush");
                }
                else
                {
                    RamStatusText.Foreground = (System.Windows.Media.Brush)FindResource("ErrorBrush");
                    RamProgressBorder.Background = (System.Windows.Media.Brush)FindResource("ErrorBrush");
                }
            }

            // Update OneClick RAM Metric Card
            if (OneClickRamText != null && OneClickRamProgressBar != null)
            {
                var ramPercent = _systemMonitor.RamUsagePercent;
                OneClickRamText.Text = $"{ramPercent:F0}%";
                OneClickRamProgressBar.Value = ramPercent;
            }

            // Update Disk Space Metric Card / Cập nhật Thẻ Đĩa
            if (DiskText != null && DiskProgressBorder != null && DiskStatusText != null && DiskDetailText != null)
            {
                var diskInfo = GetSystemDriveInfo();
                DiskText.Text = $"{diskInfo.UsedPercent:F0}%";
                DiskDetailText.Text = $"Free {diskInfo.FreeGB:F0}GB of {diskInfo.TotalGB:F0}GB";
                DiskProgressBorder.Width = (diskInfo.UsedPercent / 100.0) * 188;
                
                // Color-coded status / Trạng thái theo màu
                if (diskInfo.UsedPercent < 70)
                {
                    DiskStatusText.Text = "● Normal";
                    DiskStatusText.Foreground = (System.Windows.Media.Brush)FindResource("GreenBrush");
                    DiskProgressBorder.Background = (System.Windows.Media.Brush)FindResource("AccentBlueBrush");
                }
                else if (diskInfo.UsedPercent < 90)
                {
                    DiskStatusText.Text = "● High Usage";
                    DiskStatusText.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
                    DiskProgressBorder.Background = (System.Windows.Media.Brush)FindResource("OrangeButtonBrush");
                }
                else
                {
                    DiskStatusText.Text = "● Critical";
                    DiskStatusText.Foreground = (System.Windows.Media.Brush)FindResource("ErrorBrush");
                    DiskProgressBorder.Background = (System.Windows.Media.Brush)FindResource("ErrorBrush");
                }
            }

            // Update Threats Metric Card / Cập nhật Thẻ Mối đe dọa
            if (ThreatsText != null && ThreatsProgressBorder != null && ThreatsStatusText != null)
            {
                var threatsCount = GetActiveThreatsCount();
                ThreatsText.Text = threatsCount.ToString();
                
                if (threatsCount == 0)
                {
                    ThreatsText.Foreground = (System.Windows.Media.Brush)FindResource("GreenBrush");
                    ThreatsStatusText.Text = "● System Secure";
                    ThreatsStatusText.Foreground = (System.Windows.Media.Brush)FindResource("GreenBrush");
                    ThreatsProgressBorder.Width = 0;
                }
                else if (threatsCount < 5)
                {
                    ThreatsText.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
                    ThreatsStatusText.Text = "● Minor Threats";
                    ThreatsStatusText.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
                    ThreatsProgressBorder.Width = 94; // 50%
                }
                else
                {
                    ThreatsText.Foreground = (System.Windows.Media.Brush)FindResource("ErrorBrush");
                    ThreatsStatusText.Text = "● Critical Alert";
                    ThreatsStatusText.Foreground = (System.Windows.Media.Brush)FindResource("ErrorBrush");
                    ThreatsProgressBorder.Width = 188; // 100%
                }
            }

            // Legacy CPU/RAM progress bars (if they exist from old UI)
            // Cập nhật thanh tiến trình CPU/RAM cũ (nếu còn tồn tại)
            if (CpuProgressBar != null && CpuText != null)
            {
                CpuProgressBar.Value = _systemMonitor.CpuUsagePercent;
            }

            if (RamProgressBar != null && RamText != null)
            {
                RamProgressBar.Value = _systemMonitor.RamUsagePercent;
            }

            // Check license periodically (every 30s)
            if (DateTime.Now.Second % 30 == 0)
            {
                CheckForLicenseUpgrade();
            }
        }

        /// <summary>
        /// Get system drive (C:) information / Lấy thông tin ổ đĩa hệ thống
        /// </summary>
        private (double TotalGB, double FreeGB, double UsedPercent) GetSystemDriveInfo()
        {
            try
            {
                var drive = new System.IO.DriveInfo("C");
                if (drive.IsReady)
                {
                    double totalGB = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                    double freeGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                    double usedGB = totalGB - freeGB;
                    double usedPercent = (usedGB / totalGB) * 100.0;
                    
                    return (totalGB, freeGB, usedPercent);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardView] Error getting disk info: {ex.Message}");
            }
            
            // Fallback values / Giá trị mặc định
            return (1000, 250, 75);
        }

        /// <summary>
        /// Get active threats count from quarantine / Lấy số mối đe dọa từ khu vực cách ly
        /// </summary>
        private int GetActiveThreatsCount()
        {
            try
            {
                // Count threats in quarantine that are not restored
                // Đếm mối đe dọa trong khu vực cách ly chưa được xử lý
                return _quarantineService.GetQuarantinedFilesAsync().Result.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardView] Error getting threats count: {ex.Message}");
                return 0;
            }
        }

        private async void LoadDashboard()
        {
            try
            {
                // Retrieve Authentication Service from DI (Hybrid)
                var authService = App.ServiceProvider.GetRequiredService<IAuthenticationService>();
                var user = await authService.GetUserAsync(_userId);

                if (user != null && user.License != null)
                {
                    // Update UI with License Info
                    if (LicenseStatusText != null) 
                    {
                        LicenseStatusText.Text = user.License.LicenseType;
                        
                        // Hide Upgrade Button if Pro/Premium
                        if (UpgradeButton != null)
                        {
                            bool isPremium = user.License.LicenseType.Equals("Pro", StringComparison.OrdinalIgnoreCase) || 
                                           user.License.LicenseType.Equals("Premium", StringComparison.OrdinalIgnoreCase) ||
                                           user.License.LicenseType.Equals("Ultra", StringComparison.OrdinalIgnoreCase) ||
                                           user.License.LicenseType.Equals("Lifetime", StringComparison.OrdinalIgnoreCase) ||
                                           user.License.LicenseType.Equals("Ultimate", StringComparison.OrdinalIgnoreCase);
                            UpgradeButton.Visibility = isPremium ? Visibility.Collapsed : Visibility.Visible;
                            
                            // Hide Header Go Premium button too if premium
                            if (HeaderGoPremiumBtn != null) 
                                HeaderGoPremiumBtn.Visibility = isPremium ? Visibility.Collapsed : Visibility.Visible;
                        }
                    }
                    
                    if (LicenseExpiryText != null)
                    {
                        var daysLeft = (user.License.ExpiryDate - DateTime.Now).Days;
                        LicenseExpiryText.Text = daysLeft > 0 ? $"{daysLeft} days left" : "Expired";
                        
                        // Localization for expiry
                        if (daysLeft > 0)
                            LicenseExpiryText.Text = $"{daysLeft} days left / {daysLeft} ngày còn lại";
                        else
                            LicenseExpiryText.Text = "Expired / Hết hạn";
                    }
                }
                else
                {
                    // Fallback if no license found (e.g. Guest or Error)
                    if (LicenseStatusText != null) LicenseStatusText.Text = "Guest / Khách";
                    if (LicenseExpiryText != null) LicenseExpiryText.Text = "";
                    
                    // Show Upgrade Button for Guest
                    if (UpgradeButton != null)
                    {
                        UpgradeButton.Visibility = Visibility.Visible;
                        UpgradeButton.Content = "Upgrade VIP / Nâng cấp VIP";
                    }
                     if (HeaderGoPremiumBtn != null) HeaderGoPremiumBtn.Visibility = Visibility.Visible;
                }

                // Load virus DB info
                var dbInfo = await _virusDbUpdate.GetDatabaseInfoAsync();

                // Trigger Sample Achievement (Gamification Demo)
                // Delay slightly to let window load fully
                await Task.Delay(2000);
                AVASec.UI.Services.GamificationService.Instance.ShowAchievement(
                    "Welcome Back! / Chào mừng trở lại!", 
                    "You logged in successfully. / Bạn đã đăng nhập thành công.", 
                    50);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }

        private async void CheckForLicenseUpgrade()
        {
            var authService = new AuthenticationService(_context);
            var user = await authService.GetUserAsync(_userId);
            
            if (user != null && user.License != null)
            {
                // Logic to update UI if license changed
            }
        }

        private async void CheckVirusDatabaseUpdates()
        {
            var (available, newVersion) = await _virusDbUpdate.CheckForUpdatesAsync();
            if (available)
            {
                // Optional: Show notification toast instead of MessageBox to be less intrusive
                // For now, keep logic simple
            }
        }

        #endregion

        #region Notification Logic / Xử lý Thông báo

        private async Task CheckNotifications()
        {
            try 
            {
                // Skip if no user logged in or user is guest (0)
                if (_userId <= 0) return;

                var serverNotifications = await _notificationService.CheckNotificationsAsync(_userId);
                if (serverNotifications == null) return;

                int unreadCount = 0;
                
                // Clear and reload list (simple approach) or merge
                // Here we just reload for simplicity
                Application.Current.Dispatcher.Invoke(() => 
                {
                    _notifications.Clear();
                    foreach (var n in serverNotifications)
                    {
                        if (!n.IsRead) unreadCount++;
                        
                        string icon = "ℹ️";
                        if (n.Type == "success" || n.IsPromotional) icon = "🎉";
                        else if (n.Type == "warning") icon = "⚠️";
                        else if (n.Type == "error") icon = "❌";

                        _notifications.Add(new NotificationItem 
                        {
                            Id = n.NotificationID,
                            Title = n.Title,
                            Message = n.Message,
                            Icon = icon,
                            ActionUrl = n.ActionUrl,
                            IsPromotional = n.IsPromotional
                        });

                        // Show Popup Window for High Priority or Promo
                        if (!n.IsRead && (n.IsPromotional || n.Type == "error"))
                        {
                             // Only show if not already shown this session (tracked by service typically, but here simplified)
                             // Assuming new fetch means new notification
                             var window = new NotificationWindow(n);
                             window.Show();
                             
                             // Mark as read immediately when popping up window? or wait?
                             // Let's mark read so it doesn't pop up again next poll
                             _notificationService.MarkAsReadAsync(n.NotificationID);
                        }
                    }

                    // Update Badge
                     if (unreadCount > 0)
                    {
                        NotificationBadge.Visibility = Visibility.Visible;
                        NotificationCount.Text = unreadCount > 9 ? "9+" : unreadCount.ToString();
                    }
                    else
                    {
                        NotificationBadge.Visibility = Visibility.Collapsed;
                    }

                    // Show empty state if needed
                    NoNotificationsText.Visibility = _notifications.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking notifications: {ex.Message}");
            }
        }

        private void Bell_Click(object sender, MouseButtonEventArgs e)
        {
            NotificationPopup.IsOpen = !NotificationPopup.IsOpen;
        }

        private void MarkAllRead_Click(object sender, RoutedEventArgs e)
        {
             // Clear badge locally
             NotificationBadge.Visibility = Visibility.Collapsed;
             
             // In real app, call API to mark all as read
             // For now just hide badge until next poll
        }

        private void NotificationItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is NotificationItem item)
            {
                if (!string.IsNullOrEmpty(item.ActionUrl))
                {
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = item.ActionUrl, UseShellExecute = true }); } catch { }
                }
                
                // Mark as read (optimistic)
                _notifications.Remove(item);
                
                // If list empty
                if (_notifications.Count == 0) NoNotificationsText.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Window Controls / Điều khiển Cửa sổ
        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TrayButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            if (_trayIcon != null)
            {
                _trayIcon.Visible = true;
                _trayIcon.ShowBalloonTip(3000, "AVA Security Hidden", "Application is running in background. Double-click icon to restore.", Forms.ToolTipIcon.Info);
            }
        }



        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Fully exit the application
            var result = System.Windows.MessageBox.Show("Are you sure you want to exit AVA Security?\nBạn có chắc chắn muốn thoát AVA Security?", 
                "Exit AVA Security", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                if (_trayIcon != null) _trayIcon.Visible = false;
                Application.Current.Shutdown();
            }
        }

        #endregion

        #region Navigation / Điều hướng

        private void NavItem_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Tag is string tag)
            {
                switch (tag)
                {
                    case "Overview":
                        CategoryTitle.Text = FindResource("Lang.Nav.Overview")?.ToString() ?? "Overview";
                        ShowPanel("Overview");
                        break;
                    case "OneClick":
                        CategoryTitle.Text = FindResource("Lang.Nav.OneClick")?.ToString() ?? "1-Click Maintenance";
                        ShowPanel("OneClick"); 
                        break;
                    case "CleanUp":
                        CategoryTitle.Text = FindResource("Lang.Nav.CleanRepair")?.ToString() ?? "Clean Up & Repair";
                        ShowPanel("CleanUp");
                        break;
                    case "Optimize":
                        CategoryTitle.Text = FindResource("Lang.Nav.Optimization")?.ToString() ?? "Optimize & Improve";
                        ShowPanel("Optimize");
                        break;
                    case "GameMode":
                        CategoryTitle.Text = FindResource("Lang.Nav.GameMode")?.ToString() ?? "Game Booster / Tăng tốc Game";
                        ShowPanel("GameMode");
                        break;
                    case "Benchmark":
                        CategoryTitle.Text = FindResource("Lang.Nav.Benchmark")?.ToString() ?? "Benchmark / Đánh giá";
                        ShowPanel("Benchmark");
                        break;
                    case "Privacy":
                        CategoryTitle.Text = FindResource("Lang.Nav.Protection")?.ToString() ?? "Privacy & Security";
                        ShowPanel("Privacy");
                        break;
                    case "Files":
                        CategoryTitle.Text = FindResource("Lang.Nav.Files")?.ToString() ?? "Files & Folders";
                        ShowPanel("Overview");
                        break;
                    case "System":
                        CategoryTitle.Text = FindResource("Lang.Nav.System")?.ToString() ?? "System Tools";
                        ShowPanel("System");
                        break;
                }
            }
        }

        private void ShowPanel(string panelName)
        {
            if (OverviewPanel == null) return;

            OverviewPanel.Visibility = Visibility.Collapsed;
            CleanUpPanel.Visibility = Visibility.Collapsed;
            OptimizePanel.Visibility = Visibility.Collapsed;
            SystemPanel.Visibility = Visibility.Collapsed;
            if (GameModePanel != null) GameModePanel.Visibility = Visibility.Collapsed;
            if (PrivacyPanel != null) PrivacyPanel.Visibility = Visibility.Collapsed;
            if (BenchmarkPanel != null) BenchmarkPanel.Visibility = Visibility.Collapsed;

            switch (panelName)
            {
                case "Overview":
                    OverviewPanel.Visibility = Visibility.Visible;
                    if (OverviewStatsPanel != null) OverviewStatsPanel.Visibility = Visibility.Visible;
                    if (QuickActionsPanel != null) QuickActionsPanel.Visibility = Visibility.Visible;
                    if (OneClickStatusPanel != null) OneClickStatusPanel.Visibility = Visibility.Collapsed;
                    
                    // Show all cards in Overview
                    if (DiskCleanupCard != null) DiskCleanupCard.Visibility = Visibility.Visible;
                    if (StartupManagerCard != null) StartupManagerCard.Visibility = Visibility.Visible;
                    if (RegistryTweaksCard != null) RegistryTweaksCard.Visibility = Visibility.Visible;
                    if (WindowsTweaksCard != null) WindowsTweaksCard.Visibility = Visibility.Visible;
                    break;
                case "OneClick":
                    OverviewPanel.Visibility = Visibility.Visible;
                    if (OverviewStatsPanel != null) OverviewStatsPanel.Visibility = Visibility.Collapsed;
                    if (QuickActionsPanel != null) QuickActionsPanel.Visibility = Visibility.Visible;
                    if (OneClickStatusPanel != null) OneClickStatusPanel.Visibility = Visibility.Visible;
                    
                    // Show only Registry and Windows Tweaks in OneClick
                    if (DiskCleanupCard != null) DiskCleanupCard.Visibility = Visibility.Collapsed;
                    if (StartupManagerCard != null) StartupManagerCard.Visibility = Visibility.Collapsed;
                    if (RegistryTweaksCard != null) RegistryTweaksCard.Visibility = Visibility.Visible;
                    if (WindowsTweaksCard != null) WindowsTweaksCard.Visibility = Visibility.Visible;
                    
                    if (AdvancedToolsExpander != null) AdvancedToolsExpander.IsExpanded = true;
                    break;
                case "CleanUp":
                    CleanUpPanel.Visibility = Visibility.Visible;
                    break;
                case "Optimize":
                    OptimizePanel.Visibility = Visibility.Visible;
                    break;
                case "System":
                    SystemPanel.Visibility = Visibility.Visible;
                    break;
                case "GameMode":
                    if (GameModePanel != null) GameModePanel.Visibility = Visibility.Visible;
                    break;
                case "Privacy":
                    if (PrivacyPanel != null) PrivacyPanel.Visibility = Visibility.Visible;
                    break;
                case "Benchmark":
                    if (BenchmarkPanel != null) BenchmarkPanel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private async void SmartScan_Click(object sender, RoutedEventArgs e)
        {
            await RunSmartScanAsync(requireProLicense: true);
        }

        /// <summary>Called after onboarding wizard — allows first scan without Pro gate.</summary>
        public void TriggerSmartScanFromOnboarding()
        {
            _ = RunSmartScanAsync(requireProLicense: false);
        }

        private async Task RunSmartScanAsync(bool requireProLicense)
        {
            if (requireProLicense && !_featureGate.CanAccess(FeatureGateService.Features.VirusScanner))
            {
                var upgrade = App.ServiceProvider.GetRequiredService<UpgradeWindow>();
                upgrade.Owner = this;
                upgrade.ShowDialog();
                return;
            }

            var progress = new Progress<SmartScanProgress>(p =>
            {
                CategoryTitle.Text = p.Step;
            });

            try
            {
                var scanner = App.ServiceProvider.GetRequiredService<FileScannerService>();
                var smartScan = new SmartScanService(scanner, _diskCleaner, _ramOptimizer);
                var result = await smartScan.RunAsync(new SmartScanOptions
                {
                    RunThreatScan = true,
                    RunAiPass = true,
                    RunCleanup = true,
                    RunRamTrim = true,
                    RunBenchmark = false,
                    ScanPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                }, progress);

                var freedMb = result.BytesFreed / (1024 * 1024);
                var message =
                    $"Smart Scan complete / Quét thông minh hoàn tất\n\n" +
                    $"Files: {result.FilesScanned} | Threats: {result.ThreatsFound + result.AiSuspiciousCount}\n" +
                    $"Freed: {freedMb} MB | RAM: {result.MemoryFreedMb} MB\n" +
                    $"Duration: {result.Duration.TotalSeconds:F0}s";

                System.Windows.MessageBox.Show(message, "Vigil Smart Scan", MessageBoxButton.OK,
                    result.ThreatsFound > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
                LoadDashboard();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Smart Scan Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToPanel(string panelTag)
        {
            ShowPanel(panelTag);
            switch (panelTag)
            {
                case "Privacy":
                    if (NavPrivacy != null) NavPrivacy.IsChecked = true;
                    CategoryTitle.Text = FindResource("Lang.Nav.Protection")?.ToString() ?? "Privacy & Security";
                    break;
                case "Benchmark":
                    if (NavBenchmark != null) NavBenchmark.IsChecked = true;
                    CategoryTitle.Text = FindResource("Lang.Nav.Benchmark")?.ToString() ?? "Benchmark";
                    break;
            }
        }

        #endregion

        #region Tool Click Handlers / Xử lý Click Công cụ

        private void DiskCleanup_Click(object sender, RoutedEventArgs e) => OpenDiskCleanupWindow();
        private void StartupManager_Click(object sender, RoutedEventArgs e) => OpenStartupManagerWindow();
        private void RegistryTweaks_Click(object sender, RoutedEventArgs e) => OpenRegistryTweaksWindow();
        private void Windows11Tweaks_Click(object sender, RoutedEventArgs e) => OpenWindowsTweaksWindow();


        private void VirusScanner_Click(object sender, MouseButtonEventArgs e)
        {
            try 
            {
               var window = new VirusScannerWindow();
               window.Owner = this;
               window.ShowDialog();
            }
            catch (Exception ex)
            {
               System.Windows.MessageBox.Show("Could not open Virus Scanner: " + ex.Message);
            }
        }

        private async void RamBooster_Click(object sender, MouseButtonEventArgs e)
        {
            // Migrated logic from OptimizeRAM_Click
            try 
            {
                var result = await _ramOptimizer.OptimizeMemoryAsync();
                System.Windows.MessageBox.Show($"Freed {result.MemoryFreedMB} MB, optimized {result.ProcessesOptimized} processes.\nGiải phóng {result.MemoryFreedMB} MB, tối ưu {result.ProcessesOptimized} tiến trình.",
                    "RAM Optimization / Tối ưu RAM", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDashboard();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error optimizing RAM: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Toolbar Click Handlers / Xử lý Click Thanh công cụ

        private void ToolbarDiskCleanup_Click(object sender, RoutedEventArgs e) => OpenDiskCleanupWindow();
        private void ToolbarRegistry_Click(object sender, RoutedEventArgs e) => OpenRegistryTweaksWindow();
        private void ToolbarStartup_Click(object sender, RoutedEventArgs e) => OpenStartupManagerWindow();
        private void ToolbarSystem_Click(object sender, RoutedEventArgs e) => OpenWindowsTweaksWindow();
        
        private void ToolbarPrivacy_Click(object sender, RoutedEventArgs e) => NavigateToPanel("Privacy");
        private void ToolbarFiles_Click(object sender, RoutedEventArgs e) => OpenDiskCleanupWindow(); // Files management
        private void ToolbarSearch_Click(object sender, RoutedEventArgs e) 
        {
            try { System.Diagnostics.Process.Start("explorer.exe"); } catch {}
        }
        private void ToolbarUpdates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var upgrade = App.ServiceProvider.GetRequiredService<UpgradeWindow>();
                upgrade.Owner = this;
                upgrade.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Could not open upgrade window: " + ex.Message);
            }
        }

        private void SystemMonitor_Click(object sender, MouseButtonEventArgs e) => OpenProcessManagerWindow();
        private void SystemInfo_Click(object sender, MouseButtonEventArgs e) => ShowSystemInfo();
        private void Settings_Click(object sender, RoutedEventArgs e) => OpenSettingsWindow();
        private void Toolbox_Click(object sender, RoutedEventArgs e) => OpenToolboxWindow();
        private void SystemBooster_Click(object sender, MouseButtonEventArgs e) => OpenSystemBoosterWindow();

        private void OpenSystemBoosterWindow()
        {
            if (!CheckFeatureAccess(FeatureGateService.Features.SystemBooster)) return;
            try { new SystemBoosterWindow { Owner = this }.ShowDialog(); }
            catch (Exception ex) { ShowError("System Booster", ex); }
        }

        private ChatWindow? _supportChatWindow;
        private void ChatToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the window is already open / Kiểm tra nếu cửa sổ đã mở
                if (_supportChatWindow == null || !Application.Current.Windows.OfType<ChatWindow>().Any())
                {
                    _supportChatWindow = App.ServiceProvider.GetRequiredService<ChatWindow>();
                    _supportChatWindow.Owner = this;
                    
                    // Top-right corner of the screen / Góc trên bên phải màn hình
                    _supportChatWindow.Left = SystemParameters.WorkArea.Right - _supportChatWindow.Width - 10;
                    _supportChatWindow.Top = SystemParameters.WorkArea.Top + 10;
                    
                    _supportChatWindow.Show();
                }
                else
                {
                    _supportChatWindow.Activate();
                    if (_supportChatWindow.WindowState == WindowState.Minimized)
                        _supportChatWindow.WindowState = WindowState.Normal;
                }
            }
            catch (Exception ex)
            {
                ShowError("Chat", ex);
            }
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            // Show Login Overlay
            if (LoginScreen != null)
            {
                LoginScreen.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region New Advanced Tools Handlers

        private void Uninstaller_Click(object sender, MouseButtonEventArgs e) => ShowFeatureBuilding("Uninstaller");
        private void DriverUpdate_Click(object sender, MouseButtonEventArgs e) => ShowFeatureBuilding("Driver Manager");
        private void SoftwareUpdate_Click(object sender, MouseButtonEventArgs e) => ShowFeatureBuilding("Software Updater");
        private void SmartDefrag_Click(object sender, MouseButtonEventArgs e) => ShowFeatureBuilding("Smart Defrag");
        private void InternetBooster_Click(object sender, MouseButtonEventArgs e) => ShowFeatureBuilding("Internet Booster");
        private void DiskDoctor_Click(object sender, MouseButtonEventArgs e) => ShowFeatureBuilding("Disk Doctor");
        private void FileShredder_Click(object sender, MouseButtonEventArgs e) => ShowFeatureBuilding("File Shredder");
        private void Undelete_Click(object sender, MouseButtonEventArgs e)
        {
            if (!CheckFeatureAccess(FeatureGateService.Features.FileRecovery)) return;
            var recoveryWindow = new FileRecoveryWindow();
            recoveryWindow.Owner = this;
            recoveryWindow.ShowDialog();
        }
        private void LargeFile_Click(object sender, MouseButtonEventArgs e) => ShowFeatureBuilding("Large Files Finder");
        private void EmptyFolder_Click(object sender, MouseButtonEventArgs e) => ShowFeatureBuilding("Empty Folder Scanner");
        private void ShortcutFixer_Click(object sender, MouseButtonEventArgs e) => ShowFeatureBuilding("Shortcut Fixer");
        private void ProcessManager_Click(object sender, MouseButtonEventArgs e) => OpenProcessManagerWindow();
        private void AutoShutdown_Click(object sender, MouseButtonEventArgs e) => ShowFeatureBuilding("Auto Shutdown");

        private void ShowFeatureBuilding(string featureName)
        {
             MessageBox.Show($"{featureName} will be available in the next update.", "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Helper Methods / Các phương thức hỗ trợ

        private void OpenDiskCleanupWindow()
        {
            if (!CheckFeatureAccess(FeatureGateService.Features.FullDiskCleanup)) return;
            try { new DiskCleanupWindow { Owner = this }.ShowDialog(); }
            catch (Exception ex) { ShowError("Disk Cleanup", ex); }
        }

        private void OpenStartupManagerWindow()
        {
            if (!CheckFeatureAccess(FeatureGateService.Features.StartupManager)) return;
            try { new StartupManagerWindow { Owner = this }.ShowDialog(); }
            catch (Exception ex) { ShowError("Startup Manager", ex); }
        }

        private void OpenRegistryTweaksWindow()
        {
            if (!CheckFeatureAccess(FeatureGateService.Features.RegistryTweaks)) return;
            try { new RegistryTweaksWindow { Owner = this }.ShowDialog(); }
            catch (Exception ex) { ShowError("Registry Tweaks", ex); }
        }

        private void OpenWindowsTweaksWindow()
        {
            if (!CheckFeatureAccess(FeatureGateService.Features.Windows11Tweaks)) return;
            try { new Windows11TweaksWindow { Owner = this }.ShowDialog(); }
            catch (Exception ex) { ShowError("Windows Tweaks", ex); }
        }

        private void OpenProcessManagerWindow()
        {
            if (!CheckFeatureAccess(FeatureGateService.Features.ProcessManager)) return;
            try { new ProcessManagerWindow { Owner = this }.ShowDialog(); }
            catch (Exception ex) { ShowError("Process Manager", ex); }
        }

        private void OpenSettingsWindow()
        {
            // SettingsView is likely a UserControl based on name, but possibly hosted in a Window or needs navigation
            // Checking file list... SettingsView.xaml exists.
            // If it's a Window:
            try { 
                // Open Settings Window
                var settingsWin = App.ServiceProvider.GetRequiredService<SettingsView>();
                settingsWin.Owner = this;
                settingsWin.ShowDialog();

                // Check if Login was requested
                if (settingsWin.Tag?.ToString() == "LOGIN_REQUEST")
                {
                    LoginScreen.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex) { ShowError("Settings", ex); }
        }

        private void OpenToolboxWindow()
        {
            if (!CheckFeatureAccess(FeatureGateService.Features.Toolbox)) return;
            try { new ToolboxWindow { Owner = this }.ShowDialog(); }
            catch (Exception ex) { ShowError("Toolbox", ex); }
        }

        private void ShowSystemInfo()
        {
             var info = $"OS: {Environment.OSVersion}\n" +
                        $"Machine: {Environment.MachineName}\n" +
                        $"User: {Environment.UserName}\n" +
                        $"Processors: {Environment.ProcessorCount}\n" +
                        $"System Directory: {Environment.SystemDirectory}";
             MessageBox.Show(info, "System Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowError(string feature, Exception ex)
        {
            MessageBox.Show($"Error opening {feature}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Check if user has access to a feature, show UpgradeWindow if not
        /// Kiểm tra quyền truy cập tính năng, hiển thị cửa sổ nâng cấp nếu không có
        /// </summary>
        private bool CheckFeatureAccess(string featureId)
        {
            if (_featureGate.CanAccess(featureId))
                return true;

            // Show upgrade window / Hiển thị cửa sổ nâng cấp
            var upgradeWindow = new UpgradeWindow(_featureGate, _userId);
            upgradeWindow.Owner = this;
            upgradeWindow.ShowDialog();

            // Re-check after potential activation / Kiểm tra lại sau khi có thể đã kích hoạt
            return _featureGate.CanAccess(featureId);
        }

        #endregion
    }
}
