using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AVASec.Core.Models;

namespace AVASec.UI.Converters
{
    public class NotificationColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NotificationType type)
            {
                switch (type)
                {
                    case NotificationType.Success: return new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Green
                    case NotificationType.Warning: return new SolidColorBrush(Color.FromRgb(234, 179, 8)); // Yellow
                    case NotificationType.Error: return new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                    case NotificationType.Security: return new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                    case NotificationType.Update: return new SolidColorBrush(Color.FromRgb(6, 182, 212)); // Cyan
                    default: return new SolidColorBrush(Colors.White); // Info
                }
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
