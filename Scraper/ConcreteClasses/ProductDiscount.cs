using Scraper.Contracts;

namespace Scraper.ConcreteClasses;

public class ProductDiscount
{
    public IProduct Product { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public string DiscountMessage { get; set; }
}
