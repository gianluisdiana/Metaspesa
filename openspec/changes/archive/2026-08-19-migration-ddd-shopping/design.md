## Context

Current code under `server/src/Domain/Shopping` models `AShoppingList` and `AShoppingItem` as records and also contains obsolete product, price, quantity, and price-policy types. Shopping handlers validate list state through FluentValidation and ask `IShoppingRepository` to perform field-level mutations. `PostgreSqlShoppingRepository` combines aggregate persistence, item queries, and checkout: `RecordShoppingList` resolves latest price snapshots and writes `PurchaseDbEntity` rows.

The target in `plan.md` and `docs/domain.puml` separates planning from purchasing. Shopping owns a list, its owners, and planned items referencing Markets formats by ID. The later Purchasing module owns checkout, exact paid snapshots, and purchase history. Identity and Markets already establish typed cross-context IDs, concrete handlers, and exception-based failures. This slice follows those conventions without moving the still-mixed purchase workflow early.

## Goals / Non-Goals

**Goals:**

- Establish `Metaspesa.Domain.Shopping` as the Shopping bounded-context module.
- Model `ShoppingList` as the aggregate root for owners, optional name/temporary state, deletion state, and items.
- Reference `UserId` and `ProductFormatId` only; never hold Identity or Markets aggregate objects.
- Add only behavior exercised by current list-planning workflows.
- Put Shopping invariants in value objects and aggregate methods with exact concrete exceptions.
- Make Shopping-owned handlers concrete and exception-based and return Application read models from queries.
- Replace field-oriented persistence operations with an aggregate-oriented `IShoppingListRepository` while preserving the schema and public API.

**Non-Goals:**

- Do not create the Purchasing module, `Purchase`, `PurchaseItem`, `PurchaseId`, or `IPurchaseRepository`.
- Do not extract or redesign `RecordShoppingList`, its latest-price lookup, purchase writes, or rollback/reset sequence in this slice.
- Do not add unused diagram operations such as a public list soft-delete command when no current use case invokes them; existing deletion state remains representable for persistence.
- Do not load Product, ProductFormat, Market, PriceSnapshot, or User objects into a Shopping aggregate.
- Do not rename gRPC fields, change proto files, regenerate client types, or refactor the client.
- Do not change database tables, columns, keys, migrations, configured currency, Docker/Compose, scraper, or observability configuration.

## Decisions

1. Replace the read-shaped records with one `ShoppingList` aggregate and owned `ShoppingItem` entities.

   `ShoppingList` contains `ShoppingListId`, one or more `UserId` owner references, optional `ShoppingListName`, `IsTemporary`, existing deletion state, and a bounded item collection. `ShoppingItem` contains `ProductFormatId`, positive amount, and checked state. The aggregate exposes only create, rename, add, update, remove, checked-item selection, and checked-item reset behavior required by current handlers.

   Rationale: These values change together under list rules. Keeping mutations on the aggregate prevents Application and Database code from constructing invalid combinations.

   Alternative considered: Implement every method shown in `docs/domain.puml`. The diagram is orientative; adding unused transitions now would create rules with no application behavior or tests to define them.

2. Use module-owned identity and name values while reusing established cross-context IDs.

   `ShoppingListId` wraps the existing positive integer key. `ShoppingListName` trims surrounding whitespace and rejects empty text; absence of a name represents a temporary list. Owners use Identity `UserId`, and items use Markets `ProductFormatId`. Shopping does not introduce IDs for other bounded contexts or a `ShoppingItemId` because current aggregate behavior identifies an item uniquely by `ProductFormatId`.

   Rationale: IDs remain specific to their owning bounded contexts, while cross-context relationships remain explicit and object-free.

   Alternative considered: Duplicate `UserId` or `ProductFormatId` in Shopping. That would create competing definitions for the same identity and weaken adapter contracts.

3. Add a positive integer amount value to Shared Kernel.

   A small shared `PositiveAmount` value object represents item counts and throws its own dedicated shared-kernel exception. Shopping uses it now and Purchasing can reuse it in the next slice. Shared `Quantity` remains package quantity plus unit of measure and is not used for list count.

   Rationale: Amount has the same meaning for planned and purchased lines, while Shopping's obsolete `Quantity` duplicates neither the target type nor Markets package quantity correctly.

   Alternative considered: Keep raw `int` or add `ShoppingItemAmount`. Raw integers permit invalid state; a Shopping-only value would be duplicated immediately by Purchasing.

4. Preserve one item per ProductFormat within a list.

   Adding an already-present `ProductFormatId` throws a dedicated duplicate-item exception and leaves state unchanged. Updating or removing an unknown format throws a dedicated item-not-found exception. An update applies optional amount and checked fields atomically and rejects a request with neither field.

   Rationale: The plan explicitly identifies uniqueness by ProductFormat and current commands address items by product-format reference.

   Alternative considered: Merge duplicate additions by increasing amount. Existing validation rejects duplicates, so merging would be a behavior change.

5. Use a Shopping exception hierarchy and eliminate duplicate validators for migrated use cases.

   `ShoppingDomainException` derives from shared `DomainException`. Dedicated concrete exceptions cover invalid Shopping IDs/names, owner and item invariants, list conflicts/not-found state, missing Market format references, empty additions, and invalid operations. Shared and referenced-context value constructors preserve their own exact exception types. Shopping-owned handlers construct values and invoke aggregate methods directly; expected failures propagate as exceptions.

   Rationale: This matches Identity and Markets and makes failures diagnosable by type without parsing `DomainError` codes.

   Alternative considered: Retain FluentValidation and convert failures to `Result`. That would preserve parallel validation paths and allow callers to bypass aggregate rules.

6. Use concrete handlers only for Shopping-owned workflows.

   `CreateShoppingList`, `AddItemsToList`, `UpdateItem`, `RemoveItem`, `UpdateShoppingList`, `GetShoppingList`, and `GetShoppingListSummaries` are registered and injected as concrete handlers. Commands return `Task`; queries return `Task<T>` with Application read models. `RecordShoppingList` remains on its current transitional handler/result boundary because it is Purchasing-owned behavior awaiting the next migration.

   Rationale: This advances the migrated module without partially redesigning checkout or inventing a temporary Purchasing API.

   Alternative considered: Convert `RecordShoppingList` now. That would mix a handler-boundary cleanup with purchase aggregate decisions explicitly assigned to the next slice.

7. Replace `IShoppingRepository` with aggregate-oriented Shopping persistence.

   `IShoppingListRepository` loads lists by owner and current name selector, checks owner-scoped name conflicts, adds new aggregates, and persists changed aggregates. `PostgreSqlShoppingRepository` maps typed IDs and values to existing EF entities, applies item additions/updates/removals from aggregate state, and keeps the existing `IClock`-based soft-delete mapping for removed rows. Database-generated list IDs are assigned and reconstructed at the adapter boundary without weakening `ShoppingListId` validation for persisted aggregates.

   Rationale: Repositories should persist aggregate roots, not expose one method per field mutation.

   Alternative considered: Keep the current broad method set. That would continue bypassing aggregate behavior and retain separate existence/item lookup calls for every command.

8. Keep read composition in Application.

   Shopping list queries return Application summary/detail models, not Domain aggregates. `GetShoppingList` reads the aggregate, gathers `ProductFormatId` values, and uses the existing Markets query projection to enrich the response with product/format data. Missing lists or referenced formats produce concrete exceptions. GrpcApi maps read models to unchanged proto responses.

   Rationale: Shopping owns planning state but not market catalog display data, and the full plan prohibits returning Domain objects to presentation.

   Alternative considered: Put product names and format details on `ShoppingItem`. That would couple the aggregate to Markets state and make it stale.

9. Preserve purchase behavior behind a temporary compatibility boundary.

   Purchase-writing methods needed only by `RecordShoppingList` remain isolated from `IShoppingListRepository`, either on the existing transitional abstraction or a narrowly named compatibility abstraction. They continue resolving the latest `PriceSnapshot`, creating purchase rows, and resetting checked items exactly as today. Shopping aggregate checked-item selection/reset behavior may be used only where this can be done without changing purchase semantics.

   Rationale: Leaving mixed use cases means preserving behavior, not allowing purchase concerns into the new aggregate repository.

   Alternative considered: Keep purchase methods on `IShoppingListRepository`. That would encode a known bounded-context violation into the new target boundary.

10. Preserve contracts, storage, cancellation, and observability.

   Existing cancellation tokens flow through repository reads and `SaveChangesAsync`; no retries are added. Commands commit once through `IUnitOfWork`, and failed validation or aggregate operations occur before persistence so they do not partially mutate stored state. Existing gRPC fields and statuses remain stable through Shopping exception mapping. Existing traces/logs remain, with no owner IDs, list contents, authorization metadata, or raw exception internals added as telemetry.

   Rationale: This is an internal domain migration, not an API, rollout, or observability change.

## Risks / Trade-offs

- Legacy rows may violate new owner, name, or duplicate-item invariants -> Reconstruct through validated values, add integration coverage for valid rows, and surface exact exceptions instead of silently accepting invalid aggregates.
- Database-generated integer identity complicates new aggregate creation -> Keep assignment inside the adapter and require positive IDs whenever an aggregate is reconstructed or exposed as persisted.
- Aggregate persistence can accidentally hard-delete items -> Preserve existing `DeletedAt` behavior and verify removed items are excluded and recover no longer as active items.
- Query enrichment can fail when a referenced Market format is missing -> Throw a dedicated missing-reference exception and keep the list aggregate unchanged.
- Keeping `RecordShoppingList` transitional temporarily leaves two handler/repository styles -> Isolate and label the compatibility boundary so the next Purchasing migration can remove it without changing Shopping aggregate APIs.
- Exception conversion can change gRPC status mapping -> Add Shopping exception mapping tests and preserve current public status/message behavior.

## Migration Plan

1. Add `PositiveAmount`, Shopping values, exception hierarchy, `ShoppingItem`, and `ShoppingList` aggregate behavior with focused Domain tests.
2. Add Shopping Application read models and `IShoppingListRepository`; convert only Shopping-owned handlers to concrete exception-based flows.
3. Isolate the preserved `RecordShoppingList` purchase workflow behind its transitional compatibility boundary.
4. Adapt `PostgreSqlShoppingRepository` to aggregate load/save mapping and preserve item soft deletion and the existing schema.
5. Adapt GrpcApi registration, response mapping, and Shopping exception mapping without proto changes.
6. Update focused Domain, Application, Database, and GrpcApi tests, then run `dotnet build server/Metaspesa.slnx` and `dotnet test server/Metaspesa.slnx`.

Rollback is code-only: revert Shopping domain, handlers, abstractions, adapter mapping, and exception mapping. No database, proto, generated-code, configuration, or deployment rollback is required.

## Open Questions

- The next Purchasing proposal will decide the final command name and ownership for the current `RecordShoppingList` endpoint and its compatibility abstraction.
- A later public-contract slice will decide whether external `reference_uid` and `product_reference_uid` fields become `product_format_uid`.
