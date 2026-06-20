using System.Text.Json;
using AVASec.Database;
using AVASec.UI.Views;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AVASec.Core.Interfaces;
using AVASec.Core.Services;
using AVASec.UI.Services;
using AVASec.Core.Models;
using BCrypt.Net;
using System.Linq;

namespace AVASec.UI
{
    /// <summary>
    /// Interaction logic for App.xaml / Logic tương tác cho App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Set explicit shutdown mode to prevent closing splash screen from shutting down the app
            // Thiết lập chế độ tắt rõ ràng để tránh đóng splash screen làm tắt ứng dụng
            this.ShutdownMode = ShutdownMode.OnLastWindowClose;

            // Global exception handling / Xử lý ngoại lệ toàn cục
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                // Show SplashScreen / Hiển thị màn hình chờ
                var splash = new Views.SplashScreen();
                splash.Show();
                splash.StartLoading();

                var serviceCollection = new ServiceCollection();
                LoadPortalConfig();
                ConfigureServices(serviceCollection);
                ServiceProvider = serviceCollection.BuildServiceProvider();

                splash.SetStatus("Đang khởi tạo cơ sở dữ liệu... / Initializing database...");
                splash.SetProgress(20);

                // Initialize database / Khởi tạo cơ sở dữ liệu
                using (var scope = ServiceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AVASecContext>();
                    context.Database.EnsureCreated();

#if DEBUG
                    // Dev-only admin seed — never in release builds
                    if (!context.Users.Any(u => u.Username == "admin"))
                    {
                        var adminUser = new AVASec.Core.Models.User
                        {
                            Username = "admin",
                            Email = "admin@avasec.app",
                            CreatedAt = DateTime.Now,
                            IsActive = true,
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin")
                        };
                        context.Users.Add(adminUser);
                        
                        var adminLicense = new AVASec.Core.Models.License
                        {
                            LicenseKey = "SA-ADMIN-LIFETIME-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                            IssueDate = DateTime.Now,
                            ExpiryDate = DateTime.Now.AddYears(99),
                            IsActive = true,
                            LicenseType = "Ultimate"
                        };
                        
                        adminUser.License = adminLicense;
                        context.SaveChanges();
                    }
#endif

                    splash.SetStatus("Đang kiểm tra giấy phép... / Checking license...");
                    splash.SetProgress(50);

                    // Initialize FeatureGate from cached license or local DB
                    var featureGate = ServiceProvider.GetRequiredService<FeatureGateService>();
                    InitializeFeatureGate(featureGate, context);
                }

                splash.SetStatus("Đang tải cấu hình... / Loading configuration...");
                splash.SetProgress(70);

                // Initialize Language / Khởi tạo Ngôn ngữ
                var settingsService = ServiceProvider.GetRequiredService<ISettingsService>();
                var settings = settingsService.LoadSettings();
                LanguageService.Instance.SetLanguage(settings.Language);
                ThemeService.Instance.SetTheme(settings.IsDarkMode);
                AccessibilityService.Instance.Apply(settings);

                splash.SetStatus("Hoàn tất! / Ready!");
                splash.SetProgress(100);

                // Small delay to show "Ready" state / Chờ một chút để hiển thị trạng thái "Sẵn sàng"
                System.Threading.Thread.Sleep(400);

                // Show DashboardView (Guest mode initially) 
                // userId 0 implies Guest
                var dashboardView = ServiceProvider.GetRequiredService<DashboardView>();
                this.MainWindow = dashboardView;
                dashboardView.Show();
                AccessibilityService.Instance.ApplyFontScaleToWindows(settings.FontScale);

                // First-run onboarding wizard
                if (!settings.HasCompletedOnboarding)
                {
                    var onboarding = new OnboardingWindow(settingsService);
                    onboarding.Owner = dashboardView;
                    if (onboarding.ShowDialog() == true && onboarding.RunSmartScanAfterClose)
                    {
                        dashboardView.TriggerSmartScanFromOnboarding();
                    }
                }

                // Close splash / Đóng màn hình chờ
                splash.Close();
            }
            catch (Exception ex)
            {
                LogException(ex);
                MessageBox.Show($"Lỗi khởi động / Startup Error: {ex.Message}\nKiểm tra startup_error.txt", "Lỗi / Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
            // Show user-friendly error without stack trace / Hiển thị lỗi thân thiện không có stack trace
            MessageBox.Show(
                $"Đã xảy ra lỗi không mong muốn. Ứng dụng sẽ tiếp tục hoạt động.\n" +
                $"An unexpected error occurred. The application will continue.\n\n" +
                $"Chi tiết / Details: {e.Exception.Message}",
                "AVA Security - Lỗi / Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException(ex);
            }
        }

        private void LogException(Exception ex)
        {
            try
            {
                string logDir = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    BrandConstants.AppDataFolder, "Logs");
                System.IO.Directory.CreateDirectory(logDir);

                string logFile = System.IO.Path.Combine(logDir, "error.log");
                string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n{"--",50}\n";
                System.IO.File.AppendAllText(logFile, message);
            }
            catch
            {
                // Last resort: write to current directory / Biện pháp cuối: ghi vào thư mục hiện tại
                try { System.IO.File.AppendAllText("startup_error.txt", $"{DateTime.Now}: {ex}\n"); } catch { }
            }
        }

        private static void LoadPortalConfig()
        {
            try
            {
                var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.Development.json");
                if (!System.IO.File.Exists(path))
                {
                    path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                }

                if (!System.IO.File.Exists(path))
                {
                    return;
                }

                using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(path));
                if (doc.RootElement.TryGetProperty("ApiBaseUrl", out var api))
                {
                    PortalConfig.ApiBaseUrl = api.GetString() ?? PortalConfig.ApiBaseUrl;
                }
                if (doc.RootElement.TryGetProperty("PortalUrl", out var portal))
                {
                    PortalConfig.PortalUrl = portal.GetString() ?? PortalConfig.PortalUrl;
                }
                if (doc.RootElement.TryGetProperty("LoginUrl", out var login))
                {
                    PortalConfig.LoginUrl = login.GetString() ?? PortalConfig.LoginUrl;
                }
            }
            catch
            {
                // Keep defaults for local dev
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Database
            services.AddDbContext<AVASecContext>();

            // Services
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<LicenseCacheService>();
            services.AddSingleton<WebLicenseService>();
            services.AddSingleton<AVASec.Optimization.Services.ISystemTweaksService, AVASec.Optimization.Services.SystemTweaksService>();
            
            services.AddSingleton<FeatureGateService>();
            
            // Web portal auth + local offline fallback
            services.AddScoped<IAuthenticationService, HybridAuthenticationService>();
            
            // Local AI assistant — no Socket.io / cloud chat
            services.AddSingleton<AVASec.Chat.Core.Interfaces.IAIBotService, AVASec.Chat.Core.Services.AIBotService>();
            services.AddSingleton<AVASec.Chat.Core.Interfaces.IChatService, AVASec.Chat.Core.Services.LocalChatService>();
            
            // Reporting / Báo cáo
            services.AddSingleton<IReportingService, ReportingService>();
            services.AddSingleton<INotificationService, AVASec.Core.Services.NotificationService>();
            
            // Plugins
            services.AddSingleton<IPluginManager, PluginManager>();
            
            // Views
            services.AddTransient<DashboardView>();
            // services.AddTransient<MainWindow>(); // Removed / Đã xóa
            services.AddTransient<SettingsView>();
            services.AddTransient<VirusScannerWindow>();
            services.AddTransient<QuarantineWindow>();
            
            // HttpClient (optional plugins only)
            services.AddHttpClient();
            
            // Antivirus — local signatures + AI heuristic only
            services.AddTransient<AVASec.Antivirus.Services.FileScannerService>();
            services.AddTransient<AVASec.Antivirus.Services.QuarantineService>();

            // Controls / Điều khiển
            services.AddSingleton<AVASec.UI.Controls.ChatWidget>();
            services.AddTransient<ChatWindow>();
            services.AddTransient<UpgradeWindow>();
            services.AddTransient<OnboardingWindow>();
        }

        private static void InitializeFeatureGate(FeatureGateService featureGate, AVASecContext context)
        {
            var cache = ServiceProvider.GetRequiredService<LicenseCacheService>();
            var cached = cache.Load();
            if (cached != null && cached.IsActive)
            {
                var remaining = (cached.ExpiryDate - DateTime.Now).Days;
                var isLifetime = cached.LicenseType is "Lifetime" or "Ultimate";
                var isValid = isLifetime || remaining > 0 || cache.IsWithinGracePeriod();
                featureGate.SetLicense(cached.LicenseType, isValid, isLifetime ? 9999 : Math.Max(0, remaining));
                return;
            }

#if DEBUG
            var adminUserDb = context.Users.FirstOrDefault(u => u.Username == "admin");
            if (adminUserDb != null)
            {
                var license = context.Licenses.FirstOrDefault(l => l.UserId == adminUserDb.UserId);
                if (license != null)
                {
                    featureGate.SetLicense(license.LicenseType, license.IsValid(), license.GetRemainingDays());
                }
            }
#endif
        }
    }
}
