using System.Globalization;
using System.Windows.Data;

namespace Holiday_Explorer.MVVM.Converters
{
    public sealed class ToolTipCenterOffsetConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (values.Length < 2)
                return 0d;

            if (values[0] is not double targetWidth)
                return 0d;

            if (values[1] is not double toolTipWidth)
                return 0d;

            if (targetWidth <= 0 || toolTipWidth <= 0)
                return 0d;

            return (targetWidth - toolTipWidth) / 2d;
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}