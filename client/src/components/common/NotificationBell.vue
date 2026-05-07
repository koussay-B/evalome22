<template>
  <DropdownMenu>
    <DropdownMenuTrigger as-child>
      <Button variant="ghost" size="icon" class="size-8 relative">
        <Bell class="size-4" />
        <span
          v-if="notificationStore.unreadCount > 0"
          class="absolute -top-0.5 -right-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-bold text-white leading-none"
        >
          {{ notificationStore.unreadCount > 99 ? '99+' : notificationStore.unreadCount }}
        </span>
      </Button>
    </DropdownMenuTrigger>

    <DropdownMenuContent align="end" class="w-80 p-0">
      <!-- Header -->
      <div class="flex items-center justify-between px-4 py-3 border-b">
        <span class="font-semibold text-sm">{{ t('notification.title') }}</span>
        <Button
          v-if="notificationStore.unreadCount > 0"
          variant="ghost"
          size="sm"
          class="h-7 text-xs text-muted-foreground px-2"
          @click.stop="notificationStore.markAllRead()"
        >
          {{ t('notification.markAllRead') }}
        </Button>
      </div>

      <!-- List -->
      <ScrollArea class="max-h-[360px]">
        <template v-if="notificationStore.state.notifications.length === 0">
          <div class="flex flex-col items-center gap-2 py-10 text-muted-foreground">
            <BellOff class="size-8 opacity-40" />
            <p class="text-sm">{{ t('notification.empty') }}</p>
          </div>
        </template>

        <template v-else>
          <div
            v-for="notification in notificationStore.state.notifications"
            :key="notification.id"
            class="flex items-start gap-3 px-4 py-3 cursor-pointer transition-colors hover:bg-muted/50 border-b last:border-0"
            :class="{ 'bg-muted/30': !notification.received }"
            @click="handleClick(notification)"
          >
            <!-- Type indicator dot -->
            <span
              class="mt-1 size-2 shrink-0 rounded-full"
              :class="typeColor(notification.type)"
            />

            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium leading-tight truncate">{{ notification.title }}</p>
              <p class="text-xs text-muted-foreground mt-0.5 line-clamp-2">{{ notification.body }}</p>
              <p class="text-[10px] text-muted-foreground mt-1 opacity-70">
                {{ formatDate(notification.date) }}
              </p>
            </div>

            <!-- Unread dot -->
            <span
              v-if="!notification.received"
              class="mt-1.5 size-1.5 shrink-0 rounded-full bg-primary"
            />
          </div>
        </template>
      </ScrollArea>
    </DropdownMenuContent>
  </DropdownMenu>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import { Bell, BellOff } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { ScrollArea } from '@/components/ui/scroll-area'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { useNotificationStore } from '@/store/notificationStore'
import { useLocale } from '@/composables/useLocale'
import type { Notification, NotificationType } from '@/utils/models/notification.model'

// T-sala7 mochekla el date:
import { formatDistanceToNow, parseISO } from 'date-fns'
import { enUS, fr } from 'date-fns/locale'

const notificationStore = useNotificationStore()
const { t, locale } = useLocale() // Zid el locale bch n-riguél el lougha
const router = useRouter()

onMounted(() => {
  notificationStore.fetchNotifications()
})

function handleClick(notification: Notification) {
  if (!notification.received) {
    notificationStore.markRead(notification.id)
  }
  if (notification.actionUrl) {
    router.push(notification.actionUrl)
  }
}

function typeColor(type: NotificationType): string {
  const map: Record<NotificationType, string> = {
    Info: 'bg-blue-500',
    Success: 'bg-green-500',
    Warning: 'bg-yellow-500',
    Danger: 'bg-red-500',
  }
  return map[type] ?? 'bg-muted-foreground'
}

// Function qui retourne date 
function formatDate(dateStr: string | Date): string {
  if (!dateStr) return ''
  
  try {
    let date = typeof dateStr === 'string' ? parseISO(dateStr) : dateStr
    
    const offset = new Date().getTimezoneOffset() * 60000;
    date = new Date(date.getTime() - offset); 

    if (isNaN(date.getTime())) return 'Date invalide'

    return formatDistanceToNow(date, { 
      addSuffix: true,
      locale: locale.value === 'fr' ? fr : enUS 
    })
  } catch (e) {
    console.error('Error formatting date:', e)
    return '...'
  }
}
</script>