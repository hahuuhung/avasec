using System;
using System.Diagnostics;
using System.Windows;
using AVASec.Plugin.Core.Interfaces;
using AVASec.Plugin.Core.Models;

namespace AVASec.Plugins.GameBooster
{
    public class GameBoosterPlugin : IPlugin
    {
        private IPluginContext? _context;
        private BoosterWindow? _window;

        public PluginMetadata Metadata => new PluginMetadata
        {
            Id = "sysanti.tool.gamebooster",
            Name = "Game Booster Ultra",
            Description = "Maximize performance for gaming.\nTối ưu hóa hiệu năng chơi game.",
            Version = "1.0.0",
            Author = "AVA Security Inc.",
            Category = PluginCategory.Optimization,
            IconUri = "🎮",
            IsPremium = true
        };

        public void Initialize(IPluginContext context)
        {
            _context = context;
            _context.Log("Game Booster initialized.", LogLevel.Info);
        }

        public void Execute()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_window == null || !_window.IsLoaded)
                {
                    _window = new BoosterWindow(_context);
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
