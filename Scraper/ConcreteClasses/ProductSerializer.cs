using System.Text.Json;

namespace Scraper.ConcreteClasses;

public class ProductSerializer
{
    public List<Product> DeserializeProducts(string jsonString)
    {
        return JsonSerializer.Deserialize<List<Product>>(jsonString) ?? new List<Product>();
    }

    public string SerializeProducts(List<Product> products)
    {
        return JsonSerializer.Serialize(products);
    }
}
