<template>
  <div class="space-y-6">

    <!-- ── Detail Sheet (theme or subtheme) ───────────────────────── -->
    <ThemeDetailSheet
      v-model:open="isDetailOpen"
      :theme="selectedItem"
      :parent-name="selectedParentName"
      :current-user-id="currentUserId"
      :can-edit="!!selectedItem"
      :can-delete="!!selectedItem"
      :is-subtheme="selectedIsSubtheme"
      @edit="openEdit"
      @delete="confirmDelete"
      @subtheme-click="openSubthemeDetail"
      @add-questionnaire="handleAddQuestionnaire"
    />

    <!-- ── Form Sheet ──────────────────────────────────────────────── -->
    <ThemeFormSheet
      v-model:open="isFormOpen"
      :theme="editingItem"
      :parent-id="formParentId"
      :is-subtheme="formIsSubtheme"
      :can-delete-subthemes="true"
      @saved="onSaved"
    />

    <!-- ── Delete Dialog ───────────────────────────────────────────── -->
    <AlertDialog v-model:open="isDeleteOpen">
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{{ t('themes.deleteConfirmTitle') }}</AlertDialogTitle>
          <AlertDialogDescription>
            {{ deletingIsSubtheme ? t('themes.deleteSubthemeWarn') : t('themes.deleteThemeWarn') }}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel @click="isDeleteOpen = false">{{ t('common.cancel') }}</AlertDialogCancel>
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
        <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">{{ t('companyPanel.themes.subtitle') }}</p>
        <h1 class="text-3xl text-slate-800 tracking-tight text-slate-800">{{ t('companyPanel.themes.title') }}</h1>
      </div>
      <button
        class="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 shadow-[0_2px_12px_0_oklch(0.58_0.21_42/0.18)] hover:shadow-[0_4px_20px_0_oklch(0.58_0.21_42/0.28)] transition-all shrink-0"
        @click="openCreate"
      >
        <Plus class="w-4 h-4" /> {{ t('companyPanel.themes.addTheme') }}
      </button>
    </div>

    <div class="grid grid-cols-3 gap-4">
  
  <div class="rounded-2xl border border-[#D6D3D1]/50 bg-[#F1EFEC] p-4 flex items-center gap-3 shadow-[0_2px_8px_rgba(0,0,0,0.02)]">
    <div class="h-10 w-10 rounded-xl bg-orange-100/50 flex items-center justify-center shrink-0">
      <Layers class="w-4.5 h-4.5 text-orange-600" />
    </div>
    <div>
      <div class="text-xl text-slate-800 text-slate-800">{{ store.themes.length }}</div>
      <div class="text-xs text-slate-500 font-medium">{{ t('themes.statThemes') }}</div>
    </div>
  </div>

  <div class="rounded-2xl border border-[#D6D3D1]/50 bg-[#F1EFEC] p-4 flex items-center gap-3 shadow-[0_2px_8px_rgba(0,0,0,0.02)]">
    <div class="h-10 w-10 rounded-xl bg-orange-100/50 flex items-center justify-center shrink-0">
      <FolderTree class="w-4.5 h-4.5 text-orange-600" />
    </div>
    <div>
      <div class="text-xl text-slate-800 text-slate-800">{{ store.totalSubthemes }}</div>
      <div class="text-xs text-slate-500 font-medium">{{ t('themes.statSubthemes') }}</div>
    </div>
  </div>

  <div class="rounded-2xl border border-[#D6D3D1]/50 bg-[#F1EFEC] p-4 flex items-center gap-3 shadow-[0_2px_8_rgba(0,0,0,0.02)]">
    <div class="h-10 w-10 rounded-xl bg-orange-100/50 flex items-center justify-center shrink-0">
      <BookOpen class="w-4.5 h-4.5 text-orange-600" />
    </div>
    <div>
      <div class="text-xl text-slate-800 text-slate-800">{{ store.totalQuestions }}</div>
      <div class="text-xs text-slate-500 font-medium">{{ t('themes.statQuestions') }}</div>
    </div>
  </div>
</div>

    <!-- ── Search ──────────────────────────────────────────────────── -->
    <div class="relative max-w-sm">
      <Search class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
      <Input
        v-model="searchQuery"
        :placeholder="t('companyPanel.tests.searchPlaceholder')"
        class="ps-9 h-9 bg-card border-border focus-visible:ring-primary/30"
      />
    </div>

    <!-- ── Loading ─────────────────────────────────────────────────── -->
    <div v-if="store.loading" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <div
        v-for="i in 6"
        :key="i"
        class="h-40 rounded-2xl border border-border bg-card animate-pulse"
      />
    </div>

    <!-- ── Error ───────────────────────────────────────────────────── -->
    <div
      v-else-if="store.error"
      class="flex items-center gap-3 px-4 py-3 rounded-2xl bg-red-50 dark:bg-red-950/20 border border-red-200 dark:border-red-900"
    >
      <AlertCircle class="w-4 h-4 text-red-500 shrink-0" />
      <p class="text-sm text-red-600 dark:text-red-400">{{ store.error }}</p>
      <button
        class="ms-auto text-xs font-semibold text-red-600 hover:underline"
        @click="store.fetchThemes()"
      >
        {{ t('common.retry') }}
      </button>
    </div>

    <!-- ── Themes Grid ─────────────────────────────────────────────── -->
    <div v-else-if="filteredThemes.length > 0" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <ThemeCard
        v-for="theme in filteredThemes"
        :key="theme.id"
        :theme="theme"
        :current-user-id="currentUserId"
        :can-edit="true"
        :can-delete="true"
        @click="openDetail(theme)"
        @edit="openEdit(theme)"
        @delete="confirmDelete(theme)"
        @subtheme-click="openSubthemeDetail"
      />
    </div>

    <!-- ── Empty State ─────────────────────────────────────────────── -->
    <EmptyData
      v-else-if="!store.loading"
      :icon="FolderOpen"
      :title="t('companyPanel.themes.emptyTitle')"
      :description="searchQuery ? t('companyPanel.themes.emptyFilterDesc') : t('companyPanel.themes.emptyDesc')"
    >
      <button
        v-if="searchQuery"
        class="flex items-center gap-2 px-4 py-2 rounded-xl border border-border text-sm font-semibold text-slate-800 hover:bg-primary/5 hover:border-primary/30 hover:text-primary transition-colors mt-2"
        @click="searchQuery = ''"
      >
        <X class="w-3.5 h-3.5" /> {{ t('themes.clearSearch') }}
      </button>
      <button
        v-else
        class="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 shadow-[0_2px_12px_0_oklch(0.58_0.21_42/0.18)] transition-all mt-2"
        @click="openCreate"
      >
        <Plus class="w-4 h-4" /> {{ t('companyPanel.themes.addTheme') }}
      </button>
    </EmptyData>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Input } from '@/components/ui/input'
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent,
  AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import EmptyData from '@/components/common/EmptyData.vue'
import ThemeCard from '@/components/theme/ThemeCard.vue'
import ThemeDetailSheet from '@/components/theme/ThemeDetailSheet.vue'
import ThemeFormSheet from '@/components/theme/ThemeFormSheet.vue'
import { useTopicStore } from '@/store/topicStore'
import { useAuthStore } from '@/store/authStore'
import { useLocale } from '@/composables/useLocale'
import type { ThemeItem } from '@utils/models'
import {
  Layers, FolderTree, BookOpen, FolderOpen, Plus, Search, X, AlertCircle, Loader2,
} from 'lucide-vue-next'

const store     = useTopicStore()
const auth      = useAuthStore()
const router    = useRouter()
const { t }     = useLocale()

const currentUserId = computed(() => auth.state.user?.id ?? undefined)
const searchQuery   = ref('')

// ── Sheet / dialog state ────────────────────────────────────────────────────
const isDetailOpen      = ref(false)
const isFormOpen        = ref(false)
const isDeleteOpen      = ref(false)
const deleting          = ref(false)

const selectedItem      = ref<ThemeItem | null>(null)
const selectedIsSubtheme = ref(false)
const selectedParentName = ref<string | undefined>()

const editingItem       = ref<ThemeItem | null>(null)
const formParentId      = ref<number | null>(null)
const formIsSubtheme    = ref(false)

const deletingItem      = ref<ThemeItem | null>(null)
const deletingParentId  = ref<number | null>(null)
const deletingIsSubtheme = ref(false)

// ── Computed ────────────────────────────────────────────────────────────────
const filteredThemes = computed(() => {
  if (!searchQuery.value) return store.themes
  const q = searchQuery.value.toLowerCase()
  return store.themes.filter(
    t =>
      t.name.toLowerCase().includes(q) ||
      (t.description ?? '').toLowerCase().includes(q) ||
      t.children?.some(c => c.name.toLowerCase().includes(q)),
  )
})

// ── Lifecycle ───────────────────────────────────────────────────────────────
onMounted(() => {
  if (!store.themes.length) store.fetchThemes()
})

// ── Detail ──────────────────────────────────────────────────────────────────
function openDetail(theme: ThemeItem) {
  selectedItem.value      = theme
  selectedIsSubtheme.value = false
  selectedParentName.value = undefined
  isDetailOpen.value      = true
}

function openSubthemeDetail(sub: ThemeItem) {
  const parent = store.themes.find(t => t.children?.some(c => c.id === sub.id))
  selectedItem.value      = sub
  selectedIsSubtheme.value = true
  selectedParentName.value = parent?.name
  isDetailOpen.value      = true
}

// ── Create / Edit ───────────────────────────────────────────────────────────
function openCreate() {
  editingItem.value   = null
  formParentId.value  = null
  formIsSubtheme.value = false
  isDetailOpen.value  = false
  isFormOpen.value    = true
}

function openEdit(theme: ThemeItem) {
  editingItem.value    = theme
  formIsSubtheme.value = selectedIsSubtheme.value
  formParentId.value   = theme.parentId ?? null
  isDetailOpen.value   = false
  isFormOpen.value     = true
}

function onSaved() {
  // store already updated optimistically; optionally refetch for full sync
  store.fetchThemes()
}

// ── Delete ──────────────────────────────────────────────────────────────────
function confirmDelete(theme: ThemeItem) {
  deletingItem.value       = theme
  deletingIsSubtheme.value = selectedIsSubtheme.value
  deletingParentId.value   = theme.parentId ?? null
  isDetailOpen.value       = false
  isDeleteOpen.value       = true
}

async function handleDelete() {
  if (!deletingItem.value) return
  deleting.value = true

  const { error } = deletingIsSubtheme.value && deletingParentId.value != null
    ? await store.deleteSubTheme(deletingItem.value.id, deletingParentId.value)
    : await store.deleteTheme(deletingItem.value.id)

  deleting.value     = false
  isDeleteOpen.value = false

  if (error) {
    store.error = error ?? null
  }
}

// ── Questionnaire ───────────────────────────────────────────────────────────
function handleAddQuestionnaire(theme: ThemeItem) {
  router.push({ path: '/company/questionnaires', query: { themeId: theme.id } })
}
</script>