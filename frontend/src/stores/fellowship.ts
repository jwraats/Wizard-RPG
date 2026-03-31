import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fellowshipApi } from '../api/fellowship'
import type { FellowshipResponse } from '../types'

export const useFellowshipStore = defineStore('fellowship', () => {
  const fellowships = ref<FellowshipResponse[]>([])
  const myFellowship = ref<FellowshipResponse | null>(null)
  const loading = ref(false)
  const error = ref('')

  async function fetchAll() {
    loading.value = true
    error.value = ''
    try {
      const res = await fellowshipApi.getAll()
      fellowships.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load fellowships'
    } finally {
      loading.value = false
    }
  }

  async function fetchMine() {
    loading.value = true
    error.value = ''
    try {
      const res = await fellowshipApi.getMine()
      myFellowship.value = res.status === 204 ? null : res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load fellowship'
    } finally {
      loading.value = false
    }
  }

  async function create(name: string) {
    loading.value = true
    error.value = ''
    try {
      const res = await fellowshipApi.create({ name })
      myFellowship.value = res.data
      return res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to create fellowship'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function join(referralCode: string) {
    loading.value = true
    error.value = ''
    try {
      const res = await fellowshipApi.join(referralCode)
      myFellowship.value = res.data
      return res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to join fellowship'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function leave(id: string) {
    loading.value = true
    try {
      await fellowshipApi.leave(id)
      myFellowship.value = null
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to leave fellowship'
      throw e
    } finally {
      loading.value = false
    }
  }

  return { fellowships, myFellowship, loading, error, fetchAll, fetchMine, create, join, leave }
})
