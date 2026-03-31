import api from './client'
import type {
  BankAccountResponse,
  BankItemResponse,
} from '../types'

export const bankApi = {
  getAccount: () =>
    api.get<BankAccountResponse>('/bank'),

  deposit: (amount: number) =>
    api.post<BankAccountResponse>('/bank/deposit', { amount }),

  withdraw: (amount: number) =>
    api.post<BankAccountResponse>('/bank/withdraw', { amount }),

  getItems: () =>
    api.get<BankItemResponse[]>('/bank/items'),

  storeItem: (itemId: string) =>
    api.post<BankItemResponse>('/bank/items', { itemId }),

  retrieveItem: (bankItemId: string) =>
    api.delete(`/bank/items/${bankItemId}`),
}
