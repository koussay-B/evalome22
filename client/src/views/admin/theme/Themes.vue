<!-- ──────────────────────────────────────────────── -->

































<template>
  <div class="space-y-6">

    <!-- ── Detail Sheet ────────────────────────────────────────────── -->
    <Sheet v-model:open="isDetailOpen">
      <SheetContent :side="sheetSide" class="sm:max-w-md flex flex-col p-0">
        <template v-if="selectedTheme">
          <SheetHeader class="px-6 pt-6 pb-5 border-b border-border shrink-0">
            <div class="flex items-center gap-4">
              <div class="w-14 h-14 rounded-2xl bg-primary/10 text-primary flex items-center justify-center shrink-0">
                <Layers class="w-7 h-7" />
              </div>
              <div class="min-w-0 flex-1">
                <SheetTitle class="text-xl font-black leading-tight">{{ selectedTheme.name }}</SheetTitle>
                <SheetDescription class="mt-1 text-sm line-clamp-2">
                  {{ selectedTheme.description || t('themes.noDescription') }}
                </SheetDescription>
              </div>
            </div>
          </SheetHeader>

          <div class="flex-1 overflow-y-auto flex flex-col gap-6 p-6">
            <!-- Company badge -->
            <div class="space-y-2">
              <p class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('themes.company') }}</p>
              <div class="flex items-center gap-2 px-3 py-2.5 rounded-lg bg-muted/60 border border-border w-fit">
                <Building2 class="w-4 h-4 text-muted-foreground shrink-0" />
                <span class="text-sm font-semibold text-foreground">{{ companyName(selectedTheme.companyId) }}</span>
              </div>
            </div>

            <!-- Subthemes -->
            <div class="space-y-3">
              <div class="flex items-center justify-between">
                <p class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('themes.subthemes') }}</p>
                <span class="px-2 py-0.5 rounded-full bg-muted text-xs font-semibold text-muted-foreground">
                  {{ selectedTheme.children?.length ?? 0 }}
                </span>
              </div>
              <div v-if="selectedTheme.children?.length" class="flex flex-wrap gap-2">
                <button
                  v-for="sub in selectedTheme.children"
                  :key="sub.id"
                  class="group/sub px-3 py-1.5 text-sm font-medium rounded-lg bg-secondary text-secondary-foreground border border-border/50 hover:border-primary/40 hover:bg-primary/5 transition-all flex items-center gap-1.5"
                  @click="openSubthemeDetail(sub)"
                >
                  {{ sub.name }}
                  <ChevronRight class="w-3 h-3 opacity-0 group-hover/sub:opacity-100 transition-opacity text-primary" />
                </button>
              </div>
              <div v-else class="px-3 py-3 rounded-lg bg-muted/40 border border-dashed border-border">
                <p class="text-xs text-muted-foreground italic">{{ t('themes.noSubthemesYet') }}</p>
              </div>
            </div>

            <!-- Stats -->
            <div class="grid grid-cols-2 gap-3">
              <div class="rounded-xl border border-border bg-muted/40 p-4 text-center">
                <div class="text-2xl font-black">{{ selectedTheme.children?.length ?? 0 }}</div>
                <div class="text-xs text-muted-foreground mt-1">{{ t('themes.subthemes') }}</div>
              </div>
              <div class="rounded-xl border border-border bg-muted/40 p-4 text-center">
                <div class="text-2xl font-black">{{ selectedTheme.questions?.length ?? 0 }}</div>
                <div class="text-xs text-muted-foreground mt-1">Questions</div>
              </div>
            </div>
          </div>

          <div class="flex gap-3 px-6 py-5 border-t border-border shrink-0">
            <button
              class="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 transition-colors"
              @click="openEditFromDetail"
            >
              <Pencil class="w-4 h-4" /> {{ t('common.edit') }}
            </button>
            <button
              class="flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg border border-red-200 dark:border-red-900 text-sm font-semibold text-red-600 hover:bg-red-50 dark:hover:bg-red-950/30 transition-colors"
              @click="confirmDeleteFromDetail"
            >
              <Trash2 class="w-4 h-4" />
            </button>
          </div>
        </template>
      </SheetContent>
    </Sheet>

    <!-- ── Add / Edit Sheet ────────────────────────────────────────── -->
    <Sheet v-model:open="isSheetOpen">
      <SheetContent :side="sheetSide" class="sm:max-w-md flex flex-col p-0">
        <SheetHeader class="px-6 pt-6 pb-5 border-b border-border shrink-0">
          <SheetTitle class="text-xl font-black">
            {{ editingTheme ? 'Modifier l\'Affectation' : 'Affecter un nouveau Thème' }}
          </SheetTitle>
          <SheetDescription class="mt-1">
            {{ editingTheme ? 'Modifier les détails du thème pour cette entreprise.' : 'Assigner un thème à une entreprise spécifique.' }}
          </SheetDescription>
        </SheetHeader>

        <form class="flex flex-col flex-1 overflow-y-auto" @submit.prevent="handleSave">
          <div class="flex flex-col gap-6 p-6 flex-1">

            <!-- Name -->
            <div class="space-y-2">
              <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
                {{ t('themes.themeName') }} *
              </label>
              <div class="relative">
                <Layers class="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                <Input v-model="form.name" :placeholder="t('themes.themeNamePlaceholder')" class="ps-9 h-10" required :disabled="saving" />
              </div>
            </div>

            <!-- Description -->
            <div class="space-y-2">
              <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
                {{ t('themes.themeDesc') }}
              </label>
              <Input v-model="form.description" :placeholder="t('themes.themeDescPlaceholder')" class="h-10" :disabled="saving" />
            </div>

            <!-- Company picker (create only — cannot change company on edit) -->
            <div class="space-y-2">
              <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
                {{ t('themes.company') }} *
              </label>
              <button
                v-if="!editingTheme"
                type="button"
                class="w-full h-10 flex items-center gap-2.5 px-3 rounded-md border border-input bg-background text-sm hover:bg-muted/40 transition-colors text-start"
                :class="!form.selectedCompany ? 'text-muted-foreground' : 'text-foreground'"
                :disabled="saving"
                @click="isCompanyPickerOpen = true"
              >
                <template v-if="form.selectedCompany">
                  <div class="w-5 h-5 rounded bg-muted border border-border flex items-center justify-center overflow-hidden shrink-0">
                    <img v-if="form.selectedCompany.logo" :src="form.selectedCompany.logo" class="w-full h-full object-cover" alt="" />
                    <Building2 v-else class="w-3 h-3 text-muted-foreground" />
                  </div>
                  <span class="font-medium truncate flex-1">{{ form.selectedCompany.name }}</span>
                </template>
                <template v-else>
                  <Building2 class="w-4 h-4 shrink-0" />
                  <span class="flex-1">{{ t('themes.selectCompany') }}</span>
                </template>
                <ChevronsUpDown class="w-3.5 h-3.5 text-muted-foreground ms-auto shrink-0" />
              </button>
              <!-- When editing, show locked company -->
              <div
                v-else
                class="w-full h-10 flex items-center gap-2.5 px-3 rounded-md border border-border bg-muted/40 text-sm opacity-70 cursor-not-allowed"
              >
                <Building2 class="w-4 h-4 text-muted-foreground shrink-0" />
                <span class="truncate">{{ companyName(editingTheme.companyId) }}</span>
              </div>

              <CompanyPickerDialog
                v-model:open="isCompanyPickerOpen"
                :model-value="form.selectedCompany"
                @update:model-value="form.selectedCompany = $event"
              />
            </div>

            <!-- Subthemes -->
            <div class="space-y-3 pt-5 border-t border-border">
              <div class="flex items-center justify-between">
                <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
                  {{ t('themes.subthemes') }}
                </label>
                <button
                  type="button"
                  class="text-xs flex items-center gap-1 font-semibold text-primary hover:text-primary/80 transition-colors"
                  :disabled="saving"
                  @click="addSubtheme"
                >
                  <Plus class="w-3.5 h-3.5" /> {{ t('common.add') }}
                </button>
              </div>

              <!-- Existing subthemes -->
              <div v-if="existingSubthemes.length" class="space-y-2">
                <p class="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground">Existing</p>
                <div v-for="sub in existingSubthemes" :key="sub.id" class="flex items-center gap-2">
                  <Input v-model="sub.name" class="h-9 text-sm" :disabled="saving" required />
                  <button
                    type="button"
                    class="p-1.5 rounded-md text-muted-foreground hover:bg-red-50 hover:text-red-600 dark:hover:bg-red-950 dark:hover:text-red-400 transition-colors shrink-0"
                    :disabled="saving"
                    @click="markSubthemeDeleted(sub)"
                  >
                    <X class="w-4 h-4" />
                  </button>
                </div>
              </div>

              <!-- New subthemes -->
              <div v-if="newSubthemes.length" class="space-y-2">
                <p v-if="existingSubthemes.length" class="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground">New</p>
                <div v-for="(sub, i) in newSubthemes" :key="i" class="flex items-center gap-2">
                  <Input v-model="sub.name" :placeholder="t('themes.subthemePlaceholder')" class="h-9 text-sm" :disabled="saving" required />
                  <button
                    type="button"
                    class="p-1.5 rounded-md text-muted-foreground hover:bg-red-50 hover:text-red-600 dark:hover:bg-red-950 dark:hover:text-red-400 transition-colors shrink-0"
                    :disabled="saving"
                    @click="newSubthemes.splice(i, 1)"
                  >
                    <X class="w-4 h-4" />
                  </button>
                </div>
              </div>

              <p v-if="!existingSubthemes.length && !newSubthemes.length" class="text-xs text-muted-foreground italic py-1">
                {{ t('themes.noSubthemes') }}
              </p>
            </div>

            <p v-if="formError" class="text-sm text-red-500 font-medium">{{ formError }}</p>
          </div>

          <div class="flex gap-3 px-6 py-5 border-t border-border shrink-0">
            <button
              type="submit"
              class="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 transition-colors disabled:opacity-60"
              :disabled="saving"
            >
              <Loader2 v-if="saving" class="w-4 h-4 animate-spin" />
              <Save v-else class="w-4 h-4" />
              {{ saving ? t('common.saving') : t('common.save') }}
            </button>
            <button
              type="button"
              class="px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-foreground hover:bg-muted/50 transition-colors"
              :disabled="saving"
              @click="isSheetOpen = false"
            >
              {{ t('common.cancel') }}
            </button>
          </div>
        </form>
      </SheetContent>
    </Sheet>

    <!-- ── Delete Dialog ───────────────────────────────────────────── -->
    <AlertDialog v-model:open="isDeleteOpen">
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{{ t('themes.deleteConfirmTitle') }}</AlertDialogTitle>
          <AlertDialogDescription>{{ t('themes.deleteConfirmDesc') }}</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel :disabled="deleting">{{ t('common.cancel') }}</AlertDialogCancel>
          <AlertDialogAction
            class="bg-red-600 hover:bg-red-700 text-white border-none"
            :disabled="deleting"
            @click="handleDelete"
          >
            <Loader2 v-if="deleting" class="w-3.5 h-3.5 animate-spin me-1" />
            {{ t('common.delete') }}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>

    <!-- ── Page Header ─────────────────────────────────────────────── -->
    <div class="flex items-end justify-between gap-4">
      <div>
        <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground mb-1">
          AFFECTATION
        </p>
        <h1 class="text-3xl font-black tracking-tight text-foreground">Affecter des Thèmes</h1>
      </div>
      <button
        class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 transition-colors shrink-0"
        @click="openAddSheet"
      >
        <Plus class="w-4 h-4" /> Affecter un Thème
      </button>
    </div>

    <!-- ── Search & Company filter ─────────────────────────────────── -->
    <div class="flex flex-wrap gap-3 items-center">
      <div class="relative flex-1 min-w-[200px] max-w-sm">
        <Search class="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
        <Input v-model="searchQuery" :placeholder="t('themes.searchPlaceholder')" class="ps-9 h-9" />
      </div>
      <Select v-model="filterCompanyId">
        <SelectTrigger class="w-[200px] h-9">
          <SelectValue :placeholder="t('themes.allCompanies')" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">{{ t('themes.allCompanies') }}</SelectItem>
          <SelectItem
            v-for="c in companyStore.list.currentItems"
            :key="c.id"
            :value="String(c.id)"
          >
            {{ c.name }}
          </SelectItem>
        </SelectContent>
      </Select>
    </div>

    <!-- ── Loading ─────────────────────────────────────────────────── -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <div v-for="i in 6" :key="i" class="h-40 rounded-xl border border-border bg-secondary animate-pulse" />
    </div>

    <!-- ── Error ───────────────────────────────────────────────────── -->
    <div
      v-else-if="loadError"
      class="flex items-center gap-3 px-4 py-3 rounded-xl bg-red-50 dark:bg-red-950/20 border border-red-200 dark:border-red-900"
    >
      <AlertCircle class="w-4 h-4 text-red-500 shrink-0" />
      <p class="text-sm text-red-600 dark:text-red-400">{{ loadError }}</p>
      <button class="ms-auto text-xs font-semibold text-red-600 hover:underline" @click="loadData">Retry</button>
    </div>

    <!-- ── Themes grouped by company ──────────────────────────────── -->
    <div v-else-if="groupedThemes.length" class="space-y-10">
      <div v-for="group in groupedThemes" :key="group.companyId" class="space-y-4">

        <!-- Company header -->
        <div class="flex items-center gap-3 pb-2 border-b border-border">
          <div class="w-8 h-8 rounded-lg bg-muted border border-border flex items-center justify-center overflow-hidden shrink-0">
            <img v-if="group.logo" :src="group.logo" class="w-full h-full object-cover" alt="" />
            <Building2 v-else class="w-4 h-4 text-muted-foreground" />
          </div>
          <h2 class="text-lg font-bold text-foreground">{{ group.companyName }}</h2>
          <span class="ms-auto px-2 py-0.5 rounded-md bg-muted text-xs font-semibold text-muted-foreground">
            {{ group.themes.length }} {{ t('themes.title').toLowerCase() }}
          </span>
        </div>

        <!-- Cards -->
        <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
          <div
            v-for="theme in group.themes"
            :key="theme.id"
            class="p-5 rounded-xl border border-border bg-secondary hover:border-primary/30 hover:shadow-sm transition-all flex flex-col group cursor-pointer relative"
            @click="openDetailSheet(theme)"
          >
            <!-- Actions -->
            <div class="absolute top-3 end-3 opacity-0 group-hover:opacity-100 focus-within:opacity-100 transition-opacity z-10">
              <DropdownMenu>
                <DropdownMenuTrigger as-child>
                  <button class="p-1.5 rounded-lg text-muted-foreground hover:bg-muted transition-colors" @click.stop>
                    <MoreHorizontal class="w-4 h-4" />
                  </button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" class="w-40">
                  <DropdownMenuLabel class="text-xs">{{ t('common.actions') }}</DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem class="gap-2 cursor-pointer" @click.stop="openEditSheet(theme)">
                    <Pencil class="w-3.5 h-3.5" /> {{ t('common.edit') }}
                  </DropdownMenuItem>
                  <DropdownMenuItem
                    class="gap-2 cursor-pointer text-red-600 focus:text-red-600 focus:bg-red-50 dark:focus:bg-red-950/30"
                    @click.stop="confirmDelete(theme)"
                  >
                    <Trash2 class="w-3.5 h-3.5" /> {{ t('common.delete') }}
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </div>

            <!-- Card header -->
            <div class="flex items-start gap-3 mb-4 pe-8">
              <div class="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center shrink-0">
                <Layers class="w-5 h-5" />
              </div>
              <div class="min-w-0">
                <h3 class="font-bold text-foreground truncate">{{ theme.name }}</h3>
                <p class="text-xs text-muted-foreground line-clamp-1">{{ theme.description || t('themes.noDescription') }}</p>
              </div>
            </div>

            <!-- Subthemes -->
            <div class="mt-auto pt-3 border-t border-border/60 space-y-2">
              <div class="text-[10px] font-bold uppercase tracking-widest text-muted-foreground flex items-center justify-between">
                <span>{{ t('themes.subthemes') }}</span>
                <span v-if="theme.children?.length" class="px-1.5 rounded-full bg-muted">{{ theme.children.length }}</span>
              </div>
              <div v-if="theme.children?.length" class="flex flex-wrap gap-1.5">
                <span
                  v-for="sub in theme.children.slice(0, 4)"
                  :key="sub.id"
                  class="px-2 py-0.5 text-xs font-medium rounded-md bg-background text-secondary-foreground border border-border/50"
                >{{ sub.name }}</span>
                <span
                  v-if="theme.children.length > 4"
                  class="px-2 py-0.5 text-xs font-medium rounded-md bg-muted text-muted-foreground"
                >+{{ theme.children.length - 4 }}</span>
              </div>
              <p v-else class="text-xs text-muted-foreground italic">{{ t('themes.noSubthemesYet') }}</p>
            </div>
          </div>
        </div>

      </div>
    </div>

    <!-- ── Empty ───────────────────────────────────────────────────── -->
    <EmptyData
      v-else-if="!loading"
      :icon="FolderOpen"
      :title="t('themes.emptyTitle')"
      :description="t('themes.emptyDesc')"
    >
      <button
        v-if="searchQuery || filterCompanyId !== 'all'"
        class="flex items-center gap-2 px-4 py-2 rounded-lg border border-border text-sm font-semibold text-foreground hover:bg-muted/50 transition-colors mt-2"
        @click="searchQuery = ''; filterCompanyId = 'all'"
      >
        <X class="w-3.5 h-3.5" /> {{ t('common.clearFilters') }}
      </button>
    </EmptyData>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { Input } from '@/components/ui/input'
import {
  DropdownMenu, DropdownMenuTrigger, DropdownMenuContent,
  DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator,
} from '@/components/ui/dropdown-menu'
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from '@/components/ui/select'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet'
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent,
  AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import EmptyData from '@/components/common/EmptyData.vue'
import CompanyPickerDialog from '@/components/company/CompanyPickerDialog.vue'
import { useCompanyStore } from '@/store/companyStore'
import { useLocale } from '@/composables/useLocale'
import type { Company, ThemeItem } from '@utils/models'
import {
  getAdminThemesApi,
  adminCreateThemeApi,
  adminUpdateThemeApi,
  adminDeleteThemeApi,
} from '@utils/api'
import {
  Layers, FolderOpen, Plus, Search, MoreHorizontal, Pencil, Trash2,
  Building2, X, Save, Loader2, AlertCircle, ChevronRight, ChevronsUpDown,
} from 'lucide-vue-next'

const { t, locale }  = useLocale()
const companyStore   = useCompanyStore()

const isRtl      = computed(() => locale.value === 'ar')
const sheetSide  = computed<'right' | 'left'>(() => isRtl.value ? 'left' : 'right')

// ── Data state ───────────────────────────────────────────────────────────────
const themes    = ref<ThemeItem[]>([])
const loading   = ref(false)
const loadError = ref<string | null>(null)

// ── Filter state ─────────────────────────────────────────────────────────────
const searchQuery    = ref('')
const filterCompanyId = ref('all')

// ── Sheet / Dialog state ──────────────────────────────────────────────────────
const isDetailOpen        = ref(false)
const isSheetOpen         = ref(false)
const isDeleteOpen        = ref(false)
const isCompanyPickerOpen = ref(false)

const selectedTheme = ref<ThemeItem | null>(null)
const editingTheme  = ref<ThemeItem | null>(null)
const deletingTheme = ref<ThemeItem | null>(null)

// ── Form ─────────────────────────────────────────────────────────────────────
interface SubEdit { id: number; name: string }
interface SubNew  { name: string }

const form = reactive<{
  name: string
  description: string
  selectedCompany: Company | null
}>({ name: '', description: '', selectedCompany: null })

const existingSubthemes  = ref<SubEdit[]>([])
const newSubthemes       = ref<SubNew[]>([])
const deletedSubIds      = ref<number[]>([])
const saving             = ref(false)
const formError          = ref<string | null>(null)
const deleting           = ref(false)

// ── Helpers ──────────────────────────────────────────────────────────────────
function companyName(id: number): string {
  return companyStore.list.currentItems.find(c => c.id === id)?.name ?? `Company #${id}`
}

function companyLogo(id: number): string | null | undefined {
  return companyStore.list.currentItems.find(c => c.id === id)?.logo
}

// ── Computed: grouped & filtered ─────────────────────────────────────────────
const groupedThemes = computed(() => {
  let list = themes.value

  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(
      t =>
        t.name.toLowerCase().includes(q) ||
        (t.description ?? '').toLowerCase().includes(q) ||
        t.children?.some(c => c.name.toLowerCase().includes(q)),
    )
  }

  if (filterCompanyId.value !== 'all') {
    list = list.filter(t => String(t.companyId) === filterCompanyId.value)
  }

  // Group by companyId
  const map = new Map<number, { companyId: number; companyName: string; logo?: string | null; themes: ThemeItem[] }>()
  for (const theme of list) {
    if (!map.has(theme.companyId)) {
      map.set(theme.companyId, {
        companyId:   theme.companyId,
        companyName: companyName(theme.companyId),
        logo:        companyLogo(theme.companyId),
        themes:      [],
      })
    }
    map.get(theme.companyId)!.themes.push(theme)
  }
  return Array.from(map.values()).sort((a, b) => a.companyName.localeCompare(b.companyName))
})

// ── Load ──────────────────────────────────────────────────────────────────────
async function loadData() {
  loading.value   = true
  loadError.value = null
  const { data, error } = await getAdminThemesApi()
  if (error || !data) {
    loadError.value = error ?? 'Failed to load themes'
  } else {
    themes.value = data
  }
  loading.value = false
}

onMounted(async () => {
  // Load companies for filter dropdown + name resolution
  if (!companyStore.list.hasData) {
    companyStore.list.fetchPage({ pageSize: 200 })
  }
  await loadData()
})

// ── Detail sheet ─────────────────────────────────────────────────────────────
function openDetailSheet(theme: ThemeItem) {
  selectedTheme.value = theme
  isDetailOpen.value  = true
}

function openEditFromDetail() {
  if (!selectedTheme.value) return
  isDetailOpen.value = false
  openEditSheet(selectedTheme.value)
}

function confirmDeleteFromDetail() {
  if (!selectedTheme.value) return
  isDetailOpen.value = false
  confirmDelete(selectedTheme.value)
}

function openSubthemeDetail(sub: ThemeItem) {
  selectedTheme.value = sub
  isDetailOpen.value  = true
}

// ── Add / Edit sheet ─────────────────────────────────────────────────────────
function resetForm() {
  form.name            = ''
  form.description     = ''
  form.selectedCompany = null
  existingSubthemes.value  = []
  newSubthemes.value       = []
  deletedSubIds.value      = []
  formError.value          = null
}

function openAddSheet() {
  editingTheme.value = null
  resetForm()
  isSheetOpen.value = true
}

function openEditSheet(theme: ThemeItem) {
  editingTheme.value   = theme
  form.name            = theme.name
  form.description     = theme.description ?? ''
  form.selectedCompany = null // locked — shown from companyName()
  existingSubthemes.value  = (theme.children ?? []).map(c => ({ id: c.id, name: c.name }))
  newSubthemes.value       = []
  deletedSubIds.value      = []
  formError.value          = null
  isSheetOpen.value        = true
}

function addSubtheme() { newSubthemes.value.push({ name: '' }) }

function markSubthemeDeleted(sub: SubEdit) {
  deletedSubIds.value.push(sub.id)
  existingSubthemes.value = existingSubthemes.value.filter(s => s.id !== sub.id)
}

// ── Save ──────────────────────────────────────────────────────────────────────
async function handleSave() {
  formError.value = null

  if (editingTheme.value) {
    await doUpdate()
  } else {
    await doCreate()
  }
}

async function doCreate() {
  if (!form.selectedCompany) { formError.value = 'Please select a company.'; return }
  saving.value = true

  const { data: created, error } = await adminCreateThemeApi({
    name:        form.name.trim(),
    description: form.description.trim() || null,
    companyId:   form.selectedCompany.id,
  })

  if (error || !created) { formError.value = error ?? 'Failed to create'; saving.value = false; return }

  // Create new subthemes
  for (const sub of newSubthemes.value) {
    if (!sub.name.trim()) continue
    await adminCreateThemeApi({
      name:      sub.name.trim(),
      companyId: created.companyId,
      parentId:  created.id,
    })
  }

  saving.value      = false
  isSheetOpen.value = false
  await loadData()
}

async function doUpdate() {
  if (!editingTheme.value) return
  saving.value = true
  const theme  = editingTheme.value

  const { error } = await adminUpdateThemeApi(theme.id, {
    name:      form.name.trim(),
    description: form.description.trim() || null,
    parentId:  theme.parentId ?? null,
  })

  if (error) { formError.value = error; saving.value = false; return }

  // Update changed existing subthemes
  for (const sub of existingSubthemes.value) {
    const original = theme.children?.find(c => c.id === sub.id)
    if (original && original.name !== sub.name && sub.name.trim()) {
      await adminUpdateThemeApi(sub.id, { name: sub.name.trim(), parentId: theme.id })
    }
  }

  // Delete removed subthemes
  for (const delId of deletedSubIds.value) {
    await adminDeleteThemeApi(delId)
  }

  // Create new subthemes
  for (const sub of newSubthemes.value) {
    if (!sub.name.trim()) continue
    await adminCreateThemeApi({ name: sub.name.trim(), companyId: theme.companyId, parentId: theme.id })
  }

  saving.value      = false
  isSheetOpen.value = false
  await loadData()
}

// ── Delete ────────────────────────────────────────────────────────────────────
function confirmDelete(theme: ThemeItem) {
  deletingTheme.value = theme
  isDeleteOpen.value  = true
}

async function handleDelete() {
  if (!deletingTheme.value) return
  deleting.value = true
  const { error } = await adminDeleteThemeApi(deletingTheme.value.id)
  deleting.value    = false
  isDeleteOpen.value = false

  if (error) {
    loadError.value = error
  } else {
    await loadData()
  }
}
</script>
