using System.Text.Json;

namespace AVASec.Core.Services;

/// <summary>
/// Caches validated license locally for offline grace period.
/// </summary>
public class LicenseCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _cachePath;

    public LicenseCacheService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Models.BrandConstants.AppDataFolder);
        Directory.CreateDirectory(dir);
        _cachePath = Path.Combine(dir, "license-cache.json");
    }

    public CachedLicenseSnapshot? Load()
    {
        try
        {
            if (!File.Exists(_cachePath))
            {
                return null;
            }

            var json = File.ReadAllText(_cachePath);
            return JsonSerializer.Deserialize<CachedLicenseSnapshot>(json);
        }
        catch
        {
            return null;
        }
    }

    public void Save(CachedLicenseSnapshot snapshot)
    {
        snapshot.CachedAt = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        File.WriteAllText(_cachePath, json);
    }

    public bool IsWithinGracePeriod()
    {
        var cached = Load();
        if (cached == null)
        {
            return false;
        }

        return DateTime.UtcNow - cached.CachedAt <= TimeSpan.FromDays(Models.ProductMode.LicenseOfflineGraceDays);
    }
}

public sealed class CachedLicenseSnapshot
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseType { get; set; } = "Free";
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CachedAt { get; set; }
}
