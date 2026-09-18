import { expect, test } from '@playwright/test';

import { registerAndLogin } from './e2e-helpers';

test.describe('market search e2e', () => {
  test('product search writes name query to URL', async ({ page }) => {
    await registerAndLogin(page);

    await page.getByPlaceholder('Search products...').fill('milk');

    await expect(page).toHaveURL(/query=milk/);
  });

  test('brand filter stays available during product search', async ({
    page,
  }) => {
    await registerAndLogin(page);

    await page.getByPlaceholder('Search products...').fill('milk');

    await expect(page.getByPlaceholder('Brand...')).toBeVisible();
  });
});
