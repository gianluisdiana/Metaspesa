# domain-markets Specification

## Purpose
Define Markets bounded-context aggregates, value objects, domain exceptions, persistence boundaries, and application behavior while preserving external contracts.
## Requirements
### Requirement: Market aggregate
The Domain layer SHALL provide a Markets `Market` aggregate root composed of `MarketId`, `MarketName`, and an optional Markets `ImageUrl`, without owning Product aggregates.

#### Scenario: Creating a valid market
- **WHEN** Domain code creates a Market from a valid ID, name, and optional logo URL
- **THEN** the Market exposes the validated values

#### Scenario: Keeping products outside market
- **WHEN** a Market aggregate is loaded
- **THEN** it does not load or own Product aggregates

### Requirement: Product aggregate
The Domain layer SHALL provide a Markets `Product` aggregate root composed of `ProductId`, `ProductName`, `BrandName`, `MarketId`, and a bounded collection of Product formats.

#### Scenario: Creating a valid product
- **WHEN** Domain code creates a Product from valid identity, name, brand, and Market reference values
- **THEN** the Product exposes those values and its bounded format collection

#### Scenario: Referencing market by id
- **WHEN** a Product belongs to a Market
- **THEN** the Product stores only `MarketId` and does not hold a Market object reference

#### Scenario: Excluding price history
- **WHEN** a Product aggregate is loaded
- **THEN** neither the Product nor its formats contain PriceSnapshot history or a current price

### Requirement: Product format ownership
The Product aggregate SHALL own `ProductFormat` entities identified by `ProductFormatId`, with shared-kernel `Quantity` and an optional Markets `ImageUrl`.

#### Scenario: Adding a format
- **WHEN** a valid format ID, quantity, and optional image URL are added to a Product
- **THEN** the Product contains the new ProductFormat

#### Scenario: Rejecting duplicate format
- **WHEN** Domain code adds a format whose ID already exists in the Product
- **THEN** the operation throws the specific duplicate-product-format exception and leaves the aggregate unchanged

#### Scenario: Updating a format
- **WHEN** Domain code updates an existing format with a valid quantity and optional image URL
- **THEN** that ProductFormat stores the new values

#### Scenario: Rejecting unknown format update
- **WHEN** Domain code updates a format ID not owned by the Product
- **THEN** the operation throws the specific product-format-not-found exception and leaves the aggregate unchanged

### Requirement: Append-only price snapshots
The Markets module SHALL model each price observation as an independently persisted `PriceSnapshot` composed of `PriceSnapshotId`, `ProductFormatId`, shared-kernel `Money`, and `ObservedAt`.

#### Scenario: Creating a price snapshot
- **WHEN** Domain code creates a snapshot from valid identity, format reference, money, and observation time
- **THEN** the PriceSnapshot exposes those immutable observation values

#### Scenario: Referencing format by id
- **WHEN** a PriceSnapshot identifies its observed format
- **THEN** it stores only `ProductFormatId` and does not hold a Product or ProductFormat object reference

#### Scenario: Appending snapshot
- **WHEN** Application records a valid new price observation
- **THEN** persistence inserts a new PriceSnapshot and does not update an existing snapshot

#### Scenario: Loading product independently
- **WHEN** persistence loads a Product aggregate
- **THEN** it does not load PriceSnapshot records

### Requirement: Markets value objects
The Markets module SHALL provide value objects for `MarketId`, `ProductId`, `ProductFormatId`, `PriceSnapshotId`, `MarketName`, `ProductName`, `BrandName`, and `ImageUrl`.

#### Scenario: Creating valid market values
- **WHEN** Domain code creates Markets value objects from valid values
- **THEN** each object exposes its normalized value and compares by value

#### Scenario: Rejecting invalid id
- **WHEN** Domain code creates any Markets ID from a default or non-positive persistence identifier
- **THEN** creation throws the concrete invalid-ID exception for that ID type

#### Scenario: Rejecting invalid name
- **WHEN** Domain code creates a market, product, or brand name from null, empty, or whitespace text
- **THEN** creation throws the concrete invalid-name exception for that value type

#### Scenario: Rejecting invalid image URL
- **WHEN** Domain code creates `ImageUrl` from a relative or malformed URL
- **THEN** creation throws the specific invalid-image-URL exception

### Requirement: Markets domain exception hierarchy
Every Markets-specific invalid value or aggregate operation SHALL throw a dedicated concrete exception derived from `MarketDomainException`, which SHALL derive from the shared-kernel `DomainException`.

#### Scenario: Diagnosing an invalid value
- **WHEN** a Markets ID, name, image URL, or observation timestamp is invalid
- **THEN** the thrown concrete exception identifies that exact error kind without message parsing

#### Scenario: Diagnosing an invalid aggregate operation
- **WHEN** Product format behavior fails because a format is duplicate or missing
- **THEN** the thrown concrete exception identifies that exact operation failure

#### Scenario: Preserving shared-kernel errors
- **WHEN** Money, Quantity, or UnitOfMeasure validation fails while constructing a Markets model
- **THEN** the corresponding shared-kernel concrete exception is preserved

### Requirement: Aggregate-oriented Markets persistence
Application persistence abstractions SHALL separate Market, Product, and PriceSnapshot responsibilities and Database adapters SHALL map their typed values to the existing schema.

#### Scenario: Persisting aggregate values
- **WHEN** a Market, Product, ProductFormat, or PriceSnapshot is saved
- **THEN** the Database adapter stores its primitive values in existing columns without a schema migration

#### Scenario: Loading aggregate values
- **WHEN** an existing Market or Product is loaded
- **THEN** the Database adapter reconstructs its Markets value objects and aggregate state

#### Scenario: Cancelling a partial import
- **WHEN** a Market product import is cancelled after writing only part of its operation
- **THEN** the existing rollback workflow removes only records created by that operation

#### Scenario: Isolating snapshot persistence
- **WHEN** PriceSnapshot observations are read, appended, or removed during rollback
- **THEN** a dedicated PriceSnapshot repository adapter performs those operations rather than the Market/Product adapter

#### Scenario: Isolating every persistence interface
- **WHEN** Market, Market Product, and PriceSnapshot persistence abstractions are registered
- **THEN** each abstraction resolves to a separate database repository class that implements only that interface

### Requirement: Market application boundaries
Market use cases SHALL expose concrete handler classes, and `GetMarketProductsFilter` SHALL enforce its finite pagination invariants when constructed.

#### Scenario: Using concrete handlers
- **WHEN** Market use cases are registered and consumed by GrpcApi
- **THEN** GrpcApi depends directly on the concrete Market handler classes rather than command/query handler interfaces

#### Scenario: Rejecting invalid filter pagination
- **WHEN** a Market product filter is created with a non-positive finite page index or page size
- **THEN** construction throws an argument-out-of-range exception identifying the invalid pagination member

#### Scenario: Avoiding duplicate validation
- **WHEN** GetMarketProducts handles a constructed filter
- **THEN** it does not invoke a separate FluentValidation validator

#### Scenario: Handling Market use cases without Result
- **WHEN** a Market command or query succeeds
- **THEN** its concrete handler completes without a result wrapper and returns only the query value when applicable

#### Scenario: Propagating Market failures
- **WHEN** a Market command or query encounters an application or domain rule failure
- **THEN** it throws the concrete Market exception for that failure instead of returning domain errors

#### Scenario: Validating product imports without FluentValidation
- **WHEN** AddMarketProducts receives an empty import, old registration date, duplicate product identity, unsupported unit, or invalid domain value
- **THEN** the handler throws the corresponding concrete Market or Shared Kernel exception without invoking a FluentValidation validator

### Requirement: Markets migration boundaries
This Markets migration slice SHALL preserve external Market contracts and SHALL NOT migrate other bounded contexts.

#### Scenario: Preserving gRPC contracts
- **WHEN** the Markets module migration is implemented
- **THEN** Market `.proto` files, public request and response field names, and generated client types remain unchanged

#### Scenario: Avoiding cross-context migration
- **WHEN** the Markets module migration is implemented
- **THEN** Shopping and Purchasing behavior remain unchanged except for compile-preserving adapter use of Markets IDs

#### Scenario: Avoiding infrastructure changes
- **WHEN** the Markets module migration is implemented
- **THEN** database schema, migrations, scraper, client, Docker/Compose, observability configuration, and generated files remain unchanged
