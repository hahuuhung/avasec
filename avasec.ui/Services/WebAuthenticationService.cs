using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;

namespace AVASec.UI.Services;

/// <summary>
/// Web portal auth — register / login / fetch license from website API.
/// </summary>
public class WebAuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public WebAuthenticationService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(PortalConfig.ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task<(bool Success, string Message)> RegisterAsync(string username, string password, string email)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", new { username, password, email });
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
            if (response.IsSuccessStatusCode && result?.Success == true)
            {
                return (true, result.Message);
            }

            return (false, result?.Message ?? "Registration failed / Đăng ký thất bại");
        }
        catch (Exception ex)
        {
            return (false, $"API Connection Error / Lỗi kết nối: {ex.Message}");
        }
    }

    public async Task<(bool Success, int? UserId, string Message)> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new { username, password });
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
            if (response.IsSuccessStatusCode && result?.Success == true && result.UserId.HasValue)
            {
                if (result.License != null)
                {
                    await LicensePortalSync.SyncUserLicenseToLocalAsync(new User
                    {
                        UserId = result.UserId.Value,
                        Username = result.Username ?? username,
                        License = MapLicense(result.License, result.UserId.Value)
                    });
                }

                return (true, result.UserId, result.Message);
            }

            return (false, null, result?.Message ?? "Login failed / Đăng nhập thất bại");
        }
        catch (Exception ex)
        {
            return (false, null, $"API Connection Error / Lỗi kết nối: {ex.Message}");
        }
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/auth/user/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
            if (result?.Data != null)
            {
                await LicensePortalSync.SyncUserLicenseToLocalAsync(result.Data);
            }

            return result?.Data;
        }
        catch
        {
            return null;
        }
    }

    public Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        return Task.FromResult((false, "Change password on website / Đổi mật khẩu trên website"));
    }

    private static License MapLicense(PortalLicensePayload p, int userId) => new()
    {
        UserId = userId,
        LicenseKey = p.LicenseKey,
        LicenseType = p.LicenseType,
        ExpiryDate = p.ExpiryDate,
        IsActive = p.IsActive
    };

    private sealed class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    private sealed class LoginResponse
    {
        public bool Success { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string Message { get; set; } = string.Empty;
        public PortalLicensePayload? License { get; set; }
    }

    private sealed class UserResponse
    {
        public bool Success { get; set; }
        public User? Data { get; set; }
    }

    private sealed class PortalLicensePayload
    {
        public string LicenseKey { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
