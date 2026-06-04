using HolidayExplorer.Core.Models;
using HolidayExplorer.Core.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<HolidayCatalogueStorageService>();

builder.WebHost.UseUrls("http://0.0.0.0:5050");

WebApplication app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/holidays", async (HolidayCatalogueStorageService storage) =>
{
    var holidays = await storage.LoadAsync();
    return Results.Ok(holidays);
});

app.MapGet("/images/{*fileName}", (string fileName) =>
{
    if (string.IsNullOrWhiteSpace(fileName))
    {
        return Results.BadRequest("Missing image file name.");
    }

    string safeFileName = Path.GetFileName(Uri.UnescapeDataString(fileName));

    string imagePath = Path.Combine(
        HolidayExplorerPaths.AttractionImagesDirectory,
        safeFileName);

    if (!File.Exists(imagePath))
    {
        return Results.NotFound(new
        {
            message = "Image file was not found.",
            requestedFile = safeFileName,
            expectedFolder = HolidayExplorerPaths.AttractionImagesDirectory,
            expectedPath = imagePath
        });
    }

    string contentType = Path.GetExtension(imagePath).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".jpg" => "image/jpeg",
        ".jpeg" => "image/jpeg",
        _ => "application/octet-stream"
    };

    return Results.File(imagePath, contentType);
});

app.Run();