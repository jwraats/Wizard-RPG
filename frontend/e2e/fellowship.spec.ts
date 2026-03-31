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

test.describe('Fellowship Page', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page)
    await page.route('**/api/fellowship/mine', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 'f1',
          name: 'Order of the Phoenix',
          ownerId: '00000000-0000-0000-0000-000000000001',
          ownerUsername: 'Merlin',
          referralCode: 'PHOENIX',
          goldPerHour: 100,
          createdAt: '2026-01-01T00:00:00Z',
          members: [
            {
              id: 'm1',
              playerId: '00000000-0000-0000-0000-000000000001',
              username: 'Merlin',
              joinedAt: '2026-01-01T00:00:00Z',
              contributionPercent: 50,
            },
          ],
        }),
      }),
    )
    await page.route('**/api/fellowship', (route) => {
      if (route.request().method() === 'GET' && !route.request().url().includes('/mine')) {
        return route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]),
        })
      }
      return route.continue()
    })
    await page.goto('/fellowship')
  })

  test('shows fellowship page', async ({ page }) => {
    await expect(page.getByTestId('fellowship-view')).toBeVisible()
    await expect(page.getByText('Fellowships')).toBeVisible()
  })

  test('shows my fellowship', async ({ page }) => {
    await expect(page.getByTestId('my-fellowship')).toBeVisible()
    await expect(page.getByText('Order of the Phoenix')).toBeVisible()
    await expect(page.getByText('PHOENIX')).toBeVisible()
  })
})

test.describe('Fellowship - No membership', () => {
  test('shows create/join buttons when not in a fellowship', async ({ page }) => {
    const player = {
      id: '00000000-0000-0000-0000-000000000099',
      username: 'NewWizard',
      email: 'new@test.com',
      isAdmin: false,
    }
    await page.addInitScript((p) => {
      localStorage.setItem('accessToken', 'fake-jwt-token')
      localStorage.setItem('refreshToken', 'fake-refresh-token')
      localStorage.setItem('player', JSON.stringify(p))
    }, player)

    await page.route('**/api/fellowship/mine', (route) =>
      route.fulfill({ status: 204 }),
    )
    await page.route('**/api/fellowship', (route) => {
      if (route.request().method() === 'GET' && !route.request().url().includes('/mine')) {
        return route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]),
        })
      }
      return route.continue()
    })
    await page.goto('/fellowship')

    await expect(page.getByTestId('create-fellowship-btn')).toBeVisible()
    await expect(page.getByTestId('join-fellowship-btn')).toBeVisible()
  })
})
