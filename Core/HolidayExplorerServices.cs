namespace Holiday_Explorer.Core
{
    public class HolidayExplorerServices
    {
        public static void Shutdown() =>
            System.Windows.Application.Current?.Shutdown();
    }
}
