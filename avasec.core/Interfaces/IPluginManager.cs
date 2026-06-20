using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AVASec.Plugin.Core.Interfaces;

namespace AVASec.Core.Interfaces
{
    public interface IPluginManager
    {
        Task LoadPluginsAsync();
        IEnumerable<IPlugin> GetPlugins();
        void ExecutePlugin(string pluginId);
    }
}
