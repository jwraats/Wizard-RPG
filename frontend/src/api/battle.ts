import api from './client'
import type {
  BattleResponse,
  CastSpellRequest,
  ChallengeBattleRequest,
  SpellResponse,
} from '../types'

export const battleApi = {
  getSpells: () =>
    api.get<SpellResponse[]>('/battle/spells'),

  challenge: (data: ChallengeBattleRequest) =>
    api.post<BattleResponse>('/battle/challenge', data),

  accept: (battleId: string) =>
    api.post<BattleResponse>(`/battle/${battleId}/accept`),

  getBattle: (battleId: string) =>
    api.get<BattleResponse>(`/battle/${battleId}`),

  getMyBattles: () =>
    api.get<BattleResponse[]>('/battle/mine'),

  castSpell: (battleId: string, data: CastSpellRequest) =>
    api.post<BattleResponse>(`/battle/${battleId}/cast`, data),
}
