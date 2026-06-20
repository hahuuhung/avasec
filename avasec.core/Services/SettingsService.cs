using System;
using System.IO;
using System.Text.Json;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;

namespace AVASec.Core.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings _currentSettings;

        public AppSettings CurrentSettings => _currentSettings;

        public SettingsService()
        {
            try
            {
                // Save to AppData/avasec/settings.json
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appData, BrandConstants.AppDataFolder);
                
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                _settingsFilePath = Path.Combine(appFolder, "settings.json");
                _currentSettings = LoadSettings();
            }

            catch (Exception)
            {
                // Fallback if permission issues
                _settingsFilePath = "settings.json"; // Default fallback
                _currentSettings = new AppSettings();
            }
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        _currentSettings = settings;
                        return settings;
                    }
                }
            }
            catch (Exception) { /* Log error? */ }

            return new AppSettings(); // Return defaults
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                _currentSettings = settings;
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception) { /* Log error */ }
        }

        public void ResetDefaults()
        {
            SaveSettings(new AppSettings());
        }
    }
}
