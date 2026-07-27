## Why

The DDD migration needs a stable domain kernel before aggregates and use cases can move to bounded-context-specific modules. This change creates shared value objects and base domain exception structure first without introducing bounded-context IDs too early.

## What Changes

- Add the initial shared kernel domain module for reusable, dependency-free value objects.
- Introduce shared value objects that are valid across domain modules: `Money`, `Quantity`, and `UnitOfMeasure`.
- Keep `Money` currency-free.
- Introduce a base `DomainException` and specific exception classes for each shared kernel validation error.
- Keep typed identifiers out of the shared kernel because IDs belong to their bounded contexts.
- Keep market-specific concepts such as `ImageUrl` out of the shared kernel.
- Add focused domain tests for shared kernel validation, equality, and exception behavior.
- Do not refactor Identity, Markets, Shopping, Purchasing, Application handlers, persistence mappings, gRPC contracts, or client code in this slice.

## Capabilities

### New Capabilities
- `domain-shared-kernel`: Shared domain kernel value objects and base domain exceptions used by later DDD migration slices.

### Modified Capabilities

## Impact

- Affected service: `server/` only.
- Affected layer: Domain project and nearest unit tests.
- No gRPC contract changes.
- No database schema or migration changes.
- No Docker, Compose, observability, scraper, or client changes.
- No generated files.
- Backward compatibility risk is limited to server compile-time references if existing code chooses to adopt the new value objects or exceptions later; this change does not require those adoptions.
- Data integrity and debugging improve by providing explicit shared validation rules and specific domain exception types for later aggregate boundaries.
