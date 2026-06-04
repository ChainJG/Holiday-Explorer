namespace Holiday_Explorer.Core.Models
{
    public sealed class HolidayOption
    {
        public required string Id { get; init; }

        public required string Name { get; init; }

        public required string Country { get; init; }

        public required double Latitude { get; init; }

        public required double Longitude { get; init; }

        public required MoneyRange EstimatedFlightPriceForTwo { get; init; }

        public required string FlightDuration { get; init; }

        public required string SummerTemperature { get; init; }

        public required double Score { get; init; }

        public required string Verdict { get; init; }

        public List<string> Tags { get; init; } = [];

        public List<Attraction> Attractions { get; init; } = [];
    }
}
