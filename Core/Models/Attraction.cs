namespace Holiday_Explorer.Core.Models
{
    public sealed class Attraction
    {
        public required string Name { get; init; }

        public required string Description { get; init; }

        public required string ImagePath { get; init; }

        public string? ImageCredit { get; init; }

        public double? Latitude { get; init; }

        public double? Longitude { get; init; }
    }
}
