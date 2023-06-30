using System.Collections;
using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Services;

public class StoreDiscountService
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

    public async Task<IEnumerable<ProductDiscount>> GetDiscounts()
    {
        var products = productService.GetAllProducts().ToList();
        var productsByStore = products.GroupBy(x => x.StoreName);
        var discountedProducts = new List<ProductDiscount>();

        foreach (var group in productsByStore)
        {
            var store = stores.SingleOrDefault(x => x.StoreName == group.Key);
            if (store is null) continue;

            var tasks = group.Select(product => store.Scraper.IsOnDiscount(product));
            var results = await Task.WhenAll(tasks);
            var nonNullProductDiscounts = results.Where(productDiscount => productDiscount is not null);
            discountedProducts.AddRange(nonNullProductDiscounts!);
        }

        return discountedProducts;
    }
}