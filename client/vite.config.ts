import path from 'node:path';

import { defineConfig } from 'vitest/config';

export default defineConfig({
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src/'),
      '@protos/': path.resolve(
        __dirname,
        './src/infrastructure/protos_generated/',
      ),
      '@protos/shopping': path.resolve(
        __dirname,
        './src/infrastructure/protos_generated/Metaspesa/Protos/Shopping/',
      ),
    },
  },
});
