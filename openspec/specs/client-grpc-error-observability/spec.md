# client-grpc-error-observability Specification

## Purpose

Ensure client-side gRPC read failures are observable through structured OpenTelemetry logs and are propagated to callers instead of being represented as successful empty results.

## Requirements
### Requirement: Client gRPC read failures are logged

Client gRPC adapters SHALL emit a structured OpenTelemetry error log when a shopping or market read operation fails because of a gRPC error or invalid missing response.

#### Scenario: Shopping read fails

- **WHEN** a shopping gRPC read operation fails
- **THEN** the client SHALL emit an OpenTelemetry error log for the failed shopping operation
- **AND** the log SHALL include safe operation metadata without auth tokens or raw request metadata

#### Scenario: Market read fails

- **WHEN** a market gRPC read operation fails
- **THEN** the client SHALL emit an OpenTelemetry error log for the failed market operation
- **AND** the log SHALL include safe operation metadata without auth tokens or raw request metadata

### Requirement: Client gRPC read failures are propagated

Client gRPC adapters SHALL rethrow logged shopping and market read failures so callers can distinguish failures from successful empty results.

#### Scenario: Backend returns empty data successfully

- **WHEN** a shopping or market read operation succeeds with no data
- **THEN** the client SHALL return the mapped empty result
- **AND** the client SHALL NOT emit an error log

#### Scenario: Backend read fails

- **WHEN** a shopping or market read operation fails
- **THEN** the client SHALL rethrow the original failure after logging it
- **AND** the client SHALL NOT return an empty fallback result

### Requirement: API routes expose explicit read failures

Client API routes that depend on shopping or market gRPC reads SHALL return explicit error responses when propagated read failures prevent them from producing valid data.

#### Scenario: Market products cannot be loaded

- **WHEN** the market products API route receives a propagated gRPC read failure
- **THEN** the route SHALL return a non-success HTTP response
- **AND** the response body SHALL contain a stable user-facing error message

#### Scenario: Shopping lists cannot be loaded

- **WHEN** the shopping lists API route receives a propagated gRPC read failure
- **THEN** the route SHALL return a non-success HTTP response
- **AND** the response body SHALL contain a stable user-facing error message
