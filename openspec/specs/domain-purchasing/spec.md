# domain-purchasing Specification

## Purpose
Define immutable purchase receipt state and exception-based checkout orchestration that records exact paid price snapshots while preserving Shopping, Markets, and persistence boundaries.

## Requirements
### Requirement: Immutable Purchase aggregate
The Purchasing module SHALL provide an immutable `Purchase` aggregate composed of an optional persisted `PurchaseId`, optional Identity `UserId` buyer reference, optional Shopping `ShoppingListId` source reference, a UTC purchase timestamp, and one or more PurchaseItems.

#### Scenario: Creating a purchase
- **WHEN** Purchasing creates a receipt from a buyer, source list, non-empty valid lines, and a valid UTC timestamp
- **THEN** the Purchase exposes that state and no mutation operation is available

#### Scenario: Preserving deleted-reference compatibility
- **WHEN** persistence reconstructs a Purchase whose user or source list was deleted
- **THEN** the Purchase accepts the corresponding nullable ID reference without loading a User or ShoppingList aggregate

#### Scenario: Rejecting an empty purchase
- **WHEN** Purchasing creates a Purchase without lines
- **THEN** creation throws the dedicated empty-purchase-items exception

#### Scenario: Rejecting duplicate purchase lines
- **WHEN** Purchasing creates a Purchase with more than one line for the same PriceSnapshotId
- **THEN** creation throws the dedicated duplicate-purchase-item exception

### Requirement: Purchase values and items
The Purchasing module SHALL provide a positive `PurchaseId` value object and immutable PurchaseItems containing only a Markets `PriceSnapshotId` and shared-kernel `PositiveAmount`.

#### Scenario: Creating valid purchase values
- **WHEN** Domain code creates a PurchaseId and PurchaseItem from valid values
- **THEN** each exposes typed values and PurchaseId compares by value

#### Scenario: Rejecting an invalid purchase id
- **WHEN** Domain code creates PurchaseId from a default or non-positive persistence identifier
- **THEN** creation throws the dedicated invalid-purchase-ID exception

#### Scenario: Referencing exact paid price
- **WHEN** a PurchaseItem represents a bought shopping line
- **THEN** it stores the exact PriceSnapshotId and does not store ProductFormatId, Product, ProductFormat, current price, or Market objects

#### Scenario: Preserving referenced value failures
- **WHEN** PriceSnapshotId or PositiveAmount is invalid while constructing a PurchaseItem
- **THEN** the corresponding Markets or Shared Kernel concrete exception is preserved

### Requirement: Purchasing domain exception hierarchy
Every Purchasing-specific invalid value, aggregate invariant, or expected checkout failure SHALL throw a dedicated concrete exception derived from `PurchaseDomainException`, which SHALL derive from shared-kernel `DomainException`.

#### Scenario: Diagnosing invalid purchase state
- **WHEN** a Purchase ID, timestamp, line collection, or line uniqueness invariant is invalid
- **THEN** the concrete exception identifies the exact error kind without message parsing

#### Scenario: Diagnosing missing checkout price
- **WHEN** checkout cannot resolve a latest PriceSnapshot for a checked ProductFormatId
- **THEN** it throws the dedicated Purchasing missing-snapshot exception before persisting or resetting state

### Requirement: Purchasing checkout orchestration
Purchasing Application SHALL expose a concrete exception-based checkout handler that coordinates Shopping, Markets, Purchasing persistence, and one unit-of-work commit without a validator, `Result`, or generic command-handler interface.

#### Scenario: Checking out a shopping list
- **WHEN** an owner-accessible ShoppingList contains checked items and every checked ProductFormatId has a latest PriceSnapshot
- **THEN** the handler creates one Purchase with the buyer ID, source ShoppingListId, exact latest snapshot IDs, checked amounts, and `IClock` timestamp, resets checked items, and commits once

#### Scenario: Rejecting a missing shopping list
- **WHEN** checkout cannot load the requested list for the authenticated owner
- **THEN** the existing exact Shopping list-not-found exception propagates and no Purchase, reset, or commit occurs

#### Scenario: Rejecting a list without checked items
- **WHEN** checkout loads a list with no checked items
- **THEN** the dedicated empty-purchase-items exception is thrown and no snapshot query, Purchase, reset, or commit occurs

#### Scenario: Rejecting a missing latest snapshot
- **WHEN** at least one checked ProductFormatId has no PriceSnapshot
- **THEN** checkout throws the dedicated missing-snapshot exception and does not add a Purchase, reset the list, or commit

#### Scenario: Propagating cancellation
- **WHEN** list loading, snapshot lookup, aggregate persistence, or commit is cancelled
- **THEN** cancellation propagates without retry and checkout does not report success

### Requirement: Aggregate-oriented Purchase persistence
Application SHALL expose a minimal `IPurchaseRepository`, and Database SHALL implement it in a dedicated Purchase repository that maps aggregate state to the existing Purchasing schema without selecting prices, resetting lists, or committing independently.

#### Scenario: Persisting a purchase
- **WHEN** checkout adds a valid Purchase and commits the unit of work
- **THEN** Database stores one purchase header and its lines with exact buyer, optional list, timestamp, PriceSnapshotId, and amount values

#### Scenario: Preserving nullable references
- **WHEN** an associated user or shopping list is deleted after checkout
- **THEN** the existing database delete behavior retains the Purchase and clears only the corresponding nullable foreign key

#### Scenario: Avoiding schema migration
- **WHEN** Purchasing aggregate persistence is introduced
- **THEN** existing purchase tables, columns, keys, indexes, constraints, and delete behaviors remain unchanged

#### Scenario: Isolating repository ownership
- **WHEN** Shopping and Purchasing repositories are registered
- **THEN** each resolves to a separate database repository class implementing only its bounded-context interface

### Requirement: Purchasing transport integration
The existing Shopper-authorized checkout RPC SHALL invoke the concrete Purchasing handler and map expected Purchasing exceptions to stable gRPC failures without exposing internal or sensitive data.

#### Scenario: Invoking checkout
- **WHEN** an authenticated Shopper records a named or temporary shopping list
- **THEN** GrpcApi passes the authenticated user ID and optional list name to the Purchasing handler and returns the existing empty success response

#### Scenario: Mapping expected failure
- **WHEN** the Purchasing handler throws an expected Purchasing exception
- **THEN** GrpcApi returns the corresponding stable status and sanitized message without a stack trace, SQL detail, buyer ID, list content, or snapshot ID

#### Scenario: Preserving authorization
- **WHEN** a caller invokes checkout without the Shopper role
- **THEN** existing gRPC authorization rejects the call before Purchasing behavior executes

### Requirement: Purchasing migration boundaries
This migration SHALL complete Purchasing ownership and current checkout behavior without adding speculative receipt features or unrelated infrastructure changes.

#### Scenario: Avoiding speculative purchase behavior
- **WHEN** the Purchasing module is implemented
- **THEN** it adds no purchase history query, receipt UI, refund, edit, delete, total, currency, or duplicated product metadata behavior

#### Scenario: Avoiding unrelated system changes
- **WHEN** the Purchasing module is implemented
- **THEN** scraper, Docker/Compose, observability configuration, and unrelated bounded-context behavior remain unchanged

#### Scenario: Protecting telemetry data
- **WHEN** checkout is traced or logged
- **THEN** telemetry adds no buyer IDs, list names or contents, authorization metadata, snapshot IDs, or raw internal exception details
