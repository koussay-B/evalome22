<template>
  <SidebarProvider v-model:open="uiStore.sidebarOpen">
    <AppSidebar
      :nav-main="navMain"
      :nav-secondary="navSecondary"
      :platform-label="t('layout.candidatPanel')"
      :brand-subtitle="t('layout.candidatPanel')"
      brand-url="/candidat"
      :side="sidebarSide"
      @logout="handleLogout"
    />

    <SidebarInset>
      <SiteHeader :breadcrumbs="breadcrumbs">
        <template #actions>
          <SidebarTrigger class="text-muted-foreground" />
          <LanguageSwitcher />
          <ThemeToggle />
        </template>
      </SiteHeader>

      <main class="flex-1 p-6 overflow-y-auto overflow-x-auto">
        <router-view />
      </main>
    </SidebarInset>
  </SidebarProvider>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/store/authStore'
import { useUiStore } from '@/store/uiStore'
import { SidebarProvider, SidebarInset, SidebarTrigger } from '@/components/ui/sidebar'
import ThemeToggle from '@/components/common/ThemeToggle.vue'
import LanguageSwitcher from '@/components/common/LanguageSwitcher.vue'
import AppSidebar from '@/components/sidebar/AppSidebar.vue'
import SiteHeader from '@/components/sidebar/SiteHeader.vue'
import { useLocale } from '@/composables/useLocale'
import { Settings, LayoutDashboard, Send, LifeBuoy, ClipboardList, Award } from 'lucide-vue-next'

const authStore = useAuthStore()
const uiStore   = useUiStore()
const router = useRouter()
const route = useRoute()
const { t, locale } = useLocale()
const sidebarSide = computed(() => locale.value === 'ar' ? 'right' : 'left')

const navMain = computed(() => [
  { title: t('layout.navMain.dashboard'),               url: '/candidat/dashboard',  icon: LayoutDashboard },
  { title: t('candidatTests.title'),                    url: '/candidat/tests',      icon: ClipboardList },
  { title: t('certificates.title'),                     url: '/candidat/certificates', icon: Award },
  { title: t('companyPanel.layout.navMain.settings'),   url: '/candidat',            icon: Settings },
])

//const navSecondary = computed(() => [
  //{ title: t('layout.navSecondary.support'),  url: '/candidat/support',  icon: LifeBuoy },
  //{ title: t('layout.navSecondary.feedback'), url: '/candidat/feedback', icon: Send },
//])

const breadcrumbs = computed(() => {
  const segments = route.path.split('/').filter(Boolean)
  return segments.map((seg, idx) => ({
    label: decodeURIComponent(seg.charAt(0).toUpperCase() + seg.slice(1)),
    url: '/' + segments.slice(0, idx + 1).join('/'),
  }))
})

const handleLogout = () => {
  authStore.logout()
  router.push({ name: 'Login' })
}
</script>
