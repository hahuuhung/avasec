using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AVASec.UI.Services;

namespace AVASec.UI.Views
{
    public partial class NotificationWindow : Window
    {
        private string _actionUrl;

        public NotificationWindow(NotificationModel notification)
        {
            InitializeComponent();
            LoadNotification(notification);
            
            // Position at bottom right
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - 20;
            this.Top = desktopWorkingArea.Bottom - this.Height - 20;
        }

        private void LoadNotification(NotificationModel notification)
        {
            TitleText.Text = notification.Title;
            MessageText.Text = notification.Message;
            _actionUrl = notification.ActionUrl;

            // Load Image if exists
            if (!string.IsNullOrEmpty(notification.ImageUrl))
            {
                try
                {
                    NotificationImage.Source = new BitmapImage(new Uri(notification.ImageUrl));
                    NotificationImage.Visibility = Visibility.Visible;
                }
                catch { NotificationImage.Visibility = Visibility.Collapsed; }
            }

            // Show Action Button if URL exists
            if (!string.IsNullOrEmpty(notification.ActionUrl))
            {
                ActionButton.Visibility = Visibility.Visible;
                if (notification.IsPromotional)
                {
                    ActionButton.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#8B5CF6")); // Purple for promo
                    ActionButton.Content = "Nhận ưu đãi / Get Offer";
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_actionUrl))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _actionUrl,
                        UseShellExecute = true
                    });
                }
                catch { }
            }
            this.Close();
        }
    }
}
