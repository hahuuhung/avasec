using System;
using System.Windows;
using AVASec.UI.Views.Components;

namespace AVASec.UI.Services
{
    public class GamificationService
    {
        private static GamificationService? _instance;
        public static GamificationService Instance => _instance ??= new GamificationService();

        private GamificationService() { }

        public void ShowAchievement(string title, string description, int points)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var popup = new AchievementPopup(title, description, points);
                popup.Show();
            });
        }
    }
}
