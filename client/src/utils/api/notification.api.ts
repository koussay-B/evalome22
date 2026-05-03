import http from '@utils/http'
import type { Notification } from '@utils/models/notification.model'

export async function getNotificationsApi() {
  try {
    const res = await http.get<Notification[]>('/notification')
    return { data: res.data, error: undefined }
  } catch (err: unknown) {
    const e = err as { response?: { data?: unknown } }
    const raw = e.response?.data
    const error =
      typeof raw === 'string'
        ? raw
        : (raw as { message?: string })?.message ?? 'Failed to fetch notifications'
    return { data: undefined, error }
  }
}

export async function markNotificationReadApi(id: number) {
  try {
    await http.patch(`/notification/${id}/read`)
    return { error: undefined }
  } catch (err: unknown) {
    const e = err as { response?: { data?: unknown } }
    const raw = e.response?.data
    const error =
      typeof raw === 'string'
        ? raw
        : (raw as { message?: string })?.message ?? 'Failed to mark notification as read'
    return { error }
  }
}

export async function markAllNotificationsReadApi() {
  try {
    await http.patch('/notification/read-all')
    return { error: undefined }
  } catch (err: unknown) {
    const e = err as { response?: { data?: unknown } }
    const raw = e.response?.data
    const error =
      typeof raw === 'string'
        ? raw
        : (raw as { message?: string })?.message ?? 'Failed to mark all as read'
    return { error }
  }
}
