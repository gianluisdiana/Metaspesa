## Why

Client-side gRPC adapters currently swallow some backend and transport failures by returning empty shopping or market data. This hides operational failures from OpenTelemetry and can make users see a valid empty state when the backend actually failed.

## What Changes

- Add structured OpenTelemetry error logging around client gRPC read failures.
- Propagate logged gRPC failures so API routes and UI flows can treat them as errors instead of successful empty responses.
- Preserve successful empty results for genuine backend responses with no data.
- Add focused tests that verify failures are logged and rethrown by the client gRPC adapters.

## Capabilities

### New Capabilities

- `client-grpc-error-observability`: Client gRPC adapters log read failures through OpenTelemetry and propagate failures to callers.

### Modified Capabilities

## Impact

- `client/src/infrastructure/grpc-api-service.ts`: log and rethrow shopping gRPC read failures.
- `client/src/infrastructure/grpc-market-api-service.ts`: log and rethrow market gRPC read failures.
- `client/src/app/api/**`: ensure propagated failures produce explicit API errors where needed.
- `client/test/unit/**`: add adapter coverage for failure logging and propagation.
