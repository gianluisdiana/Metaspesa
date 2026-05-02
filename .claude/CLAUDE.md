# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Metaspesa is a shopping list manager with market price tracking. It consists of:
- **Client**: Next.js / React frontend communicating via gRPC
- **Server**: C# .NET 10 gRPC backend
- **Scraper**: Python background worker scraping supermarkets
- **MigrationService**: Standalone database migration runner
- **Observability**: Grafana + Loki + Mimir + Tempo + Alloy (OTEL collector) in `observability/`

Services are orchestrated via two compose files:
- `compose.yaml` — main services (server, client, scraper, db, migrations)
- `compose.observability.yaml` — telemetry stack

All services share a PostgreSQL database.

## Commands

### Frontend (client/)
```bash
npm install           # Install dependencies
npm run dev           # Dev server (port 3000)
npm run build         # Production build
npm test              # Run Vitest tests
npm test -- test/product.test.ts # Run a single test file
npm run coverage      # Test coverage
npm run lint          # Check formatting + ESLint
npm run format        # Auto-fix formatting + ESLint
npm run generate-proto  # Regenerate TypeScript types from .proto files
```

### Backend (server/)
```bash
dotnet build # Build solution
dotnet test  # Run all tests
dotnet test test/Application.UnitTests # Run single test project
dotnet test --filter "FullyQualifiedName~ClassName"  # Filter by class
dotnet run --project src/GrpcApi # Run gRPC server
dotnet format # Format code with .editorconfig rules
```

### Scraper (scraper/)
```bash
uv sync                  # Install dependencies
uv run pytest            # Run all tests
uv run pytest test/application/product_processors/test_brand_extractor.py  # Single file
uv run pytest -v         # Verbose output
uv run ruff check src --fix    # Lint and auto-fix
uv run ruff format src   # Format
```

### Docker
```bash
docker compose up                              # Start all services
docker compose down                            # Stop all services
docker compose -p tools up migrations          # Run database migrations
docker compose -f compose.observability.yaml up  # Start telemetry stack
```

## Architecture

### Client Architecture
```
client/src/
├── app/
│   ├── auth/login/        # Login page
│   ├── auth/register/     # Register page
│   ├── shopping/          # Shopping list management
│   ├── markets/           # Market browser
│   ├── evolution/         # Price evolution analytics
│   └── components/        # Shared: modal, text-field, top-nav, bottom-nav, side-nav
├── infrastructure/
│   ├── protos/            # .proto source files
│   └── protos_generated/  # Generated TypeScript gRPC clients
└── lib/
    ├── api-service.ts     # gRPC client wrapper
    ├── domain.ts          # Shared type definitions
    └── messages.ts        # Message utilities
```

`vite.config.ts` is used by Vitest only — Next.js build is unaffected.

### Server Architecture
```
server/src/
├── Domain/          # Entities, value objects — no external dependencies
├── Application/     # Use cases, handlers, DTOs, validators
├── Database/        # EF Core repositories, PostgreSQL
├── GrpcApi/         # gRPC service layer, .proto files, entry point
├── MigrationService/  # Standalone DB migration runner
└── ServiceDefaults/ # Shared configuration (OTEL, health checks, logging)
```
Dependencies flow inward: GrpcApi → Application → Domain. Database implements Application interfaces.

**gRPC Services**:
- `AuthService` — Register, Login
- `MarketService` — AddProducts
- `ShoppingService` — GetRegisteredItems, GetCurrentShoppingList, CreateShoppingList, AddItemsToList, UpdateItem, RemoveItem, RecordShoppingList

**Key patterns**:
- `Result<T>` — Railway-oriented error handling; all handlers return `Result<T>`, never throw for domain errors
- `IClock` — Time abstraction; never use `DateTime.Now/UtcNow` directly
- `CancellableCommandHandler<TCommand>` — Base for use cases with rollback support
- `IHandler<TCommand, TResult>` — Base handler interface in `Application/Abstractions/Core/`

### gRPC Contract
`.proto` files live in `server/src/GrpcApi/Protos/` and `client/src/infrastructure/protos/`. When proto files change, regenerate the frontend client types with `npm run generate-proto` (from `client/`). Generated files land in `client/src/infrastructure/protos_generated/`.

### Scraper Architecture
```
scraper/src/
├── main.py                               # Entry point; scheduled scraping
├── application/
│   ├── abstractions.py                   # Base processor classes
│   ├── product_processors.py             # Brand extraction, normalization
│   └── use_case.py                       # Scraping orchestration
├── infrastructure/
│   ├── market_scrapers/                  # Per-market scrapers (Alcampo, Mercadona)
│   ├── grpc/                             # gRPC stubs (generated)
│   ├── telemetry/                        # OTEL tracing + metrics
│   ├── playwright_driver.py              # Playwright web driver abstraction
│   ├── instrumented_playwright_driver.py # Tracing wrapper
│   └── secrets.py                        # Docker secrets loader
├── dependency_injection.py               # DI container
├── domain.py                             # Domain models
└── config.py                             # Loads config.yaml
```
Each market has its own scraper class in `infrastructure/market_scrapers/`.

### Observability Architecture
```
observability/
├── alloy/     # OTEL Collector — receives OTLP from server + scraper on :4317
├── grafana/   # Dashboards
├── loki/      # Log aggregation
├── mimir/     # Prometheus-compatible metrics
└── tempo/     # Distributed tracing
```
All services emit OpenTelemetry traces, metrics, and logs to `http://alloy:4317`.

## Secrets

Docker secrets used at runtime (files in `secrets/`, git-ignored):
- `jwt_secret_key` — JWT signing key for auth service
- `scraper_username` / `scraper_password` — Market login credentials

Loaded in scraper via `infrastructure/secrets.py`.

## Key Technical Constraints

- **.NET build**: All analyzer warnings and errors must be resolved.
- **Nullable reference types**: Enabled across the .NET solution.
- **Result<T> pattern**: Never throw for domain/application errors — return `Result<T>`.
- **IClock**: Never call `DateTime.Now/UtcNow` in Domain or Application layers.
- **Python**: Ruff enforces `E, F, I, UP` rules. Pre-commit hooks in `scraper/.pre-commit-config.yaml`.
- **CI**: Tests run on Ubuntu, Windows, and macOS — avoid OS-specific assumptions.
