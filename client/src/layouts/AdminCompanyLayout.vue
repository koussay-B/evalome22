<template>
  <SidebarProvider>
    <AppSidebar
      :nav-main="navMain"
      :nav-secondary="navSecondary"
      :platform-label="t('companyPanel.layout.panel')"
      :brand-subtitle="companyName"
      brand-url="/company"
      :side="sidebarSide"
      @logout="handleLogout"
    />

    <SidebarInset class="bg-background min-h-screen flex flex-col">
      <SiteHeader :breadcrumbs="breadcrumbs" class="border-b border-border/60 bg-card/80 backdrop-blur-md sticky top-0 z-10 shadow-[0_1px_0_0_oklch(0.88_0.025_50/0.6)]">
        <template #actions>
          <LanguageSwitcher />
          <ThemeToggle />
        </template>
      </SiteHeader>

      <main class="flex-1 p-6 lg:p-8 overflow-y-auto overflow-x-auto">
        <router-view />
      </main>
    </SidebarInset>
  </SidebarProvider>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/store/authStore'
import { SidebarProvider, SidebarInset } from '@/components/ui/sidebar'
import ThemeToggle from '@/components/common/ThemeToggle.vue'
import LanguageSwitcher from '@/components/common/LanguageSwitcher.vue'
import AppSidebar from '@/components/sidebar/AppSidebar.vue'
import SiteHeader from '@/components/sidebar/SiteHeader.vue'
import { useLocale } from '@/composables/useLocale'
import { LayoutDashboard, Users, BarChart3, ClipboardList, Settings, LifeBuoy, Send, Layers, FileText, FileBarChart } from 'lucide-vue-next'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()
const { t, locale } = useLocale()

const companyName = computed(() => authStore.state.company?.name ?? 'Company')

const sidebarSide = computed(() => locale.value === 'ar' ? 'right' : 'left')

const navMain = computed(() => [
  { title: t('companyPanel.layout.navMain.dashboard'),  url: '/company',            icon: LayoutDashboard },
  { title: t('companyPanel.layout.navMain.employees'),  url: '/company/employees',  icon: Users },
  { title: t('companyPanel.layout.navMain.questionnaires'), url: '/company/questionnaires', icon: FileText },
  { title: t('companyPanel.layout.navMain.themes'),         url: '/company/themes',         icon: Layers },
  { title: t('companyPanel.layout.navMain.tests'),      url: '/company/tests',      icon: ClipboardList },
  { title: t('companyPanel.layout.navMain.reports'),    url: '/company/reports',    icon: FileBarChart },
  { title: t('companyPanel.layout.navMain.settings'),   url: '/company/settings',   icon: Settings },
])

const navSecondary = computed(() => [
  { title: t('layout.navSecondary.support'),  url: '/company/support',  icon: LifeBuoy },
  { title: t('layout.navSecondary.feedback'), url: '/company/feedback', icon: Send },
])

const breadcrumbs = computed(() => {
  const segments = route.path.split('/').filter(Boolean)
  return segments.map((seg, idx) => ({
    label: seg.charAt(0).toUpperCase() + seg.slice(1),
    url: '/' + segments.slice(0, idx + 1).join('/'),
  }))
})

const handleLogout = () => {
  authStore.logout()
  router.push({ name: 'Login' })
}
</script>