using Microsoft.Extensions.DependencyInjection;
using Scraper.Extensions;
using Scraper.Services;

namespace Scraper;

public class Startup
{
    public Startup()
    {
        var services = new ServiceCollection();
        
        services
            .AddStoreServices()
            .AddSingleton<StoreMatcherService>();
    }
}