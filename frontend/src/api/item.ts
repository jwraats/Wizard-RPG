import api from './client'
import type { ItemResponse } from '../types'

export const itemApi = {
  getAll: () =>
    api.get<ItemResponse[]>('/item'),
}
