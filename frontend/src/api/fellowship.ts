import api from './client'
import type {
  CreateFellowshipRequest,
  FellowshipResponse,
} from '../types'

export const fellowshipApi = {
  getAll: () =>
    api.get<FellowshipResponse[]>('/fellowship'),

  get: (id: string) =>
    api.get<FellowshipResponse>(`/fellowship/${id}`),

  getMine: () =>
    api.get<FellowshipResponse | null>('/fellowship/mine'),

  create: (data: CreateFellowshipRequest) =>
    api.post<FellowshipResponse>('/fellowship', data),

  join: (referralCode: string) =>
    api.post<FellowshipResponse>('/fellowship/join', { referralCode }),

  leave: (id: string) =>
    api.delete(`/fellowship/${id}/leave`),

  updateContribution: (id: string, memberId: string, contributionPercent: number) =>
    api.put<FellowshipResponse>(`/fellowship/${id}/contribution`, {
      memberId,
      contributionPercent,
    }),
}
