using System.IO;
using System.Net.Http;

namespace Holiday_Explorer.Core
{
    public sealed class AttractionImageStorageService
    {
        private static readonly HashSet<string> SupportedExtensions =
        [
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        ];

        public string SaveAttractionImage(string sourceFilePath, string attractionId)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                throw new ArgumentException("Image path cannot be empty.", nameof(sourceFilePath));
            }

            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("The dropped image file does not exist.", sourceFilePath);
            }

            string extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();

            if (!SupportedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Only JPG, JPEG, PNG, and WEBP images are supported.");
            }

            string imageDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Holiday Explorer",
                "Attraction Images");

            Directory.CreateDirectory(imageDirectory);

            string safeAttractionId = string.Join(
                "_",
                attractionId.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

            string destinationFileName = $"{safeAttractionId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            string destinationPath = Path.Combine(imageDirectory, destinationFileName);

            File.Copy(sourceFilePath, destinationPath, overwrite: true);

            return destinationPath;
        }

        public async Task<string> SaveAttractionImageFromUrlAsync(
            HttpClient httpClient,
            string imageUrl,
            string attractionId)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                throw new ArgumentException("Image URL cannot be empty.", nameof(imageUrl));
            }

            using HttpResponseMessage response = await httpClient.GetAsync(imageUrl);

            response.EnsureSuccessStatusCode();

            string extension = GetImageExtension(response.Content.Headers.ContentType?.MediaType);

            string imageDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Holiday Explorer",
                "Attraction Images");

            Directory.CreateDirectory(imageDirectory);

            string safeAttractionId = string.Join(
                "_",
                attractionId.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

            string destinationFileName = $"{safeAttractionId}_auto_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            string destinationPath = Path.Combine(imageDirectory, destinationFileName);

            await using FileStream fileStream = File.Create(destinationPath);

            await response.Content.CopyToAsync(fileStream);

            return destinationPath;
        }

        private static string GetImageExtension(string? mediaType)
        {
            return mediaType?.ToLowerInvariant() switch
            {
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                _ => ".jpg"
            };
        }
    }
}