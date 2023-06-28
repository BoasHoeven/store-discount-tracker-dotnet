using Scraper.Contracts;

namespace Scraper.Services;

public class StoreDiscountService
{
    private readonly IEnumerable<IStore> stores;

    public StoreDiscountService(IEnumerable<IStore> stores)
    {
        this.stores = stores;
    }

    public void GetDiscounts()
    {
        foreach (var store in stores)
        {
            
        }
    }
}