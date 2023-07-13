using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Services;

public sealed class JsonProductStorageService : IProductStorage
{
    private readonly ProductRepository productRepository;
    private readonly ProductSerializer productSerializer;
    private readonly string filePath;

    public JsonProductStorageService(ProductSerializer productSerializer)
    {
        this.productSerializer = productSerializer;
        
        var productsJsonPath = Environment.GetEnvironmentVariable("PRODUCTS_JSON_PATH");
        
        if(productsJsonPath == null)
        {
            // Get the full path to the currently executing assembly
            var assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

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
        return productSerializer.DeserializeProducts(jsonString);
    }
    
    public async Task<bool> Store(IProduct product)
    {
        productRepository.AddProduct((Product)product);
        var updatedProducts = productSerializer.SerializeProducts(productRepository.GetProducts());
        await File.WriteAllTextAsync(filePath, updatedProducts);
        return true;
    }

    public Task<IProduct?> Exists(string productId, string storeName)
    {
        var existingProduct = productRepository.FindProductByIdAndStore(productId, storeName);
        
        return Task.FromResult<IProduct>(existingProduct);
    }

    public IEnumerable<IProduct> GetAllProducts() => productRepository.GetProducts();
    public IEnumerable<IProduct> GetProductsByName(string message)
    {
        return productRepository.GetProductsByName(message);
    }

    public async Task<IProduct?> RemoveProduct(string productId, string storeName)
    {
        var product = productRepository.FindProductByIdAndStore(productId, storeName);

        if (product is null) return null;
        productRepository.RemoveProduct(product);
        var updatedProducts = productSerializer.SerializeProducts(productRepository.GetProducts());
        await File.WriteAllTextAsync(filePath, updatedProducts);

        return product;
    }
}