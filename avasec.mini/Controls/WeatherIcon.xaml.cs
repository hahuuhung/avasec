using System.Windows;
using System.Windows.Controls;
using AVASec.Mini.Models;

namespace AVASec.Mini.Controls;

public partial class WeatherIcon : System.Windows.Controls.UserControl
{
    public WeatherIcon() => InitializeComponent();

    public void SetKind(WeatherKind kind)
    {
        SunnyPanel.Visibility = Visibility.Collapsed;
        PartlyCloudyPanel.Visibility = Visibility.Collapsed;
        CloudyPanel.Visibility = Visibility.Collapsed;
        FogPanel.Visibility = Visibility.Collapsed;
        RainPanel.Visibility = Visibility.Collapsed;
        DrizzlePanel.Visibility = Visibility.Collapsed;
        ShowersPanel.Visibility = Visibility.Collapsed;
        StormPanel.Visibility = Visibility.Collapsed;
        SnowPanel.Visibility = Visibility.Collapsed;
        NightPanel.Visibility = Visibility.Collapsed;
        FairPanel.Visibility = Visibility.Collapsed;

        var panel = kind switch
        {
            WeatherKind.Sunny => SunnyPanel,
            WeatherKind.PartlyCloudy => PartlyCloudyPanel,
            WeatherKind.Cloudy => CloudyPanel,
            WeatherKind.Fog => FogPanel,
            WeatherKind.Rain => RainPanel,
            WeatherKind.Drizzle => DrizzlePanel,
            WeatherKind.Showers => ShowersPanel,
            WeatherKind.Thunderstorm => StormPanel,
            WeatherKind.Snow => SnowPanel,
            WeatherKind.Night => NightPanel,
            _ => FairPanel
        };
        panel.Visibility = Visibility.Visible;
    }
}