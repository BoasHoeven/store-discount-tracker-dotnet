using Microsoft.Extensions.DependencyInjection;
using ProductMonitoringService.ConcreteClasses;
using ProductMonitoringService.Contracts;
using ProductMonitoringService.Services;

namespace ProductMonitoringService.Extensions;

public static class ScraperServices
{
    public static IServiceCollection AddScraperServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddStoreServices()
            .AddAlbertHeijnStoreServices()
            .AddTransient<StoreMatcherService>()
            .AddTransient<StoreDiscountService>()
            .AddSingleton<IProductStorage, JsonProductStorageService>()
            .AddScoped<ProductSerializer>()
            .AddTransient<ProductService>()
            .AddTransient<UrlExtractorService>()
            .AddMemoryCache();

        return serviceCollection;
    }
}