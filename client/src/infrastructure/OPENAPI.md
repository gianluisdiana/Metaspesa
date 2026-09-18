# REST API contract

`server/src/RestApi/openapi.yaml` is the source for client API generation. The
server build keeps it updated. Do not edit `openapi_generated/` by hand.

Run `pnpm run generate-api` from `client/` after server contract changes. It
generates TypeScript types and Zod schemas from the server YAML. Commit the
generated output with the contract change.
