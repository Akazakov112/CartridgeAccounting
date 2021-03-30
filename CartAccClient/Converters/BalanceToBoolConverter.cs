﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CartAccClient.Converters
{
    class BalanceToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
