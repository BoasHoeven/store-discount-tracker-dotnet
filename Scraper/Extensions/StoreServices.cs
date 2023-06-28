using Microsoft.Extensions.DependencyInjection;
using Scraper.Contracts;

namespace Scraper.Extensions;

public static class StoreServices
{
    public static IServiceCollection AddStoreServices(this IServiceCollection services)
    {
        var storeTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IStore).IsAssignableFrom(p) && !p.IsInterface);

        foreach (var type in storeTypes)
        {
            services.AddSingleton(typeof(IStore), type);
        }

        return services;
    }
}