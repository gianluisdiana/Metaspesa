import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
  input: '../server/src/RestApi/openapi.yaml',
  output: './src/infrastructure/openapi_generated',
  plugins: ['@hey-api/typescript', 'zod'],
});
