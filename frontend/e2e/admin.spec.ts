import { test, expect, type Page } from '@playwright/test'

async function loginAsAdmin(page: Page) {
  const player = {
    id: '00000000-0000-0000-0000-000000000001',
    username: 'Admin',
    email: 'admin@test.com',
    isAdmin: true,
  }
  await page.addInitScript((p) => {
    localStorage.setItem('accessToken', 'fake-jwt-token')
    localStorage.setItem('refreshToken', 'fake-refresh-token')
    localStorage.setItem('player', JSON.stringify(p))
  }, player)
}

test.describe('Admin Panel', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page)
    await page.route('**/api/admin/players', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          {
            id: '00000000-0000-0000-0000-000000000001',
            username: 'Admin',
            email: 'admin@test.com',
            goldCoins: 9999,
            level: 99,
            experience: 99999,
            isAdmin: true,
            createdAt: '2026-01-01T00:00:00Z',
          },
          {
            id: '00000000-0000-0000-0000-000000000002',
            username: 'Player1',
            email: 'player1@test.com',
            goldCoins: 500,
            level: 3,
            experience: 1000,
            isAdmin: false,
            createdAt: '2026-02-01T00:00:00Z',
          },
        ]),
      }),
    )
    await page.route('**/api/item', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          {
            id: 'item-1',
            name: 'Elder Wand',
            description: 'Legendary wand',
            type: 0,
            magicBonus: 50,
            strengthBonus: 10,
            wisdomBonus: 30,
            speedBonus: 5,
            price: 5000,
          },
        ]),
      }),
    )
    await page.goto('/admin')
  })

  test('shows admin panel', async ({ page }) => {
    await expect(page.getByTestId('admin-view')).toBeVisible()
    await expect(page.getByText('Admin Panel')).toBeVisible()
  })

  test('shows players list', async ({ page }) => {
    await expect(page.getByText('Admin')).toBeVisible()
    await expect(page.getByText('Player1')).toBeVisible()
  })

  test('can switch to items tab', async ({ page }) => {
    await page.getByRole('button', { name: 'items' }).click()
    await expect(page.getByText('Elder Wand')).toBeVisible()
  })

  test('can switch to leagues tab', async ({ page }) => {
    await page.getByRole('button', { name: 'leagues' }).click()
    await expect(page.getByText('Create Broom League')).toBeVisible()
  })

  test('shows set gold form', async ({ page }) => {
    await expect(page.getByTestId('admin-gold-player-id')).toBeVisible()
    await expect(page.getByTestId('admin-gold-amount')).toBeVisible()
  })
})
