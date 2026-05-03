<template>
  <SidebarMenu>
    <SidebarMenuItem>
      <DropdownMenu>
        <DropdownMenuTrigger as-child>
          <SidebarMenuButton
            size="lg"
            class="data-[state=open]:bg-sidebar-accent data-[state=open]:text-sidebar-accent-foreground"
          >
            <Avatar class="size-8 rounded-lg">
              <AvatarImage v-if="avatarUrl" :src="avatarUrl" :alt="userName" />
              <AvatarFallback class="rounded-lg bg-primary text-primary-foreground text-xs font-semibold">
                {{ initials }}
              </AvatarFallback>
            </Avatar>
            <div class="grid flex-1 text-left text-sm leading-tight">
              <span class="truncate font-medium">{{ userName }}</span>
              <span class="truncate text-xs text-muted-foreground">{{ userEmail }}</span>
            </div>
            <ChevronsUpDown class="ml-auto size-4" />
          </SidebarMenuButton>
        </DropdownMenuTrigger>

        <DropdownMenuContent
          class="w-(--radix-dropdown-menu-trigger-width) min-w-56 rounded-lg"
          :side="isMobile ? 'bottom' : 'right'"
          align="end"
          :side-offset="4"
        >
          <!-- User info header inside dropdown -->
          <DropdownMenuLabel class="p-0 font-normal">
            <div class="flex items-center gap-2 px-1 py-1.5 text-left text-sm">
              <Avatar class="size-8 rounded-lg">
                <AvatarImage v-if="avatarUrl" :src="avatarUrl" :alt="userName" />
                <AvatarFallback class="rounded-lg bg-primary text-primary-foreground text-xs font-semibold">
                  {{ initials }}
                </AvatarFallback>
              </Avatar>
              <div class="grid flex-1 text-left text-sm leading-tight">
                <span class="truncate font-semibold">{{ userName }}</span>
                <span class="truncate text-xs text-muted-foreground">{{ userEmail }}</span>
              </div>
            </div>
          </DropdownMenuLabel>

          

          <DropdownMenuItem
            class="text-destructive focus:bg-destructive/10 focus:text-destructive"
            @click="handleLogout"
          >
            <LogOut class="mr-2 size-4" />
            {{ t('layout.userMenu.logout') }}
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </SidebarMenuItem>
  </SidebarMenu>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { BadgeCheck, Bell, ChevronsUpDown, CreditCard, LogOut, Sparkles } from 'lucide-vue-next';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuGroup,
  DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { SidebarMenu, SidebarMenuButton, SidebarMenuItem, useSidebar } from '@/components/ui/sidebar';
import { useAuthStore } from '@/store/authStore';
import { useLocale } from '@/composables/useLocale';

// ── Stores ────────────────────────────────────────────────
const authStore = useAuthStore();
const router = useRouter();
const { isMobile } = useSidebar();
const { t } = useLocale();

// ── User data from store ──────────────────────────────────
const userName  = computed(() => authStore.state.user?.userName  ?? t('users.user'));
const userEmail = computed(() => authStore.state.user?.email     ?? '');
const avatarUrl = computed(() => authStore.state.user?.imageUrl  ?? '');

// ── Initials fallback (e.g. "John Doe" → "JD") ───────────
const initials = computed(() =>
  userName.value
    .split(/[\s._-]/)
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0]?.toUpperCase())
    .join('') || '?'
);

// ── Logout ────────────────────────────────────────────────
const handleLogout = () => {
  authStore.logout();
  router.push({ name: 'Login' });
};
</script>
