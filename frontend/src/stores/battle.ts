import { defineStore } from 'pinia'
import { ref } from 'vue'
import { battleApi } from '../api/battle'
import type { BattleResponse, SpellResponse } from '../types'

export const useBattleStore = defineStore('battle', () => {
  const battles = ref<BattleResponse[]>([])
  const currentBattle = ref<BattleResponse | null>(null)
  const spells = ref<SpellResponse[]>([])
  const loading = ref(false)
  const error = ref('')

  async function fetchSpells() {
    try {
      const res = await battleApi.getSpells()
      spells.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load spells'
    }
  }

  async function fetchMyBattles() {
    loading.value = true
    error.value = ''
    try {
      const res = await battleApi.getMyBattles()
      battles.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load battles'
    } finally {
      loading.value = false
    }
  }

  async function fetchBattle(battleId: string) {
    loading.value = true
    error.value = ''
    try {
      const res = await battleApi.getBattle(battleId)
      currentBattle.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load battle'
    } finally {
      loading.value = false
    }
  }

  async function challenge(defenderId: string) {
    loading.value = true
    error.value = ''
    try {
      const res = await battleApi.challenge({ defenderId })
      currentBattle.value = res.data
      return res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Challenge failed'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function acceptBattle(battleId: string) {
    loading.value = true
    try {
      const res = await battleApi.accept(battleId)
      currentBattle.value = res.data
      return res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Accept failed'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function castSpell(battleId: string, spellId: string) {
    loading.value = true
    error.value = ''
    try {
      const res = await battleApi.castSpell(battleId, { spellId })
      currentBattle.value = res.data
      return res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Cast spell failed'
      throw e
    } finally {
      loading.value = false
    }
  }

  return {
    battles, currentBattle, spells, loading, error,
    fetchSpells, fetchMyBattles, fetchBattle, challenge, acceptBattle, castSpell,
  }
})
