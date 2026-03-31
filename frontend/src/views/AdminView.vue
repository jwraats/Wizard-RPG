<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useAdminStore } from '../stores/admin'
import { adminApi } from '../api/admin'
import { ItemType } from '../types'
import type { CreateLeagueRequest } from '../types'

const adminStore = useAdminStore()
const activeTab = ref<'players' | 'items' | 'leagues'>('players')

/* Gold editing */
const editGoldPlayerId = ref('')
const editGoldAmount = ref<number>(0)
const goldError = ref('')

/* New Item */
const newItemName = ref('')
const newItemDesc = ref('')
const newItemType = ref<ItemType>(ItemType.Wand)
const newItemPrice = ref<number>(0)
const newItemMagic = ref<number>(0)
const newItemStrength = ref<number>(0)
const newItemWisdom = ref<number>(0)
const newItemSpeed = ref<number>(0)
const itemError = ref('')

/* New League */
const newLeagueName = ref('')
const newLeagueStart = ref('')
const newLeagueEnd = ref('')
const newLeagueTeams = ref<Array<{ name: string; odds: number }>>([{ name: '', odds: 2.0 }, { name: '', odds: 2.0 }])
const leagueError = ref('')

function itemTypeLabel(t: ItemType) {
  return ['Wand', 'Robe', 'Hat', 'Broom', 'Potion', 'Amulet'][t] ?? 'Unknown'
}

onMounted(async () => {
  await Promise.all([adminStore.fetchPlayers(), adminStore.fetchItems()])
})

async function setGold() {
  goldError.value = ''
  if (!editGoldPlayerId.value) return
  try {
    await adminStore.setGold(editGoldPlayerId.value, editGoldAmount.value)
    editGoldPlayerId.value = ''
    editGoldAmount.value = 0
  } catch {
    goldError.value = adminStore.error
  }
}

async function toggleAdmin(playerId: string, currentAdmin: boolean) {
  try {
    await adminStore.setAdminStatus(playerId, !currentAdmin)
  } catch {
    /* handled by store */
  }
}

async function createItem() {
  itemError.value = ''
  try {
    await adminApi.createItem({
      name: newItemName.value,
      description: newItemDesc.value,
      type: newItemType.value,
      magicBonus: newItemMagic.value,
      strengthBonus: newItemStrength.value,
      wisdomBonus: newItemWisdom.value,
      speedBonus: newItemSpeed.value,
      price: newItemPrice.value,
    })
    await adminStore.fetchItems()
    newItemName.value = ''
    newItemDesc.value = ''
  } catch {
    itemError.value = 'Failed to create item'
  }
}

async function deleteItem(itemId: string) {
  try {
    await adminApi.deleteItem(itemId)
    await adminStore.fetchItems()
  } catch {
    /* handled */
  }
}

function addTeam() {
  newLeagueTeams.value.push({ name: '', odds: 2.0 })
}

async function createLeague() {
  leagueError.value = ''
  const req: CreateLeagueRequest = {
    name: newLeagueName.value,
    startTime: new Date(newLeagueStart.value).toISOString(),
    endTime: new Date(newLeagueEnd.value).toISOString(),
    teams: newLeagueTeams.value.filter((t) => t.name.trim()),
  }
  try {
    await adminApi.createLeague(req)
    newLeagueName.value = ''
    newLeagueStart.value = ''
    newLeagueEnd.value = ''
    newLeagueTeams.value = [{ name: '', odds: 2.0 }, { name: '', odds: 2.0 }]
  } catch {
    leagueError.value = 'Failed to create league'
  }
}
</script>

<template>
  <div data-testid="admin-view">
    <h1 class="text-3xl font-bold text-yellow-700 mb-6">⚡ Admin Panel</h1>

    <!-- Tabs -->
    <div class="flex gap-2 mb-6">
      <button
        v-for="tab in (['players', 'items', 'leagues'] as const)"
        :key="tab"
        :class="activeTab === tab ? 'bg-yellow-600 text-white' : 'bg-gray-200 text-gray-700'"
        class="px-4 py-2 rounded-lg text-sm font-medium transition capitalize"
        @click="activeTab = tab"
      >
        {{ tab }}
      </button>
    </div>

    <!-- Players Tab -->
    <div v-if="activeTab === 'players'">
      <!-- Set Gold -->
      <div class="bg-white rounded-xl shadow p-6 mb-6">
        <h2 class="text-lg font-semibold mb-3">Set Player Gold</h2>
        <div v-if="goldError" class="bg-red-50 text-red-700 rounded-lg p-2 mb-3 text-sm">{{ goldError }}</div>
        <form @submit.prevent="setGold" class="flex gap-2">
          <input
            v-model="editGoldPlayerId"
            type="text"
            placeholder="Player ID"
            data-testid="admin-gold-player-id"
            class="flex-1 border rounded-lg px-3 py-2 text-sm outline-none"
          />
          <input
            v-model.number="editGoldAmount"
            type="number"
            placeholder="Amount"
            data-testid="admin-gold-amount"
            class="w-32 border rounded-lg px-3 py-2 text-sm outline-none"
          />
          <button type="submit" class="bg-yellow-600 hover:bg-yellow-700 text-white px-4 py-2 rounded-lg text-sm transition">
            Set Gold
          </button>
        </form>
      </div>

      <!-- Players List -->
      <div class="bg-white rounded-xl shadow overflow-hidden">
        <table class="w-full text-sm">
          <thead class="bg-yellow-50">
            <tr>
              <th class="px-4 py-3 text-left">Username</th>
              <th class="px-4 py-3 text-left">Email</th>
              <th class="px-4 py-3 text-right">Level</th>
              <th class="px-4 py-3 text-right">Gold</th>
              <th class="px-4 py-3 text-center">Admin</th>
              <th class="px-4 py-3 text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in adminStore.players" :key="p.id" class="border-t hover:bg-gray-50">
              <td class="px-4 py-3 font-semibold">{{ p.username }}</td>
              <td class="px-4 py-3 text-gray-500">{{ p.email }}</td>
              <td class="px-4 py-3 text-right">{{ p.level }}</td>
              <td class="px-4 py-3 text-right text-yellow-600">🪙 {{ p.goldCoins }}</td>
              <td class="px-4 py-3 text-center">
                <span v-if="p.isAdmin" class="text-green-600">✓</span>
                <span v-else class="text-gray-300">✗</span>
              </td>
              <td class="px-4 py-3 text-right">
                <button
                  class="text-xs text-indigo-600 hover:underline"
                  @click="toggleAdmin(p.id, p.isAdmin)"
                >
                  {{ p.isAdmin ? 'Remove Admin' : 'Make Admin' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Items Tab -->
    <div v-if="activeTab === 'items'">
      <div class="bg-white rounded-xl shadow p-6 mb-6">
        <h2 class="text-lg font-semibold mb-3">Create Item</h2>
        <div v-if="itemError" class="bg-red-50 text-red-700 rounded-lg p-2 mb-3 text-sm">{{ itemError }}</div>
        <form @submit.prevent="createItem" class="grid grid-cols-2 gap-3">
          <input v-model="newItemName" type="text" placeholder="Name" class="border rounded-lg px-3 py-2 text-sm outline-none" />
          <input v-model="newItemDesc" type="text" placeholder="Description" class="border rounded-lg px-3 py-2 text-sm outline-none" />
          <select v-model.number="newItemType" class="border rounded-lg px-3 py-2 text-sm outline-none">
            <option :value="ItemType.Wand">Wand</option>
            <option :value="ItemType.Robe">Robe</option>
            <option :value="ItemType.Hat">Hat</option>
            <option :value="ItemType.Broom">Broom</option>
            <option :value="ItemType.Potion">Potion</option>
            <option :value="ItemType.Amulet">Amulet</option>
          </select>
          <input v-model.number="newItemPrice" type="number" placeholder="Price" class="border rounded-lg px-3 py-2 text-sm outline-none" />
          <input v-model.number="newItemMagic" type="number" placeholder="Magic Bonus" class="border rounded-lg px-3 py-2 text-sm outline-none" />
          <input v-model.number="newItemStrength" type="number" placeholder="Strength Bonus" class="border rounded-lg px-3 py-2 text-sm outline-none" />
          <input v-model.number="newItemWisdom" type="number" placeholder="Wisdom Bonus" class="border rounded-lg px-3 py-2 text-sm outline-none" />
          <input v-model.number="newItemSpeed" type="number" placeholder="Speed Bonus" class="border rounded-lg px-3 py-2 text-sm outline-none" />
          <button type="submit" class="col-span-2 bg-indigo-600 hover:bg-indigo-700 text-white py-2 rounded-lg text-sm transition">
            Create Item
          </button>
        </form>
      </div>

      <div class="bg-white rounded-xl shadow overflow-hidden">
        <table class="w-full text-sm">
          <thead class="bg-indigo-50">
            <tr>
              <th class="px-4 py-3 text-left">Name</th>
              <th class="px-4 py-3 text-left">Type</th>
              <th class="px-4 py-3 text-right">Price</th>
              <th class="px-4 py-3 text-right">Bonuses</th>
              <th class="px-4 py-3 text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in adminStore.items" :key="item.id" class="border-t hover:bg-gray-50">
              <td class="px-4 py-3 font-semibold">{{ item.name }}</td>
              <td class="px-4 py-3">{{ itemTypeLabel(item.type) }}</td>
              <td class="px-4 py-3 text-right text-yellow-600">🪙 {{ item.price }}</td>
              <td class="px-4 py-3 text-right text-xs">
                🔮{{ item.magicBonus }} 💪{{ item.strengthBonus }} 📚{{ item.wisdomBonus }} ⚡{{ item.speedBonus }}
              </td>
              <td class="px-4 py-3 text-right">
                <button class="text-xs text-red-600 hover:underline" @click="deleteItem(item.id)">Delete</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Leagues Tab -->
    <div v-if="activeTab === 'leagues'">
      <div class="bg-white rounded-xl shadow p-6">
        <h2 class="text-lg font-semibold mb-3">Create Broom League</h2>
        <div v-if="leagueError" class="bg-red-50 text-red-700 rounded-lg p-2 mb-3 text-sm">{{ leagueError }}</div>
        <form @submit.prevent="createLeague" class="space-y-3">
          <input v-model="newLeagueName" type="text" placeholder="League Name" class="w-full border rounded-lg px-3 py-2 text-sm outline-none" />
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs text-gray-500 mb-1">Start Time</label>
              <input v-model="newLeagueStart" type="datetime-local" class="w-full border rounded-lg px-3 py-2 text-sm outline-none" />
            </div>
            <div>
              <label class="block text-xs text-gray-500 mb-1">End Time</label>
              <input v-model="newLeagueEnd" type="datetime-local" class="w-full border rounded-lg px-3 py-2 text-sm outline-none" />
            </div>
          </div>
          <div>
            <div class="flex items-center justify-between mb-2">
              <span class="text-sm font-medium">Teams</span>
              <button type="button" class="text-xs text-indigo-600 hover:underline" @click="addTeam">+ Add Team</button>
            </div>
            <div v-for="(team, idx) in newLeagueTeams" :key="idx" class="flex gap-2 mb-2">
              <input v-model="team.name" type="text" :placeholder="`Team ${idx + 1} name`" class="flex-1 border rounded-lg px-3 py-2 text-sm outline-none" />
              <input v-model.number="team.odds" type="number" step="0.1" placeholder="Odds" class="w-24 border rounded-lg px-3 py-2 text-sm outline-none" />
            </div>
          </div>
          <button type="submit" class="bg-yellow-600 hover:bg-yellow-700 text-white px-6 py-2 rounded-lg text-sm transition">
            Create League
          </button>
        </form>
      </div>
    </div>
  </div>
</template>
