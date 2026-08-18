## 1. Markets Domain Foundations

- [x] 1.1 Inspect current Markets Domain, Market application handlers, repository mappings, gRPC mappings, and affected tests before editing.
- [x] 1.2 Add `MarketId`, `ProductId`, `ProductFormatId`, and `PriceSnapshotId` value objects that wrap existing positive persistence IDs.
- [x] 1.3 Add `MarketName`, `ProductName`, `BrandName`, and `ImageUrl` value objects with normalization, validation, and value equality.
- [x] 1.4 Add `MarketDomainException` derived from shared `DomainException`.
- [x] 1.5 Add dedicated `InvalidMarketIdException`, `InvalidProductIdException`, `InvalidProductFormatIdException`, and `InvalidPriceSnapshotIdException` classes.
- [x] 1.6 Add dedicated `InvalidMarketNameException`, `InvalidProductNameException`, `InvalidBrandNameException`, `InvalidImageUrlException`, and `InvalidPriceSnapshotObservedAtException` classes.
- [x] 1.7 Add dedicated `DuplicateProductFormatException` and `ProductFormatNotFoundException` classes for aggregate operation failures.
- [x] 1.8 Add Domain unit tests for every Markets ID and value object, including exact concrete exception types.

## 2. Markets Aggregates

- [x] 2.1 Refactor `Market` into an aggregate root containing `MarketId`, `MarketName`, and optional `ImageUrl`, without Product ownership.
- [x] 2.2 Add `ProductFormat` as a Product-owned entity containing `ProductFormatId`, shared-kernel `Quantity`, and Markets `ImageUrl`.
- [x] 2.3 Add `Product` as an aggregate root containing `ProductId`, `ProductName`, `BrandName`, `MarketId`, and its bounded ProductFormat collection.
- [x] 2.4 Implement Product `AddFormat` and `UpdateFormat` behavior with duplicate and missing-format exceptions and no partial mutation on failure.
- [x] 2.5 Add immutable `PriceSnapshot` with `PriceSnapshotId`, `ProductFormatId`, shared-kernel `Money`, and validated `ObservedAt`.
- [x] 2.6 Remove obsolete `MarketProduct`, `ProductBrand`, Markets `AQuantity`, and ProductFormat dependencies on Shopping `Price`.
- [x] 2.7 Add Domain unit tests for Market and Product composition, format add/update behavior, exact operation exceptions, PriceSnapshot immutability, and absence of price history from Product.

## 3. Application Boundaries

- [x] 3.1 Split Market persistence abstractions into aggregate-oriented `IMarketRepository`, `IProductRepository`, and `IPriceSnapshotRepository` responsibilities.
- [x] 3.2 Update `AddMarketProducts` to construct validated Markets and Shared Kernel values, persist aggregates and snapshots separately, and preserve cancellation propagation.
- [x] 3.3 Preserve operation-scoped rollback across the split repositories so cancellation removes only markets, products, formats, brands, and snapshots created by the current import.
- [x] 3.4 Update `GetMarkets` and `GetMarketProducts` to obtain existing query output without reintroducing Product-to-PriceSnapshot ownership.
- [x] 3.5 Update dependency injection registrations for the split repository abstractions.
- [x] 3.6 Update Application unit tests for aggregate construction, repository collaboration, concrete Market validation exceptions, cancellation, and rollback behavior.

## 4. Database Adapters

- [x] 4.1 Adapt Market entity mapping to reconstruct `MarketId`, `MarketName`, and optional Markets `ImageUrl` from existing columns.
- [x] 4.2 Adapt Product and ProductFormat mapping to reconstruct typed IDs, names, brand, Market reference, shared `Quantity`, and image URL without loading PriceSnapshots into Product.
- [x] 4.3 Implement independent PriceSnapshot append and query mapping with typed IDs, shared `Money`, and observation timestamp.
- [x] 4.4 Preserve existing integer key generation, table/column mappings, configured currency storage, and transaction/cancellation behavior without adding a migration.
- [x] 4.5 Update Database unit tests for aggregate reconstruction and exact invalid-row exception behavior.
- [x] 4.6 Update Market repository integration tests for aggregate persistence, Product loading without price history, append-only snapshots, queries, and partial-import rollback.

## 5. Transport Compatibility

- [x] 5.1 Adapt `MarketGrpcService` mapping to the existing proto response shapes without returning new Domain aggregates directly.
- [x] 5.2 Map expected `MarketDomainException` failures to stable gRPC errors without exposing stack traces or internal persistence details.
- [x] 5.3 Update GrpcApi Market tests and confirm Market proto files and generated client types remain unchanged.

## 6. Verification

- [x] 6.1 Run `dotnet build server/Metaspesa.slnx` with analyzers and warnings as errors.
- [x] 6.2 Run `dotnet test server/Metaspesa.slnx`.
- [x] 6.3 Inspect changed files and confirm Shopping/Purchasing behavior, database migrations, proto/generated files, client, scraper, Docker/Compose, and observability configuration remain outside this slice.

## 7. Market Boundary Refinement

- [x] 7.1 Move finite pagination validation into `GetMarketProductsFilter`, throw argument-out-of-range exceptions, and delete the GetMarketProducts validator.
- [x] 7.2 Update GetMarketProducts unit tests for constructor-enforced filter validation and validator-free handling.
- [x] 7.3 Remove Market handler implementations of command/query handler interfaces and inject concrete handlers directly through Application DI and GrpcApi.
- [x] 7.4 Extract `IPriceSnapshotRepository` methods into `PostgreSqlPriceSnapshotRepository` and update persistence registration.
- [x] 7.5 Split and update Market database integration tests to exercise the Product/Market and PriceSnapshot repositories independently.
- [x] 7.6 Run strict OpenSpec validation, server build, and full server tests.

## 8. Exception-Based Market Application

- [x] 8.1 Add concrete Market exceptions for AddMarketProducts application rules and cover their exact types.
- [x] 8.2 Delete the AddMarketProducts validator and enforce empty imports, registration date, duplicate identities, supported units, and domain values by throwing.
- [x] 8.3 Convert all Market handlers and the cancellation base used by AddMarketProducts from Result returns to direct Task/Task<T> exception propagation.
- [x] 8.4 Update MarketGrpcService and Market Application/GrpcApi tests for direct handler values and exceptions.
- [x] 8.5 Split PostgreSqlMarketRepository into one implementation per `IMarketRepository`, Markets `IProductRepository`, and `IPriceSnapshotRepository`, including DI and integration tests.
- [x] 8.6 Run strict OpenSpec validation, server build, and full server tests.
