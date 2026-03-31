import api from './client'
import type { AchievementResponse } from '../types'

export const achievementApi = {
  getMyAchievements: () =>
    api.get<AchievementResponse[]>('/achievement'),

  checkAchievements: () =>
    api.post('/achievement/check'),
}
