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

app.Run();