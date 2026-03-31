<script setup lang="ts">
import { onMounted } from 'vue'
import { useAdminStore } from '../stores/admin'
import { ItemType } from '../types'

const adminStore = useAdminStore()

function itemTypeLabel(t: ItemType) {
  return ['Wand', 'Robe', 'Hat', 'Broom', 'Potion', 'Amulet'][t] ?? 'Unknown'
}

onMounted(async () => {
  await adminStore.fetchItems()
})
</script>

<template>
  <div data-testid="items-view">
    <h1 class="text-3xl font-bold text-indigo-900 mb-6">🎒 Items Catalog</h1>

    <div v-if="adminStore.loading" class="text-center py-12 text-gray-500">Loading…</div>

    <div v-else-if="adminStore.items.length" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <div
        v-for="item in adminStore.items"
        :key="item.id"
        class="bg-white rounded-xl shadow p-5"
      >
        <div class="flex items-center justify-between mb-2">
          <h3 class="font-semibold">{{ item.name }}</h3>
          <span class="text-xs bg-indigo-100 text-indigo-700 px-2 py-0.5 rounded">{{ itemTypeLabel(item.type) }}</span>
        </div>
        <p class="text-sm text-gray-500 mb-3">{{ item.description }}</p>
        <div class="flex justify-between text-xs">
          <div class="space-x-2">
            <span v-if="item.magicBonus">🔮+{{ item.magicBonus }}</span>
            <span v-if="item.strengthBonus">💪+{{ item.strengthBonus }}</span>
            <span v-if="item.wisdomBonus">📚+{{ item.wisdomBonus }}</span>
            <span v-if="item.speedBonus">⚡+{{ item.speedBonus }}</span>
          </div>
          <span class="font-semibold text-yellow-600">🪙 {{ item.price }}</span>
        </div>
      </div>
    </div>

    <div v-else class="text-center py-12 text-gray-400">No items available</div>
  </div>
</template>
