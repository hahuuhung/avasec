using System;

namespace AVASec.Plugin.Core.Models
{
    public class PluginMetadata
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0";
        public string Author { get; set; } = "AVA Security Team";
        public PluginCategory Category { get; set; } = PluginCategory.Utility;
        
        /// <summary>
        /// Relative path to icon resource or Emoji character
        /// </summary>
        public string IconUri { get; set; } = "🧩";
        
        public bool IsPremium { get; set; } = false;
    }
}
