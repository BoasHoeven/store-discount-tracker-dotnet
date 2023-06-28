namespace Scraper.Contracts;

public interface IProductScraper
{
    Task<IProduct?> GetProductFromId(string id);
}