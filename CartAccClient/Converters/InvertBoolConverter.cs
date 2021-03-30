using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CartAccClient.Converters
{
    /// <summary>
    /// Инвентированный конвертер для булевых свойств.
    /// </summary>
    class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
