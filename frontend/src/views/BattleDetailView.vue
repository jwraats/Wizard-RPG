<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useBattleStore } from '../stores/battle'
import { useAuthStore } from '../stores/auth'
import { BattleStatus, SpellElement } from '../types'

const props = defineProps<{ id: string }>()
const battleStore = useBattleStore()
const auth = useAuthStore()

const battle = computed(() => battleStore.currentBattle)
const isMyTurn = computed(() => {
  if (!battle.value || battle.value.status !== BattleStatus.Active) return false
  const turns = battle.value.turns.length
  if (turns === 0) return battle.value.challengerId === auth.player?.id
  const lastTurn = battle.value.turns[battle.value.turns.length - 1]
  return lastTurn.attackerId !== auth.player?.id
})

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
  await battleStore.fetchBattle(props.id)
  if (!battleStore.spells.length) await battleStore.fetchSpells()
})

async function castSpell(spellId: string) {
  await battleStore.castSpell(props.id, spellId)
}

async function acceptBattle() {
  await battleStore.acceptBattle(props.id)
}
</script>

<template>
  <div data-testid="battle-detail-view">
    <router-link to="/battle" class="text-indigo-600 hover:underline text-sm mb-4 inline-block">← Back to Arena</router-link>

    <div v-if="battleStore.loading && !battle" class="text-center py-12 text-gray-500">Loading…</div>

    <div v-else-if="battle">
      <div class="flex items-center gap-4 mb-6">
        <h1 class="text-2xl font-bold text-indigo-900">
          {{ battle.challengerUsername }} vs {{ battle.defenderUsername }}
        </h1>
        <span
          :class="{
            'bg-amber-100 text-amber-700': battle.status === BattleStatus.Pending,
            'bg-green-100 text-green-700': battle.status === BattleStatus.Active,
            'bg-gray-100 text-gray-700': battle.status === BattleStatus.Finished,
          }"
          class="text-xs px-2 py-1 rounded-full font-medium"
        >
          {{ ['Pending', 'Active', 'Finished'][battle.status] }}
        </span>
      </div>

      <!-- Accept button for pending battles -->
      <div v-if="battle.status === BattleStatus.Pending && battle.defenderId === auth.player?.id" class="mb-6">
        <button
          data-testid="accept-battle-btn"
          class="bg-green-600 hover:bg-green-700 text-white px-6 py-2 rounded-lg transition"
          @click="acceptBattle"
        >
          Accept Challenge
        </button>
      </div>

      <!-- Cast Spell Panel -->
      <div v-if="battle.status === BattleStatus.Active && isMyTurn" class="bg-white rounded-xl shadow p-6 mb-6">
        <h2 class="text-lg font-semibold text-indigo-800 mb-3">🔮 Your Turn — Cast a Spell!</h2>
        <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
          <button
            v-for="spell in battleStore.spells"
            :key="spell.id"
            :data-testid="`cast-${spell.id}`"
            class="border-2 border-indigo-200 hover:border-indigo-500 rounded-lg p-3 text-sm text-left transition"
            :disabled="battleStore.loading"
            @click="castSpell(spell.id)"
          >
            <div class="font-semibold">{{ elementEmoji(spell.element) }} {{ spell.name }}</div>
            <div class="text-xs text-gray-500 mt-1">DMG: {{ spell.baseDamage }} | Mana: {{ spell.manaCost }}</div>
          </button>
        </div>
      </div>

      <div v-else-if="battle.status === BattleStatus.Active && !isMyTurn" class="bg-amber-50 rounded-xl p-6 mb-6 text-center">
        <p class="text-amber-700 font-medium">⏳ Waiting for your opponent's turn…</p>
      </div>

      <!-- Winner -->
      <div v-if="battle.status === BattleStatus.Finished" class="bg-indigo-50 rounded-xl p-6 mb-6 text-center">
        <p class="text-2xl font-bold text-indigo-800">🏆 {{ battle.winnerUsername }} wins!</p>
        <p v-if="battle.narratorStory" class="mt-3 text-gray-600 italic text-sm max-w-2xl mx-auto">
          {{ battle.narratorStory }}
        </p>
      </div>

      <!-- Battle Log -->
      <div class="bg-white rounded-xl shadow p-6">
        <h2 class="text-lg font-semibold text-indigo-800 mb-4">📜 Battle Log</h2>
        <div v-if="battle.turns.length" class="space-y-3">
          <div
            v-for="turn in battle.turns"
            :key="turn.turnNumber"
            class="border-l-4 pl-4 py-2"
            :class="turn.attackerId === battle.challengerId ? 'border-blue-400' : 'border-red-400'"
          >
            <div class="text-sm">
              <span class="font-semibold">Turn {{ turn.turnNumber }}:</span>
              {{ elementEmoji(turn.element) }} {{ turn.attackerUsername }} casts {{ turn.spellName }}
              for <span class="font-bold text-red-600">{{ turn.damageDealt }}</span> damage
            </div>
            <div v-if="turn.narrative" class="text-xs text-gray-500 italic mt-1">{{ turn.narrative }}</div>
          </div>
        </div>
        <div v-else class="text-gray-400 text-sm">No turns yet</div>
      </div>
    </div>
  </div>
</template>
