using ProductMonitoringService.ConcreteClasses;
using ProductMonitoringService.Contracts;

namespace ProductMonitoringService.Services;

public sealed class StoreDiscountService
{
    private readonly IEnumerable<IStore> stores;
    private readonly ProductService productService;

    public StoreDiscountService(IEnumerable<IStore> stores, ProductService productService)
    {
        ArgumentNullException.ThrowIfNull(stores);
        ArgumentNullException.ThrowIfNull(productService);

        this.stores = stores;
        this.productService = productService;
    }

    public async Task<IEnumerable<ProductDiscountResponse>> GetDiscounts()
    {
        var products = productService.GetAllProducts();
        var productsByStore = products.GroupBy(x => x.StoreName);

        var discountResponses = new List<ProductDiscountResponse>();
        foreach (var group in productsByStore)
        {
            var store = stores.SingleOrDefault(x => x.StoreName == group.Key);
            if (store is null)
                continue;

            foreach (var product in group)
            {
                try
                {
                    var productDiscountResponse = await store.Scraper.IsOnDiscount(product);
                    discountResponses.Add(productDiscountResponse);
                }
                catch (Exception e)
                {
                    // ignored
                }

                await Task.Delay(3500);
            }
        }

        return discountResponses;
    }
}