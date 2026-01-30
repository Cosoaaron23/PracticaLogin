using System;
using System.Globalization;
using System.Windows.Data;

namespace PracticaLogin
{
    public class PrecioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal precio) return (precio == 0) ? "GRATIS" : precio.ToString("C", culture);
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}