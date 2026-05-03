<template>
  <SidebarProvider>
    <AppSidebar
      :nav-main="navMain"
      :platform-label="'Formateur Panel'"
      :brand-subtitle="'Formateur Panel'"
      brand-url="/formateur"
      :side="sidebarSide"
      @logout="handleLogout"
    />

    <SidebarInset>
      <SiteHeader :breadcrumbs="breadcrumbs">
        <template #actions>
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
import { SidebarProvider, SidebarInset } from '@/components/ui/sidebar'
import ThemeToggle from '@/components/common/ThemeToggle.vue'
import LanguageSwitcher from '@/components/common/LanguageSwitcher.vue'
import AppSidebar from '@/components/sidebar/AppSidebar.vue'
import SiteHeader from '@/components/sidebar/SiteHeader.vue'
import { useLocale } from '@/composables/useLocale'
import { Settings, LayoutDashboard, Send, LifeBuoy, Layers, Users, ClipboardList, FileText, FileBarChart } from 'lucide-vue-next'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()
const { t, locale } = useLocale()

const sidebarSide = computed(() => locale.value === 'ar' ? 'right' : 'left')

const navMain = computed(() => [
  { title: 'Dashboard',                                  url: '/formateur',             icon: LayoutDashboard },
  { title: t('companyPanel.layout.navMain.questionnaires'), url: '/formateur/questionnaires', icon: FileText },
  { title: 'Tests & Campaigns',                          url: '/formateur/tests',       icon: ClipboardList },
  { title: t('companyPanel.layout.navMain.reports'),     url: '/formateur/reports',    icon: FileBarChart },
  { title: t('companyPanel.layout.navMain.employees'),   url: '/formateur/employees',   icon: Users },
  { title: t('companyPanel.layout.navMain.settings'),    url: '/formateur/settings',    icon: Settings },
])

//const navSecondary = computed(() => [
 // { title: t('layout.navSecondary.support'),  url: '/formateur/support',  icon: LifeBuoy },
  //{ title: t('layout.navSecondary.feedback'), url: '/formateur/feedback', icon: Send },
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
