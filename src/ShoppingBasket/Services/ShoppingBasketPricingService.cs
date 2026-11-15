using ShoppingBasket.Abstractions;
using ShoppingBasket.Models;

namespace ShoppingBasket.Services;

/// <summary>
/// Coordinates basket grouping, product lookup and offer calculation.
/// </summary>
public sealed class ShoppingBasketPricingService : IShoppingBasketPricingService
{
    private readonly IProductCatalog _productCatalog;
    private readonly IReadOnlyDictionary<string, IPricingOfferStrategy> _offerStrategiesByType;

    public ShoppingBasketPricingService(
        IProductCatalog productCatalog,
        IEnumerable<IPricingOfferStrategy> offerStrategies)
    {
        _productCatalog = productCatalog ?? throw new ArgumentNullException(nameof(productCatalog));
        ArgumentNullException.ThrowIfNull(offerStrategies);

        var strategiesByType = new Dictionary<string, IPricingOfferStrategy>(StringComparer.OrdinalIgnoreCase);
        foreach (var strategy in offerStrategies)
        {
            ArgumentNullException.ThrowIfNull(strategy);
            ArgumentException.ThrowIfNullOrWhiteSpace(strategy.OfferType);
            if (!strategiesByType.TryAdd(strategy.OfferType, strategy))
                throw new InvalidOperationException(
                    $"More than one pricing strategy is registered for offer type '{strategy.OfferType}'.");
        }

        _offerStrategiesByType = strategiesByType;
    }

    /// <summary>
    /// Calculates the total price in pence for the given basket items, applying any offers in order of priority.
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int CalculateTotalPence(IEnumerable<string> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var quantities = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item))
                throw new ArgumentException("Basket product names cannot be empty.", nameof(items));

            var productName = item.Trim();
            _productCatalog.GetByName(productName);
            quantities[productName] = checked(quantities.GetValueOrDefault(productName) + 1);
        }

        var totalPence = 0m;

        foreach (var (productName, quantity) in quantities)
        {
            var product = _productCatalog.GetByName(productName);
            decimal lineTotal = checked(quantity * (decimal)product.UnitPricePence);

            var offers = (product.Offers ?? Array.Empty<OfferRule>())
                .Select(offer => (Offer: offer, Strategy: GetStrategy(offer.OfferType)))
                .OrderBy(entry => entry.Strategy.Priority);

            foreach (var (offer, strategy) in offers)
            {
                lineTotal = checked(strategy.Apply(
                    offer,
                    quantity,
                    product.UnitPricePence,
                    lineTotal));
            }

            totalPence = checked(totalPence + lineTotal);
        }

        return decimal.ToInt32(decimal.Round(totalPence, 0, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Calculates the total price in pounds for the given basket items.
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public decimal CalculateTotalPounds(IEnumerable<string> items) =>
        CalculateTotalPence(items) / 100m;

    /// <summary>
    /// Gets the pricing strategy for the given offer type, throwing an exception if no strategy is registered for that type.
    /// </summary>
    /// <param name="offerType"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private IPricingOfferStrategy GetStrategy(string offerType) =>
        _offerStrategiesByType.TryGetValue(offerType, out var strategy)
            ? strategy
            : throw new InvalidOperationException(
                $"No pricing strategy is registered for offer type '{offerType}'.");
}
