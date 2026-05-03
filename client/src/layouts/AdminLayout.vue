<template>
  <SidebarProvider>
    <AppSidebar
      :nav-main="navMain"
      :nav-secondary="navSecondary"
      :platform-label="t('layout.platform')"
      :brand-subtitle="t('layout.adminPanel')"
      brand-url="/admin"
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
import { computed } from "vue";
import { useRouter, useRoute } from "vue-router";
import { useAuthStore } from "@/store/authStore";
import { SidebarProvider, SidebarInset } from "@/components/ui/sidebar";
import ThemeToggle from "@/components/common/ThemeToggle.vue";
import LanguageSwitcher from "@/components/common/LanguageSwitcher.vue";
import AppSidebar from "@/components/sidebar/AppSidebar.vue";
import SiteHeader from "@/components/sidebar/SiteHeader.vue";
import { useLocale } from "@/composables/useLocale";

import {
  LayoutDashboard,
  Users,
  BarChart3,
  Settings,
  LifeBuoy,
  Send,
  Building2,
  Database,
  BadgeCheck,
} from "lucide-vue-next";

const authStore = useAuthStore();
const router = useRouter();
const route = useRoute();
const { t, locale } = useLocale();

// Move sidebar to the right when Arabic (RTL) is active
const sidebarSide = computed(() => locale.value === "ar" ? "right" : "left")

const navMain = computed(() => [
  { title: t('layout.navMain.dashboard'), url: "/admin",           icon: LayoutDashboard },
  { title: t('layout.navMain.companies'), url: "/admin/companies", icon: Building2 },
  { title: t('layout.navMain.verification'), url: "/admin/company-verification", icon: BadgeCheck },
  { title: t('layout.navMain.users'),     url: "/admin/users",     icon: Users },
  { title: t('layout.navMain.settings'),  url: "/admin/settings",  icon: Settings },
]);

const navSecondary = computed(() => [
  { title: t('layout.navSecondary.support'),  url: "/admin/support",  icon: LifeBuoy },
  { title: t('layout.navSecondary.feedback'), url: "/admin/feedback", icon: Send },
]);

const breadcrumbs = computed(() => {
  const segments = route.path.split("/").filter(Boolean);
  return segments.map((seg, idx) => ({
    label: seg.charAt(0).toUpperCase() + seg.slice(1),
    url: "/" + segments.slice(0, idx + 1).join("/"),
  }));
});

const handleLogout = () => {
  authStore.logout();
  router.push({ name: "Login" });
};
</script>
