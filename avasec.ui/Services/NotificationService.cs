using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AVASec.Core.Models;

namespace AVASec.UI.Services
{
    public class NotificationModel
    {
        public int NotificationID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info";
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public bool IsPromotional { get; set; }
    }

    /// <summary>
    /// Local notification center — tips and alerts stored on device only.
    /// </summary>
    public class NotificationService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
        private readonly string _storePath;
        private List<NotificationModel> _items = new();
        private int _nextId = 1;
        private bool _seeded;

        public NotificationService()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                BrandConstants.AppDataFolder);
            Directory.CreateDirectory(dir);
            _storePath = Path.Combine(dir, "notifications.json");
            LoadFromDisk();
        }

        public Task<List<NotificationModel>> CheckNotificationsAsync(int userId)
        {
            EnsureWelcomeTips();
            return Task.FromResult(_items.Where(n => !n.IsRead).OrderByDescending(n => n.CreatedAt).ToList());
        }

        public Task MarkAsReadAsync(int notificationId)
        {
            var item = _items.FirstOrDefault(n => n.NotificationID == notificationId);
            if (item != null)
            {
                item.IsRead = true;
                SaveToDisk();
            }

            return Task.CompletedTask;
        }

        public void AddLocalTip(string title, string message, string type = "info")
        {
            _items.Insert(0, new NotificationModel
            {
                NotificationID = _nextId++,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.Now,
                IsRead = false
            });
            SaveToDisk();
        }

        private void EnsureWelcomeTips()
        {
            if (_seeded || _items.Count > 0)
            {
                return;
            }

            _seeded = true;
            _items.Add(new NotificationModel
            {
                NotificationID = _nextId++,
                Title = $"Chào mừng {BrandConstants.ProductNameFull}!",
                Message = "Nhấn Smart Scan để quét AI + dọn rác + tối ưu RAM.\nTap Smart Scan for AI scan + cleanup + RAM boost.",
                Type = "welcome",
                CreatedAt = DateTime.Now
            });
            _items.Add(new NotificationModel
            {
                NotificationID = _nextId++,
                Title = "Offline-first / Hoạt động offline",
                Message = "Dữ liệu quét lưu trên máy. Cổng web chỉ dùng cho đăng ký & key.\nScan data stays local. Web portal is for account & license only.",
                Type = "info",
                CreatedAt = DateTime.Now
            });
            SaveToDisk();
        }

        private void LoadFromDisk()
        {
            try
            {
                if (!File.Exists(_storePath))
                {
                    return;
                }

                var json = File.ReadAllText(_storePath);
                var loaded = JsonSerializer.Deserialize<List<NotificationModel>>(json);
                if (loaded != null)
                {
                    _items = loaded;
                    _nextId = _items.Count == 0 ? 1 : _items.Max(n => n.NotificationID) + 1;
                }
            }
            catch
            {
                _items = new List<NotificationModel>();
            }
        }

        private void SaveToDisk()
        {
            try
            {
                File.WriteAllText(_storePath, JsonSerializer.Serialize(_items, JsonOptions));
            }
            catch
            {
                // Non-critical
            }
        }
    }
}
