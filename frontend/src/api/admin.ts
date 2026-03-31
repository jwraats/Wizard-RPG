import api from './client'
import type {
  AdminPlayerResponse,
  BroomLeagueResponse,
  CreateItemRequest,
  CreateLeagueRequest,
  ItemResponse,
  SetAdminRequest,
  SetGoldRequest,
} from '../types'

export const adminApi = {
  getPlayers: () =>
    api.get<AdminPlayerResponse[]>('/admin/players'),

  getPlayer: (id: string) =>
    api.get<AdminPlayerResponse>(`/admin/players/${id}`),

  setGold: (data: SetGoldRequest) =>
    api.put<AdminPlayerResponse>('/admin/players/gold', data),

  setAdminStatus: (data: SetAdminRequest) =>
    api.put<AdminPlayerResponse>('/admin/players/admin-status', data),

  createItem: (data: CreateItemRequest) =>
    api.post<ItemResponse>('/admin/items', data),

  deleteItem: (itemId: string) =>
    api.delete(`/admin/items/${itemId}`),

  grantItem: (playerId: string, itemId: string) =>
    api.post('/admin/items/grant', { playerId, itemId }),

  revokeItem: (bankItemId: string) =>
    api.delete('/admin/items/revoke', { data: { bankItemId } }),

  createLeague: (data: CreateLeagueRequest) =>
    api.post<BroomLeagueResponse>('/admin/leagues', data),

  resolveLeague: (leagueId: string, winnerTeamId: string) =>
    api.post<BroomLeagueResponse>(`/admin/leagues/${leagueId}/resolve`, { winnerTeamId }),
}
