import api from './client'
import type { NotificationResponse } from '../types'

export const notificationApi = {
  getNotifications: () =>
    api.get<NotificationResponse[]>('/notification'),

  getUnreadCount: () =>
    api.get<number>('/notification/unread-count'),

  markAsRead: (id: string) =>
    api.post(`/notification/${id}/read`),

  markAllAsRead: () =>
    api.post('/notification/read-all'),
}
