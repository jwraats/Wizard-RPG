import { defineStore } from 'pinia'
import { ref } from 'vue'
import { notificationApi } from '../api/notification'
import type { NotificationResponse } from '../types'

export const useNotificationStore = defineStore('notification', () => {
  const notifications = ref<NotificationResponse[]>([])
  const unreadCount = ref(0)
  const loading = ref(false)
  const error = ref('')

  async function fetchNotifications() {
    loading.value = true
    error.value = ''
    try {
      const res = await notificationApi.getNotifications()
      notifications.value = res.data
    } catch {
      error.value = 'Failed to load notifications'
    } finally {
      loading.value = false
    }
  }

  async function fetchUnreadCount() {
    try {
      const res = await notificationApi.getUnreadCount()
      unreadCount.value = res.data
    } catch {
      /* silent */
    }
  }

  async function markAsRead(id: string) {
    await notificationApi.markAsRead(id)
    const n = notifications.value.find(n => n.id === id)
    if (n) {
      n.isRead = true
      unreadCount.value = Math.max(0, unreadCount.value - 1)
    }
  }

  async function markAllAsRead() {
    await notificationApi.markAllAsRead()
    notifications.value.forEach(n => n.isRead = true)
    unreadCount.value = 0
  }

  return { notifications, unreadCount, loading, error, fetchNotifications, fetchUnreadCount, markAsRead, markAllAsRead }
})
