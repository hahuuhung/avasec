using System.IO;
using System.Text.Json;
using System.Windows;

namespace AVASec.Mini.Services;

public static class MiniFontScaleService
{
    public const double MinScale = 0.85;
    public const double MaxScale = 1.45;
    public const double DefaultScale = 1.15;
    public const double Step = 0.05;

    public static double CurrentScale { get; private set; } = DefaultScale;

    private static string SettingsPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AVASecMini",
            "settings.json");

    public static double Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return DefaultScale;

            var json = File.ReadAllText(SettingsPath);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("fontScale", out var el))
                return Math.Clamp(el.GetDouble(), MinScale, MaxScale);
        }
        catch
        {
        }

        return DefaultScale;
    }

    public static void Save(double scale)
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath)!;
            Directory.CreateDirectory(dir);
            var payload = JsonSerializer.Serialize(new { fontScale = Math.Clamp(scale, MinScale, MaxScale) });
            File.WriteAllText(SettingsPath, payload);
        }
        catch
        {
        }
    }

    public static void ApplyToResources(ResourceDictionary resources, double scale)
    {
        CurrentScale = Math.Clamp(scale, MinScale, MaxScale);
        resources["MiniFontScale"] = CurrentScale;
        resources["MiniFontHeaderTitle"] = 15 * CurrentScale;
        resources["MiniFontHeaderSub"] = 11 * CurrentScale;
        resources["MiniFontBadge"] = 11 * CurrentScale;
        resources["MiniFontLabel"] = 12 * CurrentScale;
        resources["MiniFontBody"] = 14 * CurrentScale;
        resources["MiniFontBodySmall"] = 12 * CurrentScale;
        resources["MiniFontCaption"] = 11 * CurrentScale;
        resources["MiniFontMetric"] = 24 * CurrentScale;
        resources["MiniFontButton"] = 14 * CurrentScale;
        resources["MiniFontLog"] = 12 * CurrentScale;
        resources["MiniFontLineHeightBody"] = 22 * CurrentScale;
        resources["MiniFontLineHeightLog"] = 18 * CurrentScale;
        resources["MiniFontCheckBox"] = 12 * CurrentScale;
    }

    public static string FormatLabel(double scale) => $"{(int)Math.Round(scale * 100)}%";
}