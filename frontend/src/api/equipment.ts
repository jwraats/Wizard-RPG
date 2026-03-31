import api from './client'
import type { EquipmentSlots, EquipItemRequest, UnequipItemRequest } from '../types'

export const equipmentApi = {
  getEquipment: () =>
    api.get<EquipmentSlots>('/equipment'),

  equipItem: (data: EquipItemRequest) =>
    api.post<EquipmentSlots>('/equipment/equip', data),

  unequipItem: (data: UnequipItemRequest) =>
    api.post<EquipmentSlots>('/equipment/unequip', data),
}
