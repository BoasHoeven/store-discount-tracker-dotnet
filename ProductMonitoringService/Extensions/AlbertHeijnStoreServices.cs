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
                c.Timeout = TimeSpan.FromSeconds(30);
                c.BaseAddress = new Uri("https://ah.nl");

                c.DefaultRequestHeaders.Add("Host", "www.ah.nl");
                c.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:109.0) Gecko/20100101 Firefox/117.0");
                c.DefaultRequestHeaders.Add("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
                c.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                c.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                c.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            })
            .AddHttpMessageHandler<AlbertHeijnRateLimitPolicy>();

        return services;
    }
}