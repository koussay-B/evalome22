<template>
  <!-- ─── Navbar ─────────────────────────────────────────────── -->
 <header
    :class="[
      'fixed top-0 inset-x-0 z-50 transition-all duration-500',
      scrolled
        ? 'glass border-b border-white/10 dark:border-white/5 shadow-lg shadow-black/5'
        : 'bg-transparent',
    ]"
  >
    <nav class="mx-auto flex h-26 max-w-[1800px] items-center justify-between px-4 sm:px-6 lg:px-8">
      <RouterLink to="/" class="min-w-[200px] flex items-center gap-2.5 group shrink-0">
        <EvaloBrand :show-subtitle="false" size="lg" compact />
      </RouterLink>

      <div class="hidden md:flex items-center gap-0.5">
        <RouterLink
          v-for="link in navLinks"
          :key="link.to"
          :to="link.to"
          class="px-4 py-2 text-lg font-medium rounded-lg text-muted-foreground hover:text-slate-800 hover:bg-muted/50 transition-all duration-200"
          active-class="!text-slate-800 !bg-muted/60 font-semibold"
          :exact="link.to === '/'"
        >
          {{ link.label }}
        </RouterLink>
      </div>

      <div class="hidden md:flex items-center gap-2">
        <LanguageSwitcher />
        <ThemeToggle />
        <div class="w-px h-5 bg-border/60 mx-1" />

        <template v-if="authStore.isAuthenticated">

          <RouterLink :to="dashboardRoute">
            <Button variant="outline" size="sm" class="font-semibold text-sm gap-1.5 h-9">
              <LayoutDashboard class="w-3.5 h-3.5" />
              {{ t('public.nav.dashboard') }}
            </Button>
          </RouterLink>

          <DropdownMenu>
            <DropdownMenuTrigger as-child>
              <button
                class="flex items-center gap-1.5 pl-1.5 pr-2.5 py-1 rounded-lg hover:bg-muted/60 transition-colors focus:outline-none"
              >
                <div class="w-7 h-7 rounded-full overflow-hidden bg-muted border border-border flex items-center justify-center shrink-0">
                  <img
                    v-if="authStore.state.user?.imageUrl"
                    :src="authStore.state.user.imageUrl"
                    class="w-full h-full object-cover"
                    alt=""
                  />
                  <span v-else class="text-[11px] text-slate-800 text-muted-foreground">
                    {{ userInitials }}
                  </span>
                </div>
                <span class="hidden lg:block text-sm font-semibold text-slate-800 max-w-[96px] truncate">
                  {{ authStore.state.user?.userName }}
                </span>
                <ChevronDown class="w-3.5 h-3.5 text-muted-foreground shrink-0" />
              </button>
            </DropdownMenuTrigger>

            <DropdownMenuContent align="end" class="w-52">
              <DropdownMenuLabel class="font-normal pb-2">
                <div class="flex items-center gap-2.5">
                  <div class="w-8 h-8 rounded-full overflow-hidden bg-muted border border-border flex items-center justify-center shrink-0">
                    <img
                      v-if="authStore.state.user?.imageUrl"
                      :src="authStore.state.user.imageUrl"
                      class="w-full h-full object-cover"
                      alt=""
                    />
                    <span v-else class="text-[11px] text-slate-800 text-muted-foreground">
                      {{ userInitials }}
                    </span>
                  </div>
                  <div class="min-w-0">
                    <p class="text-sm font-semibold text-slate-800 truncate">
                      {{ authStore.state.user?.userName }}
                    </p>
                    <p class="text-xs text-muted-foreground truncate">
                      {{ authStore.state.user?.email }}
                    </p>
                  </div>
                </div>
              </DropdownMenuLabel>

              <DropdownMenuSeparator />

              <DropdownMenuItem class="gap-2 cursor-pointer" @click="router.push(dashboardRoute)">
                <LayoutDashboard class="w-3.5 h-3.5" />
                {{ t('public.nav.dashboard') }}
              </DropdownMenuItem>

              <DropdownMenuItem class="gap-2 cursor-pointer" @click="router.push(settingsRoute)">
                <Settings class="w-3.5 h-3.5" />
                {{ t('public.nav.settings') }}
              </DropdownMenuItem>

              <DropdownMenuSeparator />

              <DropdownMenuItem
                class="gap-2 cursor-pointer text-red-600 focus:text-red-600 focus:bg-red-50 dark:focus:bg-red-950/30"
                @click="handleLogout"
              >
                <LogOut class="w-3.5 h-3.5" />
                {{ t('public.nav.logout') }}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>

        </template>

        <template v-else>
          <RouterLink to="/login">
            <Button variant="ghost" size="sm" class="font-semibold text-sm px-4">
              {{ t('public.nav.login') }}
            </Button>
          </RouterLink>
          <RouterLink to="/company-request">
            <Button
              size="sm"
              class="brand-gradient text-white border-0 font-bold px-5 h-9 shadow-md hover:opacity-90 hover:shadow-lg hover:scale-[1.02] transition-all duration-200 text-sm"
            >
              {{ t('public.nav.getStarted') }}
            </Button>
          </RouterLink>
        </template>
      </div>

      <button
        class="md:hidden p-2 rounded-lg hover:bg-muted/60 transition-colors"
        aria-label="Open menu"
        @click="mobileOpen = true"
      >
        <Menu class="h-5 w-5" />
      </button>
    </nav>
  </header>
  <!-- ─── Mobile Menu ──────────────────────────────────────── -->
  <Teleport to="body">
    <Transition name="mobile-overlay">
      <div v-if="mobileOpen" class="fixed inset-0 z-[100] md:hidden">
        <div class="absolute inset-0 bg-black/50 backdrop-blur-sm" @click="mobileOpen = false" />
        <Transition name="mobile-panel">
          <div class="absolute right-0 top-0 bottom-0 w-72 bg-background border-l border-border shadow-2xl flex flex-col">

            <!-- Header -->
            <div class="flex items-center justify-between px-5 py-4 border-b border-border">
              <EvaloBrand :show-subtitle="false" size="sm" compact />
              <button class="p-1.5 rounded-lg hover:bg-muted/60 transition-colors" @click="mobileOpen = false">
                <X class="h-5 w-5" />
              </button>
            </div>

            <!-- Logged-in: user info card -->
            <div
              v-if="authStore.isAuthenticated"
              class="flex items-center gap-3 px-4 py-3 border-b border-border bg-muted/20"
            >
              <div
                class="w-10 h-10 rounded-full overflow-hidden bg-muted border border-border flex items-center justify-center shrink-0"
              >
                <img
                  v-if="authStore.state.user?.imageUrl"
                  :src="authStore.state.user.imageUrl"
                  class="w-full h-full object-cover"
                  alt=""
                />
                <span v-else class="text-sm text-slate-800 text-muted-foreground">{{ userInitials }}</span>
              </div>
              <div class="min-w-0">
                <p class="text-sm font-semibold text-slate-800 truncate">
                  {{ authStore.state.user?.userName }}
                </p>
                <p class="text-xs text-muted-foreground truncate">{{ authStore.state.user?.email }}</p>
              </div>
            </div>

            <!-- Nav links -->
            <div class="flex-1 flex flex-col gap-0.5 p-3 overflow-y-auto">
              <RouterLink
                v-for="link in navLinks"
                :key="link.to"
                :to="link.to"
                class="flex items-center gap-3 px-4 py-3 text-sm font-medium rounded-xl text-muted-foreground hover:text-slate-800 hover:bg-muted/60 transition-colors"
                active-class="!text-slate-800 bg-muted/60 font-semibold"
                @click="mobileOpen = false"
              >
                <component :is="link.icon" class="h-4 w-4 shrink-0" />
                {{ link.label }}
              </RouterLink>

              <!-- Logged-in extra links -->
              <template v-if="authStore.isAuthenticated">
                <div class="h-px bg-border/60 my-1" />
                <RouterLink
                  :to="dashboardRoute"
                  class="flex items-center gap-3 px-4 py-3 text-sm font-medium rounded-xl text-muted-foreground hover:text-slate-800 hover:bg-muted/60 transition-colors"
                  @click="mobileOpen = false"
                >
                  <LayoutDashboard class="h-4 w-4 shrink-0" />
                  {{ t('public.nav.dashboard') }}
                </RouterLink>
                <RouterLink
                  :to="settingsRoute"
                  class="flex items-center gap-3 px-4 py-3 text-sm font-medium rounded-xl text-muted-foreground hover:text-slate-800 hover:bg-muted/60 transition-colors"
                  @click="mobileOpen = false"
                >
                  <Settings class="h-4 w-4 shrink-0" />
                  {{ t('public.nav.settings') }}
                </RouterLink>
              </template>
            </div>

            <!-- Bottom actions -->
            <div class="p-4 border-t border-border space-y-3">
              <div class="flex items-center gap-2">
                <LanguageSwitcher />
                <ThemeToggle />
              </div>

              <!-- Logged in: logout -->
              <template v-if="authStore.isAuthenticated">
                <button
                  @click="handleLogout"
                  class="w-full flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg border border-red-200 dark:border-red-900 text-red-600 text-sm font-semibold hover:bg-red-50 dark:hover:bg-red-950/30 transition-colors"
                >
                  <LogOut class="w-4 h-4" />
                  {{ t('public.nav.logout') }}
                </button>
              </template>

              <!-- Not logged in: login + get started -->
              <template v-else>
                <RouterLink to="/login" class="block" @click="mobileOpen = false">
                  <Button variant="outline" class="w-full font-semibold">
                    {{ t('public.nav.login') }}
                  </Button>
                </RouterLink>
                <RouterLink to="/company-request" class="block" @click="mobileOpen = false">
                  <Button class="w-full brand-gradient text-white border-0 font-bold shadow-md hover:opacity-90">
                    {{ t('public.nav.getStarted') }}
                  </Button>
                </RouterLink>
              </template>
            </div>

          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>

  <!-- ─── Page Content ────────────────────────────────────── -->
  <main class="min-h-screen pt-16">
    <RouterView />
  </main>

  <!-- ─── Footer ──────────────────────────────────────────── -->
  <footer class="relative border-t border-border/60 overflow-hidden">
    <div class="absolute inset-0 -z-10 mesh-bg opacity-40" />

    <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-16">

      <!-- Main footer grid -->
      <div class="grid grid-cols-2 md:grid-cols-5 gap-8 md:gap-10">

        <!-- Brand col (spans 2) -->
        <div class="col-span-2 md:col-span-2">
          <RouterLink to="/" class="flex items-center gap-2.5 mb-5 w-fit group">
            <EvaloBrand :show-subtitle="false" size="sm" compact />
          </RouterLink>
          <p class="text-sm text-muted-foreground leading-relaxed max-w-[220px]">
            {{ t('public.footer.tagline') }}
          </p>
          <!-- Mini stats strip -->
          <div class="mt-6 flex gap-5">
            <div>
              <div class="text-lg text-slate-800 brand-text-gradient">200+</div>
              <div class="text-xs text-muted-foreground">{{ t('public.footer.statsCompanies') }}</div>
            </div>
            <div class="w-px bg-border/60" />
            <div>
              <div class="text-lg text-slate-800 brand-text-gradient">15K+</div>
              <div class="text-xs text-muted-foreground">{{ t('public.footer.statsCandidates') }}</div>
            </div>
          </div>
        </div>

        <!-- Product links -->
        <div>
          <h4 class="text-xs font-bold uppercase tracking-widest text-muted-foreground mb-5">
            {{ t('public.footer.product') }}
          </h4>
          <ul class="space-y-3 text-sm">
            <li>
              <RouterLink to="/features" class="text-muted-foreground hover:text-slate-800 transition-colors inline-flex items-center gap-1.5 group">
                {{ t('public.nav.features') }}
                <ArrowRight class="h-3 w-3 opacity-0 group-hover:opacity-100 -translate-x-1 group-hover:translate-x-0 transition-all" />
              </RouterLink>
            </li>
            <li>
              <RouterLink to="/pricing" class="text-muted-foreground hover:text-slate-800 transition-colors inline-flex items-center gap-1.5 group">
                {{ t('public.nav.pricing') }}
                <ArrowRight class="h-3 w-3 opacity-0 group-hover:opacity-100 -translate-x-1 group-hover:translate-x-0 transition-all" />
              </RouterLink>
            </li>
            <li>
              <RouterLink to="/company-request" class="text-muted-foreground hover:text-slate-800 transition-colors inline-flex items-center gap-1.5 group">
                {{ t('public.nav.getStarted') }}
                <ArrowRight class="h-3 w-3 opacity-0 group-hover:opacity-100 -translate-x-1 group-hover:translate-x-0 transition-all" />
              </RouterLink>
            </li>
          </ul>
        </div>

        <!-- Company links -->
        <div>
          <h4 class="text-xs font-bold uppercase tracking-widest text-muted-foreground mb-5">
            {{ t('public.footer.company') }}
          </h4>
          <ul class="space-y-3 text-sm">
            <li>
              <RouterLink to="/about" class="text-muted-foreground hover:text-slate-800 transition-colors inline-flex items-center gap-1.5 group">
                {{ t('public.nav.about') }}
                <ArrowRight class="h-3 w-3 opacity-0 group-hover:opacity-100 -translate-x-1 group-hover:translate-x-0 transition-all" />
              </RouterLink>
            </li>
          </ul>
        </div>

        <!-- Legal links -->
        <div>
          <h4 class="text-xs font-bold uppercase tracking-widest text-muted-foreground mb-5">
            {{ t('public.footer.legal') }}
          </h4>
          <ul class="space-y-3 text-sm">
            <li>
              <span class="text-muted-foreground/60 cursor-default">{{ t('public.footer.privacy') }}</span>
            </li>
            <li>
              <span class="text-muted-foreground/60 cursor-default">{{ t('public.footer.terms') }}</span>
            </li>
          </ul>
        </div>
      </div>

      <!-- Bottom bar -->
      <div class="mt-14 pt-8 border-t border-border/60 flex flex-col sm:flex-row items-center justify-between gap-4">
        <div class="flex items-center gap-3">
          <p class="text-xs text-muted-foreground">
            © {{ new Date().getFullYear() }} EVALO.me. {{ t('public.footer.rights') }}
          </p>
          <!-- Live indicator -->
          <div class="hidden sm:flex items-center gap-1.5 rounded-full border border-green-500/20 bg-green-500/5 px-2.5 py-1">
            <span class="h-1.5 w-1.5 rounded-full bg-green-500 animate-pulse" />
            <span class="text-[10px] font-semibold text-green-600 dark:text-green-400">{{ t('public.footer.systemStatus') }}</span>
          </div>
        </div>
        <div class="flex items-center gap-3">
          <LanguageSwitcher />
          <ThemeToggle />
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { RouterLink, RouterView, useRouter } from 'vue-router'
import {
  Menu, X, ArrowRight, Home, Layers, Tag, Info,
  LayoutDashboard, ChevronDown, Settings, LogOut,
} from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu, DropdownMenuTrigger, DropdownMenuContent,
  DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator,
} from '@/components/ui/dropdown-menu'
import ThemeToggle     from '@/components/common/ThemeToggle.vue'
import LanguageSwitcher from '@/components/common/LanguageSwitcher.vue'
import { useLocale }   from '@/composables/useLocale'
import { useAuthStore } from '@/store/authStore'
import EvaloBrand from '@/components/branding/EvaloBrand.vue'

const { t } = useLocale()
const router    = useRouter()
const authStore = useAuthStore()

// ── Scroll state ──────────────────────────────────────────────────────────

const scrolled   = ref(false)
const mobileOpen = ref(false)

function onScroll() { scrolled.value = window.scrollY > 10 }
onMounted(() => window.addEventListener('scroll', onScroll, { passive: true }))
onUnmounted(() => window.removeEventListener('scroll', onScroll))

// ── Nav links ─────────────────────────────────────────────────────────────

const navLinks = computed(() => [
  { to: '/',         label: t('public.nav.home'),     icon: Home   },
  { to: '/features', label: t('public.nav.features'), icon: Layers },
  { to: '/pricing',  label: t('public.nav.pricing'),  icon: Tag    },
  { to: '/about',    label: t('public.nav.about'),    icon: Info   },
])

// ── Auth-aware routing ────────────────────────────────────────────────────

const dashboardRoute = computed(() => {
  if (authStore.isAdmin)        return '/admin'
  if (authStore.isCompanyAdmin) return '/company'
  if (authStore.isFormateur)    return '/formateur'
  if (authStore.isCondidat)     return '/candidat/dashboard'
  return '/'
})

const settingsRoute = computed(() => {
  if (authStore.isAdmin)        return '/admin/settings'
  if (authStore.isCompanyAdmin) return '/company/settings'
  if (authStore.isFormateur)    return '/formateur'
  if (authStore.isCondidat)     return '/candidat'
  return '/'
})

// ── User display ──────────────────────────────────────────────────────────

const userInitials = computed(() => {
  const name = authStore.state.user?.userName ?? ''
  return name
    .split(' ')
    .filter(Boolean)
    .map(n => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2) || '?'
})

// ── Logout ────────────────────────────────────────────────────────────────

function handleLogout() {
  mobileOpen.value = false
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
/* Mobile menu transitions */
.mobile-overlay-enter-active,
.mobile-overlay-leave-active { transition: opacity 0.25s ease; }
.mobile-overlay-enter-from,
.mobile-overlay-leave-to { opacity: 0; }

.mobile-panel-enter-active,
.mobile-panel-leave-active {
  transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
.mobile-panel-enter-from,
.mobile-panel-leave-to { transform: translateX(100%); }
</style>
