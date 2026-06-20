using System;
using System.Windows;
using AVASec.Plugin.Core.Interfaces;
using AVASec.Plugin.Core.Models;

namespace AVASec.Plugins.RegistryDoctor
{
    public class RegistryDoctorPlugin : IPlugin
    {
        private IPluginContext? _context;
        private DoctorWindow? _window;

        public PluginMetadata Metadata => new PluginMetadata
        {
            Id = "sysanti.tool.registrydoctor",
            Name = "Registry Doctor",
            Description = "Scan and repair registry errors.\nQuét và sửa lỗi Registry.",
            Version = "1.0.0",
            Author = "AVA Security Inc.",
            Category = PluginCategory.System,
            IconUri = "💉",
            IsPremium = false
        };

        public void Initialize(IPluginContext context)
        {
            _context = context;
            _context.Log("Registry Doctor initialized.", LogLevel.Info);
        }

        public void Execute()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_window == null || !_window.IsLoaded)
                {
                    _window = new DoctorWindow(_context);
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
