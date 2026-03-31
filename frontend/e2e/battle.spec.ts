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

test.describe('Battle Arena', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page)
    await page.route('**/api/battle/mine', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          {
            id: 'b1',
            challengerId: '00000000-0000-0000-0000-000000000001',
            challengerUsername: 'Merlin',
            defenderId: '00000000-0000-0000-0000-000000000002',
            defenderUsername: 'Gandalf',
            status: 2,
            winnerId: '00000000-0000-0000-0000-000000000001',
            winnerUsername: 'Merlin',
            startedAt: '2026-01-01T00:00:00Z',
            finishedAt: '2026-01-01T01:00:00Z',
            narratorStory: 'An epic battle!',
            turns: [],
          },
        ]),
      }),
    )
    await page.route('**/api/battle/spells', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          {
            id: 'spell-1',
            name: 'Fireball',
            description: 'A ball of fire',
            manaCost: 10,
            baseDamage: 30,
            effect: 'Burn',
            element: 0,
          },
          {
            id: 'spell-2',
            name: 'Ice Shard',
            description: 'Sharp ice',
            manaCost: 8,
            baseDamage: 25,
            effect: 'Freeze',
            element: 1,
          },
        ]),
      }),
    )
    await page.goto('/battle')
  })

  test('shows battle arena page', async ({ page }) => {
    await expect(page.getByTestId('battle-view')).toBeVisible()
    await expect(page.getByText('Battle Arena')).toBeVisible()
  })

  test('shows available spells', async ({ page }) => {
    await expect(page.getByText('Fireball')).toBeVisible()
    await expect(page.getByText('Ice Shard')).toBeVisible()
  })

  test('shows finished battles', async ({ page }) => {
    await expect(page.getByText('Merlin vs Gandalf')).toBeVisible()
    await expect(page.getByText('Winner: Merlin')).toBeVisible()
  })

  test('challenge button opens form', async ({ page }) => {
    await page.getByTestId('challenge-btn').click()
    await expect(page.getByTestId('challenge-id-input')).toBeVisible()
  })
})
