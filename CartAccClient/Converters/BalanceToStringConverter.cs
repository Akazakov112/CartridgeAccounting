using System;
using System.Globalization;
using System.Windows.Data;

namespace CartAccClient.Converters
{
    class BalanceToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return "1";
            }
            else if (!int.TryParse(value.ToString(), out _))
            {
                return "1";
            }
            else if (value.ToString() == "0")
            {
                return "1";
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
