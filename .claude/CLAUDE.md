# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Metaspesa is a shopping list manager with market price tracking. It consists of four services:
- **Client**: Next.js / react frontend communicating via gRPC
- **Server**: C# .NET 10 gRPC backend
- **Scrapper**: Python background worker scraping supermarkets
- **MigrationService**: Standalone database migration runner

All services are orchestrated via Docker Compose and share a PostgreSQL database.

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
```

### Scraper (scrapper/)
```bash
uv sync                  # Install dependencies
uv run pytest            # Run all tests
uv run pytest test/application/product_processors/test_brand_extractor.py  # Single file
uv run pytest -v         # Verbose output
uv run ruff check src    # Lint
uv run ruff format src   # Format
```

### Docker
```bash
docker compose up                         # Start all services
docker compose down                       # Stop all services
docker compose -p tools up migrations     # Run database migrations
```

## Architecture

### Server Architecture
```
server/src/
├── Domain/          # Entities, value objects — no external dependencies
├── Application/     # Use cases, handlers, DTOs, validators
├── Database/        # EF Core repositories, PostgreSQL
├── GrpcApi/         # gRPC service layer, .proto files, entry point
├── MigrationService/  # Standalone DB migration runner
└── ServiceDefaults/ # Shared configuration
```
Dependencies flow inward: GrpcApi → Application → Domain. Database implements Application interfaces.

### gRPC Contract
`.proto` files live in `server/src/GrpcApi/Protos/` and `client/src/infrastructure/protos/`. When proto files change, regenerate the frontend client types with `npm run generate-proto` (from `client/`). Generated files land in `client/src/infrastructure/`.

### Scraper Architecture
```
scrapper/src/
├── main.py            # Entry point; runs scheduled scraping
├── application/       # Use cases, product processors (brand extraction, normalization)
├── infrastructure/    # Selenium scrapers per market, CSV storage
└── config.py          # Loads config.yaml
```
Each market has its own scraper class in `infrastructure/`.

## Key Technical Constraints

- **.NET build**: All analyzer warnings and errors must be resolved.
- **Nullable reference types**: Enabled across the .NET solution.
- **Python**: Ruff enforces `E, F, I, UP` rules. Pre-commit hooks are configured in `scrapper/.pre-commit-config.yaml`.
- **CI**: Tests run on Ubuntu, Windows, and macOS — avoid OS-specific assumptions.
