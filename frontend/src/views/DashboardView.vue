<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { usePlayerStore } from '../stores/player'
import { useBankStore } from '../stores/bank'
import { useAuthStore } from '../stores/auth'
import { useQuestStore } from '../stores/quest'
import { loginRewardApi } from '../api/loginReward'
import type { LoginRewardStatus, LoginRewardResponse } from '../types'
import { QuestStatus } from '../types'

const auth = useAuthStore()
const playerStore = usePlayerStore()
const bankStore = useBankStore()
const questStore = useQuestStore()

const rewardStatus = ref<LoginRewardStatus | null>(null)
const lastReward = ref<LoginRewardResponse | null>(null)
const guideDismissed = ref(false)

function houseIcon(house: string) {
  const icons: Record<string, string> = { Pyromancers: '🔥', Frostwardens: '❄️', Stormcallers: '⚡', Earthshapers: '🌿' }
  return icons[house] ?? '🏠'
}

onMounted(async () => {
  await Promise.all([
    playerStore.fetchProfile(),
    bankStore.fetchAccount(),
    questStore.fetchQuests(),
  ])
  try {
    const res = await loginRewardApi.getStatus()
    rewardStatus.value = res.data
  } catch { /* ignore */ }
})

async function claimReward() {
  try {
    const res = await loginRewardApi.claimReward()
    lastReward.value = res.data
    if (rewardStatus.value) rewardStatus.value.canClaimToday = false
    await playerStore.fetchProfile()
  } catch { /* ignore */ }
}
</script>

<template>
  <div data-testid="dashboard-view">
    <h1 class="text-3xl font-bold text-indigo-900 mb-6">🏰 Dashboard</h1>

    <div v-if="playerStore.loading" class="text-center py-12 text-gray-500">Loading…</div>

    <div v-else-if="playerStore.profile">
      <!-- Getting Started Guide -->
      <div
        v-if="!playerStore.profile.hasCompletedOnboarding && !guideDismissed"
        class="bg-gradient-to-r from-indigo-50 to-purple-50 border border-indigo-200 rounded-xl p-6 mb-6"
      >
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-semibold text-indigo-800">🌟 Getting Started</h2>
          <button class="text-sm text-gray-500 hover:text-gray-700" @click="guideDismissed = true">Dismiss</button>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
          <div class="bg-white rounded-lg p-3 text-sm">
            <span class="text-lg">🏠</span>
            <p class="font-medium mt-1">1. Choose Your House</p>
            <p class="text-gray-500 text-xs">Pick Pyromancers, Frostwardens, Stormcallers, or Earthshapers</p>
          </div>
          <div class="bg-white rounded-lg p-3 text-sm">
            <span class="text-lg">⚔️</span>
            <p class="font-medium mt-1">2. Fight Your First Battle</p>
            <p class="text-gray-500 text-xs">Challenge another wizard in the Battle Arena</p>
          </div>
          <div class="bg-white rounded-lg p-3 text-sm">
            <span class="text-lg">🤝</span>
            <p class="font-medium mt-1">3. Join a Fellowship</p>
            <p class="text-gray-500 text-xs">Team up with other wizards for shared benefits</p>
          </div>
          <div class="bg-white rounded-lg p-3 text-sm">
            <span class="text-lg">🏦</span>
            <p class="font-medium mt-1">4. Visit the Bank</p>
            <p class="text-gray-500 text-xs">Store your gold and acquire powerful items</p>
          </div>
        </div>
      </div>

      <!-- Login Reward -->
      <div v-if="rewardStatus?.canClaimToday" class="bg-yellow-50 border border-yellow-200 rounded-xl p-4 mb-6 flex items-center justify-between">
        <div>
          <span class="text-lg">🎁</span>
          <span class="font-semibold text-yellow-800 ml-2">Daily Login Reward Available!</span>
          <span class="text-sm text-yellow-600 ml-2">Streak: {{ rewardStatus.loginStreak }} days</span>
        </div>
        <button class="bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded-lg text-sm font-medium transition" @click="claimReward">
          Claim Reward
        </button>
      </div>
      <div v-if="lastReward" class="bg-green-50 border border-green-200 rounded-xl p-4 mb-6 text-sm text-green-800">
        🎉 Day {{ lastReward.day }} reward claimed! +{{ lastReward.goldReward }} gold{{ lastReward.itemReward ? ` + ${lastReward.itemReward}` : '' }}
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
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
            <div v-if="playerStore.profile.house" class="flex justify-between">
              <span class="text-gray-500">House</span>
              <span class="font-semibold">{{ houseIcon(playerStore.profile.house) }} {{ playerStore.profile.house }}</span>
            </div>
            <div class="flex justify-between">
              <span class="text-gray-500">Rank</span>
              <span class="font-semibold">{{ playerStore.profile.rankBadge }} {{ playerStore.profile.rankTier }}</span>
            </div>
            <div class="flex justify-between">
              <span class="text-gray-500">ELO Rating</span>
              <span class="font-semibold">{{ playerStore.profile.eloRating }}</span>
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

        <!-- Active Quests -->
        <div class="bg-white rounded-xl shadow p-6 md:col-span-2 lg:col-span-2">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-lg font-semibold text-indigo-800">📜 Active Quests</h2>
            <div class="flex gap-2">
              <button class="text-xs bg-indigo-100 text-indigo-700 px-3 py-1 rounded hover:bg-indigo-200 transition" @click="questStore.generateDaily()">
                + Daily
              </button>
              <button class="text-xs bg-purple-100 text-purple-700 px-3 py-1 rounded hover:bg-purple-200 transition" @click="questStore.generateWeekly()">
                + Weekly
              </button>
            </div>
          </div>
          <div v-if="questStore.quests.length" class="space-y-3">
            <div v-for="quest in questStore.quests" :key="quest.id" class="border rounded-lg p-3">
              <div class="flex items-center justify-between">
                <span class="font-medium text-sm">{{ quest.title }}</span>
                <span :class="quest.status === QuestStatus.Completed ? 'text-green-600' : 'text-gray-500'" class="text-xs">
                  {{ quest.status === QuestStatus.Completed ? '✅ Complete' : `${quest.currentCount}/${quest.targetCount}` }}
                </span>
              </div>
              <p class="text-xs text-gray-500 mt-1">{{ quest.description }}</p>
              <div class="w-full bg-gray-200 rounded-full h-1.5 mt-2">
                <div class="bg-indigo-500 h-1.5 rounded-full transition-all" :style="{ width: Math.min((quest.currentCount / quest.targetCount) * 100, 100) + '%' }"></div>
              </div>
              <div class="text-xs text-gray-400 mt-1">Reward: 🪙 {{ quest.goldReward }} · {{ quest.xpReward }} XP</div>
            </div>
          </div>
          <div v-else class="text-gray-400 text-sm">No active quests. Generate some!</div>
        </div>

        <!-- Login Streak -->
        <div class="bg-white rounded-xl shadow p-6">
          <h2 class="text-lg font-semibold text-indigo-800 mb-4">🔥 Login Streak</h2>
          <div class="text-center">
            <p class="text-4xl font-bold text-orange-500">{{ playerStore.profile.loginStreak }}</p>
            <p class="text-sm text-gray-500 mt-1">consecutive days</p>
          </div>
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
            <router-link to="/house" class="bg-teal-100 text-teal-700 px-4 py-2 rounded-lg hover:bg-teal-200 transition text-sm font-medium">
              🏠 Houses
            </router-link>
            <router-link v-if="auth.isAdmin" to="/admin" class="bg-yellow-100 text-yellow-700 px-4 py-2 rounded-lg hover:bg-yellow-200 transition text-sm font-medium">
              ⚡ Admin Panel
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
