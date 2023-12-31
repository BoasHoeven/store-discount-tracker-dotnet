using ProductMonitoringService.Contracts;

namespace ProductMonitoringService.ConcreteClasses;

public sealed class ProductDiscount
{
    public IProduct Product { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public string DiscountMessage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class ProductDiscountResponse
{
    public ProductResponse ProductResponse { get; init; }
    public ProductDiscount? ProductDiscount { get; init; }

    public ProductDiscountResponse(ProductResponse productResponse)
    {
        ProductResponse = productResponse;
    }
}