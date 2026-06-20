using System;
using System.Threading.Tasks;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;
using AVASec.Authentication.Services;
using AVASec.Core.Services;
using AVASec.Database;

namespace AVASec.UI.Services
{
    public class HybridAuthenticationService : IAuthenticationService
    {
        private readonly WebAuthenticationService _webService;
        private readonly AuthenticationService _localService;
        private const int TimeoutMilliseconds = 5000; // 5 second timeout for web requests

        public HybridAuthenticationService(AVASecContext context)
        {
            _webService = new WebAuthenticationService();
            _localService = new AuthenticationService(context);
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string username, string password, string email)
        {
            try
            {
                // Try web service with timeout
                var webTask = _webService.RegisterAsync(username, password, email);
                var timeoutTask = Task.Delay(TimeoutMilliseconds);
                var completedTask = await Task.WhenAny(webTask, timeoutTask);

                if (completedTask == webTask)
                {
                    var webResult = await webTask;
                    if (webResult.Success) return webResult;
                }
                else
                {
                    // Timeout occurred
                    Console.WriteLine("[HybridAuth] Web registration timeout - falling back to local");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HybridAuth] Web registration error: {ex.Message} - falling back to local");
            }
            
            // Fallback to local
            return await _localService.RegisterAsync(username, password, email);
        }

        public async Task<(bool Success, int? UserId, string Message)> LoginAsync(string username, string password)
        {
            try
            {
                var webTask = _webService.LoginAsync(username, password);
                var timeoutTask = Task.Delay(TimeoutMilliseconds);
                var completedTask = await Task.WhenAny(webTask, timeoutTask);

                if (completedTask == webTask)
                {
                    var webResult = await webTask;
                    if (webResult.Success)
                    {
                        return webResult;
                    }

                    if (webResult.Message.Contains("API Connection Error", StringComparison.OrdinalIgnoreCase) ||
                        webResult.Message.Contains("Lỗi kết nối", StringComparison.OrdinalIgnoreCase))
                    {
                        return await TryOfflineLoginAsync(username, password);
                    }

                    return webResult;
                }

                return await TryOfflineLoginAsync(username, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HybridAuth] Web login error: {ex.Message}");
                return await TryOfflineLoginAsync(username, password);
            }
        }

        private async Task<(bool Success, int? UserId, string Message)> TryOfflineLoginAsync(string username, string password)
        {
            var localResult = await _localService.LoginAsync(username, password);
            if (localResult.Success)
            {
                return localResult;
            }

            var cache = new LicenseCacheService().Load();
            if (cache != null && cache.Username == username && cache.IsActive && cache.ExpiryDate > DateTime.Now)
            {
                return (true, cache.UserId,
                    $"Offline mode — license cached / Chế độ offline — key đã lưu ({cache.LicenseType})");
            }

            return localResult;
        }

        public async Task<User?> GetUserAsync(int userId)
        {
            try
            {
                // Try Web first with timeout
                var webTask = _webService.GetUserAsync(userId);
                var timeoutTask = Task.Delay(TimeoutMilliseconds);
                var completedTask = await Task.WhenAny(webTask, timeoutTask);

                if (completedTask == webTask)
                {
                    var webUser = await webTask;
                    if (webUser != null) return webUser;
                }
                else
                {
                    Console.WriteLine("[HybridAuth] GetUser timeout - using local cache");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HybridAuth] GetUser error: {ex.Message} - using local cache");
            }

            // Fallback to local
            return await _localService.GetUserAsync(userId);
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            // Password changes are local only for now
            // TODO: Future enhancement - sync password changes to web service
            return await _localService.ChangePasswordAsync(userId, oldPassword, newPassword);
        }
    }
}
