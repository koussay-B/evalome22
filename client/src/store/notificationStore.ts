import { defineStore } from 'pinia'
import { reactive, computed } from 'vue'
import * as signalR from '@microsoft/signalr'
import type { Notification } from '@utils/models/notification.model'
import { getNotificationsApi, markNotificationReadApi, markAllNotificationsReadApi } from '@utils/api/notification.api'
import { useAuthStore } from './authStore'
import router from '@/router'

export const useNotificationStore = defineStore('notification', () => {
  const state = reactive({
    notifications: [] as Notification[],
    onlineUsers: [] as number[],
    loading: false,
    connection: null as signalR.HubConnection | null,
    permissionGranted: false,
  })

  const unreadCount = computed(() =>
    state.notifications.filter(n => !n.received).length
  )

  async function requestNotificationPermission() {
    if (!('Notification' in window)) return
    if (Notification.permission === 'granted') {
      state.permissionGranted = true
      return
    }
    if (Notification.permission !== 'denied') {
      const result = await Notification.requestPermission()
      state.permissionGranted = result === 'granted'
    }
  }

  function showBrowserNotification(notification: Notification) {
    if (!state.permissionGranted || !('Notification' in window)) return
    try {
      const n = new globalThis.Notification(notification.title, {
        body: notification.body,
        icon: '/favicon.ico',
      })
      if (notification.actionUrl) {
        n.onclick = () => {
          window.focus()
          router.push(notification.actionUrl!)
        }
      }
    } catch {
      // silently ignore
    }
  }

  async function fetchNotifications() {
    state.loading = true
    const { data } = await getNotificationsApi()
    if (data) state.notifications = data
    state.loading = false
  }

  async function markRead(id: number) {
    await markNotificationReadApi(id)
    const n = state.notifications.find(n => n.id === id)
    if (n) n.received = true
  }

  async function markAllRead() {
    await markAllNotificationsReadApi()
    state.notifications.forEach(n => (n.received = true))
  }

  async function startSignalR() {
    if (state.connection) return // already connected

    const authStore = useAuthStore()
    const token = authStore.state.token
    if (!token) return

    const baseUrl = (import.meta.env.VITE_API_URL as string)?.replace('/api', '') ?? ''

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/presence`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    connection.on('ReceiveNotification', (notification: Notification) => {
      state.notifications.unshift(notification)
      showBrowserNotification(notification)
    })
    
    connection.on('GetOnlineUsers', (userIds: number[]) => {
      state.onlineUsers = userIds
    })

    try {
      await connection.start()
      state.connection = connection
      await requestNotificationPermission()
    } catch (err) {
      console.error('[SignalR] Connection error:', err)
    }
  }

  async function stopSignalR() {
    if (state.connection) {
      await state.connection.stop()
      state.connection = null
    }
  }

  return {
    state,
    unreadCount,
    fetchNotifications,
    markRead,
    markAllRead,
    startSignalR,
    stopSignalR,
    requestNotificationPermission,
  }
})
