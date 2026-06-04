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

app.MapPost("/api/holidays", async (
    HolidayCatalogueStorageService storage,
    List<HolidayOption> holidays) =>
{
    await storage.SaveAsync(holidays);

    return Results.Ok();
});

app.MapGet("/api/config", () =>
{
    return Results.Ok(new
    {
        startingLocation = "Derby, UK",
        latitude = 52.9225,
        longitude = -1.4746
    });
});

app.MapGet("/images/{fileName}", (string fileName) =>
{
    string imagePath = Path.Combine(
        HolidayExplorerPaths.AttractionImagesDirectory,
        fileName);

    if (!File.Exists(imagePath))
    {
        return Results.NotFound();
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