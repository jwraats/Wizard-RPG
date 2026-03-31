<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useBattleStore } from '../stores/battle'
import { useAuthStore } from '../stores/auth'
import { BattleStatus, SpellElement } from '../types'

const battleStore = useBattleStore()
const auth = useAuthStore()
const router = useRouter()

const challengeId = ref('')
const showChallenge = ref(false)

const pendingBattles = computed(() =>
  battleStore.battles.filter((b) => b.status === BattleStatus.Pending),
)
const activeBattles = computed(() =>
  battleStore.battles.filter((b) => b.status === BattleStatus.Active),
)
const finishedBattles = computed(() =>
  battleStore.battles.filter((b) => b.status === BattleStatus.Finished),
)

function elementEmoji(el: SpellElement) {
  const map: Record<SpellElement, string> = {
    [SpellElement.Fire]: '🔥',
    [SpellElement.Ice]: '❄️',
    [SpellElement.Lightning]: '⚡',
    [SpellElement.Earth]: '🌍',
    [SpellElement.Arcane]: '✨',
  }
  return map[el] ?? '🔮'
}


onMounted(async () => {
  await Promise.all([battleStore.fetchMyBattles(), battleStore.fetchSpells()])
})

async function sendChallenge() {
  if (!challengeId.value) return
  try {
    const battle = await battleStore.challenge(challengeId.value)
    if (battle) router.push(`/battle/${battle.id}`)
  } catch {
    /* error shown in store */
  }
}

async function acceptBattle(battleId: string) {
  try {
    await battleStore.acceptBattle(battleId)
    router.push(`/battle/${battleId}`)
  } catch {
    /* error shown in store */
  }
}
</script>

<template>
  <div data-testid="battle-view">
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-3xl font-bold text-indigo-900">⚔️ Battle Arena</h1>
      <button
        data-testid="challenge-btn"
        class="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-lg transition text-sm font-medium"
        @click="showChallenge = !showChallenge"
      >
        {{ showChallenge ? 'Cancel' : '🗡️ Challenge a Wizard' }}
      </button>
    </div>

    <div v-if="battleStore.error" class="bg-red-50 text-red-700 rounded-lg p-3 mb-4 text-sm">
      {{ battleStore.error }}
    </div>

    <!-- Challenge Form -->
    <div v-if="showChallenge" class="bg-white rounded-xl shadow p-6 mb-6">
      <h2 class="text-lg font-semibold mb-3">Challenge a Wizard</h2>
      <form @submit.prevent="sendChallenge" class="flex gap-3">
        <input
          v-model="challengeId"
          type="text"
          placeholder="Enter opponent's Player ID"
          data-testid="challenge-id-input"
          class="flex-1 border rounded-lg px-3 py-2 focus:ring-2 focus:ring-red-500 outline-none text-sm"
        />
        <button
          type="submit"
          :disabled="battleStore.loading"
          class="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-lg transition text-sm"
        >
          Send Challenge
        </button>
      </form>
    </div>

    <!-- Spells -->
    <div class="bg-white rounded-xl shadow p-6 mb-6">
      <h2 class="text-lg font-semibold text-indigo-800 mb-4">📜 Available Spells</h2>
      <div v-if="battleStore.spells.length" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
        <div
          v-for="spell in battleStore.spells"
          :key="spell.id"
          class="border rounded-lg p-3 text-sm"
          :data-testid="`spell-${spell.id}`"
        >
          <div class="font-semibold">{{ elementEmoji(spell.element) }} {{ spell.name }}</div>
          <div class="text-gray-500 text-xs mt-1">{{ spell.description }}</div>
          <div class="flex justify-between mt-2 text-xs">
            <span>Damage: {{ spell.baseDamage }}</span>
            <span>Mana: {{ spell.manaCost }}</span>
          </div>
        </div>
      </div>
      <div v-else class="text-gray-400 text-sm">No spells loaded</div>
    </div>

    <div v-if="battleStore.loading" class="text-center py-8 text-gray-500">Loading battles…</div>

    <!-- Pending Battles -->
    <div v-if="pendingBattles.length" class="mb-6">
      <h2 class="text-lg font-semibold text-amber-700 mb-3">⏳ Pending Challenges</h2>
      <div class="space-y-2">
        <div
          v-for="b in pendingBattles"
          :key="b.id"
          class="bg-amber-50 border border-amber-200 rounded-lg p-4 flex items-center justify-between"
        >
          <div class="text-sm">
            <span class="font-semibold">{{ b.challengerUsername }}</span>
            vs
            <span class="font-semibold">{{ b.defenderUsername }}</span>
          </div>
          <div class="flex gap-2">
            <button
              v-if="b.defenderId === auth.player?.id"
              class="bg-green-600 hover:bg-green-700 text-white px-3 py-1 rounded text-sm transition"
              @click="acceptBattle(b.id)"
            >
              Accept
            </button>
            <router-link
              :to="`/battle/${b.id}`"
              class="bg-gray-200 hover:bg-gray-300 text-gray-700 px-3 py-1 rounded text-sm transition"
            >
              View
            </router-link>
          </div>
        </div>
      </div>
    </div>

    <!-- Active Battles -->
    <div v-if="activeBattles.length" class="mb-6">
      <h2 class="text-lg font-semibold text-green-700 mb-3">⚡ Active Battles</h2>
      <div class="space-y-2">
        <router-link
          v-for="b in activeBattles"
          :key="b.id"
          :to="`/battle/${b.id}`"
          class="block bg-green-50 border border-green-200 rounded-lg p-4 hover:bg-green-100 transition"
        >
          <div class="text-sm">
            <span class="font-semibold">{{ b.challengerUsername }}</span>
            vs
            <span class="font-semibold">{{ b.defenderUsername }}</span>
            — {{ b.turns.length }} turns played
          </div>
        </router-link>
      </div>
    </div>

    <!-- Finished Battles -->
    <div v-if="finishedBattles.length">
      <h2 class="text-lg font-semibold text-gray-700 mb-3">🏆 Finished Battles</h2>
      <div class="space-y-2">
        <router-link
          v-for="b in finishedBattles"
          :key="b.id"
          :to="`/battle/${b.id}`"
          class="block bg-gray-50 border rounded-lg p-4 hover:bg-gray-100 transition"
        >
          <div class="flex justify-between text-sm">
            <span>
              {{ b.challengerUsername }} vs {{ b.defenderUsername }}
            </span>
            <span class="font-semibold text-indigo-600">
              Winner: {{ b.winnerUsername ?? 'N/A' }}
            </span>
          </div>
        </router-link>
      </div>
    </div>

    <div v-if="!battleStore.loading && !battleStore.battles.length" class="text-center py-12 text-gray-400">
      No battles yet. Challenge a wizard to get started!
    </div>
  </div>
</template>
