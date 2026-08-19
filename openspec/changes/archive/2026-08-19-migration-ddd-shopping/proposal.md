## Why

The current Shopping domain is a pair of read-shaped records, so list rules are spread across FluentValidation and repository methods and persistence mutates list state without aggregate behavior. Shopping is the next bounded context in the incremental DDD plan and must establish planning-only ownership before the separate Purchasing migration extracts checkout and purchase history.

## What Changes

- Replace the current Shopping records with a `ShoppingList` aggregate root that owns its owner IDs, optional name/temporary state, deletion state, and `ShoppingItem` entities.
- Add Shopping-specific `ShoppingListId` and `ShoppingListName` value objects; reference Identity `UserId`, Markets `ProductFormatId`, and shared-kernel `PositiveAmount` by ID/value only.
- Implement only aggregate operations required by current Shopping workflows: create, rename, add items, update an item, remove an item, select checked items, and reset checked items.
- Enforce list and item invariants inside domain construction and aggregate operations, including owner requirements, name validity, unique ProductFormat references, positive amounts, and missing-item failures.
- Add `ShoppingDomainException` derived from the shared base domain exception and a dedicated concrete exception for every Shopping-specific invalid value or failed operation.
- Replace `Result` and FluentValidation error paths in Shopping-owned use cases with exception-based concrete handlers, matching the migrated Identity and Markets modules; keep the mixed `RecordShoppingList` handler transitional until Purchasing migration.
- Replace the service-shaped Shopping repository API with an aggregate-oriented `IShoppingListRepository` and adapt PostgreSQL mapping to load and persist typed Shopping aggregates through the existing schema.
- Preserve current Shopping gRPC contracts and query response behavior through Application read models and transport mapping.
- Leave `RecordShoppingList` and purchase-writing persistence behavior in their current mixed location for the next `Purchasing` migration; this slice does not introduce `Purchase` or `PurchaseItem` domain types.
- Remove obsolete Shopping `Product`, `Price`, `PricePolicy`, and duplicate `Quantity` concepts now owned by Markets or Shared Kernel where they are no longer used by preserved purchase code.

## Capabilities

### New Capabilities
- `domain-shopping`: Shopping bounded-context aggregate, value objects, domain exceptions, application boundaries, persistence mapping, and migration limits.

### Modified Capabilities

## Impact

- Affected service: `server/` only.
- Affected layers: Domain Shopping, Shopping Application handlers/read models and abstractions, Database Shopping repository mapping, GrpcApi Shopping exception/response mapping, and focused tests.
- The existing mixed `RecordShoppingList` checkout/purchase flow remains behaviorally unchanged and is reserved for the next Purchasing slice.
- No planned gRPC/proto or generated-client changes.
- No planned database schema, migration, seed-data, Docker/Compose, configuration, scraper, client, or observability changes.
- Data integrity improves because Shopping aggregates cannot contain invalid owners, names, amounts, duplicate ProductFormat references, or invalid item transitions.
- Security and privacy impact: none; owner IDs and list contents are not added to logs, traces, or exception details exposed externally.
- Backward compatibility risk is server-internal and compile-time; existing Shopping API behavior and persisted rows remain compatible through adapter mapping.
