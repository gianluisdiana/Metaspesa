import { expect, test } from '@playwright/test';

import { loginUser, registerUser, uniqueUsername } from './e2e-helpers';

test.describe('auth e2e', () => {
  test('registration redirects to login page', async ({ page }) => {
    await registerUser(page, uniqueUsername('register_user'));

    await expect(page).toHaveURL(/\/auth\/login/);
  });

  test('login redirects to markets page', async ({ page }) => {
    const username = uniqueUsername('login_user');
    await registerUser(page, username);

    await loginUser(page, username);

    await expect(page).toHaveURL(/\/markets/);
  });
});
