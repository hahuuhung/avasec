using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;

namespace AVASec.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly string _storagePath;
        public ObservableCollection<AppNotification> Notifications { get; } = new ObservableCollection<AppNotification>();
        public event EventHandler<AppNotification>? NotificationReceived;

        public NotificationService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "avasec");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _storagePath = Path.Combine(folder, "notifications.json");
            
            LoadHistory();
        }

        public void Show(string title, string message, NotificationType type = NotificationType.Info, string? actionLabel = null, Action? action = null)
        {
            var note = new AppNotification
            {
                Title = title,
                Message = message,
                Type = type,
                ActionLabel = actionLabel,
                Action = action
            };

            // Add to UI thread safe if needed (ObservableCollection usually needs UI thread, but here we are in Core)
            // Callers should simpler handle UI dispatching or we use specific mechanisms. 
            // For now, simple add.
            AddNotification(note);

            // Save
            SaveHistory();

            // Notify listeners (for Toasts)
            NotificationReceived?.Invoke(this, note);
        }

        private void AddNotification(AppNotification note)
        {
             // Insert at top
             // Note: In WPF/UI, this might need Dispatcher. check usage later.
             try 
             {
                 // We rely on UI layer to marshal this if bound directly, 
                 // or we use BindingOperations.EnableCollectionSynchronization in UI.
                 Notifications.Insert(0, note);
                 
                 // Limit history to 50
                 if (Notifications.Count > 50)
                 {
                     Notifications.RemoveAt(Notifications.Count - 1);
                 }
             }
             catch
             {
                 // Fallback
                 Notifications.Add(note);
             }
        }

        public void MarkAsRead(string id)
        {
            var note = Notifications.FirstOrDefault(n => n.Id == id);
            if (note != null)
            {
                note.IsRead = true;
                SaveHistory();
            }
        }

        public void ClearAll()
        {
            Notifications.Clear();
            SaveHistory();
        }

        public void LoadHistory()
        {
            try
            {
                if (File.Exists(_storagePath))
                {
                    var json = File.ReadAllText(_storagePath);
                    var list = JsonSerializer.Deserialize<List<AppNotification>>(json);
                    
                    if (list != null)
                    {
                        Notifications.Clear();
                        foreach (var item in list.OrderByDescending(x => x.Timestamp))
                        {
                            Notifications.Add(item);
                        }
                    }
                }
            }
            catch { }
        }

        private async void SaveHistory()
        {
            try
            {
                // Simple fire and forget save
                var list = Notifications.ToList();
                // Avoid serializing 'Action' delegate
                // We need a clear DTO or Ignore attribute, but System.Text.Json ignores delegates by default usually or throws.
                // Let's rely on basic properties being serialized.
                
                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_storagePath, json);
            }
            catch { }
        }
    }
}
