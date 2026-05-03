<template>
  <div class="space-y-6">

    <!-- ── Add Employee Sheet ───────────────────────────────────────── -->
    <AddEmployeeSheet
      v-model:open="isAddOpen"
      @invited="fetchCurrentPage"
    />

    <!-- Page Header -->
    <div class="flex items-end justify-between gap-4">
      <div>
        <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">
          {{ t('formateur.employees.subtitle') }}
        </p>
        <h1 class="text-3xl text-slate-800 tracking-tight text-slate-800">
          {{ t('formateur.employees.title') }}
        </h1>
      </div>
      <Button
        variant="default"
        size="sm"
        @click="isAddOpen = true"
      >
        <UserPlus class="w-4 h-4" /> 
        <span class="hidden sm:block">{{ t('formateur.employees.addCandidat') }}</span>
      </Button>
    </div>

    <!-- Permission Info Banner -->
    <div class="flex items-start gap-2.5 px-3 py-2 rounded-lg border border-blue-200/60 bg-blue-50/50 text-slate-800">
      <Info class="w-3.5 h-3.5 mt-0.5 shrink-0 opacity-80" />
      
      <p class="text-[11px] font-semibold leading-tight tracking-tight">
        {{ t('formateur.employees.permissionBanner') }}
      </p>
    </div>

    <!-- Stats Bar -->
    <div class="grid grid-cols-2 sm:grid-cols-3 gap-3 ">
      <div class="px-4 py-3 rounded-xl border border-border bg-[#F5F4F2]  flex items-center gap-3">
        <div class="w-9 h-9 rounded-lg bg-[#F5F4F2]  flex items-center justify-center">
          <UsersIcon class="w-4 h-4 text-muted-foreground" />
        </div>
        <div>
          <p class="text-xs text-muted-foreground font-medium">{{ t('formateur.employees.totalUsers') }}</p>
          <p class="text-xl text-slate-800 text-slate-800 leading-none mt-0.5">
            {{ userStore.list.meta?.totalCount ?? '—' }}
          </p>
        </div>
      </div>
      <div class="px-4 py-3 rounded-xl border border-border bg-[#F5F4F2]  flex items-center gap-3">
        <div class="w-9 h-9 rounded-lg bg-[#F5F4F2]  dark:bg-emerald-900/40 flex items-center justify-center">
          <GraduationCap class="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
        </div>
        <div>
          <p class="text-xs text-muted-foreground font-medium">{{ t('users.roles.condidat') }}</p>
          <p class="text-xl text-slate-800 text-slate-800 leading-none mt-0.5">{{ candidatCount }}</p>
        </div>
      </div>
      <div class="px-4 py-3 rounded-xl border border-border bg-[#F5F4F2]  flex items-center gap-3">
        <div class="w-9 h-9 rounded-lg bg-[#F5F4F2]  dark:bg-violet-900/40 flex items-center justify-center">
          <BookOpen class="w-4 h-4 text-violet-600 dark:text-violet-400" />
        </div>
        <div>
          <p class="text-xs text-muted-foreground font-medium">{{ t('users.roles.formateur') }}</p>
          <p class="text-xl text-slate-800 text-slate-800 leading-none mt-0.5">{{ formateurCount }}</p>
        </div>
      </div>
    </div>

    <!-- Search + Role Filter -->
    <div class="flex flex-wrap gap-3 items-center">
      <div class="relative flex-1 min-w-[200px] max-w-sm">
        <Search class="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
        <Input
          v-model="searchQuery"
          :placeholder="t('companyPanel.employees.searchPlaceholder')"
          class="ps-9 h-9"
        />
      </div>
      <Select v-model="roleFilter">
        <SelectTrigger class="w-[180px] h-9">
          <SelectValue :placeholder="t('companyPanel.employees.allRoles')" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">{{ t('companyPanel.employees.allRoles') }}</SelectItem>
          <SelectItem :value="UserRole.Formateur">{{ t('users.roles.formateur') }}</SelectItem>
          <SelectItem :value="UserRole.Condidat">{{ t('users.roles.condidat') }}</SelectItem>
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
    <div
      v-else-if="mappedEmployees.length > 0"
      class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4 "
    >
      <EmployeeCard
        v-for="emp in mappedEmployees"
        :key="emp.id"
        :employee="emp"
        :show-company="false"
      >
        <template #actions>
          <!-- Only Candidat cards get actions (Formateur cards are view-only) -->
          <DropdownMenu v-if="emp.role === UserRole.Condidat">
            <DropdownMenuTrigger as-child>
              <button
                @click.stop
                class="p-1.5 rounded-lg text-muted-foreground hover:bg-muted transition-colors opacity-0 group-hover:opacity-100 focus:opacity-100 shrink-0"
              >
                <MoreHorizontal class="w-4 h-4" />
              </button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" class="w-40">
              <DropdownMenuLabel class="text-xs">{{ t('common.actions') }}</DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem class="gap-2 cursor-pointer">
                <Pencil class="w-3.5 h-3.5" /> {{ t('common.edit') }}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </template>
      </EmployeeCard>
    </div>

    <!-- Empty State -->
    <EmptyData
      v-else-if="userStore.list.hasData"
      :icon="UsersIcon"
      :title="t('companyPanel.employees.emptyTitle')"
      :description="t('companyPanel.employees.emptyDesc')"
    >
      <button
        v-if="searchQuery || roleFilter !== 'all'"
        @click="clearFilters"
        class="flex items-center gap-2 px-4 py-2 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors mt-2"
      >
        <X class="w-3.5 h-3.5" /> {{ t('common.clearFilters') }}
      </button>
    </EmptyData>

    <!-- Footer: 3-column — count | paginator | size-select -->
    <div v-if="userStore.list.meta" class="grid grid-cols-3 items-center gap-2">
      <p class="text-xs text-muted-foreground">
        {{ t('common.showing') }} {{ mappedEmployees.length }}
        {{ t('common.of') }} {{ userStore.list.meta.totalCount }}
        {{ t('formateur.employees.title').toLowerCase() }}
      </p>
      <div class="flex justify-center">
        <AppPaginator
          v-if="userStore.list.meta.totalPages > 1"
          :page="userStore.list.meta.currentPage"
          :pages="userStore.list.meta.totalPages"
          @page-change="currentPage = $event"
        />
      </div>
      <div class="flex justify-end">
        <Select
          :model-value="String(pageSize)"
          @update:model-value="(v) => { if (v) { pageSize = parseInt(v as string); resetPage() } }"
        >
          <SelectTrigger
            class="h-8 w-[110px] text-xs font-semibold bg-background border-border hover:border-foreground/30 transition-colors"
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
import AppPaginator     from '@/components/common/AppPaginator.vue'
import EmptyData        from '@/components/common/EmptyData.vue'
import EmployeeCard, { type EmployeeCardData } from '@/components/employee/EmployeeCard.vue'
import EmployeeCardShimmer from '@/components/employee/EmployeeCardShimmer.vue'
import AddEmployeeSheet    from '@/components/company/AddEmployeeSheet.vue'
import { useLocale }    from '@/composables/useLocale'
import { useTableQuery, VALID_SIZES as PAGE_SIZES } from '@/composables/useTableQuery'
import { useUserStore } from '@/store/userStore'
import type { UserListItem } from '@utils/models'
import {
  Search, UserPlus, MoreHorizontal, Pencil, X,
  Users as UsersIcon, Info, GraduationCap, BookOpen,
} from 'lucide-vue-next'
import { UserRole } from '@/utils/roles'
import Button from '@/components/ui/button/Button.vue'

const { t } = useLocale()
const { page: currentPage, size: pageSize, search: searchQuery, resetPage } = useTableQuery()
const roleFilter = ref('all')
const userStore  = useUserStore()

// ─── Data mapping ──────────────────────────────────────────────────────────

type Employee = EmployeeCardData

function toCardData(u: UserListItem): Employee {
  return {
    id:       u.id,
    name:     u.userName,
    email:    u.email,
    initials: u.userName.split(' ').filter(Boolean).map(n => n[0]).join('').toUpperCase().slice(0, 2),
    role:     u.role as UserRole,
    active:   u.isActive,
    imageUrl: u.imageUrl,
    company:  u.companyName ?? '',
  }
}

const mappedEmployees = computed<Employee[]>(() => userStore.list.currentItems.map(toCardData))

// Stats from current page items
const candidatCount  = computed(() => mappedEmployees.value.filter(e => e.role === UserRole.Condidat).length)
const formateurCount = computed(() => mappedEmployees.value.filter(e => e.role === UserRole.Formateur).length)

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

const isAddOpen = ref(false)

function clearFilters() {
  searchQuery.value = ''
  roleFilter.value  = 'all'
  resetPage()
}
</script>
