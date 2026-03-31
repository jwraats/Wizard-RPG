<script setup lang="ts">
import { onMounted } from 'vue'
import { usePlayerStore } from '../stores/player'
import { useBankStore } from '../stores/bank'
import { useAuthStore } from '../stores/auth'
import TrailerSection from '../components/TrailerSection.vue'

const auth = useAuthStore()
const playerStore = usePlayerStore()
const bankStore = useBankStore()

onMounted(async () => {
  await Promise.all([playerStore.fetchProfile(), bankStore.fetchAccount()])
})
</script>

<template>
  <div data-testid="dashboard-view">
    <h1 class="text-3xl font-bold text-indigo-900 mb-6">🏰 Dashboard</h1>

    <TrailerSection />

    <div v-if="playerStore.loading" class="text-center py-12 text-gray-500">Loading…</div>

    <div v-else-if="playerStore.profile" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <!-- Player Overview -->
      <div class="bg-white rounded-xl shadow p-6" data-testid="player-overview">
        <h2 class="text-lg font-semibold text-indigo-800 mb-4">⚔️ {{ playerStore.profile.username }}</h2>
        <div class="space-y-2 text-sm">
          <div class="flex justify-between">
            <span class="text-gray-500">Level</span>
            <span class="font-semibold">{{ playerStore.profile.level }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500">Experience</span>
            <span class="font-semibold">{{ playerStore.profile.experience }} XP</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500">Gold</span>
            <span class="font-semibold text-yellow-600">🪙 {{ playerStore.profile.goldCoins }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500">Referral Code</span>
            <span class="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{{ playerStore.profile.referralCode }}</span>
          </div>
        </div>
      </div>

      <!-- Stats -->
      <div class="bg-white rounded-xl shadow p-6" data-testid="player-stats">
        <h2 class="text-lg font-semibold text-indigo-800 mb-4">📊 Stats</h2>
        <div class="space-y-3">
          <div>
            <div class="flex justify-between text-sm mb-1">
              <span>Magic Power</span>
              <span class="font-semibold">{{ playerStore.profile.magicPower }}</span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-2">
              <div class="bg-purple-500 h-2 rounded-full" :style="{ width: Math.min(playerStore.profile.magicPower, 100) + '%' }"></div>
            </div>
          </div>
          <div>
            <div class="flex justify-between text-sm mb-1">
              <span>Strength</span>
              <span class="font-semibold">{{ playerStore.profile.strength }}</span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-2">
              <div class="bg-red-500 h-2 rounded-full" :style="{ width: Math.min(playerStore.profile.strength, 100) + '%' }"></div>
            </div>
          </div>
          <div>
            <div class="flex justify-between text-sm mb-1">
              <span>Wisdom</span>
              <span class="font-semibold">{{ playerStore.profile.wisdom }}</span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-2">
              <div class="bg-blue-500 h-2 rounded-full" :style="{ width: Math.min(playerStore.profile.wisdom, 100) + '%' }"></div>
            </div>
          </div>
          <div>
            <div class="flex justify-between text-sm mb-1">
              <span>Speed</span>
              <span class="font-semibold">{{ playerStore.profile.speed }}</span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-2">
              <div class="bg-green-500 h-2 rounded-full" :style="{ width: Math.min(playerStore.profile.speed, 100) + '%' }"></div>
            </div>
          </div>
        </div>
      </div>

      <!-- Bank -->
      <div class="bg-white rounded-xl shadow p-6" data-testid="bank-overview">
        <h2 class="text-lg font-semibold text-indigo-800 mb-4">🏦 Bank</h2>
        <div v-if="bankStore.account" class="text-sm space-y-2">
          <div class="flex justify-between">
            <span class="text-gray-500">Gold in Bank</span>
            <span class="font-semibold text-yellow-600">🪙 {{ bankStore.account.goldBalance }}</span>
          </div>
        </div>
        <div v-else class="text-sm text-gray-400">No bank account yet</div>
        <router-link to="/bank" class="inline-block mt-4 text-indigo-600 hover:underline text-sm">
          Go to Bank →
        </router-link>
      </div>

      <!-- Quick Actions -->
      <div class="bg-white rounded-xl shadow p-6 md:col-span-2 lg:col-span-3">
        <h2 class="text-lg font-semibold text-indigo-800 mb-4">⚡ Quick Actions</h2>
        <div class="flex flex-wrap gap-3">
          <router-link to="/battle" class="bg-red-100 text-red-700 px-4 py-2 rounded-lg hover:bg-red-200 transition text-sm font-medium">
            ⚔️ Battle Arena
          </router-link>
          <router-link to="/broom" class="bg-amber-100 text-amber-700 px-4 py-2 rounded-lg hover:bg-amber-200 transition text-sm font-medium">
            🧹 Broom Racing
          </router-link>
          <router-link to="/fellowship" class="bg-green-100 text-green-700 px-4 py-2 rounded-lg hover:bg-green-200 transition text-sm font-medium">
            🤝 Fellowship
          </router-link>
          <router-link to="/leaderboard" class="bg-indigo-100 text-indigo-700 px-4 py-2 rounded-lg hover:bg-indigo-200 transition text-sm font-medium">
            🏆 Leaderboard
          </router-link>
          <router-link to="/items" class="bg-purple-100 text-purple-700 px-4 py-2 rounded-lg hover:bg-purple-200 transition text-sm font-medium">
            🎒 Items
          </router-link>
          <router-link v-if="auth.isAdmin" to="/admin" class="bg-yellow-100 text-yellow-700 px-4 py-2 rounded-lg hover:bg-yellow-200 transition text-sm font-medium">
            ⚡ Admin Panel
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>
