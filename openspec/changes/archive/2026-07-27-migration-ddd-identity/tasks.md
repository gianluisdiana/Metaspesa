## 1. Identity Domain

- [x] 1.1 Inspect current `Domain.Users`, auth handlers, repository mapping, token provider, and tests before editing.
- [x] 1.2 Create `server/src/Domain/Identity` module structure and migrate user-related domain types into the Identity namespace.
- [x] 1.3 Add `UserId`, `Username`, and `PasswordHash` value objects with validation and value equality.
- [x] 1.4 Add Identity-specific domain exceptions for invalid user id, username, password hash, and role.
- [x] 1.5 Refactor `User` into an aggregate root with shopper creation and password hash change.
- [x] 1.6 Ensure `User` owns exactly one valid `Role` and rejects `Role.None` or invalid role values.
- [x] 1.7 Keep raw password hashing outside Domain; keep `PasswordPolicy` available for Application registration validation.

## 2. Application Auth Integration

- [x] 2.1 Update `RegisterUser.Handler` to build Identity value objects, hash raw password, create a `User` aggregate, save it, and commit through unit of work.
- [x] 2.2 Delete `RegisterUser.Validator` and use Identity password policy directly from the use case.
- [x] 2.3 Update `LoginUser.Handler` to load Identity users and verify raw password against `PasswordHash`.
- [x] 2.4 Update `IUserRepository` and `ITokenProvider` abstractions to use Identity `User` and value object types where needed.
- [x] 2.5 Remove `Result` and `DomainError`, use domain exceptions for validation, username conflict, and invalid credentials.
- [x] 2.6 Map domain exceptions to gRPC message in gRPC layer without changing public auth contracts.

## 3. Adapter Integration

- [x] 3.1 Update `PostgreSqlUserRepository` to map existing user entity columns to/from Identity value objects without schema changes.
- [x] 3.2 Update `JwtTokenProvider` to derive subject, name, and role claims from Identity values.
- [x] 3.3 Confirm auth `.proto` files, generated client types, database migrations, and client code remain unchanged.

## 4. Tests

- [x] 4.1 Add Domain unit tests for `UserId`, `Username`, and `PasswordHash` valid creation, invalid rejection, equality, and exception types.
- [x] 4.2 Add Domain unit tests for `User` shopper creation, password hash change, role change, and invalid role rejection.
- [x] 4.3 Update Application auth handler tests for Identity value object usage and auth outcomes.
- [x] 4.4 Update Database repository tests for Identity user save/load mapping if existing repository tests cover users.
- [x] 4.5 Update Infrastructure JWT tests for claims generated from Identity user values.
- [x] 4.6 Keep GrpcApi auth tests passing without public auth contract changes.

## 5. Verification

- [x] 5.1 Run `dotnet test server/Metaspesa.slnx`.
- [x] 5.2 Check changed files and confirm Markets, Shopping, Purchasing, client, scraper, proto, Docker/Compose, migration, generated, and observability behavior remains unchanged as part of this slice.
