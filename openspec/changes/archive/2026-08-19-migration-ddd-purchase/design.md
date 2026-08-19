## Context

`RecordShoppingList` currently performs transitional purchase recording from `Application/Shopping`, returns `Result`, and writes Purchasing rows through a compatibility repository. Existing database tables already store nullable buyer/list references and PurchaseItems keyed to exact `price_snapshot_id` values. Identity, Markets, Shopping, and Shared Kernel DDD modules are already implemented and are dependencies, not migration targets.

This change adds only the Purchasing bounded context shown in `docs/domain.puml`. Checkout must consume existing ShoppingList state and market price-snapshot data without changing those domain models or their specifications.

## Goals / Non-Goals

**Goals:**
- Add immutable Purchasing domain state and dedicated exceptions.
- Add a concrete Purchasing checkout handler without `Result`, validators, or generic handler interfaces.
- Resolve exact paid PriceSnapshotId values, create one Purchase, reset checked Shopping items, and commit atomically.
- Add minimal Purchasing-owned persistence and checkout data abstractions over the existing schema.
- Preserve current RPC, proto, client, database, and external checkout behavior.

**Non-Goals:**
- Changes to Identity, Markets, Shopping, or Shared Kernel domain models/specifications.
- Proto field renames, client regeneration, new RPCs, or purchase read models.
- Purchase history, receipt UI, refunds, edits, deletion, totals, currencies, or duplicated product data.
- Database migrations, scraper changes, Docker/Compose changes, or observability configuration changes.

## Decisions

### 1. Purchasing owns immutable receipt state

Add `Domain/Purchasing/Purchase`, `PurchaseItem`, `PurchaseId`, `PurchaseDomainException`, and one concrete exception per invalid value or invariant. Purchase contains optional persisted ID, optional Identity `UserId`, optional Shopping `ShoppingListId`, UTC `PurchasedAt`, and one or more immutable items. PurchaseItem contains only Markets `PriceSnapshotId` and shared `PositiveAmount`.

Aggregate exposes creation/rehydration construction only. No mutation methods. Optional buyer/list references preserve existing `ON DELETE SET NULL` behavior.

Alternative: keep purchase rows as persistence-only data. Rejected because Purchasing would still lack domain ownership.

### 2. Checkout belongs to Purchasing Application

Add `Application/Purchasing/CheckoutShoppingList.Handler`. Handler uses existing `IShoppingListRepository` to load/reset Shopping aggregate, Purchasing-owned checkout price reader to resolve latest snapshot IDs, `IPurchaseRepository` to stage Purchase, `IClock` for timestamp, and `IUnitOfWork` for one commit.

Flow:
1. Load owner-accessible named or temporary ShoppingList.
2. Reject missing list or empty checked selection with exact exceptions.
3. Batch-resolve latest snapshot IDs and reject any missing checked format.
4. Construct immutable Purchase.
5. Add Purchase, reset checked Shopping items, update ShoppingList, commit once.

No retry or compensation workflow. All writes share existing DbContext transaction. Cancellation flows through every async dependency.

Alternative: retain combined Shopping compatibility repository. Rejected because Purchase creation remains hidden under Shopping ownership.

### 3. Purchasing owns checkout integration ports

Add minimal Purchasing application ports:
- `IPurchaseRepository`: stage one Purchase aggregate.
- `IPurchasePriceSnapshotReader`: batch-resolve latest `PriceSnapshotId` values for checked `ProductFormatId` values.

Database implements reader against existing Markets snapshot tables, ordered by `ObservedAt` then snapshot ID descending. This is a Purchasing integration adapter, not a Markets domain change. Unknown formats are omitted so handler identifies missing checkout data.

Alternative: modify Markets specification/interface. Rejected because Markets migration is complete and user requested Purchasing-only scope.

### 4. Dedicated Purchase persistence stays minimal

Add `PostgreSqlPurchaseRepository` implementing only `IPurchaseRepository`. Map aggregate state to existing `PurchaseDbEntity` and `PurchaseItemDbEntity`; do not choose prices, reset lists, or save independently.

Remove transitional purchase-writing responsibility only after Purchasing path is active. Existing Shopping aggregate repository remains otherwise unchanged.

### 5. Existing transport remains unchanged

Keep `ShoppingService.RecordShoppingList`, request shape, empty response, and Shopper authorization. `ShoppingGrpcService` invokes concrete Purchasing handler. Purchasing exceptions map to same stable public failure categories without exposing internal details.

No server/client proto edits. No generated-file changes.

### 6. Tests target Purchasing behavior

Add one Domain test file per Purchasing class, checkout handler tests for unique success/failure paths, Purchase repository and snapshot-reader integration tests, and gRPC delegation/error tests. Move useful transitional purchase tests to new ownership. Do not restore obsolete Result/interface tests or duplicate existing Shopping/Markets tests.

### 7. Observability remains unchanged

No telemetry configuration changes. Existing traces/logs may identify operation and failure category, but must not include buyer IDs, list names/content, authorization metadata, snapshot IDs, or raw exception details.

## Risks / Trade-offs

- [Partial checkout] -> Purchase add and Shopping reset stage changes, then one `IUnitOfWork.SaveChangesAsync` commits both.
- [Wrong paid snapshot under tied times] -> Reader orders by `ObservedAt`, then snapshot ID descending.
- [Missing price data] -> Fail before staging Purchase or resetting ShoppingList.
- [Purchasing adapter reads Markets tables] -> Keep dependency behind narrow Purchasing checkout port; no Markets domain/API changes.
- [Nullable references weaken creation] -> Checkout supplies both references; nullable state exists for persisted receipts surviving user/list deletion.
- [Concurrent duplicate checkout] -> Existing schema has no concurrency token. Preserve current behavior; stronger guarantees require separate change.

## Migration Plan

1. Add Purchasing domain types and tests.
2. Add Purchasing checkout price reader and Purchase repository with integration tests.
3. Add concrete checkout handler and tests.
4. Switch existing gRPC method to Purchasing handler and add exception mapping tests.
5. Remove transitional Result-based purchase path and compatibility abstraction.
6. Run server build/tests and confirm no proto, client, schema, migration, or unrelated module changes.

Rollback is code-only. Existing schema and public contract remain compatible.

## Open Questions

None. Purchase history and concurrency guarantees remain separate future work.
