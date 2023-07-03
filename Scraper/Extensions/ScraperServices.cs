using Microsoft.Extensions.DependencyInjection;
using Scraper.ConcreteClasses;
using Scraper.Contracts;
using Scraper.Services;

namespace Scraper.Extensions;

public static class ScraperServices
{
    public static IServiceCollection AddScraperServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddStoreServices()
            .AddAlbertHeijnStoreServices()
            .AddTransient<StoreMatcherService>()
            .AddTransient<StoreDiscountService>()
            .AddTransient<IProductStorage, JsonProductStorageService>()
            .AddScoped<ProductSerializer>()
            .AddTransient<ProductService>()
            .AddTransient<UrlExtractorService>();

        return serviceCollection;
    }
}