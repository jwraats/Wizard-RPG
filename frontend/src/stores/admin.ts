import { defineStore } from 'pinia'
import { ref } from 'vue'
import { adminApi } from '../api/admin'
import { itemApi } from '../api/item'
import type { AdminPlayerResponse, ItemResponse } from '../types'

export const useAdminStore = defineStore('admin', () => {
  const players = ref<AdminPlayerResponse[]>([])
  const items = ref<ItemResponse[]>([])
  const loading = ref(false)
  const error = ref('')

  async function fetchPlayers() {
    loading.value = true
    error.value = ''
    try {
      const res = await adminApi.getPlayers()
      players.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load players'
    } finally {
      loading.value = false
    }
  }

  async function fetchItems() {
    loading.value = true
    error.value = ''
    try {
      const res = await itemApi.getAll()
      items.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load items'
    } finally {
      loading.value = false
    }
  }

  async function setGold(playerId: string, amount: number) {
    loading.value = true
    try {
      const res = await adminApi.setGold({ playerId, amount })
      const idx = players.value.findIndex((p) => p.id === playerId)
      if (idx !== -1) players.value[idx] = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to set gold'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function setAdminStatus(playerId: string, isAdmin: boolean) {
    loading.value = true
    try {
      const res = await adminApi.setAdminStatus({ playerId, isAdmin })
      const idx = players.value.findIndex((p) => p.id === playerId)
      if (idx !== -1) players.value[idx] = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to update admin status'
      throw e
    } finally {
      loading.value = false
    }
  }

  return { players, items, loading, error, fetchPlayers, fetchItems, setGold, setAdminStatus }
})
