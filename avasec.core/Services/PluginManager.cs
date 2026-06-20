using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;
using AVASec.Plugin.Core.Interfaces;
using AVASec.Plugin.Core.Models;

namespace AVASec.Core.Services
{
    public class PluginManager : IPluginManager
    {
        private readonly List<IPlugin> _loadedPlugins = new();
        private readonly INotificationService _notificationService;
        private readonly string _pluginsDirectory;

        public PluginManager(INotificationService notificationService)
        {
            _notificationService = notificationService;
            _pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        }

        /// <summary>
        /// Loads all plugins asynchronously from the Plugins directory.
        /// Tải tất cả các plugin một cách bất đồng bộ từ thư mục Plugins.
        /// </summary>
        public async Task LoadPluginsAsync()
        {
            if (!Directory.Exists(_pluginsDirectory))
            {
                Directory.CreateDirectory(_pluginsDirectory);
            }

            var dllFiles = Directory.GetFiles(_pluginsDirectory, "*.dll");
            var loadTasks = new List<Task>();

            // Load plugins in parallel to improve startup time
            // Tải plugin song song để cải thiện thời gian khởi động
            foreach (var dllPath in dllFiles)
            {
                loadTasks.Add(Task.Run(() => LoadPluginFromDll(dllPath)));
            }

            try
            {
                await Task.WhenAll(loadTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during parallel plugin loading: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads a single plugin from a DLL file and initializes it.
        /// Tải một plugin đơn lẻ từ file DLL và khởi tạo nó.
        /// </summary>
        /// <param name="dllPath">Path to the plugin DLL file / Đường dẫn tới file DLL của plugin</param>
        private void LoadPluginFromDll(string dllPath)
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllPath);
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in pluginTypes)
                {
                    if (Activator.CreateInstance(type) is IPlugin plugin)
                    {
                        // Create context for this plugin / Tạo ngữ cảnh cho plugin này
                        var context = new AVASecPluginContext(_notificationService, 
                            Path.Combine(_pluginsDirectory, "Data", plugin.Metadata.Id));
                        
                        // Initialize the plugin with the context / Khởi tạo plugin với ngữ cảnh
                        plugin.Initialize(context);
                        
                        lock (_loadedPlugins) // Ensure thread safety when adding to list / Đảm bảo an toàn luồng khi thêm vào danh sách
                        {
                            _loadedPlugins.Add(plugin);
                        }
                        
                        Console.WriteLine($"Loaded plugin: {plugin.Metadata.Name} v{plugin.Metadata.Version}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading plugin from {dllPath}: {ex.Message}");
            }
        }

        public IEnumerable<IPlugin> GetPlugins()
        {
            return _loadedPlugins;
        }

        public void ExecutePlugin(string pluginId)
        {
            var plugin = _loadedPlugins.FirstOrDefault(p => p.Metadata.Id == pluginId);
            if (plugin != null)
            {
                try
                {
                    plugin.Execute();
                }
                catch (Exception ex)
                {
                    _notificationService.Show("Plugin Error", $"Failed to execute {plugin.Metadata.Name}: {ex.Message}", NotificationType.Error);
                }
            }
        }
    }
}
