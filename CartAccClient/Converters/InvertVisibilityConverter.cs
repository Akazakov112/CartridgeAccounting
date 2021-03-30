using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CartAccClient.Converters
{
    /// <summary>
    /// Конвертер инвертирования значений Visibility.
    /// </summary>
    class InvertVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
