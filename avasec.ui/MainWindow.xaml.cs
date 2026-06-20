using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using AVASec.UI.Views;

namespace AVASec.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private System.Windows.Threading.DispatcherTimer _notificationTimer;
    private readonly Services.NotificationService _notificationService;

    public MainWindow()
    {
        InitializeComponent();
        _notificationService = new Services.NotificationService();
        StartNotificationPolling();
    }

    private void StartNotificationPolling()
    {
        _notificationTimer = new System.Windows.Threading.DispatcherTimer();
        _notificationTimer.Interval = TimeSpan.FromMinutes(1);
        _notificationTimer.Tick += async (s, e) => await CheckNotifications();
        _notificationTimer.Start();
        
        // Initial check
        Task.Run(async () => await CheckNotifications());
    }

    private async Task CheckNotifications()
    {
        // Mock User ID 1 for demo
        int userId = 1; 
        var notifications = await _notificationService.CheckNotificationsAsync(userId);
        
        foreach (var n in notifications)
        {
            if (!n.IsRead && (n.IsPromotional || n.Type == "warning" || n.Type == "error"))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var window = new NotificationWindow(n);
                    window.Show();
                });
                
                // Mark as read to avoid spam
                await _notificationService.MarkAsReadAsync(n.NotificationID);
                break; // Show one at a time
            }
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        // Resolve SettingsView from DI container to ensure ISettingsService is injected
        var settingsView = App.ServiceProvider.GetRequiredService<SettingsView>();
        settingsView.Owner = this;
        settingsView.ShowDialog();
    }
}