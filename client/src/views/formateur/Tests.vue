<template>
  <div class="space-y-6">

    <!-- ── Detail Sheet ────────────────────────────────────────────── -->
    <CampaignDetailSheet
      v-model:open="isDetailOpen"
      :campaign="selectedCampaign"
      :can-edit="selectedCanEdit"
      :can-delete="false"
      @edit="openEdit"
    />

    <!-- ── Candidates Sheet (standalone, opened from card dropdown) ── -->
    <CampaignCandidatesSheet
      v-if="candidatesTarget"
      v-model:open="isCandidatesOpen"
      :campaign-id="candidatesTarget.id"
      :campaign-name="candidatesTarget.name"
    />

    <!-- ── Page Header ─────────────────────────────────────────────── -->
    <div class="flex items-end justify-between gap-4">
      <div>
        <p class="text-[10px] text-slate-800 uppercase tracking-[0.25em] text-primary/60 mb-1">
          {{ t('formateur.tests.subtitle') }}
        </p>
        <h1 class="text-3xl text-slate-800 tracking-tight text-slate-800">{{ t('formateur.tests.title') }}</h1>
      </div>
      <button
        class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors shrink-0"
        @click="openCreate"
      >
        <Plus class="w-4 h-4" /> {{ t('formateur.tests.newCampaign') }}
      </button>
    </div>

    <!-- ── Permission Banner ───────────────────────────────────────── -->
    <div class="flex items-start gap-3 px-4 py-3 rounded-xl border border-border/60 bg-muted/30">
      <Info class="w-4 h-4 text-muted-foreground shrink-0 mt-0.5" />
      <p class="text-sm text-muted-foreground leading-relaxed">
        {{ t('formateur.tests.permissionInfo') }}
      </p>
    </div>

    <!-- ── Error banner ────────────────────────────────────────────── -->
    <div
      v-if="fetchError"
      class="flex items-center gap-3 px-4 py-3 rounded-xl bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 text-sm text-red-700 dark:text-red-400"
    >
      <CircleAlert class="w-4 h-4 shrink-0" />
      {{ fetchError }}
      <button class="ms-auto text-xs text-slate-800 underline hover:no-underline" @click="loadCampaigns">
        {{ t('common.retry') }}
      </button>
    </div>

    <!-- ── Stats Bar ───────────────────────────────────────────────── -->
    <div class="grid grid-cols-3 gap-4">
      <template v-if="loading">
        <div v-for="i in 3" :key="i" class="rounded-xl border border-border bg-secondary p-4 flex items-center gap-3">
          <div class="h-9 w-9 rounded-lg bg-muted animate-pulse shrink-0" />
          <div class="space-y-1.5">
            <div class="h-6 w-8 rounded bg-muted animate-pulse" />
            <div class="h-3 w-20 rounded bg-muted animate-pulse" />
          </div>
        </div>
      </template>
      <template v-else>
        <div class="rounded-xl border border-border bg-secondary p-4 flex items-center gap-3">
          <div class="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
            <ClipboardList class="w-4 h-4 text-primary" />
          </div>
          <div>
            <div class="text-xl text-slate-800">{{ campaigns.length }}</div>
            <div class="text-xs text-muted-foreground">{{ t('formateur.tests.statTotal') }}</div>
          </div>
        </div>
        <div class="rounded-xl border border-border bg-secondary p-4 flex items-center gap-3">
          <div class="h-9 w-9 rounded-lg bg-emerald-100 dark:bg-emerald-900/40 flex items-center justify-center shrink-0">
            <PlayCircle class="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
          </div>
          <div>
            <div class="text-xl text-slate-800">{{ activeCampaigns }}</div>
            <div class="text-xs text-muted-foreground">{{ t('formateur.tests.statActive') }}</div>
          </div>
        </div>
        <div class="rounded-xl border border-border bg-secondary p-4 flex items-center gap-3">
          <div class="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
            <UserCheck class="w-4 h-4 text-primary" />
          </div>
          <div>
            <div class="text-xl text-slate-800">{{ myCampaigns }}</div>
            <div class="text-xs text-muted-foreground">{{ t('formateur.tests.statMine') }}</div>
          </div>
        </div>
      </template>
    </div>

    <!-- ── Tabs: All / Mine ────────────────────────────────────────── -->
    <div class="flex items-center gap-1 p-1 rounded-lg bg-muted/50 border border-border w-fit">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        :class="[
          'px-4 py-1.5 rounded-md text-sm font-semibold transition-all',
          activeTab === tab.key
            ? 'bg-background text-slate-800 shadow-sm border border-border/60'
            : 'text-muted-foreground hover:text-slate-800',
        ]"
        @click="activeTab = tab.key"
      >
        {{ tab.label }}
        <span class="ms-1.5 px-1.5 py-0.5 rounded-full text-[10px] bg-muted text-muted-foreground text-slate-800">
          {{ tab.count }}
        </span>
      </button>
    </div>

    <!-- ── Search + Status Filter ──────────────────────────────────── -->
    <div class="flex flex-wrap gap-3 items-center">
      <div class="relative flex-1 min-w-[200px] max-w-sm">
        <Search class="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
        <Input v-model="searchQuery" :placeholder="t('formateur.tests.searchPlaceholder')" class="ps-9 h-9" />
      </div>
      <Select v-model="statusFilter">
        <SelectTrigger class="w-[170px] h-9">
          <SelectValue :placeholder="t('formateur.tests.allStatuses')" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">{{ t('formateur.tests.allStatuses') }}</SelectItem>
          <SelectItem value="Active">{{ t('formateur.tests.statusActive') }}</SelectItem>
          <SelectItem value="Scheduled">{{ t('formateur.tests.statusUpcoming') }}</SelectItem>
          <SelectItem value="Closed">{{ t('formateur.tests.statusClosed') }}</SelectItem>
          <SelectItem value="Draft">{{ t('formateur.tests.statusDraft') }}</SelectItem>
        </SelectContent>
      </Select>
    </div>

    <!-- ── Loading Skeleton Cards ─────────────────────────────────── -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <div
        v-for="i in 6"
        :key="i"
        class="rounded-xl border border-border bg-secondary p-5 flex flex-col gap-4"
      >
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-muted animate-pulse shrink-0" />
          <div class="flex-1 space-y-2">
            <div class="h-4 rounded bg-muted animate-pulse w-3/4" />
            <div class="h-3 rounded bg-muted animate-pulse w-1/2" />
          </div>
          <div class="h-5 w-16 rounded-md bg-muted animate-pulse" />
        </div>
        <div class="grid grid-cols-3 gap-3 pt-3 border-t border-border/60">
          <div v-for="j in 3" :key="j" class="flex flex-col items-center gap-1.5">
            <div class="h-6 w-8 rounded bg-muted animate-pulse" />
            <div class="h-2.5 w-12 rounded bg-muted animate-pulse" />
          </div>
        </div>
        <div class="h-1.5 rounded-full bg-muted animate-pulse" />
      </div>
    </div>

    <!-- ── Campaign Cards ──────────────────────────────────────────── -->
    <div v-else-if="filtered.length > 0" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <CampaignCard
        v-for="c in filtered"
        :key="c.id"
        :campaign="c"
        :can-edit="isMine(c)"
        @click="openDetail(c)"
        @edit="openEdit(c)"
        @candidates="openCandidates(c)"
      />
    </div>

    <!-- ── Empty State ─────────────────────────────────────────────── -->
    <EmptyData
      v-else-if="!loading"
      :icon="ClipboardList"
      :title="t('formateur.tests.emptyTitle')"
      :description="hasFilters ? t('formateur.tests.emptyFilterDesc') : activeTab === 'mine' ? t('formateur.tests.emptyMineDesc') : t('formateur.tests.emptyAllDesc')"
    >
      <button
        v-if="hasFilters"
        class="flex items-center gap-2 px-4 py-2 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors mt-2"
        @click="clearFilters"
      >
        <X class="w-3.5 h-3.5" /> {{ t('formateur.tests.clearFilters') }}
      </button>
      <button
        v-else-if="activeTab === 'mine'"
        class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors mt-2"
        @click="openCreate"
      >
        <Plus class="w-4 h-4" /> {{ t('formateur.tests.newCampaign') }}
      </button>
    </EmptyData>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Input } from '@/components/ui/input'
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from '@/components/ui/select'
import EmptyData from '@/components/common/EmptyData.vue'
import CampaignCard, { type CampaignCardData } from '@/components/campaign/CampaignCard.vue'
import CampaignDetailSheet from '@/components/campaign/CampaignDetailSheet.vue'
import CampaignCandidatesSheet from '@/components/campaign/CampaignCandidatesSheet.vue'
import { useAuthStore } from '@/store/authStore'
import { useLocale } from '@/composables/useLocale'
import { getCampaignsApi } from '@/utils/api'
import type { CampaignItem } from '@/utils/models'
import { Search, Plus, X, Info, ClipboardList, PlayCircle, UserCheck, CircleAlert } from 'lucide-vue-next'

const router = useRouter()
const { t } = useLocale()

// ── Auth ──────────────────────────────────────────────────────────────────
const auth          = useAuthStore()
const currentUserId = computed(() => auth.state.user?.id ?? -1)

// ── UI state ──────────────────────────────────────────────────────────────
const searchQuery      = ref('')
const statusFilter     = ref('all')
const activeTab        = ref<'all' | 'mine'>('all')
const isDetailOpen     = ref(false)
const selectedCampaign = ref<CampaignCardData | null>(null)
const selectedCanEdit  = ref(false)

// ── Candidates sheet ───────────────────────────────────────────────────────
const isCandidatesOpen = ref(false)
const candidatesTarget = ref<CampaignCardData | null>(null)

function openCandidates(c: CampaignCardData) {
  candidatesTarget.value = c
  isCandidatesOpen.value = true
}

// ── Fetch state ───────────────────────────────────────────────────────────
const loading    = ref(true)
const fetchError = ref<string | null>(null)

// ── Campaign data ──────────────────────────────────────────────────────────
interface Campaign extends CampaignCardData {
  createdById: number
}

const campaigns = ref<Campaign[]>([])

function formatDate(iso: string): string {
  if (!iso) return '—'
  return new Date(iso).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })
}

function adaptCampaign(item: CampaignItem): Campaign {
  return {
    id:              item.id,
    name:            item.name,
    startDate:       formatDate(item.availableFrom),
    endDate:         formatDate(item.availableUntil),
    status:          item.status,
    invited:         item.invitedCount   ?? 0,
    completed:       item.completedCount ?? 0,
    passRate:        item.invitedCount > 0 ? Math.round((item.passedCount / item.invitedCount) * 100) : 0,
    avgScore:        0,
    questionnairesCount: item.questionnairesCount ?? 0,
    createdById:         item.createdById,
  }
}

async function loadCampaigns() {
  loading.value    = true
  fetchError.value = null
  const { data, error } = await getCampaignsApi()
  if (error || !data) {
    fetchError.value = error ?? 'Failed to load campaigns'
  } else {
    campaigns.value = data.map(adaptCampaign)
  }
  loading.value = false
}

onMounted(loadCampaigns)

// ── Helpers ───────────────────────────────────────────────────────────────
function isMine(c: CampaignCardData): boolean {
  return campaigns.value.find(x => x.id === c.id)?.createdById === currentUserId.value
}

// ── Computed ──────────────────────────────────────────────────────────────
const activeCampaigns = computed(() => campaigns.value.filter(c => c.status === 'Active').length)
const myCampaigns     = computed(() => campaigns.value.filter(c => c.createdById === currentUserId.value).length)
const hasFilters      = computed(() => !!searchQuery.value || statusFilter.value !== 'all')

const tabs = computed(() => [
  { key: 'all'  as const, label: t('formateur.tests.tabAll'),  count: campaigns.value.length },
  { key: 'mine' as const, label: t('formateur.tests.tabMine'), count: myCampaigns.value },
])

const filtered = computed(() => {
  let list = campaigns.value as Campaign[]
  if (activeTab.value === 'mine')   list = list.filter(c => c.createdById === currentUserId.value)
  if (statusFilter.value !== 'all') list = list.filter(c => c.status === statusFilter.value)
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(c => c.name.toLowerCase().includes(q))
  }
  return list
})

// ── Actions ───────────────────────────────────────────────────────────────
function clearFilters() {
  searchQuery.value  = ''
  statusFilter.value = 'all'
}

function openDetail(c: CampaignCardData) {
  selectedCampaign.value = c
  selectedCanEdit.value  = isMine(c)
  isDetailOpen.value     = true
}

function openEdit(c: CampaignCardData) {
  if (!isMine(c)) return
  router.push({ name: 'FormateurCampaignEdit', params: { id: c.id } })
}

function openCreate() {
  router.push({ name: 'FormateurCampaignNew' })
}
</script>
