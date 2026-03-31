import { test, expect, type Page } from '@playwright/test'

async function loginAs(page: Page) {
  const player = {
    id: '00000000-0000-0000-0000-000000000001',
    username: 'Merlin',
    email: 'merlin@test.com',
    isAdmin: false,
  }
  await page.addInitScript((p) => {
    localStorage.setItem('accessToken', 'fake-jwt-token')
    localStorage.setItem('refreshToken', 'fake-refresh-token')
    localStorage.setItem('player', JSON.stringify(p))
  }, player)
}

test.describe('Bank Page', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page)
    await page.route('**/api/bank', (route) => {
      if (route.request().method() === 'GET') {
        return route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            playerId: '00000000-0000-0000-0000-000000000001',
            goldBalance: 1200,
            updatedAt: '2026-01-01T00:00:00Z',
          }),
        })
      }
      return route.continue()
    })
    await page.route('**/api/bank/items', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          {
            id: 'bi-1',
            itemId: 'item-1',
            itemName: 'Elder Wand',
            itemDescription: 'The most powerful wand',
            itemType: 0,
            magicBonus: 50,
            strengthBonus: 0,
            wisdomBonus: 30,
            speedBonus: 10,
            acquiredAt: '2026-01-01T00:00:00Z',
          },
        ]),
      }),
    )
    await page.goto('/bank')
  })

  test('shows bank page with balance', async ({ page }) => {
    await expect(page.getByTestId('bank-view')).toBeVisible()
    await expect(page.getByTestId('bank-balance')).toContainText('1200')
  })

  test('shows deposit and withdraw forms', async ({ page }) => {
    await expect(page.getByTestId('deposit-input')).toBeVisible()
    await expect(page.getByTestId('deposit-btn')).toBeVisible()
    await expect(page.getByTestId('withdraw-input')).toBeVisible()
    await expect(page.getByTestId('withdraw-btn')).toBeVisible()
  })

  test('shows stored items', async ({ page }) => {
    await expect(page.getByText('Elder Wand')).toBeVisible()
    await expect(page.getByText('The most powerful wand')).toBeVisible()
  })
})
