import api from './client'
import type {
  PlayerProfileResponse,
  UpdateProfileRequest,
} from '../types'

export const playerApi = {
  getMyProfile: () =>
    api.get<PlayerProfileResponse>('/player/me'),

  getProfile: (id: string) =>
    api.get<PlayerProfileResponse>(`/player/${id}`),

  updateProfile: (data: UpdateProfileRequest) =>
    api.put<PlayerProfileResponse>('/player/me', data),

  getLeaderboard: (top = 10) =>
    api.get<PlayerProfileResponse[]>('/player/leaderboard', { params: { top } }),
}
