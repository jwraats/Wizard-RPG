import api from './client'
import type { LoginRewardResponse, LoginRewardStatus } from '../types'

export const loginRewardApi = {
  getStatus: () =>
    api.get<LoginRewardStatus>('/login-reward/status'),

  claimReward: () =>
    api.post<LoginRewardResponse>('/login-reward/claim'),
}
