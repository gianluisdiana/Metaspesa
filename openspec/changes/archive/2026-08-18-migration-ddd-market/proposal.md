## Why

The current Markets domain is a set of thin read-shaped records that combines products, formats, and current prices. It cannot enforce Market invariants, and loading a product also loads price state that should be append-only and independently persisted. The next DDD migration slice needs to establish the Markets bounded context before Shopping and Purchasing begin referencing its typed IDs.

## What Changes

- Replace the current thin Markets records with a bounded-context module containing `Market` and `Product` aggregate roots.
- Add Markets-specific value objects and IDs for markets, products, product formats, and price snapshots.
- Make `Product` own its bounded `ProductFormat` collection and expose format add/update behavior.
- Model `PriceSnapshot` separately from `Product` as an append-only market price observation that references `ProductFormatId`.
- Reuse shared-kernel `Money`, `Quantity`, and `UnitOfMeasure`; keep `ImageUrl` and all Markets IDs/names specific to this bounded context.
- Add `MarketDomainException` derived from the shared base domain exception and one concrete exception for each invalid Markets value or operation.
- Adapt existing Market application and persistence boundaries as required to consume the new domain model without redesigning other bounded contexts.
- Make the Market transport depend directly on concrete Market handlers instead of transitional command/query handler interfaces.
- Move Market product-filter validation into `GetMarketProductsFilter`, which rejects invalid pagination by throwing.
- Remove the AddMarketProducts FluentValidation validator and enforce import rules with concrete Market exceptions.
- Make every Market handler return its value directly, or no value for commands, and propagate exceptions instead of returning `Result`.
- Use one database adapter per Market persistence interface: Market, Market Product, and PriceSnapshot.
- Add focused domain, application, and persistence tests for the migrated model.

## Capabilities

### New Capabilities
- `domain-markets`: Markets bounded-context domain model for markets, products, formats, and append-only price snapshots.

### Modified Capabilities

## Impact

- Affected service: `server/` only.
- Affected layers: Domain Markets, Application Market use cases and repository abstractions, Database Market repository mapping, and their tests.
- No planned gRPC/proto or client generated-type changes.
- No planned database schema or migration changes; existing integer keys and columns are wrapped and mapped at adapter boundaries.
- No planned Shopping or Purchasing domain migration; those remain later slices and may temporarily consume primitive Market references through existing boundaries.
- No scraper, Docker/Compose, observability, or configuration changes.
- Data-integrity impact: invalid Market values and operations fail with specific domain exceptions; price observations remain independent and append-only.
- Security and privacy impact: none; no new sensitive data or logging.
- Backward compatibility risk is server-internal and compile-time. Existing external Market behavior remains stable in this slice.
