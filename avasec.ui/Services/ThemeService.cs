using System;
using System.Linq;
using System.Windows;

namespace AVASec.UI.Services
{
    public class ThemeService
    {
        private static ThemeService? _instance;
        public static ThemeService Instance => _instance ??= new ThemeService();

        public event EventHandler<bool>? ThemeChanged; // true = Dark, false = Light

        public bool IsDarkMode { get; private set; } = false;
        public bool IsHighContrast { get; private set; }

        public void SetHighContrast(bool enabled)
        {
            IsHighContrast = enabled;
            if (enabled)
            {
                LoadThemeDictionary("HighContrastTheme.xaml");
            }
            else
            {
                SetTheme(IsDarkMode);
            }
        }

        public void SetTheme(bool isDark)
        {
            try
            {
                if (IsHighContrast)
                {
                    IsDarkMode = isDark;
                    return;
                }

                LoadThemeDictionary(isDark ? "DarkTheme.xaml" : "LightTheme.xaml");
                IsDarkMode = isDark;
                ThemeChanged?.Invoke(this, isDark);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting theme: {ex.Message}");
            }
        }

        private static void LoadThemeDictionary(string themeFileName)
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Resources/Themes/{themeFileName}")
            };

            var oldDict = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("/Themes/"));

            if (oldDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(oldDict);
            }

            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}
