import { defineStore } from 'pinia'
import { ref } from 'vue'
import { bankApi } from '../api/bank'
import type { BankAccountResponse, BankItemResponse } from '../types'

export const useBankStore = defineStore('bank', () => {
  const account = ref<BankAccountResponse | null>(null)
  const items = ref<BankItemResponse[]>([])
  const loading = ref(false)
  const error = ref('')

  async function fetchAccount() {
    loading.value = true
    error.value = ''
    try {
      const res = await bankApi.getAccount()
      account.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load bank account'
    } finally {
      loading.value = false
    }
  }

  async function deposit(amount: number) {
    loading.value = true
    error.value = ''
    try {
      const res = await bankApi.deposit(amount)
      account.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Deposit failed'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function withdraw(amount: number) {
    loading.value = true
    error.value = ''
    try {
      const res = await bankApi.withdraw(amount)
      account.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Withdrawal failed'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function fetchItems() {
    loading.value = true
    try {
      const res = await bankApi.getItems()
      items.value = res.data
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to load bank items'
    } finally {
      loading.value = false
    }
  }

  async function retrieveItem(bankItemId: string) {
    loading.value = true
    try {
      await bankApi.retrieveItem(bankItemId)
      items.value = items.value.filter((i) => i.id !== bankItemId)
    } catch (e: unknown) {
      error.value = e instanceof Error ? e.message : 'Failed to retrieve item'
      throw e
    } finally {
      loading.value = false
    }
  }

  return { account, items, loading, error, fetchAccount, deposit, withdraw, fetchItems, retrieveItem }
})
