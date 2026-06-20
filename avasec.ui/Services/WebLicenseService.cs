using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using AVASec.Core.Models;
using AVASec.Core.Services;

namespace AVASec.UI.Services;

/// <summary>
/// Web API for license key activate / validate — minimal cost portal.
/// </summary>
public class WebLicenseService
{
    private readonly HttpClient _httpClient;
    private readonly LicenseCacheService _cache;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public WebLicenseService(LicenseCacheService cache)
    {
        _cache = cache;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(PortalConfig.ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task<LicenseActivateResult> ActivateKeyAsync(string licenseKey, int userId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/license/activate",
                new { licenseKey, userId },
                ct);

            var body = await response.Content.ReadFromJsonAsync<LicenseApiResponse>(JsonOptions, cancellationToken: ct);
            if (body?.Success == true && body.License != null)
            {
                CacheLicense(userId, body.License);
                return new LicenseActivateResult { Success = true, Message = body.Message, License = body.License };
            }

            return new LicenseActivateResult
            {
                Success = false,
                Message = body?.Message ?? "Activation failed / Kích hoạt thất bại"
            };
        }
        catch (Exception ex)
        {
            return new LicenseActivateResult
            {
                Success = false,
                Message = $"Offline — try again when online / Ngoại tuyến: {ex.Message}"
            };
        }
    }

    public async Task<LicenseActivateResult> ValidateKeyAsync(string licenseKey, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/license/validate/{Uri.EscapeDataString(licenseKey)}", ct);
            var body = await response.Content.ReadFromJsonAsync<LicenseApiResponse>(JsonOptions, cancellationToken: ct);
            return new LicenseActivateResult
            {
                Success = body?.Success == true,
                Message = body?.Message ?? "Unknown",
                License = body?.License
            };
        }
        catch (Exception ex)
        {
            return new LicenseActivateResult { Success = false, Message = ex.Message };
        }
    }

    public void CacheLicense(int userId, PortalLicenseDto license, string? username = null)
    {
        _cache.Save(new CachedLicenseSnapshot
        {
            UserId = userId,
            Username = username ?? string.Empty,
            LicenseKey = license.LicenseKey,
            LicenseType = license.LicenseType,
            ExpiryDate = license.ExpiryDate,
            IsActive = license.IsActive
        });
    }

    public CachedLicenseSnapshot? GetCachedLicense() => _cache.Load();

    public bool HasValidOfflineLicense()
    {
        var cached = _cache.Load();
        if (cached == null || !cached.IsActive)
        {
            return false;
        }

        if (cached.ExpiryDate < DateTime.Now)
        {
            return false;
        }

        return _cache.IsWithinGracePeriod() || cached.ExpiryDate > DateTime.Now;
    }

    private sealed class LicenseApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PortalLicenseDto? License { get; set; }
    }
}

public sealed class LicenseActivateResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public PortalLicenseDto? License { get; init; }
}

public sealed class PortalLicenseDto
{
    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int UserId { get; set; }
}
