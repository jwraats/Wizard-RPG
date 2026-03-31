import api from './client'
import type { HouseLeaderboardEntry, HousePointsResponse, SelectHouseRequest } from '../types'

export const houseApi = {
  getLeaderboard: () =>
    api.get<HouseLeaderboardEntry[]>('/house/leaderboard'),

  getHousePoints: (house: string) =>
    api.get<HousePointsResponse[]>(`/house/${house}/points`),

  selectHouse: (data: SelectHouseRequest) =>
    api.post('/house/select', data),
}
