using ShoppingBasket.Abstractions;
using ShoppingBasket.Models;

namespace ShoppingBasket.Offers;

/// <summary>
/// Class that implements the percentage discount pricing offer strategy, applying a percentage discount to the current line total based on the offer configuration.
/// </summary>
public sealed class PercentageDiscountStrategy : IPricingOfferStrategy
{
    public string OfferType => OfferTypeIds.PercentageDiscount;

    public int Priority => 200;

    public decimal Apply(
        OfferRule offer,
        int quantity,
        int unitPricePence,
        decimal currentLineTotalPence)
    {
        var configuration = offer.DeserializeConfiguration<PercentageDiscountConfiguration>();
        configuration.Validate();

        return checked(currentLineTotalPence * (100m - configuration.DiscountPercentage) / 100m);
    }
}
