import { test, expect } from '@playwright/test'

test.describe('smoke', () => {
  test('home loads catalog', async ({ page }) => {
    await page.goto('/')
    await expect(page.locator('body')).toBeVisible()
    await expect(page.getByRole('heading').first()).toBeVisible({ timeout: 15000 })
  })

  test('courses browse loads', async ({ page }) => {
    await page.goto('/courses')
    await expect(page.locator('body')).toBeVisible()
    await expect(page).toHaveURL(/\/courses/)
  })

  test('404 page for unknown route', async ({ page }) => {
    await page.goto('/this-route-does-not-exist-xyz')
    await expect(page.getByText('404')).toBeVisible({ timeout: 10000 })
    await expect(page.getByText(/nie istnieje/i)).toBeVisible()
  })

  test('login page and forgot password link', async ({ page }) => {
    await page.goto('/login')
    await expect(page.getByRole('heading', { name: /logowanie/i })).toBeVisible()
    await page.getByRole('link', { name: /nie pamiętasz hasła/i }).click()
    await expect(page).toHaveURL(/\/forgot-password/)
  })

  test('student login reaches my courses', async ({ page }) => {
    await page.goto('/login')
    await page.locator('#email-input, input[type="email"]').first().fill('piotr.nowak@courseplatform.com')
    await page.locator('#password-input, input[type="password"]').first().fill('Student123!')
    await page.getByRole('button', { name: /zaloguj/i }).click()
    await expect(page).not.toHaveURL(/\/login/, { timeout: 20000 })
    await page.goto('/my-courses')
    await expect(page).toHaveURL(/\/my-courses/)
    await expect(page.locator('body')).toBeVisible()
  })

  test('forbidden for student on admin', async ({ page }) => {
    await page.goto('/login')
    await page.locator('#email-input, input[type="email"]').first().fill('piotr.nowak@courseplatform.com')
    await page.locator('#password-input, input[type="password"]').first().fill('Student123!')
    await page.getByRole('button', { name: /zaloguj/i }).click()
    await expect(page).not.toHaveURL(/\/login/, { timeout: 20000 })
    await page.goto('/admin')
    await expect(page).toHaveURL(/\/error\/403/, { timeout: 15000 })
    await expect(page.getByText('403')).toBeVisible()
  })

  test('instructor dashboard publish controls', async ({ page }) => {
    await page.goto('/login')
    await page.locator('#email-input, input[type="email"]').first().fill('instructor@courseplatform.com')
    await page.locator('#password-input, input[type="password"]').first().fill('Instructor123!')
    await page.getByRole('button', { name: /zaloguj/i }).click()
    await expect(page).not.toHaveURL(/\/login/, { timeout: 20000 })
    await page.goto('/instructor')
    await expect(page).toHaveURL(/\/instructor/)
    await expect(page.getByText(/panel instruktora|twoje kursy/i).first()).toBeVisible({ timeout: 15000 })
  })
})
