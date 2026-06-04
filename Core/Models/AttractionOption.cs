using Holiday_Explorer.MVVM.Core;

namespace Holiday_Explorer.Core.Models
{
    public sealed class AttractionOption : ObservableObject
    {
        private string? _imagePath;

        public string Id { get; init; } = string.Empty;

        public string HolidayId { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public double Latitude { get; init; }

        public double Longitude { get; init; }

        private bool _isReplacingImage;

        public bool IsReplacingImage
        {
            get => _isReplacingImage;
            set => SetProperty(ref _isReplacingImage, value);
        }

        public string? ImagePath
        {
            get => _imagePath;
            set
            {
                if (SetProperty(ref _imagePath, value))
                {
                    OnPropertyChanged(nameof(HasImage));
                }
            }
        }

        public string? ImageSourceUrl { get; set; }

        public string? ImageProvider { get; set; }

        public string? ImageCredit { get; set; }

        public bool HasImage => !string.IsNullOrWhiteSpace(ImagePath);
    }
}