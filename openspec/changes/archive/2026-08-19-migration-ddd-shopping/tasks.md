## 1. Scope and Existing Boundaries

- [x] 1.1 Inspect current Shopping Domain types, Shopping-owned handlers, mixed `RecordShoppingList` behavior, repository/EF mapping, dependency injection, GrpcApi mapping, and affected tests before editing.
- [x] 1.2 Record the exact purchase-writing methods used by `RecordShoppingList` and keep them outside the target `IShoppingListRepository` boundary.
- [x] 1.3 Confirm Shopping proto files, generated client files, database schema/migrations, scraper, client behavior, Docker/Compose, and telemetry configuration require no changes.

## 2. Shared and Shopping Domain Foundations

- [x] 2.1 Add shared-kernel `PositiveAmount` with positive-integer validation, value equality, and a dedicated `InvalidPositiveAmountException`.
- [x] 2.2 Add `ShoppingListId` over the existing positive integer key and `ShoppingListName` with trimming, non-empty validation, and value equality.
- [x] 2.3 Add `ShoppingDomainException` derived from shared `DomainException`.
- [x] 2.4 Add one dedicated concrete Shopping exception for each invalid Shopping value, owner invariant, item invariant, list lookup/conflict, missing format reference, and failed aggregate operation required by the spec.
- [x] 2.5 Add focused Domain unit tests for `PositiveAmount`, `ShoppingListId`, `ShoppingListName`, exception inheritance, value equality, normalization, and exact invalid-value exception types.

## 3. Shopping Aggregate

- [x] 3.1 Replace `AShoppingItem` with a ShoppingList-owned `ShoppingItem` containing Markets `ProductFormatId`, shared `PositiveAmount`, and checked state.
- [x] 3.2 Replace `AShoppingList` with a `ShoppingList` aggregate containing persisted `ShoppingListId`, one or more Identity `UserId` values, optional `ShoppingListName`, temporary/deletion state, and active items.
- [x] 3.3 Implement only current planning behavior: create named/temporary list, rename, add item, update optional item fields atomically, remove item, select checked items, and reset checked items.
- [x] 3.4 Enforce owner presence, ProductFormat uniqueness, valid update fields, and missing-item failures with exact concrete exceptions and no partial aggregate mutation.
- [x] 3.5 Ensure Shopping aggregate types contain no User, Market, Product, ProductFormat, PriceSnapshot, Purchase, or paid/current-price object references.
- [x] 3.6 Remove obsolete Shopping `Product`, `Price`, `PricePolicy`, duplicate `Quantity`, and record-extension types after all preserved callers use the new ownership boundaries.
- [x] 3.7 Add one focused Domain test file per Shopping class covering construction, owner/name/temporary state, add/update/remove behavior, checked selection/reset, uniqueness, atomic failure, and exact exception types.

## 4. Shopping Application Boundaries

- [x] 4.1 Replace field-oriented `IShoppingRepository` use in Shopping-owned workflows with aggregate-oriented `IShoppingListRepository` load, conflict-check, add, and save responsibilities.
- [x] 4.2 Add Application Shopping summary/detail read models so `GetShoppingListSummaries` and `GetShoppingList` do not return Domain aggregates.
- [x] 4.3 Update `CreateShoppingList` to construct typed values and aggregate state, reject owner-scoped named/temporary conflicts with a concrete exception, add the aggregate, and commit once.
- [x] 4.4 Update `AddItemsToList` to validate ProductFormat references through the Markets query boundary, invoke aggregate additions, and commit once without FluentValidation or `Result`.
- [x] 4.5 Update `UpdateItem`, `RemoveItem`, and `UpdateShoppingList` to load the owner-accessible aggregate, invoke domain behavior, and commit once without validators or `Result`.
- [x] 4.6 Update `GetShoppingListSummaries` to return summary read models directly and throw a concrete exception only for expected failures.
- [x] 4.7 Update `GetShoppingList` to enrich ProductFormatId references through the Markets query projection and return the existing response data as an Application read model.
- [x] 4.8 Register and consume concrete Shopping-owned handlers directly; remove their command/query handler interface implementations and Result wrappers.
- [x] 4.9 Update Application unit tests for success, list conflict/not-found, empty additions, missing formats, duplicate formats, invalid updates, cancellation, direct returns, exception types, aggregate collaboration, and no commit on failure.

## 5. Purchasing Compatibility

- [x] 5.1 Isolate the purchase creation/latest-price/reset methods required by `RecordShoppingList` behind the existing transitional abstraction or a narrowly named compatibility abstraction, not `IShoppingListRepository`.
- [x] 5.2 Keep `RecordShoppingList` on its current handler and Result boundary and preserve its checked-item, latest-PriceSnapshot, purchase-write, reset, cancellation, and external error behavior.
- [x] 5.3 Update only the compile-preserving mappings needed for `RecordShoppingList` to coexist with the new Shopping aggregate, without adding Purchasing domain types.
- [x] 5.4 Keep existing `RecordShoppingList` Application and Database tests passing and add no new Purchasing behavior in this slice.

## 6. PostgreSQL Shopping Adapter

- [x] 6.1 Adapt `PostgreSqlShoppingRepository` to reconstruct ShoppingListId, owner UserIds, optional ShoppingListName, temporary/deletion state, and active ShoppingItems from existing EF entities.
- [x] 6.2 Implement aggregate add/save mapping over existing tables and database-generated integer IDs without weakening validation for persisted ShoppingListId values.
- [x] 6.3 Persist aggregate rename, item add/update, and item removal while preserving existing `IClock`-based item `DeletedAt` soft deletion and excluding deleted rows from active loads.
- [x] 6.4 Keep owner-scoped case-insensitive named-list lookup, temporary-list lookup, cancellation, unit-of-work transaction behavior, and existing schema mappings unchanged.
- [x] 6.5 Update Shopping repository integration tests for named/temporary aggregate persistence, typed reconstruction, owner isolation, item add/update/removal, soft deletion, empty lists, cancellation, and exact invalid-row failures.

## 7. GrpcApi Compatibility

- [x] 7.1 Update `ShoppingGrpcService` to inject concrete Shopping-owned handlers while retaining the transitional `RecordShoppingList` handler boundary.
- [x] 7.2 Map Shopping Application read models to existing Shopping proto responses without returning Domain aggregates.
- [x] 7.3 Add Shopping domain-exception mapping to stable gRPC statuses/messages without exposing stack traces, persistence details, owner IDs, or list contents.
- [x] 7.4 Update GrpcApi Shopping tests for direct handler values, exception mapping, unchanged response shapes, and unchanged `RecordShoppingList` behavior.

## 8. Verification

- [x] 8.1 Run `dotnet build server/Metaspesa.slnx` with analyzers and warnings as errors.
- [x] 8.2 Run focused Shopping Domain, Application, Database integration, and GrpcApi test projects while resolving all failures.
- [x] 8.3 Run `dotnet test server/Metaspesa.slnx`.
- [x] 8.4 Inspect changed files and confirm no Purchase aggregate/module, proto/generated files, database migrations, client, scraper, Docker/Compose, configuration, or observability changes were introduced.
- [x] 8.5 Confirm telemetry adds no owner IDs, list contents, authorization metadata, or raw internal exception details.
