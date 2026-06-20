namespace AVASec.Core.Models;

/// <summary>
/// Application version metadata for Vigil desktop and portal.
/// </summary>
public static class AppVersionInfo
{
    public static string Version => BrandConstants.Version;
    public const string BuildDate = "2026-06-17";
    public static string ProductName => BrandConstants.ProductNameFull;
    public const string Company = "Vigil Team";
    public const string Copyright = "© 2026 Vigil Team. All rights reserved.";
    public const string Website = "https://avasec.app";
    public const string SupportEmail = "support@avasec.app";
}
