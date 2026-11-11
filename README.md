# Shopping Basket Calculator

A .NET 8 console application that calculates shopping-basket totals using product data and optional product offer rules.

## Products and offers

| Product | Unit price | Offer |
|---|---:|---|
| Apple | 35p | None |
| Banana | 20p | None |
| Melon | 50p | Buy 2, pay for 1 |
| Lime | 15p | Buy 3, pay for 2 |

Each product is represented by the `Product` model, which can contain multiple `OfferRule` records. Each record stores an open string identifier, a configuration version, and a JSON payload. Built-in offers are created from typed configuration models, for example `OfferRule.Create(OfferTypeIds.PercentageDiscount, new PercentageDiscountConfiguration(33m))`. A combined offer uses separate records for the quantity rule and percentage discount.

Quantity offers are applied first to determine the chargeable quantity; the percentage discount is then applied to those chargeable units. Discounts may produce fractional pence, which are kept through the basket calculation and rounded once to the nearest penny, with midpoint values rounded away from zero.

The current catalog does not apply a percentage discount by default. To add one, update the relevant `Product` entry in `InMemoryProductCatalog`, for example:

```csharp
new("Apple", 35, [OfferRule.Create(
	OfferTypeIds.PercentageDiscount,
	new PercentageDiscountConfiguration(33m))])
```

## Architecture

- `Product` and `OfferRule` in `ShoppingBasket.Models` hold product and offer data separately from the pricing algorithm.
- `InMemoryProductCatalog` currently holds a collection of `Product` model instances. Add another product instance there to add a product to the in-memory catalog.
- `IProductCatalog` abstracts product retrieval. A database-backed catalog can implement this interface and be registered in `AddShoppingBasket`, without changing the pricing service.
- `IPricingOfferStrategy` defines the runtime offer-calculation contract. Each strategy advertises a string identifier; the pricing service looks it up in a registry and runs only strategies for configured product offers. Strategy priorities ensure quantity offers are applied before percentage discounts.
- `ShoppingBasketPricingService` groups basket quantities, runs the configured strategies for each product, and calculates the total.
- `IShoppingBasketPricingService` is the application-facing pricing contract.
- `AddShoppingBasket` registers the catalog, offer strategies, and pricing service with Microsoft's built-in dependency injection container.

The offer models contain data, so offer details can be loaded from a persistence layer or serialized independently of the runtime strategies. Each strategy deserializes and validates its own typed JSON configuration.

To add another offer type, add a configuration model and strategy, assign a new string identifier, and register the strategy. Existing database columns do not need to change; the new configuration is stored in the JSON payload.


## Run

Install the .NET 8 SDK, then run:

```bash
dotnet run --project src/ShoppingBasket
```

Enter comma-separated product and quantity pairs:

```text
Apple: 2, Banana: 1, Melon: 2, Lime: 3
```

With the default catalog, the example totals 170p (£1.70). Product names are case-insensitive; quantities must be positive integers.

## Tests

Run the test suite with:

```bash
dotnet test
```

## Documentation

Project documentation is in the [`Documents`](Documents/) folder:

| File | Description |
|---|---|
| [ArchitectureDiagram.puml](Documents/ArchitectureDiagram.puml) | PlantUML architecture diagram |
| [ClassDiagram.puml](Documents/ClassDiagram.puml) | PlantUML class diagram |
| [SequenceDiagram.puml](Documents/SequenceDiagram.puml) | PlantUML sequence diagram |
| [ShoppingBasketAssumptions.md](Documents/ShoppingBasketAssumptions.md) | Assumptions, SOLID principles, patterns used, and steps for adding offers |


## Database schema (Offers and Products catalog can be stored and read from Database with small changes in code)

`database/schema.sql` defines the SQL Server schema. `Product` maps to `dbo.Products`:

| Column | Type | Purpose |
|---|---|---|
| `ProductId` | `int IDENTITY` | Primary key |
| `Name` | `nvarchar(100)` | Unique product name |
| `UnitPricePence` | `int` | Non-negative unit price |

`OfferRule` maps to `dbo.ProductOffers`:

| Column | Type | Purpose |
|---|---|---|
| `ProductOfferId` | `int IDENTITY` | Primary key |
| `ProductId` | `int` | Foreign key to `Products` |
| `OfferType` | `nvarchar(100)` | Strategy registry key, such as `quantity-bundle` |
| `ConfigurationVersion` | `int` | Positive payload schema version |
| `ConfigurationJson` | `nvarchar(max)` | JSON payload interpreted by the matching strategy |

The models have parameterless constructors and writable properties for ORM materialization. The schema validates non-empty identifiers, positive configuration versions, and valid JSON. It allows one row per product and offer identifier. The project does not include EF Core, a database provider, or a database-backed `IProductCatalog`; those must be added to connect to a live database.