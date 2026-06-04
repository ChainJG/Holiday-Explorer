using System;
using System.Collections.Generic;
using System.Text;

namespace HolidayExplorer.Core.Models
{
    public sealed class AirportOption
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool IsPreferredForCity { get; set; }
    }
}
