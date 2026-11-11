# Shopping Basket Calculator — Assumptions and Technical Notes

## Assumptions

- The console accepts entries like `Apple: 2, Banana: 3`; quantities must be positive integers.
- Product names are trimmed and compared without regard to case. Duplicate names are combined.
- The product catalog is in memory and currently contains Apple, Banana, Melon, and Lime; there is no database or web service.
- Prices are in pence. Offers run by priority, and the final total is rounded to a whole penny.
- Offer settings are stored as versioned JSON. The diagrams show the current code and omit framework internals and deployment details.

## SOLID principles used

- **Single Responsibility:** The console handles input/output, the catalog finds products, the pricing service coordinates totals, and each offer strategy calculates one type of offer.
- **Open/Closed:** New offer behavior can be added as another strategy without putting offer-specific calculations into the pricing service. The new strategy still needs registration.
- **Liskov Substitution:** Any catalog or offer strategy implementation should be usable through its interface while honoring that interface's behavior.
- **Interface Segregation:** The code uses focused interfaces for pricing, product lookup, and offer calculation rather than one large interface.
- **Dependency Inversion:** The pricing service depends on `IProductCatalog` and `IPricingOfferStrategy`; dependency injection supplies their concrete implementations.

These are design choices in the current code, not a claim that every class is perfectly isolated or that every SOLID principle is applied exhaustively.

## Patterns used

- **Strategy:** `IPricingOfferStrategy` lets the service apply different offer calculations through a common contract. Current strategies are `QuantityOfferStrategy` and `PercentageDiscountStrategy`.
- **Dependency Injection:** `AddShoppingBasket()` registers the service, catalog, and strategies. This keeps object creation out of the pricing logic and makes implementations replaceable.
- **Catalog abstraction:** `IProductCatalog` separates product lookup from pricing. The current implementation is `InMemoryProductCatalog`; it is not a database repository.
- **Factory method:** `OfferRule.Create<TConfiguration>()` creates an offer rule and serializes its typed configuration into versioned JSON.

## Adding another offer

1. Add a configuration class for the offer's settings, with validation for allowed values.
2. Add a unique offer ID to `OfferTypeIds`.
3. Implement `IPricingOfferStrategy`. Set its offer ID and priority, deserialize and validate its configuration, then calculate the adjusted line total.
4. Register the strategy in `ServiceCollectionExtensions.AddShoppingBasket()`.
5. Attach an `OfferRule` with that ID and configuration to the relevant product in the catalog.
6. Add tests for the offer's normal cases, boundary quantities/settings, and interaction with other offers. Strategies run in ascending priority, so choose and test the priority deliberately.

The pricing service should not need offer-specific branches; it discovers the strategy by offer ID and applies registered offers in priority order. A duplicate offer ID is rejected during service construction.
