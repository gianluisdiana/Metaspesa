## Context

Current code under `server/src/Domain/Markets` models `Market`, `MarketProduct`, `ProductFormat`, `ProductBrand`, and quantity as thin records shaped for Market queries. `ProductFormat` also embeds the latest Shopping `Price`, so Product persistence loads price snapshots to construct a domain product. `AddMarketProducts` groups scraper input into those records, and `PostgreSqlMarketRepository` owns market, brand, product, format, and snapshot persistence through one broad interface.

The target in `plan.md` and `docs/domain.puml` defines a Markets module with separate `Market` and `Product` aggregate roots. `Product` owns formats, while `PriceSnapshot` is append-only, references a format by ID, and is never loaded as Product history. Markets reuses `Money`, `Quantity`, and `UnitOfMeasure` from Shared Kernel. This is one incremental slice of the full DDD migration, so later Shopping, Purchasing, read-model, and public-contract work must remain out of scope.

## Goals / Non-Goals

**Goals:**

- Establish `Metaspesa.Domain.Markets` as the Markets bounded-context module.
- Model `Market` and `Product` as separate aggregate roots linked only by `MarketId`.
- Model `ProductFormat` as a Product-owned entity with `ProductFormatId`, shared `Quantity`, and Markets `ImageUrl`.
- Model `PriceSnapshot` independently with `PriceSnapshotId`, `ProductFormatId`, shared `Money`, and an observation timestamp.
- Introduce Markets-owned IDs and value objects for all concepts specific to this bounded context.
- Introduce `MarketDomainException` and one concrete exception for each invalid value or aggregate operation.
- Split persistence boundaries by aggregate/repository responsibility and map existing database primitives at adapters.
- Preserve external Market behavior and existing storage schema while adding focused tests.

**Non-Goals:**

- Do not migrate Shopping or Purchasing domain models.
- Do not make Markets reference Shopping entities, value objects, or repositories.
- Do not add cross-bounded-context object references; later modules consume Markets IDs only.
- Do not convert Market queries to Application read models in this slice.
- Do not rename gRPC/proto fields or regenerate client code.
- Do not alter database tables, columns, keys, migrations, seed data, Compose, telemetry configuration, or scraper code.
- Do not add currency to `Money`; persistence keeps the existing configured currency.

## Decisions

1. Keep the module namespace `Metaspesa.Domain.Markets` and replace read-shaped records with domain types.

   Rationale: The namespace already matches the target bounded context, while its contents do not. Replacing the model avoids a compatibility layer that would preserve the invalid coupling between formats and Shopping prices.

   Alternative considered: Add new types beside `MarketProduct`. This would create two competing Market models and make repository ownership unclear.

2. Use Markets-specific ID value objects over the existing integer database keys.

   `MarketId`, `ProductId`, `ProductFormatId`, and `PriceSnapshotId` wrap positive identifiers and reject default or non-positive values. Database adapters unwrap and reconstruct them at the persistence boundary.

   Rationale: IDs belong to this bounded context and provide explicit cross-module references without exposing persistence primitives. Existing integer columns can remain unchanged.

   Alternative considered: Generate GUIDs and migrate columns. That adds schema and rollout risk unrelated to establishing the domain module.

3. Keep names and URLs as Markets value objects.

   `MarketName`, `ProductName`, and `BrandName` normalize surrounding whitespace and reject empty values. `ImageUrl` accepts only absolute URLs and remains Markets-specific because it describes Market/product presentation data. Market logos and Product format images remain optional to preserve the existing contract.

   Rationale: These rules protect aggregate state and follow the target diagram without promoting contextual concepts into Shared Kernel.

   Alternative considered: Keep strings and `Uri` primitives validated by FluentValidation. That permits invalid aggregates whenever another caller bypasses the application validator.

4. Make `Product` own only its bounded collection of `ProductFormat`.

   `Product` contains `ProductId`, `ProductName`, `BrandName`, `MarketId`, and formats. It exposes `AddFormat` and `UpdateFormat`. Duplicate format IDs fail; updating an unknown format fails. The aggregate does not contain `Market`, prices, or price history.

   Rationale: Formats share Product consistency rules, while Market and price observations have independent lifecycles.

   Alternative considered: Keep Product nested under Market. This would make scraped product updates load and persist an unnecessarily large Market aggregate.

5. Persist `PriceSnapshot` independently and append-only.

   `PriceSnapshot` contains its own ID, a `ProductFormatId`, shared `Money`, and `ObservedAt`. `IPriceSnapshotRepository` exposes additions and reads required by current workflows but no domain update operation. Product loading never includes snapshots.

   Rationale: A price is an observation, not mutable Product state. Separating it supports exact historical references in the later Purchasing slice.

   Alternative considered: Keep the latest price on `ProductFormat`. This hides historical identity and forces Product loading to depend on snapshot state.

6. Split the broad Market repository by aggregate responsibility.

   `IMarketRepository` handles Market aggregates, `IProductRepository` handles Product aggregates and Market product projections, and `IPriceSnapshotRepository` handles observations. Each interface has its own database adapter: `PostgreSqlMarketRepository`, `PostgreSqlMarketProductRepository`, and `PostgreSqlPriceSnapshotRepository`. Application orchestration may use all three; database adapters retain transaction and cancellation behavior. Rollback for failed or cancelled imports deletes only records created by that operation, preserving current behavior until a later transaction redesign.

   Rationale: Repository boundaries follow aggregate roots and the explicit repositories in `plan.md`.

   Alternative considered: Keep one `IMarketRepository`. That would preserve a service-shaped persistence API rather than bounded aggregate ownership.

7. Use a Markets exception hierarchy for domain failures.

   `MarketDomainException` derives from shared `DomainException`. Concrete exceptions cover invalid Market/Product/Format/Snapshot IDs, invalid market/product/brand names, invalid image URLs, invalid observation timestamps, duplicate product formats, and missing product formats. Shared `Money`, `Quantity`, and `UnitOfMeasure` continue throwing their own shared-kernel exceptions.

   Rationale: Exact exception types make invalid state and failed operations diagnosable without parsing messages. Catching the base type remains available at transport boundaries.

   Alternative considered: One exception with an error code. That centralizes mapping but weakens type-level diagnostics requested for this migration.

8. Keep external contracts and telemetry stable.

   Existing Market gRPC request/response fields remain unchanged. Application and GrpcApi map between external primitives/read shapes and new domain types. Expected Market exceptions are mapped without exposing stack traces or internal data. Existing traces and structured logs remain; no product payloads or URLs are added to logs.

   Rationale: Public contract and read-model migration are explicit later steps in the full plan.

9. Let the Market product filter enforce its own pagination invariants.

   `GetMarketProductsFilter` rejects non-positive finite page indexes and sizes by throwing `ArgumentOutOfRangeException`. `GetMarketProducts` no longer has a FluentValidation validator.

   Rationale: The filter cannot exist in an invalid state, matching the constructor-enforced invariant style used by migrated domain values.

10. Inject concrete Market handlers at transport boundaries.

   Market handlers no longer implement the transitional `ICommandHandler` and `IQueryHandler` interfaces or return `Result`. Commands return `Task`; queries return `Task<T>`. Application dependency injection and `MarketGrpcService` use concrete handler types directly. Domain and application failures propagate as exceptions to the existing interceptor.

   Rationale: This matches the migrated Identity handlers. Handler interfaces and Result-based domain errors are transitional and will be removed after the incremental migration.

11. Use one database adapter per persistence interface.

   `PostgreSqlMarketRepository` implements only `IMarketRepository`, `PostgreSqlMarketProductRepository` implements only the Markets `IProductRepository`, and `PostgreSqlPriceSnapshotRepository` implements only `IPriceSnapshotRepository`.

   Rationale: Separate adapters make every split Application persistence boundary explicit instead of grouping unrelated interfaces in one implementation.

12. Replace AddMarketProducts validation results with exceptions.

   AddMarketProducts has no FluentValidation validator. The handler constructs domain values so their existing exceptions enforce value invariants, and throws dedicated Market exceptions for empty imports, old registration dates, duplicate product identities, and unsupported units of measure.

   Rationale: This follows the migrated Identity handler pattern and keeps failure diagnostics type-based rather than encoded in Result values.

## Risks / Trade-offs

- Repository splitting touches Application and Database code beyond Domain -> Limit edits to Market workflows and preserve current external behavior with integration tests.
- Existing queries are shaped around products containing latest prices -> Build query projections/read shapes at the Application or Database boundary without adding snapshots back to the Product aggregate.
- Integer IDs may be unavailable before EF insertion -> Let persistence assign IDs and reconstruct persisted aggregates/read models; do not fake valid IDs in Domain.
- Cancellation rollback spans multiple repositories -> Preserve operation-scoped tracking and rollback ordering, and test partial import cancellation.
- Exception propagation changes current `Result` behavior -> Map expected Market exceptions in the existing GrpcApi interceptor and retain stable gRPC statuses/messages.
- URL and name validation may reject legacy malformed rows -> Cover adapter reconstruction with integration tests and surface the exact concrete exception; do not silently normalize invalid URLs.

## Migration Plan

1. Add Markets IDs, value objects, exception hierarchy, aggregates, format entity, and price snapshot model.
2. Replace `MarketProduct`, `ProductBrand`, Markets `AQuantity`, and Shopping `Price` use with new Markets and Shared Kernel types.
3. Split Application repository abstractions, use direct exception-based concrete Market handlers, and adapt `AddMarketProducts`, `GetMarkets`, and `GetMarketProducts` without changing gRPC contracts.
4. Adapt EF repository mapping with one adapter per interface so Product loads formats without snapshots and price observations persist independently.
5. Update Domain, Application, Database, and GrpcApi tests affected by the new model.
6. Run `dotnet build server/Metaspesa.slnx` and `dotnet test server/Metaspesa.slnx`.

Rollback is code-only: revert the Markets model and adapter changes. No database, proto, generated-code, configuration, or deployment rollback is required.

## Open Questions

- The later Application read-model slice will decide the final shape and ownership of Market query DTOs.
- The later public-contract slice will decide whether external `reference_uid` fields become `product_format_uid`.
- Whether imports should become one database transaction instead of explicit cancellation rollback remains outside this domain migration.
