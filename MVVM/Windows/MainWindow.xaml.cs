using Holiday_Explorer.Core;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Holiday_Explorer.MVVM.Windows
{
    public partial class MainWindow : Window
    {
        #region Application Functions
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWind, int wMsg, int wParam, int lParam);
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void BtnMinimiseWindow_Click(object sender, RoutedEventArgs e) =>
            this.WindowState = WindowState.Minimized;

        private void BtnExitApplication_Click(object sender, RoutedEventArgs e) =>
            HolidayExplorerServices.Shutdown();
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Loaded += HolidayMapView_Loaded;
        }

        private async void HolidayMapView_Loaded(object sender, RoutedEventArgs e)
        {
            await HolidayMapWebView.EnsureCoreWebView2Async();

            HolidayMapWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            string mapPath = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Map",
                "map.html");

            HolidayMapWebView.Source = new Uri(mapPath);

            HolidayMapWebView.NavigationCompleted += async (_, _) =>
            {
                await SendHolidayMarkersToMapAsync();
            };
        }

        private async Task SendHolidayMarkersToMapAsync()
        {
            var holidays = new[]
            {
                new
                {
                    id = "barcelona",
                    name = "Barcelona",
                    country = "Spain",
                    latitude = 41.3874,
                    longitude = 2.1686,
                    score = 9.0,
                    flightDuration = "2h 20m"
                },
                new
                {
                    id = "paris",
                    name = "Paris",
                    country = "France",
                    latitude = 48.8566,
                    longitude = 2.3522,
                    score = 8.5,
                    flightDuration = "1h 20m"
                },
                new
                {
                    id = "dubai",
                    name = "Dubai",
                    country = "United Arab Emirates",
                    latitude = 25.2048,
                    longitude = 55.2708,
                    score = 7.0,
                    flightDuration = "7h"
                }
            };

            string json = JsonSerializer.Serialize(holidays);

            string script = $"loadHolidayMarkers({json});";

            await HolidayMapWebView.CoreWebView2.ExecuteScriptAsync(script);
        }

        private void CoreWebView2_WebMessageReceived(
            object? sender,
            CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.WebMessageAsJson;

            using JsonDocument document = JsonDocument.Parse(json);

            string? messageType = document.RootElement.GetProperty("type").GetString();

            if (messageType != "holiday-selected")
            {
                return;
            }

            string? holidayId = document.RootElement.GetProperty("holidayId").GetString();

            MessageBox.Show($"Selected holiday: {holidayId}");
        }
    }
}