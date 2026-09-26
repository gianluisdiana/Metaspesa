# REST API contract

`server/src/RestApi/openapi.yaml` is the source for client API generation. The
server build keeps it updated. Do not edit `openapi_generated/` by hand.

Run `pnpm run generate-api` from `client/` after server contract changes. It
generates TypeScript types and Zod schemas from the server YAML. Commit the
generated output with the contract change.

OpenAPI 3.1 cannot describe HTTP `QUERY` operations. Product-query request
typing therefore lives in `lib/market-api-service.ts`; shared product response
schemas remain generated from the server's documented response components.
