# Proto Boundary

Source `.proto` files live in this directory. Generated TypeScript is emitted to
`src/infrastructure/protos_generated` by:

```bash
npm run generate-proto
```

Generated files are ignored by lint/format and should not be edited by hand.
Application code should import generated types through `@/generated-protos/*`
aliases so generated code stays visually separate from hand-written
infrastructure code.
