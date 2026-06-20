using System;
using AVASec.Plugin.Core.Models;

namespace AVASec.Plugin.Core.Interfaces
{
    /// <summary>
    /// The main contract for all AVA Security Cyber-Plugins.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Metadata describing the plugin.
        /// </summary>
        PluginMetadata Metadata { get; }

        /// <summary>
        /// Called when the plugin is loaded. use this to set up context.
        /// </summary>
        void Initialize(IPluginContext context);

        /// <summary>
        /// Main execution entry point (e.g., when user clicks the tool icon).
        /// </summary>
        void Execute(); 

        /// <summary>
        /// Cleanup resources before unloading.
        /// </summary>
        void Terminate();
    }
}
