using System.Collections.ObjectModel;

namespace Holiday_Explorer.Core.Models
{
    public sealed class HolidayOption
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Country { get; init; } = string.Empty;

        public double Latitude { get; init; }

        public double Longitude { get; init; }

        public double Score { get; init; }

        public string FlightDuration { get; init; } = string.Empty;

        public string SummerTemperature { get; init; } = string.Empty;

        public string Verdict { get; init; } = string.Empty;

        public MoneyRange EstimatedFlightPriceForTwo { get; init; } = new();

        public ObservableCollection<AttractionOption> Attractions { get; init; } = [];
    }
}
