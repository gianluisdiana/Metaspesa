## Why

Purchase recording still lacks its own bounded context and domain model. This final migration adds the Purchasing module from `plan.md`, separating immutable purchase receipts from Shopping planning state while reusing already implemented Identity, Markets, Shopping, and Shared Kernel types.

## What Changes

- Add a Purchasing module with immutable `Purchase` and `PurchaseItem` domain types, a `PurchaseId` value object, and dedicated Purchasing domain exceptions.
- Move the existing checkout workflow into a concrete exception-based Purchasing handler that loads an owner-accessible ShoppingList, resolves exact paid PriceSnapshot references, creates a Purchase, resets checked Shopping items, and commits once.
- Add minimal Purchasing-owned application abstractions for Purchase persistence and checkout price resolution.
- Add a dedicated PostgreSQL Purchase repository and Purchasing checkout data adapter over the existing purchase and price-snapshot tables.
- Replace the transitional Result-based purchase-recording implementation after the Purchasing workflow is active.
- Preserve existing Shopping and Markets domain models, public gRPC/proto contracts, client behavior, database schema, migrations, and external checkout behavior.
- Do not add purchase history queries, receipt screens, mutable purchase operations, refunds, totals, currencies, speculative domain methods, or unrelated infrastructure changes.

## Capabilities

### New Capabilities
- `domain-purchasing`: Purchasing aggregate/value/exception rules, checkout orchestration, aggregate persistence, immutability, and transport failure mapping.

### Modified Capabilities

None.

## Impact

- **Server Domain/Application:** new `Domain/Purchasing`, `Application/Purchasing`, and Purchasing application abstractions; existing Identity, Markets, Shopping, and Shared Kernel types are consumed without changing their specifications.
- **Server Database:** dedicated Purchasing adapters use existing `purchasing.purchases`, `purchasing.purchase_items`, and Markets price-snapshot data; no schema or migration change.
- **GrpcApi:** existing `ShoppingService.RecordShoppingList` delegates to the Purchasing handler while retaining its request, response, authorization, and public status behavior.
- **Client/protos:** unchanged.
- **Data integrity:** checkout resolves immutable snapshot IDs before creating one immutable Purchase and resets the source list in the same unit-of-work commit. Failure or cancellation must not report partial success.
- **Security/privacy:** buyer IDs, list contents, authorization metadata, snapshot IDs, and raw exception details must not be added to logs or telemetry.
- **Unaffected:** scraper, Docker/Compose, observability configuration, and existing bounded-context behavior outside the checkout integration point.
