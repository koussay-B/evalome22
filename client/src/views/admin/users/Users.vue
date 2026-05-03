<template>

  <!-- ── Add Employee Sheet ───────────────────────────────────────── -->
  <AddEmployeeSheet
    v-model:open="isAddOpen"
    @invited="fetchCurrentPage"
  />

  <UserDetailsSheet
    v-model:open="isDetailOpen"
    :user="selectedUser?.source ?? null"
    :initial-action="detailAction"
    @updated="handleUserUpdated"
    @deleted="handleUserDeleted"
    @password-reset="handleUserReset"
  />

  <!-- ── Page ──────────────────────────────────────────────────────── -->
  <div class="space-y-8">

    <div class="flex items-end justify-between">
      <div>
        <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">
          {{ t('users.subtitle') }}
        </p>
        <h1 class="text-3xl text-slate-800 tracking-tight text-foreground">{{ t('users.title') }}</h1>
      </div>
      <button
        @click="isAddOpen = true"
        class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 transition-colors"
      >
        <UserPlus class="w-4 h-4" /> {{ t('users.addUser') }}
      </button>
    </div>

    <!-- Search & Filter -->
    <div class="flex flex-wrap gap-3 items-center">
      <div class="relative flex-1 min-w-[200px] max-w-sm">
        <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
        <Input
          v-model="searchQuery"
          :placeholder="t('users.searchPlaceholder')"
          class="pl-9 h-9"
        />
      </div>
      <Select v-model="activeFilter">
        <SelectTrigger class="w-[180px] h-9">
          <SelectValue :placeholder="t('users.roles.all')" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="All">{{ t('users.roles.all') }}</SelectItem>
          <SelectItem v-for="role in roles" :key="role.value" :value="role.value">
            {{ role.label }}
          </SelectItem>
        </SelectContent>
      </Select>
    </div>

    <!-- Shimmer grid -->
    <div v-if="userStore.list.isLoading" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <EmployeeCardShimmer v-for="i in pageSize" :key="i" />
    </div>

    <!-- Error -->
    <div
      v-else-if="userStore.list.isError"
      class="rounded-xl border border-destructive/30 bg-destructive/5 px-5 py-4 text-sm text-destructive font-medium"
    >
      {{ t('common.loadError') }}
    </div>

    <!-- Cards Grid -->
    <div v-else-if="mappedUsers.length > 0" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <ContextMenuRoot v-for="user in mappedUsers" :key="user.id">
        <ContextMenuTrigger as-child>
          <EmployeeCard
            :employee="user"
            :show-company="true"
            @click="openDetail(user)"
          >
            <template #actions>
              <DropdownMenu>
                <DropdownMenuTrigger as-child>
                  <button
                    @click.stop
                    class="p-1.5 rounded-lg text-muted-foreground hover:bg-muted hover:text-foreground transition-colors opacity-0 group-hover:opacity-100 focus:opacity-100 shrink-0"
                  >
                    <MoreHorizontal class="w-4 h-4" />
                  </button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" class="w-44">
                  <DropdownMenuLabel class="text-xs text-muted-foreground font-semibold">
                    {{ t('common.actions') }}
                  </DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem class="gap-2 cursor-pointer" @click="openDetail(user)">
                    <Eye class="w-3.5 h-3.5" /> {{ t('users.viewProfile') }}
                  </DropdownMenuItem>
                  <DropdownMenuItem class="gap-2 cursor-pointer" @click="openDetail(user, 'edit')">
                    <Pencil class="w-3.5 h-3.5" /> {{ t('common.edit') }}
                  </DropdownMenuItem>
                  <DropdownMenuItem class="gap-2 cursor-pointer" @click="openDetail(user, 'reset')">
                    <RefreshCw class="w-3.5 h-3.5" /> {{ t('users.resetPassword') }}
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    class="gap-2 cursor-pointer text-red-600 focus:text-red-600 focus:bg-red-50 dark:focus:bg-red-950/30"
                    @click="openDetail(user, 'delete')"
                  >
                    <Trash2 class="w-3.5 h-3.5" /> {{ t('common.delete') }}
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </template>
          </EmployeeCard>
        </ContextMenuTrigger>
        <ContextMenuPortal>
          <ContextMenuContent class="z-50 min-w-[160px] rounded-xl border border-border bg-background shadow-xl p-1 animate-in fade-in-0 zoom-in-95">
            <ContextMenuItem
              class="flex items-center gap-2 px-3 py-2 rounded-lg text-sm cursor-pointer hover:bg-muted outline-none"
              @click="openDetail(user)"
            >
              <Eye class="w-3.5 h-3.5" /> View Profile
            </ContextMenuItem>
            <ContextMenuItem
              class="flex items-center gap-2 px-3 py-2 rounded-lg text-sm cursor-pointer hover:bg-muted outline-none"
              @click="openDetail(user, 'edit')"
            >
              <Pencil class="w-3.5 h-3.5" /> Edit
            </ContextMenuItem>
            <ContextMenuItem
              class="flex items-center gap-2 px-3 py-2 rounded-lg text-sm cursor-pointer hover:bg-muted outline-none"
              @click="openDetail(user, 'reset')"
            >
              <RefreshCw class="w-3.5 h-3.5" /> Reset Password
            </ContextMenuItem>
            <ContextMenuSeparator class="my-1 h-px bg-border" />
            <ContextMenuItem
              class="flex items-center gap-2 px-3 py-2 rounded-lg text-sm cursor-pointer text-red-600 hover:bg-red-50 dark:hover:bg-red-950/30 outline-none"
              @click="openDetail(user, 'delete')"
            >
              <Trash2 class="w-3.5 h-3.5" /> Delete
            </ContextMenuItem>
          </ContextMenuContent>
        </ContextMenuPortal>
      </ContextMenuRoot>
    </div>

    <!-- Empty state -->
    <EmptyData
      v-else-if="userStore.list.hasData"
      :icon="UsersIcon"
      :title="t('users.emptyTitle')"
      :description="t('users.emptyDesc')"
    >
      <button
        v-if="searchQuery || activeFilter !== 'All'"
        @click="clearFilters"
        class="flex items-center gap-2 px-4 py-2 rounded-lg border border-border text-sm font-semibold text-foreground hover:bg-muted/50 transition-colors"
      >
        <X class="w-3.5 h-3.5" /> {{ t('common.clearFilters') }}
      </button>
    </EmptyData>

    <!-- Footer: 3-column — count | paginator | size-select -->
    <div
      v-if="userStore.list.hasData && userStore.list.meta"
      class="grid grid-cols-3 items-center gap-2"
    >
      <!-- Left: record count -->
      <p class="text-xs text-muted-foreground">
        {{ t('common.showing') }} {{ mappedUsers.length }}
        {{ t('common.of') }} {{ userStore.list.meta.totalCount }}
        {{ t('users.title').toLowerCase() }}
      </p>

      <!-- Center: paginator -->
      <div class="flex justify-center">
        <AppPaginator
          v-if="userStore.list.meta.totalPages > 1"
          :page="userStore.list.meta.currentPage"
          :pages="userStore.list.meta.totalPages"
          @page-change="currentPage = $event"
        />
      </div>

      <!-- Right: page-size select -->
      <div class="flex justify-end">
        <Select
          :model-value="String(pageSize)"
          @update:model-value="(v) => { if (v) { pageSize = parseInt(v as string); resetPage() } }"
        >
          <SelectTrigger
            class="h-8 w-[110px] text-xs font-semibold bg-background border-border hover:border-primary/30 transition-colors"
          >
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem v-for="s in PAGE_SIZES" :key="s" :value="String(s)">
              {{ s }} {{ t('pagination.perPage') }}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { watchDebounced } from '@vueuse/core'
import { Input } from '@/components/ui/input'
import { DropdownMenu, DropdownMenuTrigger, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator } from '@/components/ui/dropdown-menu'
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from '@/components/ui/select'
import {
  ContextMenuRoot, ContextMenuTrigger, ContextMenuPortal,
  ContextMenuContent, ContextMenuItem, ContextMenuSeparator,
} from 'reka-ui'
import AppPaginator    from '@/components/common/AppPaginator.vue'
import EmptyData       from '@/components/common/EmptyData.vue'
import EmployeeCard, { type EmployeeCardData } from '@/components/employee/EmployeeCard.vue'
import EmployeeCardShimmer from '@/components/employee/EmployeeCardShimmer.vue'
import AddEmployeeSheet    from '@/components/company/AddEmployeeSheet.vue'
import UserDetailsSheet    from '@/components/user/UserDetailsSheet.vue'
import { useLocale }    from '@/composables/useLocale'
import { useTableQuery, VALID_SIZES as PAGE_SIZES } from '@/composables/useTableQuery'
import { useUserStore } from '@/store/userStore'
import type { UserListItem } from '@utils/models'
import {
  UserPlus, Users as UsersIcon, Pencil,
  Trash2, Search, MoreHorizontal, Eye, RefreshCw, X,
} from 'lucide-vue-next'
import { UserRole } from '@/utils/roles'

const { t } = useLocale()
const { page: currentPage, size: pageSize, search: searchQuery, filter: activeFilter, resetPage } = useTableQuery()
const userStore = useUserStore()

interface AppUser extends EmployeeCardData {
  role: UserRole
  company: string
  source: UserListItem
}

function toCardData(u: UserListItem): AppUser {
  return {
    id:       u.id,
    name:     u.userName,
    email:    u.email,
    initials: u.userName.split(' ').filter(Boolean).map(n => n[0]).join('').toUpperCase().slice(0, 2),
    role:     u.role as UserRole,
    active:   u.isActive,
    imageUrl: u.imageUrl,
    company:  u.companyName ?? '',
    source:   u,
  }
}

const mappedUsers = computed<AppUser[]>(() => userStore.list.currentItems.map(toCardData))

// ─── Role options for the filter dropdown ─────────────────────────────────

const roles = computed(() => [
  { value: UserRole.Admin,        label: t('users.roles.admin')        },
  { value: UserRole.CompanyAdmin, label: t('users.roles.companyAdmin') },
  { value: UserRole.Formateur,    label: t('users.roles.formateur')    },
  { value: UserRole.Condidat,     label: t('users.roles.condidat')     },
])

// ─── Data fetching ─────────────────────────────────────────────────────────

async function fetchCurrentPage() {
  await userStore.list.fetchPage({
    pageNumber:     currentPage.value,
    pageSize:       pageSize.value,
    search:         searchQuery.value || undefined,
    role:           activeFilter.value !== 'All' ? activeFilter.value : undefined,
    includeCompany: true,
    orderBy:        'username',
  })
}

onMounted(fetchCurrentPage)
watch([currentPage, pageSize], fetchCurrentPage)
watchDebounced(searchQuery, () => {
  if (currentPage.value > 1) resetPage()
  else fetchCurrentPage()
}, { debounce: 350 })
watch(activeFilter, () => {
  if (currentPage.value > 1) resetPage()
  else fetchCurrentPage()
})

// Clamp page if meta changes (filter applied, fewer pages available)
watch(() => userStore.list.meta?.totalPages, (max) => {
  if (max && currentPage.value > max) currentPage.value = max
})

// ─── UI state ──────────────────────────────────────────────────────────────

const isAddOpen    = ref(false)
const isDetailOpen = ref(false)
const selectedUser = ref<AppUser | null>(null)
const detailAction = ref<'edit' | 'reset' | 'delete' | null>(null)

function clearFilters() {
  searchQuery.value  = ''
  activeFilter.value = 'All'
  resetPage()
}

watch(isDetailOpen, (open) => {
  if (!open) detailAction.value = null
})

function openDetail(user: AppUser, action: 'edit' | 'reset' | 'delete' | null = null) {
  selectedUser.value = user
  detailAction.value = action
  isDetailOpen.value = true
}

async function handleUserUpdated() {
  await fetchCurrentPage()
}

async function handleUserDeleted(id: number) {
  selectedUser.value = null
  userStore.list.removeFromCurrentPage(id)
  await fetchCurrentPage()
}

async function handleUserReset() {
  await fetchCurrentPage()
}
</script>
