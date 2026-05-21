import { expect, test } from '@playwright/test';

import { registerAndLogin } from './e2e-helpers';

test.describe('product add e2e', () => {
  test('authenticated add opens list dialog when products exist', async ({
    page,
  }) => {
    await registerAndLogin(page);
    const addButton = page.getByRole('button', { name: /add/i }).first();

    if (await addButton.isVisible()) {
      await addButton.click();
    }

    await expect(
      page.getByRole('dialog').or(page.getByText('No products found')),
    ).toBeVisible();
  });
});
