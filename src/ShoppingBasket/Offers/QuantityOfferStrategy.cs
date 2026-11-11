using ShoppingBasket.Abstractions;
using ShoppingBasket.Models;

namespace ShoppingBasket.Offers;

/// <summary>
/// Class that implements the quantity bundle pricing offer strategy, applying a "buy X for the price of Y" discount to the current line total based on the offer configuration.
/// </summary>
public sealed class QuantityOfferStrategy : IPricingOfferStrategy
{
    public string OfferType => OfferTypeIds.QuantityBundle;

    public int Priority => 100;

    public decimal Apply(
        OfferRule offer,
        int quantity,
        int unitPricePence,
        decimal currentLineTotalPence)
    {
        var configuration = offer.DeserializeConfiguration<QuantityBundleConfiguration>();
        configuration.Validate();

        var chargeableQuantity = checked(
            (quantity / configuration.BuyQuantity) * configuration.PayForQuantity +
            quantity % configuration.BuyQuantity);

        return checked(chargeableQuantity * unitPricePence);
    }
}
