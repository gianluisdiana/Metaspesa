## Why

The current auth flow builds `User` from raw primitives in Application and keeps identity validation split between validators and thin Domain records. The DDD migration needs Identity to become its own bounded-context module so user creation, role ownership, password hash state, and typed IDs are enforced by the domain before later modules reference `UserId`.

## What Changes

- Rename/refactor the current Domain `Users` area into an Identity module shape.
- Introduce Identity value objects such as `UserId`, `Username`, and `PasswordHash`.
- Refactor `User` into an aggregate root that owns exactly one `Role` and exposes behavior such as creation and password change.
- Move identity-specific domain validation into Identity types and domain exceptions instead of relying only on Application validators or debug assertions.
- Update auth use cases (`RegisterUser`, `LoginUser`) to call Identity aggregate/value object behavior while throwing domain exceptions for auth failures.
- Update user repository and token-provider boundaries as needed so persistence and JWT generation adapt to Identity types without leaking persistence concerns into Domain.
- Add focused Domain and Application tests for Identity behavior and auth-handler mapping.
- Do not change gRPC/proto request or response fields in this slice unless implementation exposes an unavoidable contract issue.

## Capabilities

### New Capabilities
- `domain-identity`: Identity bounded-context domain model and auth use-case integration for users, credentials, and roles.

### Modified Capabilities

## Impact

- Affected service: `server/` only.
- Affected layers: Domain Identity/Users, Application auth handlers and user abstractions, Database user repository mapping, Infrastructure token provider if it consumes `User`.
- No planned gRPC/proto contract changes.
- No planned client, scraper, Docker/Compose, observability, generated-file, or database schema changes.
- Security impact: password hashes remain stored and handled as hashes only; raw passwords stay in Application command/query input and hasher verification, not persisted or logged.
- Privacy impact: no new logging of usernames, passwords, password hashes, JWTs, or auth metadata.
- Backward compatibility risk is compile-time/server-internal unless a later task decides to rename public auth contracts.
