## ADDED Requirements

### Requirement: Identity user aggregate
The Domain layer SHALL provide an Identity `User` aggregate root that owns `UserId`, `Username`, `PasswordHash`, and exactly one valid `Role`.

#### Scenario: Creating shopper user
- **WHEN** Application creates a user from a valid `UserId`, `Username`, and `PasswordHash`
- **THEN** the Domain creates a `User` aggregate with `Role.Shopper`

#### Scenario: Rejecting invalid role
- **WHEN** Domain code attempts to create or change a user with an invalid role
- **THEN** the operation fails with a specific Identity domain exception

#### Scenario: Changing password hash
- **WHEN** Domain code changes a user's password hash to a valid new hash
- **THEN** the `User` aggregate stores the new `PasswordHash`

### Requirement: Identity value objects
The Domain layer SHALL provide Identity value objects for `UserId`, `Username`, and `PasswordHash`.

#### Scenario: Creating identity value objects from valid values
- **WHEN** Domain code creates Identity value objects from valid values
- **THEN** each value object exposes its value and compares by value

#### Scenario: Rejecting empty user id
- **WHEN** Domain code attempts to create `UserId` from an empty GUID
- **THEN** creation fails with a specific Identity domain exception

#### Scenario: Rejecting invalid username
- **WHEN** Domain code attempts to create `Username` from an empty or invalid value
- **THEN** creation fails with a specific Identity domain exception

#### Scenario: Rejecting invalid password hash
- **WHEN** Domain code attempts to create `PasswordHash` from an empty value
- **THEN** creation fails with a specific Identity domain exception

### Requirement: Auth registration uses Identity domain
Application registration SHALL create users through Identity domain value objects and aggregate behavior while preserving existing public registration contract behavior.

#### Scenario: Successful registration
- **WHEN** a valid registration command is handled
- **THEN** Application hashes the raw password, creates an Identity `User` aggregate, saves it through `IUserRepository`, commits the unit of work, and completes without returning a domain model

#### Scenario: Registration validation failure
- **WHEN** a registration command fails existing username or raw password validation
- **THEN** Application throws validation domain exception and does not hash, save, or commit a user

#### Scenario: Username conflict
- **WHEN** a registration command uses an existing username
- **THEN** Application throws a conflict domain exception  and does not save a duplicate user

### Requirement: Auth login uses Identity domain
Application login SHALL load Identity users and verify credentials without exposing raw passwords or password hashes outside auth boundaries.

#### Scenario: Successful login
- **WHEN** a login query has an existing username and matching password
- **THEN** Application generates and returns a token for the loaded Identity user

#### Scenario: Invalid credentials
- **WHEN** a login query has a missing user or non-matching password
- **THEN** Application throws an unauthenticated domain exception and does not generate a token

### Requirement: Identity adapter boundaries
Database and Infrastructure adapters SHALL convert between persistence/JWT primitives and Identity domain types at the adapter boundary.

#### Scenario: Saving identity user
- **WHEN** `IUserRepository` saves an Identity `User`
- **THEN** the database adapter stores the user id, username, password hash, and single role in existing user columns without schema changes

#### Scenario: Loading identity user
- **WHEN** `IUserRepository` loads an existing user record
- **THEN** the database adapter returns an Identity `User` aggregate with typed value objects

#### Scenario: Generating JWT claims
- **WHEN** Infrastructure generates a JWT for an Identity user
- **THEN** the token includes subject, name, and role claims derived from Identity values without logging JWTs or password hashes

### Requirement: Identity migration contract stability
This Identity migration slice SHALL preserve external auth contracts and avoid unrelated module changes.

#### Scenario: Preserving gRPC auth contract
- **WHEN** this slice is implemented
- **THEN** auth `.proto` files, generated client types, and public request/response field names remain unchanged

#### Scenario: Avoiding unrelated module refactors
- **WHEN** this slice is implemented
- **THEN** Markets, Shopping, Purchasing, client, scraper, Docker/Compose, observability, and generated files remain behaviorally unchanged
