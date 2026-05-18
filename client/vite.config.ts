import path from 'node:path';

import { defineConfig } from 'vitest/config';

export default defineConfig({
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src/'),
      '@/protos/auth': path.resolve(
        __dirname,
        './src/infrastructure/protos_generated/Metaspesa/Protos/Auth/',
      ),
      '@/protos/markets': path.resolve(
        __dirname,
        './src/infrastructure/protos_generated/Metaspesa/Protos/Markets/',
      ),
      '@/protos/shopping': path.resolve(
        __dirname,
        './src/infrastructure/protos_generated/Metaspesa/Protos/Shopping/',
      ),
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
