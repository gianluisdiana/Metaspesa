## Context

Current identity code lives in `server/src/Domain/Users` as thin records and policy helpers. `RegisterUser.Handler` creates `User` directly with `Guid.CreateVersion7()`, raw username text, raw hashed-password text, and `Role.Shopper`. `LoginUser.Handler`, `IUserRepository`, `PostgreSqlUserRepository`, and `JwtTokenProvider` all consume that primitive-shaped `User`.

The target DDD plan and `docs/domain.puml` define an `Identity Module` with `User` as aggregate root, `UserId`, `Username`, `PasswordHash`, and one `Role`. This slice migrates identity/auth internals only. It should keep public auth gRPC contracts stable and avoid database schema changes.

## Goals / Non-Goals

**Goals:**

- Introduce an Identity domain module under `server/src/Domain`.
- Model `User` as aggregate root with `UserId`, `Username`, `PasswordHash`, and exactly one `Role`.
- Add domain behavior for user creation, password hash change, and role change.
- Move identity validation into value objects and Identity-specific exceptions derived from the shared kernel domain exception base.
- Update Application auth handlers and abstractions to use Identity domain types while throwing domain exceptions for auth failures.
- Update Database and Infrastructure adapters so persistence and JWT generation map Identity types at boundaries.
- Add focused tests for Identity value objects, aggregate behavior, auth handlers, repository mapping, and JWT claims.

**Non-Goals:**

- Do not change auth `.proto` files, generated client types, or frontend auth flows.
- Do not change database schema, table names, role seed data, or migrations.
- Do not introduce multi-role users; each `User` owns one `Role`.
- Do not introduce password hashing in Domain; hashing remains Application/Infrastructure boundary behavior through `IHasher`.
- Do not refactor Markets, Shopping, Purchasing, or cross-module references beyond using Identity types where auth already touches users.
- Do not return Domain objects to GrpcApi/presentation.

## Decisions

1. Create `Metaspesa.Domain.Identity` and migrate current `Domain.Users` concepts there.

   Rationale: The target module name is Identity. Moving now establishes the bounded-context language before Markets/Shopping/Purchasing introduce references to `UserId`.

   Alternative considered: Keep namespace as `Users` and only add value objects. That would reduce churn but postpone the module boundary this slice is meant to create.

2. Use value objects for `UserId`, `Username`, and `PasswordHash`.

   Rationale: `UserId` is the future cross-module identity reference. `Username` and `PasswordHash` protect domain state from empty or malformed primitives. `PasswordHash` stores only the hash; raw passwords remain outside Domain.

   Alternative considered: Keep raw primitives and validate in FluentValidation only. That keeps current code simple but allows invalid Identity domain objects to exist.

3. Keep `PasswordPolicy` for raw password policy checks in Application validation.

   Rationale: Raw password rules apply before hashing during registration. Domain should not receive or store raw passwords. The policy can remain in Identity as a domain policy, but `User` stores `PasswordHash`.

   Alternative considered: Move all password checks into `User.Create`. That would force raw password into Domain and blur hash responsibility.

4. Keep `IHasher` and `ITokenProvider` in Application abstractions, but adapt their signatures to Identity types as needed.

   Rationale: Hashing and JWT creation are not domain behavior. Application orchestrates them, Infrastructure implements them, and Identity supplies validated state.

   Alternative considered: Add token DTOs/read models detached from `User`. That may be useful later for "no Domain objects to GrpcApi", but it is broader than this auth-internal slice.

5. Keep persistence schema stable and map value objects in `PostgreSqlUserRepository`.

   Rationale: Existing columns already store user UID, username, encrypted password, and role. This slice can convert between value objects and columns without migrations.

   Alternative considered: Add EF owned/value-converter mappings now. That is stronger but changes persistence model more than needed for this incremental migration.

## Risks / Trade-offs

- Namespace rename can cause wide compile churn -> Mitigation: keep changes limited to user/auth/repository/token references and update tests near those seams.
- Domain exceptions must map cleanly to gRPC errors -> Mitigation: GrpcApi catches expected domain exceptions and maps them to stable public auth failures without changing proto contracts.
- `Role.None` can remain an invalid domain state -> Mitigation: disallow it in `User.Create`/`ChangeRole` and add tests.
- Repository mapping can accidentally leak primitives back into Application -> Mitigation: keep primitive conversion inside `PostgreSqlUserRepository` and token generation adapter.
- Auth contract stability can mask internal rename regressions -> Mitigation: keep GrpcApi auth tests and run server tests.

## Migration Plan

1. Add Identity value objects, exceptions, role rules, and `User` aggregate behavior.
2. Update Application auth handlers and user abstractions to use Identity types and throw domain exceptions for auth failures.
3. Update Database repository mapping from EF user entity columns to Identity types.
4. Update Infrastructure `JwtTokenProvider` to read claims from Identity value objects.
5. Update Domain, Application, Database, Infrastructure, and GrpcApi tests touched by auth.
6. Verify with `dotnet test server/Metaspesa.slnx`.

Rollback is code-only: revert Identity files and auth/repository/token adapter changes. No schema, proto, config, or generated artifacts need rollback.

## Open Questions

- Whether later slices should introduce Application auth read models that fully decouple `ITokenProvider` from `User` can be decided in the "Application read models only" migration.
- Whether `Username` should normalize case or preserve display casing while repository checks remain case-insensitive should be confirmed during implementation.
