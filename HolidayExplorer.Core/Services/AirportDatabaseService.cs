using HolidayExplorer.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace HolidayExplorer.Core.Services
{
    public sealed class AirportDatabaseService
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _airportDatabasePath;

        public AirportDatabaseService()
        {
            _airportDatabasePath = Path.Combine(
                AppContext.BaseDirectory,
                "Data",
                "airports.json");
        }

        public async Task<IReadOnlyList<AirportOption>> LoadAirportsAsync()
        {
            if (!File.Exists(_airportDatabasePath))
            {
                return [];
            }

            string json = await File.ReadAllTextAsync(_airportDatabasePath);

            List<AirportOption>? airports =
                JsonSerializer.Deserialize<List<AirportOption>>(json, _jsonOptions);

            return airports ?? [];
        }

        public async Task<AirportOption?> FindNearestAirportAsync(
            double latitude,
            double longitude)
        {
            IReadOnlyList<AirportOption> airports = await LoadAirportsAsync();

            return airports
                .OrderBy(airport => CalculateDistanceKm(
                    latitude,
                    longitude,
                    airport.Latitude,
                    airport.Longitude))
                .FirstOrDefault();
        }

        public async Task<AirportOption?> FindBestAirportForHolidayAsync(HolidayOption holiday)
        {
            IReadOnlyList<AirportOption> airports = await LoadAirportsAsync();

            AirportOption? cityPreferredAirport = airports
                .Where(airport =>
                    airport.IsPreferredForCity &&
                    string.Equals(airport.City, holiday.Name, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (cityPreferredAirport is not null)
            {
                return cityPreferredAirport;
            }

            return airports
                .OrderBy(airport => CalculateDistanceKm(
                    holiday.Latitude,
                    holiday.Longitude,
                    airport.Latitude,
                    airport.Longitude))
                .FirstOrDefault();
        }

        private static double CalculateDistanceKm(
            double latitude1,
            double longitude1,
            double latitude2,
            double longitude2)
        {
            const double earthRadiusKm = 6371;

            double dLat = ToRadians(latitude2 - latitude1);
            double dLon = ToRadians(longitude2 - longitude1);

            double lat1 = ToRadians(latitude1);
            double lat2 = ToRadians(latitude2);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}