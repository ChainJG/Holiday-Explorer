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

        public bool HasImage => !string.IsNullOrWhiteSpace(ImagePath);
    }
}