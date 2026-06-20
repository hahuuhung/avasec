using System;
using System.Collections.ObjectModel;
using AVASec.Core.Models;

namespace AVASec.Core.Interfaces
{
    public interface INotificationService
    {
        ObservableCollection<AppNotification> Notifications { get; }
        event EventHandler<AppNotification> NotificationReceived;
        
        void Show(string title, string message, NotificationType type = NotificationType.Info, string? actionLabel = null, Action? action = null);
        void MarkAsRead(string id);
        void ClearAll();
        void LoadHistory();
    }
}
