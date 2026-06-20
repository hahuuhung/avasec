using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AVASec.UI.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            bool invert = parameter?.ToString()?.ToLower() == "invert";

            if (invert)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            bool boolValue = visibility == Visibility.Visible;
            bool invert = parameter?.ToString()?.ToLower() == "invert";

            if (invert)
                boolValue = !boolValue;

            return boolValue;
        }
    }
}
