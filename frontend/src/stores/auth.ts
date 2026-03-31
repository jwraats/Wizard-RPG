import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '../api/auth'
import type { PlayerAuthInfo, LoginRequest, RegisterRequest } from '../types'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('accessToken') || '')
  const refreshToken = ref(localStorage.getItem('refreshToken') || '')
  const player = ref<PlayerAuthInfo | null>(
    JSON.parse(localStorage.getItem('player') || 'null'),
  )

  const isLoggedIn = computed(() => !!token.value)
  const isAdmin = computed(() => player.value?.isAdmin ?? false)

  function persist(accessToken: string, refresh: string, p: PlayerAuthInfo) {
    token.value = accessToken
    refreshToken.value = refresh
    player.value = p
    localStorage.setItem('accessToken', accessToken)
    localStorage.setItem('refreshToken', refresh)
    localStorage.setItem('player', JSON.stringify(p))
  }

  async function login(data: LoginRequest) {
    const res = await authApi.login(data)
    persist(res.data.accessToken, res.data.refreshToken, res.data.player)
  }

  async function register(data: RegisterRequest) {
    const res = await authApi.register(data)
    persist(res.data.accessToken, res.data.refreshToken, res.data.player)
  }

  function logout() {
    token.value = ''
    refreshToken.value = ''
    player.value = null
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('player')
  }

  return { token, refreshToken, player, isLoggedIn, isAdmin, login, register, logout }
})
