import { test, expect, type Page } from '@playwright/test'

/** Injects fake auth tokens into localStorage so the app thinks we're logged in. */
async function loginAs(page: Page, opts: { username: string; isAdmin?: boolean }) {
  const player = {
    id: '00000000-0000-0000-0000-000000000001',
    username: opts.username,
    email: `${opts.username.toLowerCase()}@test.com`,
    isAdmin: opts.isAdmin ?? false,
  }
  await page.addInitScript((p) => {
    localStorage.setItem('accessToken', 'fake-jwt-token')
    localStorage.setItem('refreshToken', 'fake-refresh-token')
    localStorage.setItem('player', JSON.stringify(p))
  }, player)
}

test.describe('Dashboard (authenticated)', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, { username: 'Merlin' })
    // Mock the API calls so the page doesn't fail
    await page.route('**/api/player/me', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: '00000000-0000-0000-0000-000000000001',
          username: 'Merlin',
          email: 'merlin@test.com',
          goldCoins: 1500,
          level: 5,
          experience: 2500,
          magicPower: 35,
          strength: 20,
          wisdom: 40,
          speed: 25,
          referralCode: 'MERL1N',
          createdAt: '2026-01-01T00:00:00Z',
          isAdmin: false,
        }),
      }),
    )
    await page.route('**/api/bank', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          playerId: '00000000-0000-0000-0000-000000000001',
          goldBalance: 500,
          updatedAt: '2026-01-01T00:00:00Z',
        }),
      }),
    )
    await page.goto('/dashboard')
  })

  test('shows player overview', async ({ page }) => {
    await expect(page.getByTestId('dashboard-view')).toBeVisible()
    await expect(page.getByTestId('player-overview')).toBeVisible()
    await expect(page.getByText('Merlin')).toBeVisible()
    await expect(page.getByText('1500')).toBeVisible()
  })

  test('shows stats section', async ({ page }) => {
    await expect(page.getByTestId('player-stats')).toBeVisible()
    await expect(page.getByText('Magic Power')).toBeVisible()
    await expect(page.getByText('Strength')).toBeVisible()
  })

  test('shows bank overview', async ({ page }) => {
    await expect(page.getByTestId('bank-overview')).toBeVisible()
    await expect(page.getByText('500')).toBeVisible()
  })

  test('navbar shows username and logout button', async ({ page }) => {
    const navbar = page.getByTestId('navbar')
    await expect(navbar.getByText('Merlin')).toBeVisible()
    await expect(page.getByTestId('logout-btn')).toBeVisible()
  })

  test('logout clears auth and redirects to login', async ({ page }) => {
    await page.getByTestId('logout-btn').click()
    await expect(page).toHaveURL(/\/login/)
  })

  test('navigation links are visible', async ({ page }) => {
    const navbar = page.getByTestId('navbar')
    await expect(navbar.getByRole('link', { name: 'Dashboard' })).toBeVisible()
    await expect(navbar.getByRole('link', { name: 'Battle' })).toBeVisible()
    await expect(navbar.getByRole('link', { name: 'Bank' })).toBeVisible()
    await expect(navbar.getByRole('link', { name: 'Broom Racing' })).toBeVisible()
    await expect(navbar.getByRole('link', { name: 'Fellowship' })).toBeVisible()
    await expect(navbar.getByRole('link', { name: 'Leaderboard' })).toBeVisible()
  })
})

test.describe('Admin access', () => {
  test('admin link visible for admin user', async ({ page }) => {
    await loginAs(page, { username: 'Admin', isAdmin: true })
    await page.route('**/api/player/me', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: '00000000-0000-0000-0000-000000000002',
          username: 'Admin',
          email: 'admin@test.com',
          goldCoins: 9999,
          level: 99,
          experience: 99999,
          magicPower: 100,
          strength: 100,
          wisdom: 100,
          speed: 100,
          referralCode: 'ADM1N',
          createdAt: '2026-01-01T00:00:00Z',
          isAdmin: true,
        }),
      }),
    )
    await page.route('**/api/bank', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          playerId: '00000000-0000-0000-0000-000000000002',
          goldBalance: 50000,
          updatedAt: '2026-01-01T00:00:00Z',
        }),
      }),
    )
    await page.goto('/dashboard')
    await expect(page.getByTestId('navbar').getByText('Admin')).toBeVisible()
  })

  test('non-admin is redirected from admin page', async ({ page }) => {
    await loginAs(page, { username: 'Regular' })
    await page.goto('/admin')
    await expect(page).toHaveURL(/\/dashboard/)
  })
})
