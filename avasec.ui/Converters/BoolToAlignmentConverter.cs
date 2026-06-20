using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AVASec.UI.Converters
{
    /// <summary>
    /// Bool to Alignment Converter - For chat bubbles alignment
    /// Bộ chuyển đổi Bool sang Căn chỉnh - Để căn chỉnh bong bóng chat
    /// </summary>
    public class BoolToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBot)
            {
                return isBot ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
