## Context

The Next.js client has server-only gRPC adapters for shopping and market data. Some read methods catch every gRPC or response-mapping failure and return empty data, which makes backend outages, authentication issues, and malformed responses indistinguishable from successful empty results.

The client already initializes OpenTelemetry logs, traces, and metrics in `client/src/instrumentation.ts`. `GrpcAuthService` already uses `@opentelemetry/api-logs` to emit error logs before rethrowing authentication failures.

## Goals / Non-Goals

**Goals:**

- Make shopping and market gRPC read failures observable through structured OpenTelemetry error logs.
- Preserve original errors by rethrowing after logging.
- Keep genuine successful empty responses represented as empty arrays or empty result objects.
- Keep user-facing and HTTP response decisions outside the low-level gRPC adapters.
- Cover logging and propagation with focused unit tests.

**Non-Goals:**

- Add a new logging dependency or replace the existing OpenTelemetry setup.
- Redesign all client error handling or every React error state.
- Change backend gRPC contracts.
- Log sensitive data such as auth tokens, request metadata, or raw user-entered list/product names.

## Decisions

### Decision 1: Log in the gRPC adapter and rethrow

Adapters are the narrowest place that consistently see gRPC operation failures and method names. Each handled read operation will emit a structured OpenTelemetry error log, then rethrow the original error.

Alternative considered: log only in API routes. That would centralize route behavior but loses adapter-level operation context and misses direct server component calls.

### Decision 2: Use the existing OpenTelemetry logs API

Use `@opentelemetry/api-logs` with named loggers for shopping and market gRPC adapters, matching the existing authentication adapter pattern. Logs should include safe operation metadata such as service name, method name, and gRPC status code when available.

Alternative considered: use `console.error` and rely on auto-instrumentation. This is less structured and weaker for querying in Loki/Grafana.

### Decision 3: Keep fallback behavior at route/UI boundaries

Low-level adapters will not manufacture empty data after failures. API routes that need stable JSON responses should catch propagated failures and return explicit error responses. Existing successful empty responses still map normally.

Alternative considered: continue returning empty fallback data after logging. That improves observability but still misrepresents failures to the app and users.

## Risks / Trade-offs

- Previously hidden failures may now surface as API errors or page failures -> Add explicit route handling where the current route would otherwise leak an unhandled error.
- More error logs may be emitted during outages -> Use one log per failed gRPC call with concise, safe attributes.
- Tests may need to mock OpenTelemetry logs -> Keep logging behind direct `logs.getLogger(...).emit(...)` usage so tests can spy on the existing API.
