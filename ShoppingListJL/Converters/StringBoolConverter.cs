using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ShoppingListJL.Converters
{
    public class StringBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (bool.TryParse(s, out bool b)) return b;
                return s == "1" || s.Equals("yes", StringComparison.OrdinalIgnoreCase);
            }
            if (value is bool bb) return bb;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return b.ToString();
            return "False";
        }
    }
}