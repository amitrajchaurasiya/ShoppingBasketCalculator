using ShoppingBasket.Abstractions;
using ShoppingBasket.Models;

namespace ShoppingBasket.Services;

/// <summary>
/// Class that implements an in-memory product catalog, providing access to a predefined set of products and their associated offers.
/// </summary>
public sealed class InMemoryProductCatalog : IProductCatalog
{
    private readonly Product[] _products =
    [
        new("Apple", 35),
        new("Banana", 20),
        new("Melon", 50, [OfferRule.Create(
            OfferTypeIds.QuantityBundle,
            new QuantityBundleConfiguration(2, 1))]),
        new("Lime", 15, [OfferRule.Create(
            OfferTypeIds.QuantityBundle,
            new QuantityBundleConfiguration(3, 2))])
    ];

    public Product GetByName(string productName)
    {
        var product = _products.FirstOrDefault(product =>
            string.Equals(product.Name, productName, StringComparison.OrdinalIgnoreCase));

        if (product is null)
            throw new ArgumentException($"Unknown product: '{productName}'.", nameof(productName));

        return product;
    }
}
