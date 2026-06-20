using AVASec.Core.Models;

namespace AVASec.Core.Interfaces
{
    public interface ISettingsService
    {
        AppSettings LoadSettings();
        void SaveSettings(AppSettings settings);
        void ResetDefaults();
        
        // Helper to get current settings without loading
        AppSettings CurrentSettings { get; }
    }
}
