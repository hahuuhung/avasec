using System;
using System.Linq;
using System.Windows;

namespace AVASec.UI.Services
{
    public class LanguageService
    {
        private static LanguageService? _instance;
        public static LanguageService Instance => _instance ??= new LanguageService();

        public event EventHandler<string>? LanguageChanged;

        public string CurrentLanguage { get; private set; } = "vi-EN";

        public void SetLanguage(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode)) return;

            try
            {
                var dict = new ResourceDictionary();
                string source = $"pack://application:,,,/Resources/Languages/{cultureCode}.xaml";
                dict.Source = new Uri(source);

                // Find old language dictionary
                // We identify it by looking for the one that has "Lang." keys or by Source path
                // For safety, let's assume any dict with "Languages" in path is a lang dict
                var oldDict = Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("/Languages/"));

                if (oldDict != null)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                }
                else
                {
                    // Fallback: if we can't find by path (initially might not be set), 
                    // we might need to rely on the fact that we are adding it now.
                    // For the first run, there might not be one.
                }

                Application.Current.Resources.MergedDictionaries.Add(dict);
                CurrentLanguage = cultureCode;
                
                LanguageChanged?.Invoke(this, cultureCode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting language: {ex.Message}");
            }
        }
    }
}
