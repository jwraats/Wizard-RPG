import { defineStore } from 'pinia'
import { ref } from 'vue'
import { playerApi } from '../api/player'
import type { PlayerProfileResponse } from '../types'

export const usePlayerStore = defineStore('player', () => {
  const profile = ref<PlayerProfileResponse | null>(null)
  const leaderboard = ref<PlayerProfileResponse[]>([])
  const loading = ref(false)
  const error = ref('')

  async function fetchProfile() {
    loading.value = true
    error.value = ''
    try {
      const res = await playerApi.getMyProfile()
      profile.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load profile'
    } finally {
      loading.value = false
    }
  }

  async function fetchLeaderboard(top = 10) {
    loading.value = true
    try {
      const res = await playerApi.getLeaderboard(top)
      leaderboard.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load leaderboard'
    } finally {
      loading.value = false
    }
  }

  async function updateProfile(username?: string, email?: string) {
    loading.value = true
    try {
      const res = await playerApi.updateProfile({ username, email })
      profile.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to update profile'
      throw e
    } finally {
      loading.value = false
    }
  }

  return { profile, leaderboard, loading, error, fetchProfile, fetchLeaderboard, updateProfile }
})
