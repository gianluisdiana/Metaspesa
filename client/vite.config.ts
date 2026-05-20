import path from 'node:path';

import { defineConfig } from 'vitest/config';

export default defineConfig({
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src/'),
      '@/generated-protos': path.resolve(
        __dirname,
        './src/infrastructure/protos_generated',
      ),
      '@/generated-protos/auth': path.resolve(
        __dirname,
        './src/infrastructure/protos_generated/Metaspesa/Protos/Auth/',
      ),
      '@/generated-protos/markets': path.resolve(
        __dirname,
        './src/infrastructure/protos_generated/Metaspesa/Protos/Markets/',
      ),
      '@/generated-protos/shopping': path.resolve(
        __dirname,
        './src/infrastructure/protos_generated/Metaspesa/Protos/Shopping/',
      ),
    },
  },
});
