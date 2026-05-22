/* eslint-disable @typescript-eslint/no-magic-numbers */
import { defineConfig, devices } from '@playwright/test';

const port = Number(process.env.PLAYWRIGHT_PORT ?? 3000);
const baseURL = process.env.PLAYWRIGHT_BASE_URL ?? `http://127.0.0.1:${port}`;
const shouldStartServer = process.env.PLAYWRIGHT_START_SERVER !== 'false';

export default defineConfig({
  expect: {
    toHaveScreenshot: {
      maxDiffPixelRatio: 0.05,
    },
  },
  fullyParallel: true,
  outputDir: 'test-results/playwright',
  projects: [
    {
      name: 'firefox-e2e',
      testDir: './test/e2e',
      testIgnore: /.*\.visual\.spec\.ts/,
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'firefox-visual',
      testDir: './test/e2e',
      testMatch: /.*\.visual\.spec\.ts/,
      use: { ...devices['Desktop Firefox'] },
    },
  ],
  reporter: [['list'], ['html', { open: 'never' }]],
  testDir: './test/e2e',
  use: {
    baseURL,
    trace: 'on-first-retry',
  },
  webServer: shouldStartServer
    ? {
        command: `npm run dev -- --hostname 127.0.0.1 --port ${port}`,
        env: {
          GRPC_SERVER_URL: process.env.GRPC_SERVER_URL ?? '127.0.0.1:8080',
          NODE_ENV: 'test',
        },
        reuseExistingServer: true,
        timeout: 120_000,
        url: baseURL,
      }
    : undefined,
});
