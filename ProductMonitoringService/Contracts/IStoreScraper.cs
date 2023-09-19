using ProductMonitoringService.ConcreteClasses;

namespace ProductMonitoringService.Contracts;

public interface IStoreScraper
{
    Task<IProduct?> GetProductFromId(string id, long userId);
    Task<ProductDiscountResponse> IsOnDiscount(IProduct product);
}