import { test, expect } from '@playwright/test'

test.describe('Register Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/register')
  })

  test('shows register form with all fields', async ({ page }) => {
    await expect(page.getByTestId('register-form')).toBeVisible()
    await expect(page.getByTestId('username-input')).toBeVisible()
    await expect(page.getByTestId('email-input')).toBeVisible()
    await expect(page.getByTestId('password-input')).toBeVisible()
    await expect(page.getByTestId('referral-input')).toBeVisible()
    await expect(page.getByTestId('register-submit')).toBeVisible()
  })

  test('has correct page title', async ({ page }) => {
    await expect(page.getByText('Create Account')).toBeVisible()
  })

  test('has link to login page', async ({ page }) => {
    const link = page.getByRole('link', { name: 'Sign In' })
    await expect(link).toBeVisible()
    await expect(link).toHaveAttribute('href', '/login')
  })

  test('can fill in all fields', async ({ page }) => {
    await page.getByTestId('username-input').fill('MerlinTheGreat')
    await page.getByTestId('email-input').fill('merlin@wizard.com')
    await page.getByTestId('password-input').fill('magic123')
    await page.getByTestId('referral-input').fill('REF001')

    await expect(page.getByTestId('username-input')).toHaveValue('MerlinTheGreat')
    await expect(page.getByTestId('email-input')).toHaveValue('merlin@wizard.com')
    await expect(page.getByTestId('password-input')).toHaveValue('magic123')
    await expect(page.getByTestId('referral-input')).toHaveValue('REF001')
  })
})
