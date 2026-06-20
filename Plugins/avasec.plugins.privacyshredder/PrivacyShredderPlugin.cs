using System;
using System.Windows;
using AVASec.Plugin.Core.Interfaces;
using AVASec.Plugin.Core.Models;

namespace AVASec.Plugins.PrivacyShredder
{
    public class PrivacyShredderPlugin : IPlugin
    {
        private IPluginContext? _context;
        private ShredderWindow? _window;

        public PluginMetadata Metadata => new PluginMetadata
        {
            Id = "sysanti.tool.shredder",
            Name = "Privacy Shredder",
            Description = "Permanently delete files to prevent recovery.\nXóa vĩnh viễn tệp tin, không thể khôi phục.",
            Version = "1.0.0",
            Author = "AVA Security Inc.",
            Category = PluginCategory.Security,
            IconUri = "🗑️",
            IsPremium = true
        };

        public void Initialize(IPluginContext context)
        {
            _context = context;
            _context.Log("Privacy Shredder initialized.", LogLevel.Info);
        }

        public void Execute()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_window == null || !_window.IsLoaded)
                {
                    _window = new ShredderWindow(_context);
                    _window.Closed += (s, e) => _window = null;
                    _window.Show();
                }
                else
                {
                    _window.Activate();
                }
            });
        }

        public void Terminate()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _window?.Close();
                _window = null;
            });
        }
    }
}
