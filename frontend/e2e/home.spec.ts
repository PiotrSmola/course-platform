import { test, expect } from '@playwright/test'

// Full catalog/branding checks need `docker compose up` with frontend on :5173.
// This smoke test only asserts the page loads when the dev server is reachable.
test('home loads', async ({ page }) => {
  await page.goto('/')
  await expect(page.locator('body')).toBeVisible()
})
