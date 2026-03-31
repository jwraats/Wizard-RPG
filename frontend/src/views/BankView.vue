<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useBankStore } from '../stores/bank'
import { ItemType } from '../types'

const bankStore = useBankStore()
const depositAmount = ref<number>(0)
const withdrawAmount = ref<number>(0)
const actionError = ref('')

function itemTypeLabel(t: ItemType) {
  return ['Wand', 'Robe', 'Hat', 'Broom', 'Potion', 'Amulet'][t] ?? 'Unknown'
}

onMounted(async () => {
  await Promise.all([bankStore.fetchAccount(), bankStore.fetchItems()])
})

async function doDeposit() {
  actionError.value = ''
  if (depositAmount.value <= 0) return
  try {
    await bankStore.deposit(depositAmount.value)
    depositAmount.value = 0
  } catch {
    actionError.value = bankStore.error
  }
}

async function doWithdraw() {
  actionError.value = ''
  if (withdrawAmount.value <= 0) return
  try {
    await bankStore.withdraw(withdrawAmount.value)
    withdrawAmount.value = 0
  } catch {
    actionError.value = bankStore.error
  }
}

async function retrieveItem(bankItemId: string) {
  try {
    await bankStore.retrieveItem(bankItemId)
  } catch {
    actionError.value = bankStore.error
  }
}
</script>

<template>
  <div data-testid="bank-view">
    <h1 class="text-3xl font-bold text-indigo-900 mb-6">🏦 Gringotts Bank</h1>

    <div v-if="actionError" class="bg-red-50 text-red-700 rounded-lg p-3 mb-4 text-sm">{{ actionError }}</div>

    <div v-if="bankStore.loading && !bankStore.account" class="text-center py-12 text-gray-500">Loading…</div>

    <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Balance -->
      <div class="bg-white rounded-xl shadow p-6">
        <h2 class="text-lg font-semibold text-indigo-800 mb-4">💰 Balance</h2>
        <div v-if="bankStore.account" class="text-center">
          <p class="text-4xl font-bold text-yellow-600" data-testid="bank-balance">
            🪙 {{ bankStore.account.goldBalance }}
          </p>
          <p class="text-sm text-gray-500 mt-1">Gold coins in bank</p>
        </div>
      </div>

      <!-- Deposit / Withdraw -->
      <div class="bg-white rounded-xl shadow p-6">
        <h2 class="text-lg font-semibold text-indigo-800 mb-4">💳 Transactions</h2>
        <div class="space-y-4">
          <form @submit.prevent="doDeposit" class="flex gap-2">
            <input
              v-model.number="depositAmount"
              type="number"
              min="1"
              placeholder="Amount"
              data-testid="deposit-input"
              class="flex-1 border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-green-500 outline-none"
            />
            <button
              type="submit"
              :disabled="bankStore.loading"
              data-testid="deposit-btn"
              class="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-lg text-sm transition"
            >
              Deposit
            </button>
          </form>
          <form @submit.prevent="doWithdraw" class="flex gap-2">
            <input
              v-model.number="withdrawAmount"
              type="number"
              min="1"
              placeholder="Amount"
              data-testid="withdraw-input"
              class="flex-1 border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-red-500 outline-none"
            />
            <button
              type="submit"
              :disabled="bankStore.loading"
              data-testid="withdraw-btn"
              class="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-lg text-sm transition"
            >
              Withdraw
            </button>
          </form>
        </div>
      </div>

      <!-- Stored Items -->
      <div class="bg-white rounded-xl shadow p-6 lg:col-span-2">
        <h2 class="text-lg font-semibold text-indigo-800 mb-4">🎒 Stored Items</h2>
        <div v-if="bankStore.items.length" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
          <div
            v-for="item in bankStore.items"
            :key="item.id"
            class="border rounded-lg p-3"
          >
            <div class="flex items-center justify-between">
              <span class="font-semibold text-sm">{{ item.itemName }}</span>
              <span class="text-xs px-2 py-0.5 bg-gray-100 rounded">{{ itemTypeLabel(item.itemType) }}</span>
            </div>
            <p class="text-xs text-gray-500 mt-1">{{ item.itemDescription }}</p>
            <div class="flex justify-between items-center mt-2">
              <div class="text-xs text-gray-400">
                +{{ item.magicBonus }}🔮 +{{ item.strengthBonus }}💪 +{{ item.wisdomBonus }}📚 +{{ item.speedBonus }}⚡
              </div>
              <button
                class="text-xs text-red-600 hover:underline"
                @click="retrieveItem(item.id)"
              >
                Retrieve
              </button>
            </div>
          </div>
        </div>
        <div v-else class="text-gray-400 text-sm">No items stored in the bank</div>
      </div>
    </div>
  </div>
</template>
