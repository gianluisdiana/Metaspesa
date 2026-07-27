## Context

Metaspesa's current Domain project is organized around `Markets`, `Shopping`, and `Users`, with some shared concepts already crossing module boundaries. The full DDD migration plan splits domain behavior into `Identity`, `Markets`, `Shopping`, and `Purchasing`, and requires a small shared kernel before those bounded contexts can be refactored independently.

This change is the first migration slice. It adds a shared kernel under `server/src/Domain` and focused tests under `server/test/UnitTests/Domain.UnitTests`, without changing Application handlers, repositories, persistence mappings, gRPC contracts, or client code.

## Goals / Non-Goals

**Goals:**

- Provide dependency-free shared kernel value objects in the Domain project.
- Add shared `Money`, `Quantity`, and `UnitOfMeasure` value objects.
- Keep `Money` currency-free and represent currency outside the domain until multi-currency behavior exists.
- Add a base `DomainException` plus specific domain exception types for each shared kernel validation error.
- Establish validation, equality, and exception behavior with focused domain unit tests.
- Keep the implementation additive so later DDD migration slices can adopt the new types incrementally.

**Non-Goals:**

- Do not rename existing domain modules to `Identity`, `Markets`, `Shopping`, or `Purchasing`.
- Do not add typed identifier value objects in the shared kernel; identifiers belong to their bounded contexts.
- Do not refactor aggregates, repositories, use cases, DTOs, read models, persistence mappings, or gRPC services.
- Do not add `CurrencyCode` or make `Money` responsible for currency.
- Do not move market-specific concepts such as `ImageUrl` into the shared kernel.
- Do not change database schema, proto files, client generated files, scraper code, Docker/Compose, or telemetry configuration.

## Decisions

1. Add a `SharedKernel` namespace and folder inside the Domain project.

   Rationale: The kernel belongs inward with Domain and must not depend on Application, Database, GrpcApi, client, or scraper concerns. Keeping it in the existing Domain project avoids a premature new assembly while making the module boundary visible.

   Alternative considered: Create a separate SharedKernel project. That would make dependency enforcement stronger, but it adds project and solution churn before the migration proves the final module layout.

2. Keep typed identifiers out of the shared kernel.

   Rationale: Identifiers are part of each bounded context's language and consistency boundary. `UserId`, `ProductFormatId`, `PriceSnapshotId`, and `ShoppingListId` should be introduced by the Identity, Markets, Shopping, and Purchasing slices when those modules are migrated.

   Alternative considered: Add all migration-plan IDs now. That would put bounded-context-specific language in the shared kernel and make later module ownership less explicit.

3. Add shared `Money`, `Quantity`, and `UnitOfMeasure` value objects.

   Rationale: These concepts appear across domain modules and are safe shared-kernel candidates. `Money` validates amount semantics only. `Quantity` validates positive numeric amount. `UnitOfMeasure` validates supported unit naming/format without owning market-specific product behavior.

   Alternative considered: Leave current module-local `Quantity` and `Price` concepts untouched until each aggregate migration. That avoids early changes but keeps duplicated primitives and weak validation in place for every later slice.

4. Add explicit domain exception types.

   Rationale: A base `DomainException` gives shared handling semantics, while specific exceptions such as invalid money amount, invalid quantity amount, and invalid unit of measure make debugging and tests precise without parsing generic messages.

   Alternative considered: Use `ArgumentException` or debug assertions. Those are less expressive for domain failures and make later error mapping harder to inspect.

5. Keep adoption by existing domain classes out of scope.

   Rationale: This change creates the kernel module only. Updating `Price`, `ProductFormat`, `ShoppingList`, users, repositories, and APIs belongs to later migration slices where behavior and mappings can be changed coherently.

   Alternative considered: Replace existing `Price` and ID usage immediately. That would couple this foundational change to wider aggregate and persistence refactors.

## Risks / Trade-offs

- Additive types can sit unused until later slices -> Mitigation: add tests and keep names aligned with `plan.md` so follow-up changes have a clear adoption path.
- Exception taxonomy can grow too broad -> Mitigation: add specific exception classes only for distinct domain validation errors introduced by this slice.
- Existing code may already have similarly named concepts -> Mitigation: place new types under `Metaspesa.Domain.SharedKernel` and avoid changing current call sites.
- `Money` equality and decimal precision can affect later price behavior -> Mitigation: define deterministic amount validation and equality tests in this slice.
- `Quantity` and `UnitOfMeasure` can drift into market-specific product modeling -> Mitigation: keep them generic and leave image URL, product format, package presentation, and market rules out of shared kernel.

## Migration Plan

1. Add the shared kernel folder, value objects, and domain exceptions in `server/src/Domain`.
2. Add focused unit tests in `server/test/UnitTests/Domain.UnitTests`.
3. Verify with `dotnet test server/Metaspesa.slnx`.
4. Later migration changes can replace raw IDs and existing price concepts module by module.

Rollback is simple because this slice is additive: remove the new shared kernel files and their tests before any later slice adopts them.

## Open Questions

- Whether `Money` should eventually support currency remains outside this migration plan until multi-currency requirements exist.
- Exact bounded-context ID types and persistence mappings will be decided in each bounded-context migration slice.
