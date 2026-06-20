using System;
using System.Globalization;
using System.Windows.Data;

namespace AVASec.UI.Converters
{
    /// <summary>
    /// Progress to Width Converter - Calculates progress indicator width
    /// Bộ chuyển đổi Tiến trình sang Chiều rộng - Tính toán chiều rộng của chỉ báo tiến trình
    /// </summary>
    public class ProgressToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3) return 0.0;

            double value = (double)values[0];
            double maximum = (double)values[1];
            double actualWidth = (double)values[2];

            if (maximum <= 0) return 0.0;

            return (value / maximum) * actualWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
