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

test.describe('Broom Racing', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page)
    await page.route('**/api/broomgame/leagues*', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          {
            id: 'league-1',
            name: 'Grand Broom Prix',
            startTime: '2026-04-01T10:00:00Z',
            endTime: '2026-04-01T12:00:00Z',
            status: 1,
            winnerTeamId: null,
            teams: [
              { id: 'team-1', name: 'Lightning Bolts', odds: 2.5 },
              { id: 'team-2', name: 'Storm Riders', odds: 1.8 },
            ],
          },
        ]),
      }),
    )
    await page.route('**/api/broomgame/bets', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      }),
    )
    await page.goto('/broom')
  })

  test('shows broom racing page', async ({ page }) => {
    await expect(page.getByTestId('broom-view')).toBeVisible()
    await expect(page.getByText('Broom Racing')).toBeVisible()
  })

  test('shows leagues', async ({ page }) => {
    await expect(page.getByText('Grand Broom Prix')).toBeVisible()
    await expect(page.getByText('Lightning Bolts')).toBeVisible()
    await expect(page.getByText('Storm Riders')).toBeVisible()
  })

  test('can switch to bets tab', async ({ page }) => {
    await page.getByRole('button', { name: 'My Bets' }).click()
    await expect(page.getByText('No bets placed yet')).toBeVisible()
  })
})
