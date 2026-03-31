import { defineStore } from 'pinia'
import { ref } from 'vue'
import { questApi } from '../api/quest'
import type { QuestResponse } from '../types'

export const useQuestStore = defineStore('quest', () => {
  const quests = ref<QuestResponse[]>([])
  const loading = ref(false)
  const error = ref('')

  async function fetchQuests() {
    loading.value = true
    error.value = ''
    try {
      const res = await questApi.getMyQuests()
      quests.value = res.data
    } catch {
      error.value = 'Failed to load quests'
    } finally {
      loading.value = false
    }
  }

  async function generateDaily() {
    try {
      await questApi.generateDaily()
      await fetchQuests()
    } catch {
      error.value = 'Failed to generate daily quests'
    }
  }

  async function generateWeekly() {
    try {
      await questApi.generateWeekly()
      await fetchQuests()
    } catch {
      error.value = 'Failed to generate weekly quests'
    }
  }

  return { quests, loading, error, fetchQuests, generateDaily, generateWeekly }
})
