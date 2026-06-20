using System;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;
using AVASec.Plugin.Core.Interfaces;

namespace AVASec.Core.Services
{
    public class AVASecPluginContext : IPluginContext
    {
        private readonly INotificationService _notificationService;
        private readonly string _pluginDataPath;

        public AVASecPluginContext(INotificationService notificationService, string pluginDataPath)
        {
            _notificationService = notificationService;
            _pluginDataPath = pluginDataPath;
        }

        public string GetDataDirectory()
        {
            return _pluginDataPath;
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            // Simple console logging for now, can be upgraded to Serilog later
            string prefix = level switch
            {
                LogLevel.Info => "[INFO]",
                LogLevel.Warning => "[WARN]",
                LogLevel.Error => "[ERROR]",
                LogLevel.Debug => "[DEBUG]",
                _ => "[INFO]"
            };
            Console.WriteLine($"{prefix} [Plugin] {message}");
        }

        public void Notify(string title, string message, string type = "Info")
        {
            // Map plugin notification type to core enum
            NotificationType notifType = type.ToLower() switch
            {
                "success" => NotificationType.Success,
                "warning" => NotificationType.Warning,
                "error" => NotificationType.Error,
                "security" => NotificationType.Security,
                _ => NotificationType.Info
            };

            _notificationService.Show(title, message, notifType);
        }
    }
}
