import api from './client'
import type {
  BroomBetResponse,
  BroomLeagueResponse,
  PlaceBetRequest,
} from '../types'

export const broomApi = {
  getLeagues: (status?: string) =>
    api.get<BroomLeagueResponse[]>('/broomgame/leagues', {
      params: status ? { status } : undefined,
    }),

  getLeague: (leagueId: string) =>
    api.get<BroomLeagueResponse>(`/broomgame/leagues/${leagueId}`),

  placeBet: (data: PlaceBetRequest) =>
    api.post<BroomBetResponse>('/broomgame/bets', data),

  getMyBets: () =>
    api.get<BroomBetResponse[]>('/broomgame/bets'),
}
