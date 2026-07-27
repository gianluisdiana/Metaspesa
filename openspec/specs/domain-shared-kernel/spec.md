# domain-shared-kernel Specification

## Purpose
TBD - created by archiving change migration-ddd-kernel. Update Purpose after archive.
## Requirements
### Requirement: Currency-free money value object
The Domain layer SHALL provide a shared `Money` value object that represents an amount without storing currency.

#### Scenario: Creating money from a valid amount
- **WHEN** domain code creates `Money` from a valid non-negative decimal amount
- **THEN** the value object exposes the amount without requiring or storing a currency code

#### Scenario: Rejecting invalid money
- **WHEN** domain code attempts to create `Money` from a negative amount
- **THEN** the value object creation fails with a specific domain exception before the invalid amount can be used

#### Scenario: Comparing money values
- **WHEN** two `Money` values contain the same amount
- **THEN** the values compare as equal

### Requirement: Shared quantity value object
The Domain layer SHALL provide a shared `Quantity` value object that represents a positive amount and a unit of measure.

#### Scenario: Creating quantity from valid amount and unit
- **WHEN** domain code creates `Quantity` from a positive amount and valid `UnitOfMeasure`
- **THEN** the value object exposes both values without depending on Application, Database, GrpcApi, client, scraper, or infrastructure code

#### Scenario: Rejecting invalid quantity amount
- **WHEN** domain code attempts to create `Quantity` from zero or a negative amount
- **THEN** the value object creation fails with a specific domain exception before the invalid quantity can be used

#### Scenario: Comparing quantity values
- **WHEN** two `Quantity` values contain the same amount and unit of measure
- **THEN** the values compare as equal

### Requirement: Shared unit of measure value object
The Domain layer SHALL provide a shared `UnitOfMeasure` value object for generic measurement units used by shared quantities.

#### Scenario: Creating unit of measure from valid value
- **WHEN** domain code creates `UnitOfMeasure` from a valid non-empty unit value
- **THEN** the value object exposes the normalized unit value without owning product, package, or market-specific behavior

#### Scenario: Rejecting invalid unit of measure
- **WHEN** domain code attempts to create `UnitOfMeasure` from an empty or unsupported value
- **THEN** the value object creation fails with a specific domain exception before the invalid unit can be used

#### Scenario: Comparing unit of measure values
- **WHEN** two `UnitOfMeasure` values represent the same unit
- **THEN** the values compare as equal

### Requirement: Shared domain exception hierarchy
The Domain layer SHALL provide a base `DomainException` and specific exception types for each shared kernel validation error.

#### Scenario: Raising specific validation exception
- **WHEN** a shared kernel value object rejects invalid input
- **THEN** it raises a specific exception type derived from `DomainException`

#### Scenario: Debugging domain validation failures
- **WHEN** domain code catches a shared kernel validation failure
- **THEN** the exception type identifies the failed validation kind without requiring message parsing

### Requirement: Shared kernel scope boundaries
The shared kernel SHALL contain only value objects that are valid across domain modules and SHALL exclude market-specific, application, persistence, transport, client, scraper, and observability concerns.

#### Scenario: Excluding bounded-context identifiers
- **WHEN** the shared kernel module is implemented
- **THEN** it does not define `UserId`, `ProductFormatId`, `PriceSnapshotId`, `ShoppingListId`, or other bounded-context-specific identifier value objects

#### Scenario: Excluding market-specific image URLs
- **WHEN** the shared kernel module is implemented
- **THEN** it does not define an `ImageUrl` value object or otherwise move image URL behavior out of the Markets module

#### Scenario: Keeping the change additive
- **WHEN** this kernel migration slice is implemented
- **THEN** existing Application handlers, repositories, persistence mappings, gRPC contracts, client code, scraper code, Docker/Compose configuration, and telemetry configuration remain behaviorally unchanged

