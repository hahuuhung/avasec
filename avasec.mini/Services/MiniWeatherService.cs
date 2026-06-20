using System.Net.Http;
using System.Text.Json;
using AVASec.Mini.Models;

namespace AVASec.Mini.Services;

public sealed class WeatherInfo
{
    public WeatherKind Kind { get; init; } = WeatherKind.PartlyCloudy;
    public string CityVi { get; init; } = "Việt Nam";
    public string CityEn { get; init; } = "Vietnam";
    public string Temperature { get; init; } = "--";
    public string SummaryVi { get; init; } = "Đang tải...";
    public string SummaryEn { get; init; } = "Loading...";
    public string LineVi { get; init; } = string.Empty;
    public string LineEn { get; init; } = string.Empty;
}

public static class MiniWeatherService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };
    private static WeatherInfo? _cache;
    private static DateTime _cacheTime;

    public static async Task<WeatherInfo> GetTodayAsync()
    {
        if (_cache != null && DateTime.UtcNow - _cacheTime < TimeSpan.FromMinutes(30))
            return _cache;

        try
        {
            var url = "https://api.open-meteo.com/v1/forecast?latitude=21.0285&longitude=105.8542&current=temperature_2m,weather_code&timezone=Asia%2FBangkok";
            using var doc = JsonDocument.Parse(await Http.GetStringAsync(url));
            var current = doc.RootElement.GetProperty("current");
            int code = current.GetProperty("weather_code").GetInt32();
            double temp = current.GetProperty("temperature_2m").GetDouble();
            var (kind, vi, en) = MapWeather(code);
            _cache = new WeatherInfo
            {
                Kind = kind,
                CityVi = "Hà Nội",
                CityEn = "Hanoi",
                Temperature = $"{temp:0}°C",
                SummaryVi = vi,
                SummaryEn = en,
                LineVi = $"Hà Nội · {temp:0}°C · {vi}",
                LineEn = $"Hanoi · {temp:0}°C · {en}"
            };
            _cacheTime = DateTime.UtcNow;
            return _cache;
        }
        catch
        {
            return GetOfflineFallback();
        }
    }

    public static WeatherInfo GetOfflineFallback()
    {
        var hour = DateTime.Now.Hour;
        bool day = hour is >= 6 and < 18;
        var kind = day ? WeatherKind.Sunny : WeatherKind.Night;
        string vi = day ? "Trời đẹp (ngoại tuyến)" : "Ban đêm yên tĩnh (ngoại tuyến)";
        string en = day ? "Fair weather (offline)" : "Quiet night (offline)";
        return new WeatherInfo
        {
            Kind = kind,
            CityVi = "Việt Nam",
            CityEn = "Vietnam",
            Temperature = "--",
            SummaryVi = vi,
            SummaryEn = en,
            LineVi = vi,
            LineEn = en
        };
    }

    private static (WeatherKind kind, string vi, string en) MapWeather(int code) => code switch
    {
        0 => (WeatherKind.Sunny, "Trời nắng", "Sunny"),
        1 or 2 => (WeatherKind.PartlyCloudy, "Ít mây", "Partly cloudy"),
        3 => (WeatherKind.Cloudy, "Nhiều mây", "Cloudy"),
        45 or 48 => (WeatherKind.Fog, "Sương mù", "Foggy"),
        51 or 53 or 55 => (WeatherKind.Drizzle, "Mưa phùn", "Drizzle"),
        61 or 63 or 65 => (WeatherKind.Rain, "Mưa", "Rain"),
        71 or 73 or 75 => (WeatherKind.Snow, "Tuyết", "Snow"),
        80 or 81 or 82 => (WeatherKind.Showers, "Mưa rào", "Showers"),
        95 or 96 or 99 => (WeatherKind.Thunderstorm, "Dông", "Thunderstorm"),
        _ => (WeatherKind.Fair, "Thời tiết ổn", "Fair weather")
    };
}