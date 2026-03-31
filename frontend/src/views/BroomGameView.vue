<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useBroomStore } from '../stores/broom'
import { LeagueStatus, BetStatus } from '../types'

const broomStore = useBroomStore()
const betLeagueId = ref('')
const betTeamId = ref('')
const betAmount = ref<number>(0)
const showBetForm = ref(false)
const betError = ref('')
const activeTab = ref<'leagues' | 'bets'>('leagues')

function statusLabel(s: LeagueStatus) {
  return ['Upcoming', 'Running', 'Finished'][s] ?? 'Unknown'
}

function betStatusLabel(s: BetStatus) {
  return ['Pending', 'Won', 'Lost'][s] ?? 'Unknown'
}

function statusColor(s: LeagueStatus) {
  const map: Record<LeagueStatus, string> = {
    [LeagueStatus.Upcoming]: 'bg-blue-100 text-blue-700',
    [LeagueStatus.Running]: 'bg-green-100 text-green-700',
    [LeagueStatus.Finished]: 'bg-gray-100 text-gray-700',
  }
  return map[s] ?? ''
}

onMounted(async () => {
  await Promise.all([broomStore.fetchLeagues(), broomStore.fetchMyBets()])
})

function startBet(leagueId: string) {
  betLeagueId.value = leagueId
  betTeamId.value = ''
  betAmount.value = 0
  showBetForm.value = true
}

async function placeBet() {
  betError.value = ''
  if (!betTeamId.value || betAmount.value <= 0) {
    betError.value = 'Select a team and enter a valid amount'
    return
  }
  try {
    await broomStore.placeBet(betLeagueId.value, betTeamId.value, betAmount.value)
    showBetForm.value = false
  } catch {
    betError.value = broomStore.error
  }
}
</script>

<template>
  <div data-testid="broom-view">
    <h1 class="text-3xl font-bold text-indigo-900 mb-6">🧹 Broom Racing</h1>

    <!-- Tab Toggle -->
    <div class="flex gap-2 mb-6">
      <button
        :class="activeTab === 'leagues' ? 'bg-indigo-600 text-white' : 'bg-gray-200 text-gray-700'"
        class="px-4 py-2 rounded-lg text-sm font-medium transition"
        @click="activeTab = 'leagues'"
      >
        Leagues
      </button>
      <button
        :class="activeTab === 'bets' ? 'bg-indigo-600 text-white' : 'bg-gray-200 text-gray-700'"
        class="px-4 py-2 rounded-lg text-sm font-medium transition"
        @click="activeTab = 'bets'"
      >
        My Bets
      </button>
    </div>

    <!-- Bet Form Modal -->
    <div v-if="showBetForm" class="bg-white rounded-xl shadow p-6 mb-6">
      <h2 class="text-lg font-semibold mb-3">Place a Bet</h2>
      <div v-if="betError" class="bg-red-50 text-red-700 rounded-lg p-3 mb-3 text-sm">{{ betError }}</div>
      <div class="space-y-3">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Select Team</label>
          <select
            v-model="betTeamId"
            class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-500 outline-none"
            data-testid="bet-team-select"
          >
            <option value="">Choose a team…</option>
            <option
              v-for="team in broomStore.leagues.find((l) => l.id === betLeagueId)?.teams ?? []"
              :key="team.id"
              :value="team.id"
            >
              {{ team.name }} (odds: {{ team.odds }}x)
            </option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Amount</label>
          <input
            v-model.number="betAmount"
            type="number"
            min="1"
            data-testid="bet-amount-input"
            class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-500 outline-none"
          />
        </div>
        <div class="flex gap-2">
          <button
            data-testid="place-bet-btn"
            class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg text-sm transition"
            @click="placeBet"
          >
            Place Bet
          </button>
          <button
            class="bg-gray-200 hover:bg-gray-300 text-gray-700 px-4 py-2 rounded-lg text-sm transition"
            @click="showBetForm = false"
          >
            Cancel
          </button>
        </div>
      </div>
    </div>

    <!-- Leagues Tab -->
    <div v-if="activeTab === 'leagues'">
      <div v-if="broomStore.loading" class="text-center py-8 text-gray-500">Loading leagues…</div>
      <div v-else-if="broomStore.leagues.length" class="space-y-4">
        <div
          v-for="league in broomStore.leagues"
          :key="league.id"
          class="bg-white rounded-xl shadow p-5"
        >
          <div class="flex items-center justify-between mb-3">
            <h3 class="text-lg font-semibold">{{ league.name }}</h3>
            <span :class="statusColor(league.status)" class="text-xs px-2 py-1 rounded-full font-medium">
              {{ statusLabel(league.status) }}
            </span>
          </div>
          <div class="text-sm text-gray-500 mb-3">
            {{ new Date(league.startTime).toLocaleString() }} —
            {{ new Date(league.endTime).toLocaleString() }}
          </div>
          <div class="grid grid-cols-2 sm:grid-cols-3 gap-2 mb-3">
            <div
              v-for="team in league.teams"
              :key="team.id"
              class="border rounded-lg p-2 text-sm text-center"
              :class="league.winnerTeamId === team.id ? 'border-yellow-500 bg-yellow-50' : ''"
            >
              <div class="font-semibold">{{ team.name }}</div>
              <div class="text-xs text-gray-500">{{ team.odds }}x</div>
              <span v-if="league.winnerTeamId === team.id" class="text-xs text-yellow-600">🏆 Winner</span>
            </div>
          </div>
          <button
            v-if="league.status !== LeagueStatus.Finished"
            class="bg-amber-600 hover:bg-amber-700 text-white px-3 py-1.5 rounded-lg text-sm transition"
            @click="startBet(league.id)"
          >
            Place Bet
          </button>
        </div>
      </div>
      <div v-else class="text-center py-12 text-gray-400">No leagues available</div>
    </div>

    <!-- Bets Tab -->
    <div v-if="activeTab === 'bets'">
      <div v-if="broomStore.myBets.length" class="space-y-3">
        <div
          v-for="bet in broomStore.myBets"
          :key="bet.id"
          class="bg-white rounded-xl shadow p-4 flex items-center justify-between"
        >
          <div class="text-sm">
            <span class="font-semibold">{{ bet.leagueName }}</span> — {{ bet.teamName }}
            <div class="text-xs text-gray-500 mt-1">
              Bet: 🪙{{ bet.amount }} | Payout: 🪙{{ bet.payout }}
            </div>
          </div>
          <span
            :class="{
              'bg-amber-100 text-amber-700': bet.status === BetStatus.Pending,
              'bg-green-100 text-green-700': bet.status === BetStatus.Won,
              'bg-red-100 text-red-700': bet.status === BetStatus.Lost,
            }"
            class="text-xs px-2 py-1 rounded-full font-medium"
          >
            {{ betStatusLabel(bet.status) }}
          </span>
        </div>
      </div>
      <div v-else class="text-center py-12 text-gray-400">No bets placed yet</div>
    </div>
  </div>
</template>
