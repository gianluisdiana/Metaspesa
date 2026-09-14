# Client Testing

Tests are split by cost and system boundary.

## Fast Tests

- Unit/regression: `pnpm run test:unit`
  - Stack: Vitest in Node.
  - Scope: pure logic, mappers, search params, auth validation, view models.
- Component: `pnpm run test:component`
  - Stack: Vitest, jsdom, Testing Library, jest-dom.
  - Scope: React UI behavior for search filters, product cards, add-to-list modal, shopping list, and price-history shell.

`pnpm run test` runs unit and component tests.

## Connected Tests

- Integration: `pnpm run test:integration`
  - Stack: Vitest with real REST and `@grpc/grpc-js` clients.
  - Scope: REST authentication plus client-to-server gRPC contracts for shopping lists and market reads.
  - Requires `GRPC_SERVER_URL` and `REST_API_URL`; specs without their required URLs are skipped.

Start required services from repository root:

```powershell
docker compose up -d db server rest-api
```

Then run integration tests from `client/`:

```powershell
$env:GRPC_SERVER_URL = 'localhost:4000'
$env:REST_API_URL = 'http://localhost:4001/api/v1'
pnpm run test:integration
```

- E2E smoke: `pnpm run test:e2e`
  - Stack: Playwright Chromium against Next.js plus real backend.
  - Scope: page smoke, register/login, market search, product add modal when products exist, shopping navigation.

- Visual: `pnpm run test:visual`
  - Stack: Playwright screenshots.
  - Scope: screenshot render coverage for markets, shopping, and evolution pages.

E2E and visual specs live under `test/e2e` so every test type is grouped under one tree.

For full-stack browser tests, run the app stack first, for example:

```powershell
docker compose -f ../compose.yaml -f ../compose.e2e.yaml up --build
```

Then from `client/`, run Playwright with:

```powershell
$env:PLAYWRIGHT_START_SERVER = 'false'
pnpm run test:e2e
pnpm run test:visual
```
