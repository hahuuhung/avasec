using System;

namespace AVASec.Core.Models
{
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Security,
        Update
    }

    public class AppNotification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public string? ActionLabel { get; set; }
        public Action? Action { get; set; }
        
        // Helper for UI binding
        public string TimeAgo => GetTimeAgo(Timestamp);

        private string GetTimeAgo(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            return dt.ToString("dd/MM");
        }
    }
}
