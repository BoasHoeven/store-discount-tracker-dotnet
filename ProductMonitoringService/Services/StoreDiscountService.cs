using ProductMonitoringService.ConcreteClasses;
using ProductMonitoringService.Contracts;
using SharedUtilities.Extensions;

namespace ProductMonitoringService.Services;

public sealed class StoreDiscountService
{
    private readonly IEnumerable<IStore> stores;
    private readonly ProductService productService;
    private readonly Random random;

    public StoreDiscountService(IEnumerable<IStore> stores, ProductService productService)
    {
        ArgumentNullException.ThrowIfNull(stores);
        ArgumentNullException.ThrowIfNull(productService);

        this.stores = stores;
        this.productService = productService;
        random = new Random();
    }

    public async Task<IEnumerable<ProductDiscount>> GetDiscounts()
    {
        var products = productService.GetAllProducts();
        var productsByStore = products.GroupBy(x => x.StoreName);
            
        var discountedProducts = new List<ProductDiscount>();
        foreach (var group in productsByStore)
        {
            var store = stores.SingleOrDefault(x => x.StoreName == group.Key);
            if (store is null) continue;

            foreach (var product in group)
            {
                var result = await store.Scraper.IsOnDiscount(product);
                if (result is not null)
                {
                    discountedProducts.Add(result);
                }

                await Task.Delay(random.Next(500, 5000));
            }
        }

        return discountedProducts;
    }

    public async Task<IEnumerable<ProductDiscount>> GetDiscountsForWeek(int weekOffset)
    {
        // Get all discounts
        var allDiscounts = await GetDiscounts();

        // Get the current week of the year
        var currentWeek = DateTime.UtcNow.GetWeekNumber();
    
        // Filter the discounts based on the weekOffset
        var filteredDiscounts = allDiscounts.Where(discount => discount.StartDate.GetWeekNumber() == (currentWeek + weekOffset) ||
                                                               discount.EndDate.GetWeekNumber() == (currentWeek + weekOffset));

        return filteredDiscounts;
    }
    
}