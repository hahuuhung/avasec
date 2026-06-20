using System;

namespace AVASec.Plugin.Core.Interfaces
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Debug
    }

    /// <summary>
    /// Connects the plugin to the host application services.
    /// </summary>
    public interface IPluginContext
    {
        /// <summary>
        /// Log message to AVA Security's central logging system.
        /// </summary>
        void Log(string message, LogLevel level = LogLevel.Info);

        /// <summary>
        /// Show a toast notification to the user.
        /// </summary>
        void Notify(string title, string message, string type = "Info");

        /// <summary>
        /// Get path to plugin's data directory.
        /// </summary>
        string GetDataDirectory();
    }
}
