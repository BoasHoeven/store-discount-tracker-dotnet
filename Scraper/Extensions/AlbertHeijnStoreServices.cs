using Microsoft.Extensions.DependencyInjection;
using Scraper.Policies;

namespace Scraper.Extensions;

public static class AlbertHeijnStoreServices
{
    public static IServiceCollection AddAlbertHeijnStoreServices(this IServiceCollection services)
    {
        services.AddSingleton<AlbertHeijnRateLimitPolicy>();

        services.AddHttpClient("AlbertHeijnClient", c =>
            {
                c.BaseAddress = new Uri("https://ah.nl");
                c.Timeout = TimeSpan.FromMinutes(10);
            })
            .AddHttpMessageHandler<AlbertHeijnRateLimitPolicy>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        
        return services;
    }
}