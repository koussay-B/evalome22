<template>
  <div class="space-y-6">

    <!-- ── Add Employee Sheet ───────────────────────────────────────── -->
    <AddEmployeeSheet
      v-model:open="isAddOpen"
      @invited="fetchCurrentPage"
    />

    <UserDetailsSheet
      v-model:open="isDetailOpen"
      :user="selectedEmp?.source ?? null"
      :show-company="false"
      :initial-action="detailAction"
      @updated="handleUserUpdated"
      @deleted="handleUserDeleted"
      @password-reset="handleUserReset"
    />

    <!-- Page Header -->
    <div class="flex items-end justify-between gap-4">
      <div>
        <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">
          {{ t('companyPanel.employees.subtitle') }}
        </p>
        <h1 class="text-3xl text-slate-800 tracking-tight text-slate-800">
          {{ t('companyPanel.employees.title') }} 
        </h1>
      </div>
      <Button
        @click="isAddOpen = true"
        class="gap-2 shadow-[0_2px_12px_0_oklch(0.58_0.21_42/0.18)] hover:shadow-[0_4px_20px_0_oklch(0.58_0.21_42/0.28)] transition-shadow"
      >
        <UserPlus class="w-4 h-4" /> {{ t('companyPanel.employees.addEmployee') }}
      </Button>
    </div>

    <!-- Search + Role Filter -->
    <div class="flex flex-wrap gap-3 items-center">
      <div class="relative flex-1 min-w-[200px] max-w-sm">
        <Search class="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
        <Input
          v-model="searchQuery"
          :placeholder="t('companyPanel.employees.searchPlaceholder')"
          class="ps-9 h-9 bg-card border-border focus-visible:ring-primary/30"
        />
      </div>
      <Select v-model="roleFilter">
        <SelectTrigger class="w-[180px] h-9 bg-card border-border focus:ring-primary/30">
          <SelectValue :placeholder="t('companyPanel.employees.allRoles')" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">{{ t('companyPanel.employees.allRoles') }}</SelectItem>
          <SelectItem v-for="role in availableRoles" :key="role.value" :value="role.value">
            {{ role.label }}
          </SelectItem>
        </SelectContent>
      </Select>
    </div>

    <!-- Shimmer grid -->
    <div v-if="userStore.list?.isLoading" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <EmployeeCardShimmer v-for="i in pageSize" :key="i" />
    </div>

    <!-- Error -->
    <div
      v-else-if="userStore.list?.isError"
      class="rounded-2xl border border-destructive/30 bg-destructive/5 px-5 py-4 text-sm text-destructive font-medium"
    >
      {{ t('common.loadError') }}
    </div>

    <!-- Cards Grid -->
    <div
      v-else-if="mappedEmployees?.length > 0"
      class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4"
    >
      <div v-for="emp in mappedEmployees" :key="emp.id">
        <ContextMenuRoot>
        <ContextMenuTrigger as-child>
          <EmployeeCard
            :employee="emp"
            :show-company="false"
            @click="openDetail(emp)"
          >
            <template #actions>
              <DropdownMenu>
                <DropdownMenuTrigger as-child>
                  <button
                    @click.stop
                    class="p-1.5 rounded-lg text-muted-foreground hover:bg-primary/8 hover:text-primary transition-colors opacity-0 group-hover:opacity-100 focus:opacity-100 shrink-0"
                  >
                    <MoreHorizontal class="w-4 h-4" />
                  </button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" class="w-40">
                  <DropdownMenuLabel class="text-xs">{{ t('common.actions') }}</DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem class="gap-2 cursor-pointer" @click="openDetail(emp)">
                    <Eye class="w-3.5 h-3.5" /> {{ t('users.viewProfile') }}
                  </DropdownMenuItem>
                  <DropdownMenuItem class="gap-2 cursor-pointer" @click="openDetail(emp, 'edit')">
                    <Pencil class="w-3.5 h-3.5" /> {{ t('common.edit') }}
                  </DropdownMenuItem>
                  <DropdownMenuItem class="gap-2 cursor-pointer" @click="openDetail(emp, 'reset')">
                    <RefreshCw class="w-3.5 h-3.5" /> {{ t('users.resetPassword') }}
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    @click="openDetail(emp, 'delete')"
                    class="gap-2 cursor-pointer text-red-600 focus:text-red-600 focus:bg-red-50 dark:focus:bg-red-950/30"
                  >
                    <Trash2 class="w-3.5 h-3.5" /> {{ t('common.delete') }}
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </template>
          </EmployeeCard>
        </ContextMenuTrigger>
        <ContextMenuPortal>
          <ContextMenuContent class="z-50 min-w-[160px] rounded-2xl border border-border bg-card/90 backdrop-blur-md shadow-xl p-1 animate-in fade-in-0 zoom-in-95">
            <ContextMenuItem
              class="flex items-center gap-2 px-3 py-2 rounded-xl text-sm cursor-pointer hover:bg-primary/8 hover:text-primary outline-none transition-colors"
              @click="openDetail(emp)"
            >
              <Eye class="w-3.5 h-3.5" /> View Profile
            </ContextMenuItem>
            <ContextMenuItem
              class="flex items-center gap-2 px-3 py-2 rounded-xl text-sm cursor-pointer hover:bg-primary/8 hover:text-primary outline-none transition-colors"
              @click="openDetail(emp, 'edit')"
            >
              <Pencil class="w-3.5 h-3.5" /> Edit
            </ContextMenuItem>
            <ContextMenuItem
              class="flex items-center gap-2 px-3 py-2 rounded-xl text-sm cursor-pointer hover:bg-primary/8 hover:text-primary outline-none transition-colors"
              @click="openDetail(emp, 'reset')"
            >
              <RefreshCw class="w-3.5 h-3.5" /> Reset Password
            </ContextMenuItem>
            <ContextMenuSeparator class="my-1 h-px bg-border" />
            <ContextMenuItem
              class="flex items-center gap-2 px-3 py-2 rounded-xl text-sm cursor-pointer text-red-600 hover:bg-red-50 dark:hover:bg-red-950/30 outline-none transition-colors"
              @click="openDetail(emp, 'delete')"
            >
              <Trash2 class="w-3.5 h-3.5" /> Delete
            </ContextMenuItem>
          </ContextMenuContent>
        </ContextMenuPortal>
        </ContextMenuRoot>
      </div>
    </div>
  

    <!-- Empty State -->
    <EmptyData
      v-else-if="!userStore.list?.isLoading && mappedEmployees?.length === 0"
      :icon="UsersIcon"
      :title="t('companyPanel.employees.emptyTitle')"
      :description="t('companyPanel.employees.emptyDesc')"
    >
      <button
        v-if="searchQuery || roleFilter !== 'all'"
        @click="clearFilters"
        class="flex items-center gap-2 px-4 py-2 rounded-xl border border-border text-sm font-semibold text-slate-800 hover:bg-primary/5 hover:border-primary/30 hover:text-primary transition-colors mt-2"
      >
        <X class="w-3.5 h-3.5" /> {{ t('common.clearFilters') }}
      </button>
    </EmptyData>

    <!-- Footer: 3-column — count | paginator | size-select -->
    <div v-if="userStore.list.meta" class="grid grid-cols-3 items-center gap-2 pt-2 border-t border-border/50">
      <!-- Left: record count -->
      <p class="text-xs text-muted-foreground">
        {{ t('common.showing') }} {{ mappedEmployees.length }}
        {{ t('common.of') }} {{ userStore.list.meta.totalCount }}
        {{ t('companyPanel.employees.title').toLowerCase() }}
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
            class="h-8 w-[110px] text-xs font-semibold bg-card border-border hover:border-primary/30 transition-colors"
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
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from '@/components/ui/select'
import { DropdownMenu, DropdownMenuTrigger, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator } from '@/components/ui/dropdown-menu'
import {
  ContextMenuRoot, ContextMenuTrigger, ContextMenuPortal,
  ContextMenuContent, ContextMenuItem, ContextMenuSeparator,
} from 'reka-ui'
import AppPaginator       from '@/components/common/AppPaginator.vue'
import EmptyData          from '@/components/common/EmptyData.vue'
import EmployeeCard, { type EmployeeCardData } from '@/components/employee/EmployeeCard.vue'
import EmployeeCardShimmer from '@/components/employee/EmployeeCardShimmer.vue'
import AddEmployeeSheet    from '@/components/company/AddEmployeeSheet.vue'
import UserDetailsSheet    from '@/components/user/UserDetailsSheet.vue'
import { useLocale }    from '@/composables/useLocale'
import { useTableQuery, VALID_SIZES as PAGE_SIZES } from '@/composables/useTableQuery'
import { useUserStore } from '@/store/userStore'
import type { UserListItem } from '@/utils/models'
import { Search, UserPlus, MoreHorizontal, Pencil, Trash2, X, Users as UsersIcon, Eye, RefreshCw } from 'lucide-vue-next'
import { UserRole } from '@/utils/roles'
import Button from '@/components/ui/button/Button.vue'
import { useCompanyStore } from '@/store/companyStore'

const { t } = useLocale()
const { page: currentPage, size: pageSize, search: searchQuery, resetPage } = useTableQuery()
const roleFilter = ref('all')
const userStore  = useUserStore()

// ─── Role filter options (CompanyAdmin sees Formateur + Condidat) ───────────

const availableRoles = computed(() => [
  { value: UserRole.Formateur, label: t('users.roles.formateur') },
  { value: UserRole.Condidat,  label: t('users.roles.condidat')  },
])

// ─── Data mapping ──────────────────────────────────────────────────────────

interface Employee extends EmployeeCardData {
  source: UserListItem
}

function toCardData(u: UserListItem): Employee {
  const safeName = u.userName || '—'
  return {
    id:       u.id,
    name:     safeName,
    email:    u.email,
    initials: safeName.split(' ').filter(Boolean).map(n => n[0]).join('').toUpperCase().slice(0, 2) || '?',
    role:     u.role as UserRole,
    active:   u.isActive,
    imageUrl: u.imageUrl,
    company:  u.companyName ?? '',
    source:   u,
  }
}

const mappedEmployees = computed<Employee[]>(() => (userStore.list?.currentItems ?? []).map(toCardData))

// ─── Data fetching ─────────────────────────────────────────────────────────

async function fetchCurrentPage() {
  await userStore.list.fetchPage({
    pageNumber:     currentPage.value,
    pageSize:       pageSize.value,
    search:         searchQuery.value || undefined,
    role:           roleFilter.value !== 'all' ? roleFilter.value : undefined,
    includeCompany: false,
    orderBy:        'username',
  })
}

onMounted(fetchCurrentPage)
watch([currentPage, pageSize], fetchCurrentPage)
watchDebounced(searchQuery, () => {
  if (currentPage.value > 1) resetPage()
  else fetchCurrentPage()
}, { debounce: 350 })
watch(roleFilter, () => {
  if (currentPage.value > 1) resetPage()
  else fetchCurrentPage()
})
watch(() => userStore.list.meta?.totalPages, (max) => {
  if (max && currentPage.value > max) currentPage.value = max
})

// ─── UI state ──────────────────────────────────────────────────────────────

const isAddOpen    = ref(false)
const isDetailOpen = ref(false)
const selectedEmp  = ref<Employee | null>(null)
const detailAction = ref<'edit' | 'reset' | 'delete' | null>(null)

function clearFilters() {
  searchQuery.value = ''
  roleFilter.value  = 'all'
  resetPage()
}

watch(isDetailOpen, (open) => {
  if (!open) detailAction.value = null
})

function openDetail(emp: Employee, action: 'edit' | 'reset' | 'delete' | null = null) {
  selectedEmp.value  = emp
  detailAction.value = action
  isDetailOpen.value = true
}

async function handleUserUpdated() {
  await fetchCurrentPage()
}

async function handleUserDeleted(id: number) {
  selectedEmp.value = null
  userStore.list.removeFromCurrentPage(id)
  await fetchCurrentPage()
}

async function handleUserReset() {
  await fetchCurrentPage()
}
</script>