# domain-shopping Specification

## Purpose
Define the DDD boundaries and behavior for shopping-list planning, including typed domain state, exception-based application workflows, aggregate persistence, and stable gRPC integration while keeping purchase recording outside Shopping.
## Requirements
### Requirement: Shopping list aggregate
The Domain layer SHALL provide a Shopping `ShoppingList` aggregate root composed of `ShoppingListId`, one or more Identity `UserId` owner references, an optional `ShoppingListName`, temporary and deletion state, and a bounded collection of Shopping items.

#### Scenario: Creating a named list
- **WHEN** Domain code creates a list with a valid ID, at least one valid owner ID, and a valid name
- **THEN** the ShoppingList exposes the validated state and is not temporary

#### Scenario: Creating a temporary list
- **WHEN** Domain code creates a list without a name
- **THEN** the ShoppingList represents a temporary list without manufacturing a name value

#### Scenario: Rejecting a list without owners
- **WHEN** Domain code creates a ShoppingList without an owner
- **THEN** creation throws the dedicated missing-owner exception

#### Scenario: Referencing owners by id
- **WHEN** a ShoppingList represents its owners
- **THEN** it stores only Identity `UserId` values and does not hold User aggregate references

### Requirement: Shopping list values
The Shopping module SHALL provide `ShoppingListId` and `ShoppingListName` value objects, and Shared Kernel SHALL provide a positive integer amount value used by Shopping items.

#### Scenario: Creating valid Shopping values
- **WHEN** Domain code creates Shopping values from a positive persisted list ID and a non-empty name
- **THEN** each value exposes its normalized value and compares by value

#### Scenario: Rejecting an invalid list id
- **WHEN** Domain code creates a ShoppingListId from a default or non-positive persistence identifier
- **THEN** creation throws the dedicated invalid-shopping-list-ID exception

#### Scenario: Rejecting an invalid list name
- **WHEN** Domain code creates a ShoppingListName from null, empty, or whitespace text
- **THEN** creation throws the dedicated invalid-shopping-list-name exception

#### Scenario: Rejecting a non-positive amount
- **WHEN** Domain code creates a planned-item amount from zero or a negative integer
- **THEN** creation throws the dedicated shared-kernel invalid-positive-amount exception

### Requirement: Shopping item ownership
The ShoppingList aggregate SHALL own ShoppingItem entities identified within the list by Markets `ProductFormatId`, with a shared positive amount and checked state.

#### Scenario: Referencing a product format by id
- **WHEN** a ShoppingItem identifies the planned market format
- **THEN** it stores only `ProductFormatId` and does not hold Product, ProductFormat, Market, or PriceSnapshot objects

#### Scenario: Adding an item
- **WHEN** Application adds a valid ProductFormat reference, positive amount, and checked state to a list that does not contain that format
- **THEN** the ShoppingList contains the new ShoppingItem

#### Scenario: Rejecting a duplicate item
- **WHEN** Domain code adds a ProductFormatId already present in the ShoppingList
- **THEN** the operation throws the dedicated duplicate-shopping-item exception and leaves the aggregate unchanged

#### Scenario: Updating an item
- **WHEN** Domain code updates an existing item with a provided valid amount, checked state, or both
- **THEN** the ShoppingItem stores both requested changes atomically

#### Scenario: Rejecting an empty item update
- **WHEN** Domain code updates an item without providing an amount or checked state
- **THEN** the operation throws the dedicated empty-item-update exception and leaves the aggregate unchanged

#### Scenario: Rejecting an unknown item update
- **WHEN** Domain code updates a ProductFormatId not present in the ShoppingList
- **THEN** the operation throws the dedicated shopping-item-not-found exception and leaves the aggregate unchanged

#### Scenario: Removing an item
- **WHEN** Domain code removes a ProductFormatId present in the ShoppingList
- **THEN** that item is no longer part of the aggregate's active item collection

#### Scenario: Rejecting an unknown item removal
- **WHEN** Domain code removes a ProductFormatId not present in the ShoppingList
- **THEN** the operation throws the dedicated shopping-item-not-found exception and leaves the aggregate unchanged

### Requirement: Shopping list planning behavior
The ShoppingList aggregate SHALL support the list-level state transitions required by current Shopping planning workflows and SHALL NOT own checkout or purchase behavior.

#### Scenario: Renaming a list
- **WHEN** Domain code renames a temporary or named list with a valid ShoppingListName
- **THEN** the list stores the new name and is not temporary

#### Scenario: Selecting checked items
- **WHEN** a ShoppingList contains checked and unchecked items
- **THEN** its checked-item selection contains only the checked items without mutating the aggregate

#### Scenario: Resetting checked items
- **WHEN** Domain code resets a ShoppingList with checked items
- **THEN** every active item remains in the list and has unchecked state

#### Scenario: Excluding checkout ownership
- **WHEN** a ShoppingList is loaded or changed
- **THEN** it contains no Purchase, PurchaseItem, paid price, current price, or PriceSnapshot reference

#### Scenario: Avoiding unused domain operations
- **WHEN** the Shopping migration is implemented
- **THEN** the aggregate exposes only state transitions required by an existing Shopping workflow and does not add speculative diagram operations

### Requirement: Shopping domain exception hierarchy
Every Shopping-specific invalid value or aggregate operation SHALL throw a dedicated concrete exception derived from `ShoppingDomainException`, which SHALL derive from the shared-kernel `DomainException`.

#### Scenario: Diagnosing an invalid Shopping value
- **WHEN** a Shopping list ID, name, owner set, or aggregate operation is invalid
- **THEN** the concrete exception identifies that exact error kind without message parsing

#### Scenario: Diagnosing list lookup and conflict failures
- **WHEN** Application cannot find an owner-accessible list or detects an owner-scoped list-name conflict
- **THEN** it throws the dedicated Shopping exception for the exact failure

#### Scenario: Preserving referenced value errors
- **WHEN** UserId, ProductFormatId, or a shared-kernel amount is invalid while constructing Shopping state
- **THEN** the corresponding Identity, Markets, or Shared Kernel concrete exception is preserved

### Requirement: Shopping application boundaries
Shopping-owned use cases SHALL use concrete handler classes, SHALL return values directly without `Result` wrappers, and SHALL enforce failures through concrete exceptions and aggregate behavior.

#### Scenario: Creating a list
- **WHEN** a valid create-list command is handled and no owner-scoped conflict exists
- **THEN** Application creates a ShoppingList aggregate, adds it through the ShoppingList repository, commits once, and completes without returning a Domain model

#### Scenario: Rejecting a conflicting list
- **WHEN** the owner already has the requested named or temporary list
- **THEN** Application throws the dedicated list-conflict exception and does not add or commit another list

#### Scenario: Mutating a list
- **WHEN** an add-item, update-item, remove-item, or rename command is valid
- **THEN** Application loads the owner-accessible aggregate, invokes its domain behavior, persists it, and commits once

#### Scenario: Rejecting a missing list
- **WHEN** a Shopping-owned command or query targets a list unavailable to the requesting owner
- **THEN** Application throws the dedicated list-not-found exception and does not mutate or commit state

#### Scenario: Rejecting an unknown product format
- **WHEN** an add-item command references a ProductFormatId absent from the Markets query boundary
- **THEN** Application throws the dedicated missing-format exception and leaves the ShoppingList unchanged

#### Scenario: Avoiding duplicate validation paths
- **WHEN** a Shopping-owned handler receives a command
- **THEN** it constructs validated values and invokes aggregate behavior without a separate FluentValidation validator

#### Scenario: Using concrete handlers without Result
- **WHEN** GrpcApi invokes a Shopping-owned use case
- **THEN** it depends directly on the concrete handler and receives either the direct query value, command completion, or a thrown concrete exception

### Requirement: Shopping query read models
Shopping queries SHALL return Application read models and SHALL compose Market display data outside the Shopping aggregate.

#### Scenario: Getting list summaries
- **WHEN** Application retrieves lists accessible to an owner
- **THEN** it returns Shopping summary read models and does not return Domain aggregates to GrpcApi

#### Scenario: Getting list details
- **WHEN** Application retrieves an owner-accessible list with items
- **THEN** it enriches item ProductFormatId references through the Markets query boundary and returns the existing response data as an Application read model

#### Scenario: Getting an empty list
- **WHEN** an owner-accessible ShoppingList has no active items
- **THEN** its detail read model contains an empty item collection

### Requirement: Aggregate-oriented Shopping persistence
Application SHALL expose an aggregate-oriented `IShoppingListRepository`, and Database SHALL map Shopping aggregates and typed values to the existing schema.

#### Scenario: Persisting a new aggregate
- **WHEN** Application adds a new ShoppingList
- **THEN** Database stores its owner relationships, optional name, temporary state, deletion state, and items in existing tables without a schema migration

#### Scenario: Loading an aggregate
- **WHEN** Application requests a list accessible to an owner
- **THEN** Database reconstructs its ShoppingListId, owner UserIds, optional ShoppingListName, state, and active ShoppingItems through validated domain values

#### Scenario: Persisting aggregate changes
- **WHEN** Application saves an aggregate after rename, add, update, or remove behavior
- **THEN** Database persists the resulting aggregate state without exposing field-level mutation methods on the Application repository

#### Scenario: Preserving item soft deletion
- **WHEN** an item is removed from the aggregate and saved
- **THEN** Database records the existing item deletion timestamp through `IClock` and excludes that row from later active aggregate loads

#### Scenario: Propagating cancellation
- **WHEN** a repository read or unit-of-work commit is cancelled
- **THEN** cancellation propagates and no retry is performed by the Shopping handler

### Requirement: Preserved Purchasing compatibility boundary
This Shopping migration SHALL leave the current mixed `RecordShoppingList` purchase workflow behaviorally unchanged and isolated from the new Shopping aggregate repository until the Purchasing migration.

#### Scenario: Recording a checked list during transition
- **WHEN** the existing RecordShoppingList workflow succeeds during this migration stage
- **THEN** it continues resolving latest price snapshots, writing purchase rows, and resetting checked list items with its existing external behavior

#### Scenario: Keeping purchase persistence separate
- **WHEN** the new IShoppingListRepository is defined
- **THEN** it does not expose purchase creation, latest-price resolution, or purchase-item persistence methods

#### Scenario: Retaining the transitional handler boundary
- **WHEN** Shopping-owned handlers move to concrete exception-based invocation
- **THEN** RecordShoppingList may retain its current handler and Result boundary for removal by the next Purchasing migration

### Requirement: Shopping migration boundaries
This Shopping migration SHALL preserve external Shopping contracts and SHALL NOT migrate Purchasing or unrelated services.

#### Scenario: Preserving gRPC contracts
- **WHEN** the Shopping module migration is implemented
- **THEN** Shopping `.proto` files, public request and response field names, status behavior, and generated client types remain unchanged

#### Scenario: Mapping expected Shopping failures
- **WHEN** a Shopping-owned handler throws an expected Shopping exception
- **THEN** GrpcApi maps it to the corresponding stable public gRPC failure without exposing stack traces or persistence details

#### Scenario: Avoiding cross-context migration
- **WHEN** the Shopping module migration is implemented
- **THEN** Purchasing remains unmodeled and Identity and Markets behavior remain unchanged except for use of their existing typed IDs and query boundaries

#### Scenario: Avoiding infrastructure changes
- **WHEN** the Shopping module migration is implemented
- **THEN** database schema, migrations, client, scraper, Docker/Compose, observability configuration, and generated files remain unchanged

#### Scenario: Protecting telemetry data
- **WHEN** Shopping operations are traced or logged
- **THEN** telemetry does not add owner IDs, list contents, authorization metadata, or raw internal exception details
