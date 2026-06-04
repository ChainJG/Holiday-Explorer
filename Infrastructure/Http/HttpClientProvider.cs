using System.Net.Http;

namespace Holiday_Explorer.Infrastructure.Http
{
    public static class HttpClientProvider
    {
        private const int DefaultClientTimeoutSeconds = 15;

        private static readonly Lazy<HttpClient> SharedClient = new(CreateClient);

        public static HttpClient Client => SharedClient.Value;

        private static HttpClient CreateClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(DefaultClientTimeoutSeconds)
            };

            return client;
        }
    }
}
