<template>
  <div class="space-y-6">

    <!-- ── Detail Sheet (theme or subtheme) ───────────────────────── -->
    <ThemeDetailSheet
      v-model:open="isDetailOpen"
      :theme="selectedItem"
      :parent-name="selectedParentName"
      :current-user-id="currentUserId"
      :show-mine-badge="true"
      :can-edit="selectedCanEdit"
      :can-delete="false"
      :is-subtheme="selectedIsSubtheme"
      :show-permission-note="!selectedCanEdit"
      @edit="openEdit"
      @subtheme-click="openSubthemeDetail"
      @add-questionnaire="handleAddQuestionnaire"
    />

    <!-- ── Form Sheet ──────────────────────────────────────────────── -->
    <ThemeFormSheet
      v-model:open="isFormOpen"
      :theme="editingItem"
      :parent-id="formParentId"
      :is-subtheme="formIsSubtheme"
      :can-delete-subthemes="false"
      @saved="onSaved"
    />

    <!-- ── Page Header ─────────────────────────────────────────────── -->
    <div class="flex items-end justify-between gap-4">
      <div>
        <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground mb-1">{{ t('formateur.themes.subtitle') }}</p>
        <h1 class="text-3xl font-black tracking-tight text-foreground">{{ t('formateur.themes.title') }}</h1>
      </div>
      <button
        class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors shrink-0"
        @click="openCreate"
      >
        <Plus class="w-4 h-4" /> {{ t('formateur.themes.addTheme') }}
      </button>
    </div>

    <!-- ── Permission Banner ───────────────────────────────────────── -->
    <div class="flex items-start gap-3 px-4 py-3 rounded-xl border border-border/60 bg-muted/30">
      <Info class="w-4 h-4 text-muted-foreground shrink-0 mt-0.5" />
      <p class="text-sm text-muted-foreground leading-relaxed" v-html="t('formateur.themes.permissionNote')"></p>
    </div>

    <!-- ── Stats Bar ───────────────────────────────────────────────── -->
    <div class="grid grid-cols-3 gap-4">
      <div class="rounded-xl border border-border bg-secondary p-4 flex items-center gap-3">
        <div class="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
          <Layers class="w-4 h-4 text-primary" />
        </div>
        <div>
          <div class="text-xl font-black">{{ store.themes.length }}</div>
          <div class="text-xs text-muted-foreground">{{ t('themes.statThemes') }}</div>
        </div>
      </div>
      <div class="rounded-xl border border-border bg-secondary p-4 flex items-center gap-3">
        <div class="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
          <FolderTree class="w-4 h-4 text-primary" />
        </div>
        <div>
          <div class="text-xl font-black">{{ store.totalSubthemes }}</div>
          <div class="text-xs text-muted-foreground">{{ t('themes.statSubthemes') }}</div>
        </div>
      </div>
      <div class="rounded-xl border border-border bg-secondary p-4 flex items-center gap-3">
        <div class="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
          <UserCheck class="w-4 h-4 text-primary" />
        </div>
        <div>
          <div class="text-xl font-black">{{ myThemesCount }}</div>
          <div class="text-xs text-muted-foreground">{{ t('formateur.themes.statCreatedByMe') }}</div>
        </div>
      </div>
    </div>

    <!-- ── Tabs: All / Mine ────────────────────────────────────────── -->
    <div class="flex items-center gap-1 p-1 rounded-lg bg-muted/50 border border-border w-fit">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        :class="[
          'px-4 py-1.5 rounded-md text-sm font-semibold transition-all',
          activeTab === tab.key
            ? 'bg-background text-foreground shadow-sm border border-border/60'
            : 'text-muted-foreground hover:text-foreground',
        ]"
        @click="activeTab = tab.key"
      >
        {{ tab.label }}
        <span class="ms-1.5 px-1.5 py-0.5 rounded-full text-[10px] bg-muted text-muted-foreground font-bold">
          {{ tab.count }}
        </span>
      </button>
    </div>

    <!-- ── Search ──────────────────────────────────────────────────── -->
    <div class="relative max-w-sm">
      <Search class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
      <Input
        v-model="searchQuery"
        :placeholder="t('companyPanel.tests.searchPlaceholder')"
        class="ps-9 h-9"
      />
    </div>

    <!-- ── Loading ─────────────────────────────────────────────────── -->
    <div v-if="store.loading" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <div v-for="i in 6" :key="i" class="h-40 rounded-xl border border-border bg-secondary animate-pulse" />
    </div>

    <!-- ── Error ───────────────────────────────────────────────────── -->
    <div
      v-else-if="store.error"
      class="flex items-center gap-3 px-4 py-3 rounded-xl bg-red-50 dark:bg-red-950/20 border border-red-200 dark:border-red-900"
    >
      <AlertCircle class="w-4 h-4 text-red-500 shrink-0" />
      <p class="text-sm text-red-600 dark:text-red-400">{{ store.error }}</p>
      <button class="ms-auto text-xs font-semibold text-red-600 hover:underline" @click="store.fetchThemes()">
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
        :show-mine-badge="true"
        :can-edit="isMyTheme(theme)"
        :can-delete="false"
        @click="openDetail(theme)"
        @edit="openEdit(theme)"
        @subtheme-click="openSubthemeDetail"
      />
    </div>

    <!-- ── Empty State ─────────────────────────────────────────────── -->
    <EmptyData
      v-else-if="!store.loading"
      :icon="FolderOpen"
      :title="activeTab === 'mine' ? t('formateur.themes.emptyMineTitle') : t('companyPanel.tests.emptyTitle')"
      :description="searchQuery ? t('companyPanel.themes.emptyFilterDesc') : activeTab === 'mine' ? t('formateur.themes.emptyMineDesc') : t('formateur.themes.emptyAllDesc')"
    >
      <button
        v-if="searchQuery"
        class="flex items-center gap-2 px-4 py-2 rounded-lg border border-border text-sm font-semibold text-foreground hover:bg-muted/50 transition-colors mt-2"
        @click="searchQuery = ''"
      >
        <X class="w-3.5 h-3.5" /> {{ t('themes.clearSearch') }}
      </button>
      <button
        v-else-if="activeTab === 'mine'"
        class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors mt-2"
        @click="openCreate"
      >
        <Plus class="w-4 h-4" /> {{ t('formateur.themes.addTheme') }}
      </button>
    </EmptyData>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Input } from '@/components/ui/input'
import EmptyData from '@/components/common/EmptyData.vue'
import ThemeCard from '@/components/theme/ThemeCard.vue'
import ThemeDetailSheet from '@/components/theme/ThemeDetailSheet.vue'
import ThemeFormSheet from '@/components/theme/ThemeFormSheet.vue'
import { useTopicStore } from '@/store/topicStore'
import { useAuthStore } from '@/store/authStore'
import { useLocale } from '@/composables/useLocale'
import type { ThemeItem } from '@utils/models'
import {
  Layers, FolderTree, FolderOpen, Plus, Search, X, Info, UserCheck, AlertCircle,
} from 'lucide-vue-next'

const store  = useTopicStore()
const auth   = useAuthStore()
const router = useRouter()
const { t }  = useLocale()

const currentUserId  = computed(() => auth.state.user?.id ?? undefined)
const searchQuery    = ref('')
const activeTab      = ref<'all' | 'mine'>('all')

// ── Sheet state ─────────────────────────────────────────────────────────────
const isDetailOpen       = ref(false)
const isFormOpen         = ref(false)

const selectedItem       = ref<ThemeItem | null>(null)
const selectedIsSubtheme = ref(false)
const selectedParentName = ref<string | undefined>()
const selectedCanEdit    = ref(false)

const editingItem        = ref<ThemeItem | null>(null)
const formParentId       = ref<number | null>(null)
const formIsSubtheme     = ref(false)

// ── Helpers ─────────────────────────────────────────────────────────────────
function isMyTheme(theme: ThemeItem): boolean {
  return currentUserId.value != null && theme.createdById === currentUserId.value
}

// ── Computed ────────────────────────────────────────────────────────────────
const myThemesCount = computed(() => store.themes.filter(isMyTheme).length)

const tabs = computed(() => [
  { key: 'all' as const,  label: 'All Themes', count: store.themes.length },
  { key: 'mine' as const, label: 'Mine',        count: myThemesCount.value },
])

const filteredThemes = computed(() => {
  let list = store.themes
  if (activeTab.value === 'mine') list = list.filter(isMyTheme)
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(
      t =>
        t.name.toLowerCase().includes(q) ||
        (t.description ?? '').toLowerCase().includes(q) ||
        t.children?.some(c => c.name.toLowerCase().includes(q)),
    )
  }
  return list
})

// ── Lifecycle ───────────────────────────────────────────────────────────────
onMounted(() => {
  if (!store.themes.length) store.fetchThemes()
})

// ── Detail ──────────────────────────────────────────────────────────────────
function openDetail(theme: ThemeItem) {
  selectedItem.value       = theme
  selectedIsSubtheme.value = false
  selectedParentName.value = undefined
  selectedCanEdit.value    = isMyTheme(theme)
  isDetailOpen.value       = true
}

function openSubthemeDetail(sub: ThemeItem) {
  const parent = store.themes.find(t => t.children?.some(c => c.id === sub.id))
  selectedItem.value       = sub
  selectedIsSubtheme.value = true
  selectedParentName.value = parent?.name
  selectedCanEdit.value    = isMyTheme(sub)
  isDetailOpen.value       = true
}

// ── Create / Edit ───────────────────────────────────────────────────────────
function openCreate() {
  editingItem.value    = null
  formParentId.value   = null
  formIsSubtheme.value = false
  isDetailOpen.value   = false
  isFormOpen.value     = true
}

function openEdit(theme: ThemeItem) {
  if (!isMyTheme(theme)) return // Guard: cannot edit others' themes
  editingItem.value    = theme
  formIsSubtheme.value = selectedIsSubtheme.value
  formParentId.value   = theme.parentId ?? null
  isDetailOpen.value   = false
  isFormOpen.value     = true
}

function onSaved() {
  store.fetchThemes()
}

// ── Questionnaire ───────────────────────────────────────────────────────────
function handleAddQuestionnaire(theme: ThemeItem) {
  router.push({ path: '/formateur/questionnaires', query: { themeId: theme.id } })
}
</script>
