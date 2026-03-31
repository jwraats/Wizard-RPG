import api from './client'
import type { QuestResponse } from '../types'

export const questApi = {
  getMyQuests: () =>
    api.get<QuestResponse[]>('/quest'),

  generateDaily: () =>
    api.post('/quest/generate-daily'),

  generateWeekly: () =>
    api.post('/quest/generate-weekly'),
}
