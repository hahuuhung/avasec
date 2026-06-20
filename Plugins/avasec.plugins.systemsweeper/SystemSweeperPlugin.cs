using System;
using System.Windows;
using AVASec.Plugin.Core.Interfaces;
using AVASec.Plugin.Core.Models;

namespace AVASec.Plugins.SystemSweeper
{
    public class SystemSweeperPlugin : IPlugin
    {
        private IPluginContext? _context;
        private SweeperWindow? _window;

        public PluginMetadata Metadata => new PluginMetadata
        {
            Id = "sysanti.tool.systemsweeper",
            Name = "System Sweeper",
            Description = "Clean junk files and browser cache.\nDọn dẹp tệp rác và bộ nhớ đệm.",
            Version = "1.0.0",
            Author = "AVA Security Inc.",
            Category = PluginCategory.Optimization,
            IconUri = "🧹",
            IsPremium = false
        };

        public void Initialize(IPluginContext context)
        {
            _context = context;
            _context.Log("System Sweeper initialized.", LogLevel.Info);
        }

        public void Execute()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_window == null || !_window.IsLoaded)
                {
                    _window = new SweeperWindow(_context);
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
