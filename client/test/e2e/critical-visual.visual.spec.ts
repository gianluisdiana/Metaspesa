/* eslint-disable @typescript-eslint/no-magic-numbers */
import { expect, test, type Page } from '@playwright/test';

async function expectRenderedScreenshot(page: Page) {
  const screenshot = await page.screenshot({ fullPage: true });

  expect(screenshot.length).toBeGreaterThan(10_000);
}

test.describe('critical visual coverage', () => {
  test('markets page', async ({ page }) => {
    await page.goto('/markets');

    await expectRenderedScreenshot(page);
  });

  test('shopping page', async ({ page }) => {
    await page.goto('/shopping');

    await expectRenderedScreenshot(page);
  });

  test('evolution page', async ({ page }) => {
    await page.goto('/evolution');

    await expectRenderedScreenshot(page);
  });
});
