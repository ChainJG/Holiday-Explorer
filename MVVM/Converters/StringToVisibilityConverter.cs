using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Holiday_Explorer.MVVM.Converters
{
    public sealed class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value?.ToString())
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value is Visibility.Visible;
        }
    }
}