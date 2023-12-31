using Microsoft.Extensions.DependencyInjection;
using ProductMonitoringService.Contracts;

namespace ProductMonitoringService.Extensions;

public static class StoreServices
{
    public static IServiceCollection AddStoreServices(this IServiceCollection services)
    {
        var storeTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IStore).IsAssignableFrom(p) && !p.IsInterface);

        foreach (var type in storeTypes)
        {
            services.AddScoped(typeof(IStore), type);
        }

        var scrapers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IStoreScraper).IsAssignableFrom(p) && !p.IsInterface);
        
        foreach (var type in scrapers)
        {
            services.AddScoped(typeof(IStoreScraper), type);
        }
        
        return services;
    }
}