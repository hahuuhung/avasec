namespace AVASec.Core.Models;

/// <summary>
/// Product mode: app offline-first, web portal only for account & license keys.
/// </summary>
public static class ProductMode
{
    /// <summary>Heavy cloud sync / telemetry — disabled to save cost.</summary>
    public const bool CloudSyncEnabled = false;

    /// <summary>Website for register, login, buy & activate keys.</summary>
    public const bool WebPortalEnabled = true;

    /// <summary>App features work without internet after license is cached.</summary>
    public const bool OfflineFirst = true;

    public const int LicenseOfflineGraceDays = 7;

    public const string ModeLabelBilingual =
        "Offline app + Web license / App offline + Key qua Website";

    public const string SidebarHintBilingual =
        "🌐 Account & keys: website · App works offline / Tài khoản & key: website · App dùng offline";
}

/// <summary>
/// Portal URLs — override via appsettings.json ApiBaseUrl / PortalUrl.
/// </summary>
public static class PortalConfig
{
    public static string ApiBaseUrl { get; set; } = "http://localhost:3001";
    public static string PortalUrl { get; set; } = "http://localhost:3001/store.html";
    public static string LoginUrl { get; set; } = "http://localhost:3001/index.html";
}
