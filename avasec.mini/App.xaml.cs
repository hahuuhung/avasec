using System.Windows;
using AVASec.Mini.Services;

namespace AVASec.Mini;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var scale = MiniFontScaleService.Load();
        MiniFontScaleService.ApplyToResources(Resources, scale);
    }
}