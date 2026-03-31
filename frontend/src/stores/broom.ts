import { defineStore } from 'pinia'
import { ref } from 'vue'
import { broomApi } from '../api/broom'
import type { BroomBetResponse, BroomLeagueResponse } from '../types'

export const useBroomStore = defineStore('broom', () => {
  const leagues = ref<BroomLeagueResponse[]>([])
  const currentLeague = ref<BroomLeagueResponse | null>(null)
  const myBets = ref<BroomBetResponse[]>([])
  const loading = ref(false)
  const error = ref('')

  async function fetchLeagues(status?: string) {
    loading.value = true
    error.value = ''
    try {
      const res = await broomApi.getLeagues(status)
      leagues.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load leagues'
    } finally {
      loading.value = false
    }
  }

  async function fetchLeague(leagueId: string) {
    loading.value = true
    try {
      const res = await broomApi.getLeague(leagueId)
      currentLeague.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load league'
    } finally {
      loading.value = false
    }
  }

  async function placeBet(leagueId: string, teamId: string, amount: number) {
    loading.value = true
    error.value = ''
    try {
      const res = await broomApi.placeBet({ leagueId, teamId, amount })
      myBets.value.push(res.data)
      return res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Bet failed'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function fetchMyBets() {
    loading.value = true
    try {
      const res = await broomApi.getMyBets()
      myBets.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load bets'
    } finally {
      loading.value = false
    }
  }

  return { leagues, currentLeague, myBets, loading, error, fetchLeagues, fetchLeague, placeBet, fetchMyBets }
})
