using System.Net.Http;

namespace HolidayExplorer.Core.Infrastructure.Http;

public static class HttpClientProvider
{
    private const int DefaultClientTimeoutSeconds = 30;

    private static readonly Lazy<HttpClient> SharedClient = new(CreateClient);

    public static HttpClient Client => SharedClient.Value;

    private static HttpClient CreateClient()
    {
        HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(DefaultClientTimeoutSeconds)
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "HolidayExplorer/1.0 (local WPF holiday planner)");

        return client;
    }
}