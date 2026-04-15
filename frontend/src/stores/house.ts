import { defineStore } from 'pinia'
import { ref } from 'vue'
import { houseApi } from '../api/house'
import type { HouseLeaderboardEntry } from '../types'

export const useHouseStore = defineStore('house', () => {
  const leaderboard = ref<HouseLeaderboardEntry[]>([])
  const loading = ref(false)
  const error = ref('')

  async function fetchLeaderboard() {
    loading.value = true
    error.value = ''
    try {
      const res = await houseApi.getLeaderboard()
      leaderboard.value = res.data
    } catch {
      error.value = 'Failed to load house leaderboard'
    } finally {
      loading.value = false
    }
  }

  return { leaderboard, loading, error, fetchLeaderboard }
})
