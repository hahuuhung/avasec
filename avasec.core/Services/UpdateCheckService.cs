using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AVASec.Core.Services
{
    /// <summary>
    /// Service to check for application updates
    /// Dịch vụ kiểm tra cập nhật ứng dụng
    /// </summary>
    public class UpdateCheckService
    {
        private readonly HttpClient _httpClient;
        private const string DefaultUpdateUrl = "https://sysanti.com/api/version";

        public UpdateCheckService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        /// <summary>
        /// Check for updates / Kiểm tra cập nhật (local-only — no remote call)
        /// </summary>
        public Task<UpdateCheckResult> CheckForUpdatesAsync(string currentVersion)
        {
            return Task.FromResult(new UpdateCheckResult
            {
                IsUpdateAvailable = false,
                CurrentVersion = currentVersion,
                LatestVersion = currentVersion
            });
        }
    }

    /// <summary>
    /// Update check result / Kết quả kiểm tra cập nhật
    /// </summary>
    public class UpdateCheckResult
    {
        public bool IsUpdateAvailable { get; set; }
        public string CurrentVersion { get; set; } = string.Empty;
        public string LatestVersion { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public bool CheckFailed { get; set; }
    }

    /// <summary>
    /// Version API response model / Model phản hồi API phiên bản
    /// </summary>
    internal class VersionResponse
    {
        public string LatestVersion { get; set; } = string.Empty;
        public string? DownloadUrl { get; set; }
        public string? ReleaseNotes { get; set; }
    }
}
