<script setup lang="ts">
import { onMounted } from 'vue'
import { usePlayerStore } from '../stores/player'

const playerStore = usePlayerStore()

onMounted(async () => {
  await playerStore.fetchLeaderboard(20)
})
</script>

<template>
  <div data-testid="leaderboard-view">
    <h1 class="text-3xl font-bold text-indigo-900 mb-6">🏆 Leaderboard</h1>

    <div v-if="playerStore.loading" class="text-center py-12 text-gray-500">Loading…</div>

    <div v-else-if="playerStore.leaderboard.length" class="bg-white rounded-xl shadow overflow-hidden">
      <table class="w-full text-sm">
        <thead class="bg-indigo-50">
          <tr>
            <th class="px-4 py-3 text-left font-semibold text-indigo-800">#</th>
            <th class="px-4 py-3 text-left font-semibold text-indigo-800">Wizard</th>
            <th class="px-4 py-3 text-right font-semibold text-indigo-800">Level</th>
            <th class="px-4 py-3 text-right font-semibold text-indigo-800">XP</th>
            <th class="px-4 py-3 text-right font-semibold text-indigo-800">Gold</th>
            <th class="px-4 py-3 text-right font-semibold text-indigo-800">Magic</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="(p, idx) in playerStore.leaderboard"
            :key="p.id"
            class="border-t hover:bg-gray-50 transition"
          >
            <td class="px-4 py-3 font-bold">
              <span v-if="idx === 0">🥇</span>
              <span v-else-if="idx === 1">🥈</span>
              <span v-else-if="idx === 2">🥉</span>
              <span v-else>{{ idx + 1 }}</span>
            </td>
            <td class="px-4 py-3 font-semibold">{{ p.username }}</td>
            <td class="px-4 py-3 text-right">{{ p.level }}</td>
            <td class="px-4 py-3 text-right">{{ p.experience }}</td>
            <td class="px-4 py-3 text-right text-yellow-600">🪙 {{ p.goldCoins }}</td>
            <td class="px-4 py-3 text-right text-purple-600">{{ p.magicPower }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-else class="text-center py-12 text-gray-400">No players on the leaderboard yet</div>
  </div>
</template>
