using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CartAccLibrary.Services;

namespace CartAccClient.Converters
{
    /// <summary>
    /// Конвертер типа сообщений лога в цветовое обозначение.
    /// </summary>
    class LogMessageTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LogMessageType messageType)
            {
                // Возврат цвета в зависимости от типа сообщения.
                switch (messageType)
                {
                    case LogMessageType.Notification:
                        return Brushes.White;
                    case LogMessageType.Warning:
                        return Brushes.DarkOrange;
                    case LogMessageType.Error:
                        return Brushes.Red;
                    default:
                        return Brushes.White;
                }
            }
            else
            {
                return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
