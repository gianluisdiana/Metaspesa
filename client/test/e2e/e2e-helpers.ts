import type { Page } from '@playwright/test';

export const password = 'SecurePass1!';

export function uniqueUsername(prefix: string) {
  return `${prefix}_${Date.now()}_${crypto.randomUUID()}`;
}

export async function registerUser(page: Page, username: string) {
  await page.goto('/auth/register');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill(password);
  await page.getByLabel('Confirm Password', { exact: true }).fill(password);
  await page.getByRole('button', { name: /create account/i }).click();
}

export async function loginUser(page: Page, username: string) {
  await page.goto('/auth/login');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill(password);
  await page.getByRole('button', { name: /login/i }).click();
}

export async function registerAndLogin(page: Page) {
  const username = uniqueUsername('e2e_user');
  await registerUser(page, username);
  await loginUser(page, username);
}
