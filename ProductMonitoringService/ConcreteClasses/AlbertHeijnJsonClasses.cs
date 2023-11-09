using System.Net;
using ProductMonitoringService.Contracts;

namespace ProductMonitoringService.ConcreteClasses;

public sealed class RootObject
{
    public Card card { get; set; }
}

public sealed class Card
{
    public List<ProductDetails> products { get; set; }
}

public sealed class ProductDetails
{
    public string title { get; set; }
    public Price price { get; set; }
    public Discount? discount { get; set; }
    public Shield? shield { get; set; }
}

public sealed class ProductResponse
{
    public bool IsCached { get; init; }
    public bool HasFailed { get; init; }

    public HttpStatusCode StatusCode { get; init; }
    public ProductDetails? ProductDetails { get; init; }
    public IProduct Product { get; init; }

    public ProductResponse(IProduct product)
    {
        Product = product;
    }
}

public sealed class Price
{
    public decimal now { get; set; }
    public decimal? was { get; set; }
    public string unitSize { get; set; }
}

public sealed class Discount
{
    public DateTime startDate { get; set; }
    public DateTime endDate { get; set; }
    public string bonusType { get; set; }
    public string[]? tieredOffer { get; set; }
}

public sealed class Shield
{
    public string text { get; set; }
}