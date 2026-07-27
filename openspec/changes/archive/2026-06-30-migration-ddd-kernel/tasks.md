## 1. Domain Shared Kernel

- [x] 1.1 Inspect existing Domain value object and test style before adding new files.
- [x] 1.2 Create `server/src/Domain/SharedKernel` with `Money`, `Quantity`, and `UnitOfMeasure` value objects.
- [x] 1.3 Add a base `DomainException` and specific exception classes for each shared kernel validation error.
- [x] 1.4 Ensure `Money` accepts valid non-negative decimal amounts, rejects negative amounts, and remains currency-free.
- [x] 1.5 Ensure `Quantity` accepts positive amounts with a valid `UnitOfMeasure` and rejects zero or negative amounts.
- [x] 1.6 Ensure `UnitOfMeasure` accepts valid unit values and rejects empty or unsupported values.
- [x] 1.7 Keep bounded-context IDs and market-specific concepts, including image URL behavior, outside `SharedKernel`.

## 2. Domain Tests

- [x] 2.1 Add focused unit tests for `Money` valid creation, negative amount rejection, equality, and specific exception type.
- [x] 2.2 Add focused unit tests for `Quantity` valid creation, non-positive amount rejection, equality, and specific exception type.
- [x] 2.3 Add focused unit tests for `UnitOfMeasure` valid creation, invalid unit rejection, equality, and specific exception type.
- [x] 2.4 Add focused unit tests proving shared kernel exceptions derive from `DomainException`.
- [x] 2.5 Add focused checks that `SharedKernel` does not contain bounded-context ID value objects.

## 3. Verification

- [x] 3.1 Run `dotnet test server/Metaspesa.slnx`.
- [x] 3.2 Confirm no Application, Database, GrpcApi, client, scraper, proto, Docker/Compose, migration, generated, or observability files changed as part of this slice.
