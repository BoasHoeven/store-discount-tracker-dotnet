using Scraper.Contracts;

namespace Scraper.ConcreteClasses;

public class Product : IProduct
{
    public string Id { get; init; }
    public string StoreName { get; init; }
    public string Name { get; init; }
    public decimal Price { get; set; }
    public DateTime Created { get; init; }
    public DateTime LastUpdated { get; set; }
    public int AddedBy { get; init; }

    public Product(string id, string name, string storeName)
    {
        Id = id;
        Name = name;
        StoreName = storeName;
    }
}