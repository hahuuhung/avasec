using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AVASec.UI.Controls
{
    public partial class NotificationToast : UserControl
    {
        public event EventHandler Closed;

        public NotificationToast()
        {
            InitializeComponent();
            Loaded += NotificationToast_Loaded;
        }

        public void SetContent(string title, string message)
        {
            TitleText.Text = title;
            MessageText.Text = message;
        }

        private void NotificationToast_Loaded(object sender, RoutedEventArgs e)
        {
            var slideIn = (Storyboard)FindResource("SlideIn");
            slideIn.Begin(this);

            var fadeOut = (Storyboard)FindResource("FadeOut");
            fadeOut.Completed += (s, ev) => Closed?.Invoke(this, EventArgs.Empty);
            fadeOut.Begin(this);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
