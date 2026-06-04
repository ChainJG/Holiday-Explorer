using HolidayExplorer.Core.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace HolidayExplorer.Core.Services
{
    public sealed class HolidayCatalogueStorageService
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public string UserCataloguePath { get; }

        public string SeedCataloguePath { get; }

        public HolidayCatalogueStorageService()
        {
            string appDataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Holiday Explorer");

            Directory.CreateDirectory(appDataDirectory);

            UserCataloguePath = Path.Combine(appDataDirectory, "holidays.json");

            SeedCataloguePath = Path.Combine(
                AppContext.BaseDirectory,
                "Data",
                "holidays.seed.json");
        }

        public async Task<ObservableCollection<HolidayOption>> LoadAsync()
        {
            await EnsureUserCatalogueExistsAsync();

            Debug.WriteLine($"Holiday catalogue path: {UserCataloguePath}");
            Debug.WriteLine($"Seed catalogue path: {SeedCataloguePath}");

            string json = await File.ReadAllTextAsync(UserCataloguePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                await ResetUserCatalogueFromSeedAsync();
                json = await File.ReadAllTextAsync(UserCataloguePath);
            }

            ObservableCollection<HolidayOption>? holidays =
                JsonSerializer.Deserialize<ObservableCollection<HolidayOption>>(
                    json,
                    _jsonOptions);

            return holidays ?? [];
        }

        public async Task SaveAsync(IEnumerable<HolidayOption> holidays)
        {
            string? directory = Path.GetDirectoryName(UserCataloguePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(
                holidays,
                _jsonOptions);

            await File.WriteAllTextAsync(UserCataloguePath, json);
        }

        public async Task ResetUserCatalogueFromSeedAsync()
        {
            if (!File.Exists(SeedCataloguePath))
            {
                await SaveAsync([]);
                return;
            }

            string seedJson = await File.ReadAllTextAsync(SeedCataloguePath);

            if (string.IsNullOrWhiteSpace(seedJson))
            {
                await SaveAsync([]);
                return;
            }

            await File.WriteAllTextAsync(UserCataloguePath, seedJson);
        }

        private async Task EnsureUserCatalogueExistsAsync()
        {
            if (File.Exists(UserCataloguePath))
            {
                return;
            }

            await ResetUserCatalogueFromSeedAsync();
        }
    }
}