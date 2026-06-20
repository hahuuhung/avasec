using System;
using System.Windows;
using AVASec.Plugin.Core.Interfaces;
using AVASec.Plugin.Core.Models;

namespace AVASec.Plugins.NetworkFortress
{
    public class NetworkFortressPlugin : IPlugin
    {
        private IPluginContext? _context;
        private NetworkWindow? _window;

        public PluginMetadata Metadata => new PluginMetadata
        {
            Id = "sysanti.tool.networkfortress",
            Name = "Network Fortress",
            Description = "Real-time network traffic monitoring.\nGiám sát lưu lượng mạng thời gian thực.",
            Version = "1.0.0",
            Author = "AVA Security Inc.",
            Category = PluginCategory.Network,
            IconUri = "🛡️",
            IsPremium = true
        };

        public void Initialize(IPluginContext context)
        {
            _context = context;
            _context.Log("Network Fortress initialized.", LogLevel.Info);
        }

        public void Execute()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_window == null || !_window.IsLoaded)
                {
                    _window = new NetworkWindow(_context);
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
