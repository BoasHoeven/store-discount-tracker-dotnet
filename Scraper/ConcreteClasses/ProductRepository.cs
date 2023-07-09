using Scraper.Contracts;

namespace Scraper.ConcreteClasses;

public sealed class ProductRepository
{
    private readonly List<Product> products;

    public ProductRepository(List<Product> products)
    {
        this.products = products;
    }

    public Product? FindProductByIdAndStore(string id, string storeName)
    {
        return products.FirstOrDefault(p => p.Id == id && p.StoreName == storeName);
    }

    public void AddProduct(Product product)
    {
        products.Add(product);
    }

    public List<Product> GetProducts()
    {
        return products;
    }

    public IEnumerable<IProduct> GetProductsByName(string name)
    {
        return products.Where(p => p.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase));
    }

    public void RemoveProduct(Product product)
    {
        products.Remove(product);
    }
}
