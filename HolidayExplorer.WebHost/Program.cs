using HolidayExplorer.Core.Models;
using HolidayExplorer.Core.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<HolidayCatalogueStorageService>();
builder.Services.AddSingleton<AirportDatabaseService>();

builder.WebHost.UseUrls("http://0.0.0.0:5050");

WebApplication app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/airports", async (AirportDatabaseService airportDatabase) =>
{
    var airports = await airportDatabase.LoadAirportsAsync();

    return Results.Ok(airports);
});

app.MapGet("/api/holidays", async (HolidayCatalogueStorageService storage) =>
{
    var holidays = await storage.LoadAsync();
    return Results.Ok(holidays);
});

app.MapGet("/api/attractions/{attractionId}/image", async (
    string attractionId,
    HolidayCatalogueStorageService storage) =>
{
    var holidays = await storage.LoadAsync();

    var attraction = holidays
        .SelectMany(holiday => holiday.Attractions)
        .FirstOrDefault(item =>
            string.Equals(item.Id, attractionId, StringComparison.OrdinalIgnoreCase));

    if (attraction is null)
    {
        return Results.NotFound(new
        {
            message = "Attraction was not found.",
            attractionId
        });
    }

    if (string.IsNullOrWhiteSpace(attraction.ImagePath))
    {
        return Results.NotFound(new
        {
            message = "Attraction does not have an image path.",
            attractionId
        });
    }

    string imagePath = attraction.ImagePath;

    if (!File.Exists(imagePath))
    {
        return Results.NotFound(new
        {
            message = "Attraction image file was not found.",
            attractionId,
            imagePath
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