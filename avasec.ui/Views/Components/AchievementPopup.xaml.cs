using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace AVASec.UI.Views.Components
{
    public partial class AchievementPopup : Window
    {
        public AchievementPopup(string title, string description, int points)
        {
            InitializeComponent();
            
            TitleText.Text = title;
            DescText.Text = description;
            PointsText.Text = $"+{points} XP";

            Loaded += AchievementPopup_Loaded;
        }

        private async void AchievementPopup_Loaded(object sender, RoutedEventArgs e)
        {
            // Position at bottom right
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - 20;
            this.Top = desktopWorkingArea.Bottom - this.Height - 20;

            // Wait 5 seconds then fade out
            await Task.Delay(5000);
            CloseWithAnimation();
        }

        private void CloseWithAnimation()
        {
            var anim = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.5)));
            anim.Completed += (s, e) => Close();
            this.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }
}
