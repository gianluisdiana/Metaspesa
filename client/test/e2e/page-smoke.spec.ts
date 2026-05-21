import { expect, test } from '@playwright/test';

test.describe('page smoke e2e', () => {
  test('markets page renders market heading', async ({ page }) => {
    await page.goto('/markets');

    await expect(page.getByRole('heading', { name: /markets/i })).toBeVisible();
  });

  test('shopping page renders a page heading', async ({ page }) => {
    await page.goto('/shopping');

    await expect(page.getByRole('heading')).toBeVisible();
  });

  test('evolution page renders price history chart heading', async ({
    page,
  }) => {
    await page.goto('/evolution');

    await expect(
      page.getByRole('heading', { name: /price & volume trend/i }),
    ).toBeVisible();
  });
});
