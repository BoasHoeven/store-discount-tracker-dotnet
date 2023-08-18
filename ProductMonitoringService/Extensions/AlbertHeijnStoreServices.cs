using System.Net;
using Microsoft.Extensions.DependencyInjection;
using ProductMonitoringService.Policies;

namespace ProductMonitoringService.Extensions;

public static class AlbertHeijnStoreServices
{
    public static IServiceCollection AddAlbertHeijnStoreServices(this IServiceCollection services)
    {
        services.AddSingleton<AlbertHeijnRateLimitPolicy>();

        services.AddHttpClient("AlbertHeijnClient", c =>
            {
                c.BaseAddress = new Uri("https://ah.nl");
                c.Timeout = TimeSpan.FromMinutes(10);

                // Add default headers to HttpClient
                c.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                c.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                c.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,nl;q=0.8,de;q=0.7");
                c.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                c.DefaultRequestHeaders.Add("Dnt", "1");
                c.DefaultRequestHeaders.Add("Sec-Ch-Ua", "\"Not/A)Brand\";v=\"99\", \"Google Chrome\";v=\"115\", \"Chromium\";v=\"115\"");
                c.DefaultRequestHeaders.Add("Sec-Ch-Ua-Mobile", "?0");
                c.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", "\"macOS\"");
                c.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                c.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                c.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
                c.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
                c.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            .AddHttpMessageHandler<AlbertHeijnRateLimitPolicy>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        return services;
    }
}