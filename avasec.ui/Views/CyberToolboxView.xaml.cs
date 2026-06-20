using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using AVASec.Core.Interfaces;
using AVASec.Core.Services;
using AVASec.Plugin.Core.Interfaces;

namespace AVASec.UI.Views
{
    public partial class CyberToolboxView : UserControl
    {
        private readonly IPluginManager _pluginManager;

        public CyberToolboxView()
        {
            InitializeComponent();
            _pluginManager = App.ServiceProvider.GetRequiredService<IPluginManager>();
            
            Loaded += CyberToolboxView_Loaded;
        }

        private async void CyberToolboxView_Loaded(object sender, RoutedEventArgs e)
        {
             await LoadPlugins();
        }

        private async System.Threading.Tasks.Task LoadPlugins()
        {
            try
            {
                await _pluginManager.LoadPluginsAsync();
                var plugins = _pluginManager.GetPlugins().ToList();

                PluginsList.ItemsSource = plugins;

                if (plugins.Count == 0)
                {
                    EmptyStatePanel.Visibility = Visibility.Visible;
                    PluginsList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    EmptyStatePanel.Visibility = Visibility.Collapsed;
                    PluginsList.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading plugins: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LaunchPlugin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string pluginId)
            {
                _pluginManager.ExecutePlugin(pluginId);
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadPlugins();
        }
    }
}
