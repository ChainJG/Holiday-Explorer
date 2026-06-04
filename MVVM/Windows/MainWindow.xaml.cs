using HolidayExplorer.Core.Services;
using HolidayExplorer.Core.Models;
using Holiday_Explorer.MVVM.ViewModels;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Holiday_Explorer.Core;

namespace Holiday_Explorer.MVVM.Windows
{
    public partial class MainWindow : Window
    {
        private readonly HolidayCatalogueStorageService _catalogueStorageService = new();
        private readonly AttractionImageStorageService _imageStorageService = new();
        private readonly AttractionImageLookupService _attractionImageLookupService;
        private readonly MainWindowViewModel _viewModel;

        private readonly HashSet<string> _holidaysCurrentlyLoadingImages = [];
        private bool _hasLoadedMap;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWind, int wMsg, int wParam, int lParam);

        public MainWindow()
        {
            _attractionImageLookupService = new AttractionImageLookupService(_imageStorageService);
            _viewModel = new MainWindowViewModel(_catalogueStorageService);

            InitializeComponent();

            DataContext = _viewModel;
            Loaded += MainWindow_Loaded;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void BtnMinimiseWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnExitApplication_Click(object sender, RoutedEventArgs e)
        {
            HolidayExplorerServices.Shutdown();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_hasLoadedMap)
            {
                return;
            }

            _hasLoadedMap = true;

            await _viewModel.LoadAsync();

            await HolidayMapWebView.EnsureCoreWebView2Async();

            HolidayMapWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            HolidayMapWebView.NavigationCompleted += HolidayMapWebView_NavigationCompleted;

            string mapPath = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Map",
                "map.html");

            if (!File.Exists(mapPath))
            {
                MessageBox.Show(
                    $"Map file could not be found:{Environment.NewLine}{mapPath}",
                    "Holiday Explorer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            HolidayMapWebView.Source = new Uri(mapPath);
        }

        private async void HolidayMapWebView_NavigationCompleted(
            object? sender,
            CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                MessageBox.Show(
                    "The holiday map failed to load.",
                    "Holiday Explorer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            await SendHolidayMarkersToMapAsync();
        }

        private async Task SendHolidayMarkersToMapAsync()
        {
            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string json = JsonSerializer.Serialize(_viewModel.Holidays, options);
            string script = $"loadHolidayMarkers({json});";

            await HolidayMapWebView.CoreWebView2.ExecuteScriptAsync(script);
        }

        private async void CoreWebView2_WebMessageReceived(
            object? sender,
            CoreWebView2WebMessageReceivedEventArgs e)
        {
            using JsonDocument document = JsonDocument.Parse(e.WebMessageAsJson);

            if (!document.RootElement.TryGetProperty("type", out JsonElement typeElement))
            {
                return;
            }

            string? messageType = typeElement.GetString();

            switch (messageType)
            {
                case "holiday-selected":
                    await HandleHolidaySelectedAsync(document);
                    break;

                case "attraction-hovered":
                    HandleAttractionHovered(document);
                    break;

                case "attraction-hover-ended":
                    _viewModel.HoveredAttraction = null;
                    break;
            }
        }

        private async Task HandleHolidaySelectedAsync(JsonDocument document)
        {
            if (!document.RootElement.TryGetProperty("holidayId", out JsonElement holidayIdElement))
            {
                return;
            }

            string? holidayId = holidayIdElement.GetString();

            if (string.IsNullOrWhiteSpace(holidayId))
            {
                return;
            }

            _viewModel.SelectHoliday(holidayId);

            string escapedHolidayId = JsonSerializer.Serialize(holidayId);
            await HolidayMapWebView.CoreWebView2.ExecuteScriptAsync($"selectHolidayById({escapedHolidayId});");

            await AutoFindMissingImagesForSelectedHolidayAsync();
        }

        private void HandleAttractionHovered(JsonDocument document)
        {
            if (!document.RootElement.TryGetProperty("attractionId", out JsonElement attractionIdElement))
            {
                return;
            }

            string? attractionId = attractionIdElement.GetString();

            if (string.IsNullOrWhiteSpace(attractionId))
            {
                return;
            }

            _viewModel.HoverAttraction(attractionId);
        }

        private async Task AutoFindMissingImagesForSelectedHolidayAsync()
        {
            if (_viewModel.SelectedHoliday is null)
            {
                return;
            }

            string holidayId = _viewModel.SelectedHoliday.Id;

            if (!_holidaysCurrentlyLoadingImages.Add(holidayId))
            {
                return;
            }

            try
            {
                bool hasDownloadedAnyImage = false;

                foreach (AttractionOption attraction in _viewModel.SelectedHoliday.Attractions)
                {
                    if (HasUsableImage(attraction))
                    {
                        continue;
                    }

                    try
                    {
                        bool foundImage = await _attractionImageLookupService.TryAutoFillImageAsync(
                            _viewModel.SelectedHoliday,
                            attraction);

                        if (foundImage)
                        {
                            hasDownloadedAnyImage = true;
                        }
                    }
                    catch
                    {
                        // Keep selection smooth even if one image lookup fails.
                    }
                }

                if (hasDownloadedAnyImage)
                {
                    await _viewModel.SaveAsync();
                }
            }
            finally
            {
                _holidaysCurrentlyLoadingImages.Remove(holidayId);
            }
        }

        private async void RetryAttractionImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { Tag: AttractionOption attraction })
            {
                return;
            }

            if (_viewModel.SelectedHoliday is null)
            {
                return;
            }

            if (attraction.IsReplacingImage)
            {
                return;
            }

            try
            {
                attraction.IsReplacingImage = true;

                bool foundImage = await _attractionImageLookupService.TryAutoFillImageAsync(
                    _viewModel.SelectedHoliday,
                    attraction);

                if (!foundImage)
                {
                    MessageBox.Show(
                        "No replacement image was found for this attraction.",
                        "Holiday Explorer",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                await _viewModel.SaveAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Unable to replace attraction image",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            finally
            {
                attraction.IsReplacingImage = false;
            }
        }

        private static bool HasUsableImage(AttractionOption attraction)
        {
            return !string.IsNullOrWhiteSpace(attraction.ImagePath)
                   && File.Exists(attraction.ImagePath);
        }

        private void AttractionImage_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;

            e.Handled = true;
        }

        private async void AttractionImage_Drop(object sender, DragEventArgs e)
        {
            if (sender is not FrameworkElement { Tag: AttractionOption attraction })
            {
                return;
            }

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length == 0)
            {
                return;
            }

            try
            {
                attraction.ImagePath = _imageStorageService.SaveAttractionImage(
                    files[0],
                    attraction.Id);

                await _viewModel.SaveAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Unable to add attraction image",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}