using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Holiday_Explorer.MVVM.Converters
{
    public sealed class ImagePathToImageSourceConverter : IValueConverter
    {
        public object? Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            string? imagePath = value?.ToString();

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }

            if (!File.Exists(imagePath))
            {
                return null;
            }

            BitmapImage bitmapImage = new();

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        public object? ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}