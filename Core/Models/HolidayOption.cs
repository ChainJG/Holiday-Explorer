using System.Collections.ObjectModel;

namespace Holiday_Explorer.Core.Models
{
    public sealed class HolidayOption
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Score { get; set; }

        public string FlightDuration { get; set; } = string.Empty;

        public string SummerTemperature { get; set; } = string.Empty;

        public string Verdict { get; set; } = string.Empty;

        public MoneyRange EstimatedFlightPriceForTwo { get; set; } = new();

        public ObservableCollection<AttractionOption> Attractions { get; set; } = [];
    }
}
