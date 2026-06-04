using HolidayExplorer.Core.Services;
using Holiday_Explorer.MVVM.Core;
using System.Collections.ObjectModel;
using System.Diagnostics;
using HolidayExplorer.Core.Models;

namespace Holiday_Explorer.MVVM.ViewModels
{
    public sealed class MainWindowViewModel : ObservableObject
    {
        private readonly HolidayCatalogueStorageService _catalogueStorageService;

        private HolidayOption? _selectedHoliday;
        private AttractionOption? _hoveredAttraction;

        private ObservableCollection<HolidayOption> _holidays = [];
        public ObservableCollection<HolidayOption> Holidays
        {
            get => _holidays;
            set
            {
                if (SetProperty(ref _holidays, value))
                {

                }
            }
        }

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

        public MainWindowViewModel(HolidayCatalogueStorageService catalogueStorageService)
        {
            _catalogueStorageService = catalogueStorageService;
        }

        public async Task LoadAsync()
        {
            Holidays.Clear();

            ObservableCollection<HolidayOption> loadedHolidays =
                await _catalogueStorageService.LoadAsync();

            Debug.WriteLine($"Loaded {loadedHolidays.Count} holidays");
            foreach (HolidayOption holiday in loadedHolidays)
            {
                Holidays.Add(holiday);
            }

            SelectedHoliday = Holidays.FirstOrDefault();
        }

        public Task SaveAsync()
        {
            return _catalogueStorageService.SaveAsync(Holidays);
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
    }
}