import { test, expect } from '@playwright/test'

test.describe('Navigation', () => {
  test('unauthenticated user is redirected to login', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page).toHaveURL(/\/login/)
  })

  test('navbar shows login and register links when not logged in', async ({ page }) => {
    await page.goto('/login')
    const navbar = page.getByTestId('navbar')
    await expect(navbar).toBeVisible()
    await expect(navbar.getByRole('link', { name: 'Login' })).toBeVisible()
    await expect(navbar.getByRole('link', { name: 'Register' })).toBeVisible()
  })

  test('leaderboard page is accessible without login', async ({ page }) => {
    await page.goto('/leaderboard')
    await expect(page.getByTestId('leaderboard-view')).toBeVisible()
    await expect(page.getByText('Leaderboard')).toBeVisible()
  })

  test('items page is accessible without login', async ({ page }) => {
    await page.goto('/items')
    await expect(page.getByTestId('items-view')).toBeVisible()
    await expect(page.getByText('Items Catalog')).toBeVisible()
  })
})
