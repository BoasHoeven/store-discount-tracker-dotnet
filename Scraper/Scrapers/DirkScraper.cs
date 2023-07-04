using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using HtmlAgilityPack;
using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Scrapers
{
    public sealed class DirkScraper : IStoreScraper
    {
        private static readonly HttpClient Client = new();
        
        private const string ApiKey = "6d3a42a3-6d93-4f98-838d-bcc0ab2307fd";
        private const string ProductNameXPath = "//h1[@class='product-details__info__title']";
        private const string ProductUnitSizeXPath = "//span[@class='product-details__info__subtitle']";
        
        private sealed class ProductData
        {
            public int ProductID { get; set; }
            public string? ProductNumber { get; set; }
            public string? Brand { get; set; }
            public string? MainDescription { get; set; }
            public string? SubDescription { get; set; }
            public string? CommercialContent { get; set; }
            public IEnumerable<ProductPrice> ProductPrices { get; set; } = Enumerable.Empty<ProductPrice>();
        }

        private sealed class ProductPrice
        {
            public decimal Price { get; set; }
            public decimal RegularPrice { get; set; }
        }

        private static string GetProductFullName(ProductData product)
        {
            var fullNameBuilder = new StringBuilder(product.Brand);
            fullNameBuilder.Append(' ');
            fullNameBuilder.Append(product.MainDescription);

            if (!string.IsNullOrWhiteSpace(product.SubDescription))
            {
                fullNameBuilder.Append(' ');
                fullNameBuilder.Append(product.SubDescription);
            }

            return fullNameBuilder.ToString();
        }

        private static async Task<ProductData?> ApiSearch(string productName, string productUnitSize)
        {
            var productNameEncoded = WebUtility.UrlEncode(productName);
            var url = $"https://api.dirk.nl/v1/assortmentcache/search/66/?api_key={ApiKey}&search={productNameEncoded}";

            var response = await Client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var products = JsonSerializer.Deserialize<List<ProductData>>(content);
                return products?.FirstOrDefault(product =>
                    string.Equals(GetProductFullName(product), productName, StringComparison.OrdinalIgnoreCase) &&
                    product.CommercialContent == productUnitSize);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public async Task<IProduct?> GetProductFromId(string id, long userId)
        {
            var htmlWeb = new HtmlWeb();
            var htmlDocument = await htmlWeb.LoadFromWebAsync($"https://www.dirk.nl/boodschappen/_/_/_/{id}");

            var productNameNode = htmlDocument.DocumentNode.SelectSingleNode(ProductNameXPath);
            if (productNameNode == null)
            {
                throw new Exception($"Could not find product name of {id}");
            }

            var productName = productNameNode.InnerText.Trim();

            var productUnitSizeNode = htmlDocument.DocumentNode.SelectSingleNode(ProductUnitSizeXPath);
            if (productUnitSizeNode == null)
            {
                throw new Exception($"Could not find product unit size of {id}");
            }

            var productUnitSize = productUnitSizeNode.InnerText.Trim();
            var productData = await ApiSearch(productName, productUnitSize);

            if (productData is null) return null;

            IProduct product = new Product(id, productName, productUnitSize, "Dirk")
            {
                AddedBy = userId,
                Created = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Name = productName,
                Price = productData.ProductPrices.FirstOrDefault()!.RegularPrice,
                ProductNumber = productData.ProductNumber!
            };

            return product;
        }
        
        public async Task<ProductDiscount?> IsOnDiscount(IProduct product)
        {
            if (product.StoreName != "Dirk")
            {
                throw new ArgumentException($"Unsupported store: {product.StoreName}");
            }
    
            var response = await Client.GetAsync($"https://api.dirk.nl/v1/assortmentcache/number/66/{product.ProductNumber}?api_key={ApiKey}");

            if (!response.IsSuccessStatusCode) return null;
            var content = await response.Content.ReadAsStringAsync();

            // Parse the content into a dynamic object
            var jsonData = JsonDocument.Parse(content);

            // If there is no offer, return null
            var productOffers = jsonData.RootElement.GetProperty("ProductOffers");
            if (productOffers.ValueKind == JsonValueKind.Undefined || productOffers.GetArrayLength() == 0) return null;

            var offer = productOffers[0];
            var startDateString = offer.GetProperty("Offer").GetProperty("StartDate").GetString();
            var endDateString = offer.GetProperty("Offer").GetProperty("EndDate").GetString();

            // Parse the start and end dates
            var startDate = DateTime.ParseExact(startDateString, "yyyy-MM-ddTHH:mm:ss", new CultureInfo("nl-NL"), DateTimeStyles.None);
            var endDate = DateTime.ParseExact(endDateString, "yyyy-MM-ddTHH:mm:ss", new CultureInfo("nl-NL"), DateTimeStyles.None);

            // Calculate the discount message
            var offerPrice = offer.GetProperty("OfferPrice").GetDecimal();
            var normalPrice = offer.GetProperty("RegularPrice").GetDecimal();
            var discountMessage = $"Now {offerPrice} - Regular Price: {normalPrice}, Offer Price: {offerPrice}";

            // Create the ProductDiscount object
            var productDiscount = new ProductDiscount
            {
                Product = product,
                OldPrice = normalPrice,
                NewPrice = offerPrice,
                DiscountMessage = discountMessage,
                StartDate = startDate,
                EndDate = endDate
            };

            return productDiscount;
        }


    }
}
