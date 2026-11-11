using Microsoft.Extensions.DependencyInjection;
using ShoppingBasket.Abstractions;
using ShoppingBasket.Offers;
using ShoppingBasket.Services;

namespace ShoppingBasket.DependencyInjection;

/// <summary>
/// extension methods for registering ShoppingBasket services with an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShoppingBasket(this IServiceCollection services)
    {
        services.AddSingleton<IProductCatalog, InMemoryProductCatalog>();
        services.AddSingleton<IPricingOfferStrategy, QuantityOfferStrategy>();
        services.AddSingleton<IPricingOfferStrategy, PercentageDiscountStrategy>();
        services.AddSingleton<IShoppingBasketPricingService, ShoppingBasketPricingService>();
        return services;
    }
}
