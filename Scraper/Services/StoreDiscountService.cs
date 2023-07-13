using System.Globalization;
using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Services;

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

    public async Task<IEnumerable<ProductDiscount>> GetDiscounts()
    {
        var products = productService.GetAllProducts();
        var productsByStore = products.GroupBy(x => x.StoreName);
        
        var discountedProducts = new List<ProductDiscount>();
        foreach (var group in productsByStore)
        {
            var store = stores.SingleOrDefault(x => x.StoreName == group.Key);
            if (store is null) continue;

            var tasks = group.Select(product => store.Scraper.IsOnDiscount(product));
            var results = await Task.WhenAll(tasks);
            var productDiscounts = results.Where(productDiscount => productDiscount is not null);
            discountedProducts.AddRange(productDiscounts!);
        }

        return discountedProducts;
    }

    public async Task<IEnumerable<ProductDiscount>> GetDiscountsForWeek(int weekOffset)
    {
        // Get all discounts
        var allDiscounts = await GetDiscounts();

        // Get the current time in the Netherlands
        var dutchTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        var dutchTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, dutchTimeZone);

        // Get the current week of the year
        var currentWeek = GetWeekOfYear(dutchTime);
    
        // Filter the discounts based on the weekOffset
        var filteredDiscounts = allDiscounts.Where(discount => GetWeekOfYear(discount.StartDate) == (currentWeek + weekOffset) ||
                                                               GetWeekOfYear(discount.EndDate) == (currentWeek + weekOffset));

        return filteredDiscounts;
    }

    private static int GetWeekOfYear(DateTime date)
    {
        // Make sure we use the Dutch culture info for getting the week of the year
        var dutchCultureInfo = new CultureInfo("nl-NL");
        var calendar = dutchCultureInfo.Calendar;
        return calendar.GetWeekOfYear(date, dutchCultureInfo.DateTimeFormat.CalendarWeekRule, dutchCultureInfo.DateTimeFormat.FirstDayOfWeek);
    }
}