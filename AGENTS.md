# Repository Guidelines

## Project Overview

Metaspesa is a shopping list manager with market price tracking. It is split into three services plus shared infrastructure:

- `client/`: Next.js/React frontend communicating with the backend through gRPC.
- `server/`: .NET 10 gRPC backend with Domain, Application, Database, Infrastructure, GrpcApi, MigrationService, and ServiceDefaults projects.
- `scraper/`: Python 3.14 background worker that scrapes supermarkets.
- `db/`: database setup.
- `docs/`: diagrams and schema notes.
- `observability/`: Grafana, Loki, Mimir, Tempo, and Alloy OpenTelemetry collector.

Services are orchestrated with `compose.yaml` for the main stack and `compose.observability.yaml` for telemetry. All services share PostgreSQL.

## Project Structure & Module Organization

Server source lives under `server/src/`, with tests under `server/test/UnitTests` and `server/test/IntegrationTests`. Keep business rules in Domain, use cases in Application, persistence in Database, and transport concerns in GrpcApi.

Client routes and components live in `client/src/app`, services and view models in `client/src/lib`, gRPC adapters and proto files in `client/src/infrastructure`, and Vitest tests in `client/test`. `vite.config.ts` is used by Vitest only; Next.js build is unaffected.

Scraper source lives in `scraper/src`, tests in `scraper/test`, and configuration in `scraper/config.yaml`. Each market scraper has its own class under `scraper/src/infrastructure/market_scrapers/`. Generated gRPC files under `scraper/src/infrastructure/grpc/protos` are excluded from Ruff checks.

## Build, Test, and Development Commands

### Full Stack

- `docker compose up --build`: build and run the local stack.
- `docker compose up`: start all main services.
- `docker compose down`: stop all main services.
- `docker compose -p tools up migrations`: run database migrations.
- `docker compose -f compose.observability.yaml up`: start telemetry stack.

### Server

- `dotnet build server/Metaspesa.slnx`: build all server projects with analyzers enabled.
- `dotnet test server/Metaspesa.slnx`: run all server tests.
- `cd server && dotnet test test/Application.UnitTests`: run one server test project.
- `cd server && dotnet test --filter "FullyQualifiedName~ClassName"`: filter tests by class/name.
- `cd server && dotnet run --project src/GrpcApi`: run the gRPC server.
- `cd server && dotnet format`: format C# code with `.editorconfig` rules.

### Client

- `cd client && npm install`: install dependencies.
- `cd client && npm run dev`: start the Next.js dev server on port 3000.
- `cd client && npm run build`: build the production app.
- `cd client && npm run lint`: check formatting and ESLint.
- `cd client && npm run format`: auto-fix formatting and ESLint.
- `cd client && npm run typecheck`: run TypeScript type checking.
- `cd client && npm run test`: run Vitest tests.
- `cd client && npm run test -- test/product.test.ts`: run a single test file.
- `cd client && npm run coverage`: run test coverage.
- `cd client && npm run generate-proto`: regenerate TypeScript gRPC types from `.proto` files.

### Scraper

- `cd scraper && uv sync`: install dependencies.
- `cd scraper && uv run pytest`: run all scraper tests.
- `cd scraper && uv run pytest test/application/product_processors/test_brand_extractor.py`: run a single scraper test file.
- `cd scraper && uv run pytest -v`: run scraper tests with verbose output.
- `cd scraper && uv run ruff check src test`: lint scraper code.
- `cd scraper && uv run ruff check src --fix`: lint and auto-fix scraper source.
- `cd scraper && uv run ruff format src test`: format scraper code.

## Architecture

### Client Architecture

- `client/src/app/auth/login/`: login page.
- `client/src/app/auth/register/`: register page.
- `client/src/app/shopping/`: shopping list management.
- `client/src/app/markets/`: market browser.
- `client/src/app/evolution/`: price evolution analytics.
- `client/src/app/components/`: shared UI such as modal, text-field, top-nav, bottom-nav, and side-nav.
- `client/src/infrastructure/protos/`: `.proto` source files.
- `client/src/infrastructure/protos_generated/`: generated TypeScript gRPC clients.
- `client/src/lib/api-service.ts`: gRPC client wrapper.
- `client/src/lib/domain.ts`: shared type definitions.
- `client/src/lib/messages.ts`: message utilities.

### Server Architecture

Dependencies flow inward: GrpcApi -> Application -> Domain. Database implements Application interfaces.

- `server/src/Domain/`: entities and value objects; no external dependencies.
- `server/src/Application/`: use cases, handlers, DTOs, validators, and abstractions.
- `server/src/Database/`: EF Core repositories and PostgreSQL persistence.
- `server/src/GrpcApi/`: gRPC service layer, `.proto` files, and entry point.
- `server/src/MigrationService/`: standalone database migration runner.
- `server/src/ServiceDefaults/`: shared OpenTelemetry, health checks, and logging configuration.

gRPC services:

- `AuthService`: Register, Login.
- `MarketService`: AddProducts.
- `ShoppingService`: GetRegisteredItems, GetCurrentShoppingList, CreateShoppingList, AddItemsToList, UpdateItem, RemoveItem, RecordShoppingList.

Key server patterns:

- `Result<T>`: railway-oriented error handling. Handlers return `Result<T>` and do not throw for domain errors.
- `IClock`: time abstraction. Do not use `DateTime.Now` or `DateTime.UtcNow` directly in Domain or Application.
- `CancellableCommandHandler<TCommand>`: base for use cases with rollback support.
- `IHandler<TCommand, TResult>`: base handler interface in `Application/Abstractions/Core/`.

### gRPC Contract

`.proto` files live in `server/src/GrpcApi/Protos/` and `client/src/infrastructure/protos/`. When proto files change, regenerate frontend client types with `npm run generate-proto` from `client/`. Generated files land in `client/src/infrastructure/protos_generated/`.

### Scraper Architecture

- `scraper/src/main.py`: entry point and scheduled scraping.
- `scraper/src/application/abstractions.py`: base processor classes.
- `scraper/src/application/product_processors.py`: brand extraction and normalization.
- `scraper/src/application/use_case.py`: scraping orchestration.
- `scraper/src/infrastructure/market_scrapers/`: per-market scrapers such as Alcampo and Mercadona.
- `scraper/src/infrastructure/grpc/`: generated gRPC stubs.
- `scraper/src/infrastructure/telemetry/`: OpenTelemetry tracing and metrics.
- `scraper/src/infrastructure/playwright_driver.py`: Playwright web driver abstraction.
- `scraper/src/infrastructure/instrumented_playwright_driver.py`: tracing wrapper.
- `scraper/src/infrastructure/secrets.py`: Docker secrets loader.
- `scraper/src/dependency_injection.py`: DI container.
- `scraper/src/domain.py`: domain models.
- `scraper/src/config.py`: loads `config.yaml`.

### Observability Architecture

All services emit OpenTelemetry traces, metrics, and logs to `http://alloy:4317`.

- `observability/alloy/`: OpenTelemetry collector.
- `observability/grafana/`: dashboards.
- `observability/loki/`: log aggregation.
- `observability/mimir/`: Prometheus-compatible metrics.
- `observability/tempo/`: distributed tracing.

## Coding Style & Naming Conventions

Prefer clarity, consistency, and maintainability over cleverness. Keep separation of concerns clear, coupling low, and cohesion high.

C# uses nullable references, implicit usings, latest analysis, and warnings as errors. All analyzer warnings and errors must be resolved. Avoid OS-specific assumptions because CI runs on Ubuntu, Windows, and macOS.

TypeScript follows Next.js conventions with ESLint and Prettier. Use `PascalCase` for React components, `camelCase` for functions and variables, and colocate route components under their route.

Python uses Ruff with 88-character lines, double quotes, space indentation, and modern Python 3.14 syntax. Ruff enforces `E`, `F`, `I`, and `UP` rules. Pre-commit hooks live in `scraper/.pre-commit-config.yaml`.

## Testing Guidelines

Name C# tests `*Tests.cs`, Python tests `test_*.py`, and client tests `*.test.ts` or `*.test.tsx`. Add focused tests for the changed layer: domain rules, handlers, repositories, React/view-model behavior, or scraper processors.

## Commit & Pull Request Guidelines

Recent history uses Conventional Commits such as `feat: ...`, `fix: ...`, and scoped test commits like `test(client): ...`. Keep commits focused and describe behavior, not implementation trivia.

Pull requests should include a short summary, test results, linked issues when applicable, and screenshots for UI changes. Call out config, migration, or proto changes explicitly.

## Security & Configuration Tips

Do not commit real secrets. Use `.env.example` as the template and keep local values in `.env` or `secrets/`.

Docker secrets used at runtime live in git-ignored `secrets/` files:

- `jwt_secret_key`: JWT signing key for auth service.
- `scraper_username` / `scraper_password`: market login credentials.

Scraper loads Docker secrets through `scraper/src/infrastructure/secrets.py`. Treat Compose and observability files as deployable configuration.
