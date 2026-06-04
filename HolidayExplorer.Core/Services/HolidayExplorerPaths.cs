using System.IO;

namespace HolidayExplorer.Core.Services
{
    public static class HolidayExplorerPaths
    {
        public static string AppDataDirectory =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Holiday Explorer");

        public static string AttractionImagesDirectory =>
            Path.Combine(AppDataDirectory, "Attraction Images");

        public static string UserCataloguePath =>
            Path.Combine(AppDataDirectory, "holidays.json");
    }
}