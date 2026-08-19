## 1. Purchasing Baseline

- [x] 1.1 Inspect current `RecordShoppingList`, compatibility abstraction, purchase-writing repository methods, EF mappings, DI, GrpcApi, and affected tests before editing.
- [x] 1.2 Confirm existing purchase tables, nullable buyer/list references, exact `price_snapshot_id`, positive amounts, and delete behavior require no migration.
- [x] 1.3 Confirm server/client proto contracts and existing Identity, Markets, Shopping, and Shared Kernel domain models remain unchanged.

## 2. Purchasing Domain

- [x] 2.1 Add `PurchaseId` with positive integer validation, value equality, and dedicated invalid-ID exception.
- [x] 2.2 Add immutable `PurchaseItem` containing only Markets `PriceSnapshotId` and shared `PositiveAmount`.
- [x] 2.3 Add immutable `Purchase` creation/rehydration state with optional persisted ID, optional Identity buyer ID, optional ShoppingListId, valid UTC timestamp, and non-empty items.
- [x] 2.4 Enforce empty items, duplicate PriceSnapshotId, and invalid timestamp through exact Purchasing exceptions without mutation methods.
- [x] 2.5 Add `PurchaseDomainException` derived from shared `DomainException` and one concrete exception per required Purchasing failure.
- [x] 2.6 Add one focused Domain test file per Purchasing class covering valid state, null references, immutability, equality, boundaries, and exact exceptions.

## 3. Purchasing Integration Ports

- [x] 3.1 Add minimal `IPurchaseRepository` with only Purchase add responsibility.
- [x] 3.2 Add minimal `IPurchasePriceSnapshotReader` that batch-resolves latest PriceSnapshotId values for distinct ProductFormatId values.
- [x] 3.3 Implement PostgreSQL snapshot reader using existing snapshot data ordered by `ObservedAt` and snapshot ID descending.
- [x] 3.4 Add reader integration tests for empty input, one/many formats, latest selection, equal-time tie-breaking, missing formats, and cancellation without duplicating Markets repository tests.

## 4. Purchasing Checkout

- [x] 4.1 Add concrete `CheckoutShoppingList.Handler` using existing Shopping repository plus Purchasing repository/reader, `IClock`, and `IUnitOfWork`, without validator, `Result`, or generic handler interface.
- [x] 4.2 Implement owner-scoped named/temporary list loading, checked-item selection, batch snapshot resolution, exact Purchase construction, Shopping reset/update, and one commit.
- [x] 4.3 Preserve Shopping list-not-found exception and add exact Purchasing failures for empty checked selection and missing latest snapshot before mutation.
- [x] 4.4 Forward cancellation to every async dependency and prevent commit/success after failure.
- [x] 4.5 Register concrete Purchasing handler.
- [x] 4.6 Add focused handler tests for named/temporary success, exact snapshot/amount/time mapping, one commit, unique failure paths, cancellation, and no mutation/persistence on failure.

## 5. Purchase Persistence

- [x] 5.1 Add `PostgreSqlPurchaseRepository` implementing only `IPurchaseRepository` and mapping typed Purchase state to existing entities without resolving prices or saving independently.
- [x] 5.2 Register dedicated Purchasing adapters one interface per repository/reader class.
- [x] 5.3 Add integration tests for header/line mapping, exact snapshot IDs and amounts, timestamp, null-compatible references, multiple lines, and existing user/list `SET NULL` behavior.
- [x] 5.4 Confirm EF model and migration snapshot remain unchanged.

## 6. Transport and Transitional Cleanup

- [x] 6.1 Update existing `ShoppingGrpcService.RecordShoppingList` implementation to invoke concrete Purchasing handler while preserving request, response, authorization, sanitation, and cancellation behavior.
- [x] 6.2 Map Purchasing exceptions to existing stable gRPC failure categories without exposing sensitive/internal data.
- [x] 6.3 Add GrpcApi tests for concrete checkout delegation, named/temporary mapping, authorization, cancellation, success, and unique Purchasing exception statuses.
- [x] 6.4 Remove transitional `Application/Shopping/RecordShoppingList`, `IShoppingPurchaseRepository`, compatibility DI, purchase methods in Shopping repository, and obsolete Result-based tests after callers migrate.
- [x] 6.5 Move useful purchase/reset assertions to Purchasing tests; retain existing Shopping/Markets tests unchanged unless compile adaptation is required.

## 7. Boundary Audit and Verification

- [x] 7.1 Confirm PurchaseItem references only PriceSnapshotId and PositiveAmount, Purchase has no mutation methods, and no speculative Purchasing API was added.
- [x] 7.2 Confirm no Identity, Markets, Shopping, Shared Kernel specification/domain behavior, proto, generated client, database migration, scraper, Docker/Compose, or observability configuration changes.
- [x] 7.3 Confirm telemetry adds no buyer IDs, list names/content, authorization metadata, snapshot IDs, or raw internal exception details.
- [x] 7.4 Run focused Purchasing Domain/Application, Purchasing Database integration, Shopping repository, and GrpcApi tests.
- [x] 7.5 Run `dotnet build server/Metaspesa.slnx` with analyzers and warnings as errors.
- [x] 7.6 Run `dotnet test server/Metaspesa.slnx` and preserve every useful non-obsolete transitional behavior test.
- [x] 7.7 Validate OpenSpec and inspect changed files to confirm Purchasing-only scope.
