# Client Testing

Tests are split by cost and system boundary.

## Fast Tests

- Unit/regression: `npm run test:unit`
  - Stack: Vitest in Node.
  - Scope: pure logic, mappers, search params, auth validation, view models.
- Component: `npm run test:component`
  - Stack: Vitest, jsdom, Testing Library, jest-dom.
  - Scope: React UI behavior for search filters, product cards, add-to-list modal, shopping list, and price-history shell.

`npm run test` runs unit and component tests.

## Connected Tests

- Integration: `GRPC_SERVER_URL=localhost:8080 npm run test:integration`
  - Stack: Vitest with real `@grpc/grpc-js` clients.
  - Scope: client-to-server gRPC contract for auth, shopping lists, and market reads.
  - Without `GRPC_SERVER_URL`, specs are skipped.

- E2E smoke: `npm run test:e2e`
  - Stack: Playwright Chromium against Next.js plus real backend.
  - Scope: page smoke, register/login, market search, product add modal when products exist, shopping navigation.

- Visual: `npm run test:visual`
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
npm run test:e2e
npm run test:visual
```
