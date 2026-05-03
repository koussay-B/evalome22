<template>
  <header class="bg-gray-100 top-0 z-50 flex w-full items-center border-b">
    <div class="flex h-16 w-full items-center gap-2 px-4">
      <!-- Sidebar toggle button -->
      <Button class="size-8" variant="ghost" size="icon" @click="toggleSidebar">
        <PanelLeft class="size-4" />
        <span class="sr-only">{{ t('layout.header.toggleSidebar') }}</span>
      </Button>

      <Separator orientation="vertical" class="mr-2 h-4" />

      <!-- Breadcrumb (desktop only) -->
      <Breadcrumb class="hidden sm:block text-slate-900">
        <BreadcrumbList>
          <BreadcrumbItem v-for="(crumb, i) in breadcrumbs" :key="crumb.label">
            <template v-if="i < (breadcrumbs?.length ?? 0) - 1">
              <BreadcrumbLink as-child>
                <router-link :to="crumb.url ?? '#'">{{ crumb.label }}</router-link>
              </BreadcrumbLink>
              <BreadcrumbSeparator />
            </template>
            <BreadcrumbPage v-else>{{ crumb.label }}</BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

      <!-- Search -->
      <SearchForm class="w-full sm:ml-auto sm:w-auto invisible pointer-events-none" />

      <!-- Right-side actions: Notification | slot (Language + Theme) -->
      <div class="flex items-center gap-2 ml-2">
        <NotificationBell />
        <slot name="actions" />
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';
import { PanelLeft } from 'lucide-vue-next';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import {
  Breadcrumb, BreadcrumbItem, BreadcrumbLink,
  BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';
import { useSidebar } from '@/components/ui/sidebar';
import { useLocale } from '@/composables/useLocale';
import { useNotificationStore } from '@/store/notificationStore';
import SearchForm from './SearchForm.vue';
import NotificationBell from '@/components/common/NotificationBell.vue';

defineProps<{
  breadcrumbs?: { label: string; url?: string }[]
}>()

const { toggleSidebar } = useSidebar()
const { t } = useLocale()
const notificationStore = useNotificationStore()

onMounted(() => {
  notificationStore.startSignalR()
})

onUnmounted(() => {
  notificationStore.stopSignalR()
})
</script>
