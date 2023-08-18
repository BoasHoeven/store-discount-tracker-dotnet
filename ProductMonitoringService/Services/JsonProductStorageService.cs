using System.Reflection;
using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Services;

public sealed class JsonProductStorageService : IProductStorage
{
    private readonly ProductRepository productRepository;
    private readonly string filePath;

    public JsonProductStorageService()
    {
        var productsJsonPath = Environment.GetEnvironmentVariable("PRODUCTS_JSON_PATH");

        if(productsJsonPath == null)
        {
            // Get the full path to the currently executing assembly
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Use this path to find the root of the solution
            var solutionDirectory = Directory.GetParent(assemblyPath!)?.Parent?.Parent?.Parent;

            // Combine the solution directory with the relative path to the data file
            filePath = Path.Combine(solutionDirectory!.FullName, "Products.json");
        }
        else
        {
            filePath = productsJsonPath;
        }

        productRepository = new ProductRepository(LoadProductsFromFile());
    }

    private List<Product> LoadProductsFromFile()
    {
        if (!File.Exists(filePath)) return new List<Product>();

        var jsonString = File.ReadAllText(filePath);
        return ProductSerializer.DeserializeProducts(jsonString);
    }

    public async Task<bool> Add(IProduct product)
    {
        productRepository.Add((Product)product);
        var updatedProducts = ProductSerializer.SerializeProducts(productRepository.GetAll());
        await File.WriteAllTextAsync(filePath, updatedProducts);
        return true;
    }

    public Task<IProduct?> Exists(string productId, string storeName)
    {
        var existingProduct = productRepository.FindByIdAndStore(productId, storeName);

        return Task.FromResult<IProduct?>(existingProduct);
    }

    public IEnumerable<IProduct> GetAll() => productRepository.GetAll();

    public IEnumerable<IProduct> GetByName(string message)
    {
        return productRepository.GetByName(message);
    }

    public async Task<IProduct?> Remove(string productId, string storeName)
    {
        var product = productRepository.FindByIdAndStore(productId, storeName);

        if (product is null)
            return null;

        productRepository.Remove(product);
        var updatedProducts = ProductSerializer.SerializeProducts(productRepository.GetAll());
        await File.WriteAllTextAsync(filePath, updatedProducts);

        return product;
    }

    public async Task<bool> ImportProducts(IEnumerable<Product> products)
    {
        var jsonString = ProductSerializer.SerializeProducts(products);
        await File.WriteAllTextAsync(filePath, jsonString);

        productRepository.SetProducts(products);

        return true;
    }
}