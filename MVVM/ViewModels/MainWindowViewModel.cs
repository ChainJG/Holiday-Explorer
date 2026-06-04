using Holiday_Explorer.Core.Models;
using Holiday_Explorer.MVVM.Core;
using System.Collections.ObjectModel;

namespace Holiday_Explorer.MVVM.ViewModels
{
    public sealed class MainWindowViewModel : ObservableObject
    {
        private HolidayOption? _selectedHoliday;
        private AttractionOption? _hoveredAttraction;

        public ObservableCollection<HolidayOption> Holidays { get; } = [];

        public HolidayOption? SelectedHoliday
        {
            get => _selectedHoliday;
            set => SetProperty(ref _selectedHoliday, value);
        }

        public AttractionOption? HoveredAttraction
        {
            get => _hoveredAttraction;
            set => SetProperty(ref _hoveredAttraction, value);
        }

        public MainWindowViewModel()
        {
            LoadSampleData();
            SelectedHoliday = Holidays.FirstOrDefault();
        }

        public void SelectHoliday(string holidayId)
        {
            SelectedHoliday = Holidays.FirstOrDefault(x => x.Id == holidayId);
            HoveredAttraction = null;
        }

        public void HoverAttraction(string attractionId)
        {
            HoveredAttraction = SelectedHoliday?
                .Attractions
                .FirstOrDefault(x => x.Id == attractionId);
        }

        private void LoadSampleData()
        {
            Holidays.Add(new HolidayOption
            {
                Id = "barcelona",
                Name = "Barcelona",
                Country = "Spain",
                Latitude = 41.3874,
                Longitude = 2.1686,
                Score = 9.0,
                FlightDuration = "2h 20m",
                SummerTemperature = "26–28°C",
                Verdict = "Best balance of price, landmarks, weather, and beach.",
                EstimatedFlightPriceForTwo = new MoneyRange
                {
                    Minimum = 58,
                    Maximum = 74,
                    CurrencySymbol = "£"
                },
                Attractions =
                [
                    new AttractionOption
                    {
                        Id = "barcelona-sagrada-familia",
                        HolidayId = "barcelona",
                        Name = "Sagrada Família",
                        Description = "Barcelona's most famous basilica and one of the city's biggest landmarks.",
                        Latitude = 41.4036,
                        Longitude = 2.1744
                    },
                    new AttractionOption
                    {
                        Id = "barcelona-park-guell",
                        HolidayId = "barcelona",
                        Name = "Park Güell",
                        Description = "A colourful public park designed by Antoni Gaudí.",
                        Latitude = 41.4145,
                        Longitude = 2.1527
                    }
                ]
            });

            Holidays.Add(new HolidayOption
            {
                Id = "paris",
                Name = "Paris",
                Country = "France",
                Latitude = 48.8566,
                Longitude = 2.3522,
                Score = 8.5,
                FlightDuration = "1h 20m",
                SummerTemperature = "23–25°C",
                Verdict = "Easy iconic city break with world-famous landmarks.",
                EstimatedFlightPriceForTwo = new MoneyRange
                {
                    Minimum = 56,
                    Maximum = 106,
                    CurrencySymbol = "£"
                },
                Attractions =
                [
                    new AttractionOption
                    {
                        Id = "paris-eiffel-tower",
                        HolidayId = "paris",
                        Name = "Eiffel Tower",
                        Description = "The landmark symbol of Paris and one of the most recognisable structures in the world.",
                        Latitude = 48.8584,
                        Longitude = 2.2945
                    },
                    new AttractionOption
                    {
                        Id = "paris-louvre",
                        HolidayId = "paris",
                        Name = "Louvre Museum",
                        Description = "One of the world's largest and most famous museums.",
                        Latitude = 48.8606,
                        Longitude = 2.3376
                    }
                ]
            });

            Holidays.Add(new HolidayOption
            {
                Id = "dubai",
                Name = "Dubai",
                Country = "United Arab Emirates",
                Latitude = 25.2048,
                Longitude = 55.2708,
                Score = 7.0,
                FlightDuration = "7h",
                SummerTemperature = "41°C+",
                Verdict = "Massive modern wow factor, but summer heat is extreme.",
                EstimatedFlightPriceForTwo = new MoneyRange
                {
                    Minimum = 492,
                    Maximum = 616,
                    CurrencySymbol = "£"
                },
                Attractions =
                [
                    new AttractionOption
                    {
                        Id = "dubai-burj-khalifa",
                        HolidayId = "dubai",
                        Name = "Burj Khalifa",
                        Description = "The world's tallest building and Dubai's most famous landmark.",
                        Latitude = 25.1972,
                        Longitude = 55.2744
                    },
                    new AttractionOption
                    {
                        Id = "dubai-palm-jumeirah",
                        HolidayId = "dubai",
                        Name = "Palm Jumeirah",
                        Description = "Dubai's famous palm-shaped island with hotels, beaches, and views.",
                        Latitude = 25.1124,
                        Longitude = 55.1390
                    }
                ]
            });
        }
    }
}