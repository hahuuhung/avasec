using System;
using System.Windows;
using AVASec.Core.Models;

namespace AVASec.UI.Services
{
    /// <summary>
    /// Applies font scaling and high-contrast theme from user settings.
    /// </summary>
    public class AccessibilityService
    {
        private static AccessibilityService? _instance;
        public static AccessibilityService Instance => _instance ??= new AccessibilityService();

        public double FontScale { get; private set; } = 1.0;
        public bool HighContrast { get; private set; }

        public void Apply(AppSettings settings)
        {
            FontScale = Math.Clamp(settings.FontScale, 0.85, 1.5);
            HighContrast = settings.HighContrast;

            ThemeService.Instance.SetHighContrast(HighContrast);
            ApplyFontScaleToWindows(FontScale);
        }

        public void ApplyFontScaleToWindows(double scale)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.LayoutTransform is not System.Windows.Media.ScaleTransform existing ||
                    Math.Abs(existing.ScaleX - scale) > 0.001)
                {
                    window.LayoutTransform = new System.Windows.Media.ScaleTransform(scale, scale);
                }
            }
        }
    }
}
