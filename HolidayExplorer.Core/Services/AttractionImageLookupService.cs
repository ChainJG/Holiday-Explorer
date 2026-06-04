using Holiday_Explorer.HolidayExplorer.Core.Infrastructure.Http;
using HolidayExplorer.Core.Models;
using HolidayExplorer.Core.Services;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace HolidayExplorer.Core.Services
{
    public sealed class AttractionImageLookupService
    {
        private const int PreferredImageWidth = 1400;

        private readonly HttpClient _httpClient;
        private readonly AttractionImageStorageService _imageStorageService;

        public AttractionImageLookupService(
            AttractionImageStorageService imageStorageService,
            HttpClient? httpClient = null)
        {
            _imageStorageService = imageStorageService;
            _httpClient = httpClient ?? HttpClientProvider.Client;
        }

        public async Task<bool> TryAutoFillImageAsync(
            HolidayOption holiday,
            AttractionOption attraction)
        {
            string searchQuery = $"{attraction.Name} {holiday.Name} landmark";

            string requestUrl =
                "https://commons.wikimedia.org/w/api.php" +
                "?action=query" +
                "&generator=search" +
                $"&gsrsearch={Uri.EscapeDataString(searchQuery)}" +
                "&gsrnamespace=6" +
                "&gsrlimit=8" +
                "&prop=imageinfo" +
                "&iiprop=url|size|mime|extmetadata" +
                $"&iiurlwidth={PreferredImageWidth}" +
                "&format=json";

            using HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            await using Stream responseStream = await response.Content.ReadAsStreamAsync();

            using JsonDocument document = await JsonDocument.ParseAsync(responseStream);

            WikimediaImageResult? bestImage = FindBestImage(document);

            if (bestImage is null)
            {
                return false;
            }

            string savedImagePath =
                await _imageStorageService.SaveAttractionImageFromUrlAsync(
                    _httpClient,
                    bestImage.DownloadUrl,
                    attraction.Id);

            attraction.ImagePath = savedImagePath;
            attraction.ImageSourceUrl = bestImage.SourcePageUrl;
            attraction.ImageProvider = "Wikimedia Commons";
            attraction.ImageCredit = bestImage.Credit;

            return true;
        }

        private static WikimediaImageResult? FindBestImage(JsonDocument document)
        {
            if (!document.RootElement.TryGetProperty("query", out JsonElement queryElement))
            {
                return null;
            }

            if (!queryElement.TryGetProperty("pages", out JsonElement pagesElement))
            {
                return null;
            }

            List<WikimediaImageResult> candidates = [];

            foreach (JsonProperty pageProperty in pagesElement.EnumerateObject())
            {
                JsonElement page = pageProperty.Value;

                if (!page.TryGetProperty("title", out JsonElement titleElement))
                {
                    continue;
                }

                string title = titleElement.GetString() ?? string.Empty;

                if (IsBadImageTitle(title))
                {
                    continue;
                }

                if (!page.TryGetProperty("imageinfo", out JsonElement imageInfoArray))
                {
                    continue;
                }

                JsonElement imageInfo = imageInfoArray.EnumerateArray().FirstOrDefault();

                if (imageInfo.ValueKind is JsonValueKind.Undefined)
                {
                    continue;
                }

                string? mime = GetStringOrNull(imageInfo, "mime");

                if (mime is not "image/jpeg" and not "image/png" and not "image/webp")
                {
                    continue;
                }

                string? downloadUrl = GetStringOrNull(imageInfo, "thumburl")
                                      ?? GetStringOrNull(imageInfo, "url");

                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    continue;
                }

                int originalWidth = GetIntOrDefault(imageInfo, "width");
                int originalHeight = GetIntOrDefault(imageInfo, "height");
                int thumbWidth = GetIntOrDefault(imageInfo, "thumbwidth");

                if (originalWidth < 700 && thumbWidth < 700)
                {
                    continue;
                }

                string credit = ExtractCredit(imageInfo);

                candidates.Add(new WikimediaImageResult
                {
                    DownloadUrl = downloadUrl,
                    SourcePageUrl = BuildCommonsFilePageUrl(title),
                    Credit = credit,
                    OriginalWidth = originalWidth,
                    OriginalHeight = originalHeight,
                    ThumbWidth = thumbWidth
                });
            }

            return candidates
                .OrderByDescending(x => x.ThumbWidth)
                .ThenByDescending(x => x.OriginalWidth)
                .FirstOrDefault();
        }

        private static bool IsBadImageTitle(string title)
        {
            string lowerTitle = title.ToLowerInvariant();

            return lowerTitle.Contains("logo")
                   || lowerTitle.Contains("map")
                   || lowerTitle.Contains("icon")
                   || lowerTitle.Contains("symbol")
                   || lowerTitle.Contains("diagram")
                   || lowerTitle.Contains("floor plan")
                   || lowerTitle.Contains("locator");
        }

        private static string? GetStringOrNull(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out JsonElement value)
                ? value.GetString()
                : null;
        }

        private static int GetIntOrDefault(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out JsonElement value)
                   && value.TryGetInt32(out int result)
                ? result
                : 0;
        }

        private static string ExtractCredit(JsonElement imageInfo)
        {
            if (!imageInfo.TryGetProperty("extmetadata", out JsonElement metadata))
            {
                return "Image sourced from Wikimedia Commons";
            }

            string? artist = ExtractMetadataValue(metadata, "Artist");
            string? license = ExtractMetadataValue(metadata, "LicenseShortName");

            if (!string.IsNullOrWhiteSpace(artist) && !string.IsNullOrWhiteSpace(license))
            {
                return $"{StripHtml(artist)} • {StripHtml(license)}";
            }

            if (!string.IsNullOrWhiteSpace(artist))
            {
                return StripHtml(artist);
            }

            if (!string.IsNullOrWhiteSpace(license))
            {
                return StripHtml(license);
            }

            return "Image sourced from Wikimedia Commons";
        }

        private static string? ExtractMetadataValue(JsonElement metadata, string propertyName)
        {
            if (!metadata.TryGetProperty(propertyName, out JsonElement metadataEntry))
            {
                return null;
            }

            if (!metadataEntry.TryGetProperty("value", out JsonElement value))
            {
                return null;
            }

            return value.GetString();
        }

        private static string StripHtml(string value)
        {
            return System.Text.RegularExpressions.Regex.Replace(value, "<.*?>", string.Empty);
        }

        private static string BuildCommonsFilePageUrl(string title)
        {
            string fileName = title.Replace("File:", string.Empty, StringComparison.OrdinalIgnoreCase);
            return $"https://commons.wikimedia.org/wiki/File:{Uri.EscapeDataString(fileName)}";
        }

        private sealed class WikimediaImageResult
        {
            public required string DownloadUrl { get; init; }

            public required string SourcePageUrl { get; init; }

            public required string Credit { get; init; }

            public int OriginalWidth { get; init; }

            public int OriginalHeight { get; init; }

            public int ThumbWidth { get; init; }
        }
    }
}