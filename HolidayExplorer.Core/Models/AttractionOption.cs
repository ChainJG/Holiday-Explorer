using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HolidayExplorer.Core.Models
{
    public sealed class AttractionOption : INotifyPropertyChanged
    {
        private string? _imagePath;
        private bool _isReplacingImage;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id { get; set; } = string.Empty;

        public string HolidayId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string? ImagePath
        {
            get => _imagePath;
            set
            {
                if (_imagePath == value)
                {
                    return;
                }

                _imagePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasImage));
            }
        }

        public string? ImageSourceUrl { get; set; }

        public string? ImageProvider { get; set; }

        public string? ImageCredit { get; set; }

        public bool IsReplacingImage
        {
            get => _isReplacingImage;
            set
            {
                if (_isReplacingImage == value)
                {
                    return;
                }

                _isReplacingImage = value;
                OnPropertyChanged();
            }
        }

        public bool HasImage => !string.IsNullOrWhiteSpace(ImagePath);

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}