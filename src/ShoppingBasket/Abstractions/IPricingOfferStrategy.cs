using ShoppingBasket.Models;

namespace ShoppingBasket.Abstractions;

public interface IPricingOfferStrategy
{
    string OfferType { get; }

    int Priority { get; }

    decimal Apply(
        OfferRule offer,
        int quantity,
        int unitPricePence,
        decimal currentLineTotalPence);
}
