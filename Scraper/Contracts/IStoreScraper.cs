namespace Scraper.Contracts;

public interface IStoreScraper
{
    Task<IProduct?> GetProductFromId(string id);
}