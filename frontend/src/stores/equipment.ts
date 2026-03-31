import { defineStore } from 'pinia'
import { ref } from 'vue'
import { equipmentApi } from '../api/equipment'
import type { EquipmentSlots } from '../types'

export const useEquipmentStore = defineStore('equipment', () => {
  const equipment = ref<EquipmentSlots | null>(null)
  const loading = ref(false)
  const error = ref('')

  async function fetchEquipment() {
    loading.value = true
    error.value = ''
    try {
      const res = await equipmentApi.getEquipment()
      equipment.value = res.data
    } catch {
      error.value = 'Failed to load equipment'
    } finally {
      loading.value = false
    }
  }

  async function equipItem(bankItemId: string) {
    error.value = ''
    try {
      const res = await equipmentApi.equipItem({ bankItemId })
      equipment.value = res.data
    } catch {
      error.value = 'Failed to equip item'
    }
  }

  async function unequipItem(slot: string) {
    error.value = ''
    try {
      const res = await equipmentApi.unequipItem({ slot })
      equipment.value = res.data
    } catch {
      error.value = 'Failed to unequip item'
    }
  }

  return { equipment, loading, error, fetchEquipment, equipItem, unequipItem }
})
