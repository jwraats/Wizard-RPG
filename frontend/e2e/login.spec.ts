import { test, expect } from '@playwright/test'

test.describe('Login Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login')
  })

  test('shows login form with all fields', async ({ page }) => {
    await expect(page.getByTestId('login-form')).toBeVisible()
    await expect(page.getByTestId('email-input')).toBeVisible()
    await expect(page.getByTestId('password-input')).toBeVisible()
    await expect(page.getByTestId('login-submit')).toBeVisible()
  })

  test('has correct page title', async ({ page }) => {
    await expect(page.getByText('Welcome Back')).toBeVisible()
  })

  test('has link to register page', async ({ page }) => {
    const link = page.getByRole('link', { name: 'Register' })
    await expect(link).toBeVisible()
    await expect(link).toHaveAttribute('href', '/register')
  })

  test('submit button shows "Sign In" text', async ({ page }) => {
    await expect(page.getByTestId('login-submit')).toHaveText('Sign In')
  })

  test('can fill in email and password', async ({ page }) => {
    await page.getByTestId('email-input').fill('wizard@example.com')
    await page.getByTestId('password-input').fill('password123')
    await expect(page.getByTestId('email-input')).toHaveValue('wizard@example.com')
    await expect(page.getByTestId('password-input')).toHaveValue('password123')
  })
})
