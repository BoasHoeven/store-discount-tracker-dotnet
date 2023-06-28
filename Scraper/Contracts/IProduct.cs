namespace Scraper.Contracts;

public interface IProduct
{
    string Id { get; init; }
    string StoreName { get; init; }
    string Name { get; init; }
    decimal Price { get; set; }
    DateTime Created { get; init; }
    DateTime LastUpdated { get; set; }
    int AddedBy { get; init; }
}