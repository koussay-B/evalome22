<template>
  <div class="space-y-6">

    <!-- ── Detail Sheet ────────────────────────────────────────────── -->
    <CampaignDetailSheet
      v-model:open="isDetailOpen"
      :campaign="selectedCampaign"
      :can-edit="true"
      :can-delete="true"
      @edit="openEdit"
      @delete="confirmDelete"
    />

    <!-- ── Delete Confirmation ────────────────────────────────────── -->
    <AlertDialog v-model:open="isDeleteConfirmOpen">
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{{ t('companyPanel.tests.deleteConfirmTitle') }}</AlertDialogTitle>
          <AlertDialogDescription>
            {{ t('companyPanel.tests.deleteConfirmDesc') }}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>{{ t('common.cancel') }}</AlertDialogCancel>
          <AlertDialogAction
            class="bg-red-600 hover:bg-red-700 text-white border-none"
            :disabled="isDeleting"
            @click="handleDelete"
          >
            <span v-if="isDeleting">{{ t('common.loading') }}</span>
            <span v-else>{{ t('common.delete') }}</span>
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>

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
        <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">
          {{ t('companyPanel.tests.subtitle') }}
        </p>
        <h1 class="text-3xl text-slate-800 text-slate-800 tracking-tight">{{ t('companyPanel.tests.title') }}</h1>
      </div>
      <button
        class="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 shadow-[0_2px_12px_0_oklch(0.58_0.21_42/0.18)] hover:shadow-[0_4px_20px_0_oklch(0.58_0.21_42/0.28)] transition-all shrink-0"
        @click="openCreate"
      >
        <Plus class="w-4 h-4" /> {{ t('companyPanel.tests.newCampaign') }}
      </button>
    </div>

    <!-- ── Error banner ────────────────────────────────────────────── -->
    <div
      v-if="fetchError"
      class="flex items-center gap-3 px-4 py-3 rounded-2xl bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 text-sm text-red-700 dark:text-red-400"
    >
      <CircleAlert class="w-4 h-4 shrink-0" />
      {{ fetchError }}
      <button class="ms-auto text-xs font-bold underline hover:no-underline" @click="loadCampaigns">
        {{ t('common.retry') }}
      </button>
    </div>

    <!-- ── Stats Bar ───────────────────────────────────────────────── -->
    <div class="grid grid-cols-3 gap-4 ">
      <template v-if="loading">
        <div v-for="i in 3" :key="i" class="rounded-2xl  border border-border bg-card p-4 flex items-center gap-3 shadow-sm">
          <div class="h-10 w-10 rounded-xl bg-muted animate-pulse shrink-0" />
          <div class="space-y-1.5">
            <div class="h-6 w-8 rounded bg-muted animate-pulse" />
            <div class="h-3 w-20 rounded bg-muted animate-pulse" />
          </div>
        </div>
      </template>
      <template v-else>
        <div class="rounded-2xl border  bg-[#F1EFEC] p-4 flex items-center gap-3 shadow-sm">
          <div class="h-10 w-10 rounded-xl bg-primary/10 flex items-center justify-center shrink-0">
            <ClipboardList class="w-4.5 h-4.5 text-primary" />
          </div>
          <div>
            <div class="text-xs text-muted-foreground text-slate-800">{{ t('companyPanel.tests.statTotal') }}</div>
          </div>
        </div>
        <div class="rounded-2xl border  bg-[#F1EFEC]  p-4 flex items-center gap-3 shadow-sm">
          <div class="h-10 w-10 rounded-xl bg-emerald-100 dark:bg-emerald-900/40 flex items-center justify-center shrink-0">
            <PlayCircle class="w-4.5 h-4.5 text-emerald-600 dark:text-emerald-400" />
          </div>
          <div>
            <div class="text-xl  text-slate-800">{{ activeCampaigns }}</div>
            <div class="text-xs text-muted-foreground">{{ t('companyPanel.tests.statActive') }}</div>
          </div>
        </div>
        <div class="rounded-2xl border  bg-[#F1EFEC] p-4 flex items-center gap-3 shadow-sm">
          <div class="h-10 w-10 rounded-xl bg-sky-100 dark:bg-sky-900/40 flex items-center justify-center shrink-0">
            <CalendarClock class="w-4.5 h-4.5 text-sky-600 dark:text-sky-400" />
          </div>
          <div>
            <div class="text-xl  text-slate-800">{{ scheduledCampaigns }}</div>
            <div class="text-xs text-muted-foreground">{{ t('companyPanel.tests.statUpcoming') }}</div>
          </div>
        </div>
      </template>
    </div>

    <!-- ── Search + Status Filter ──────────────────────────────────── -->
    <div class="flex flex-wrap gap-3 items-center">
      <div class="relative flex-1 min-w-[200px] max-w-sm">
        <Search class="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
        <Input v-model="searchQuery" :placeholder="t('companyPanel.tests.searchPlaceholder')" class="ps-9 h-9 bg-card border-border focus-visible:ring-primary/30" />
      </div>
      <Select v-model="statusFilter">
        <SelectTrigger class="w-[180px] h-9 bg-card border-border focus:ring-primary/30">
          <SelectValue :placeholder="t('companyPanel.tests.allStatuses')" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">{{ t('companyPanel.tests.allStatuses') }}</SelectItem>
          <SelectItem value="Active">{{ t('companyPanel.tests.statusActive') }}</SelectItem>
          <SelectItem value="Scheduled">{{ t('companyPanel.tests.statusUpcoming') }}</SelectItem>
          <SelectItem value="Closed">{{ t('companyPanel.tests.statusClosed') }}</SelectItem>
          <SelectItem value="Draft">{{ t('companyPanel.tests.statusDraft') }}</SelectItem>
        </SelectContent>
      </Select>
    </div>

    <!-- ── Loading Skeleton Cards ─────────────────────────────────── -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <div
        v-for="i in 6"
        :key="i"
        class="rounded-2xl border border-border bg-card p-5 flex flex-col gap-4"
      >
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-muted animate-pulse shrink-0" />
          <div class="flex-1 space-y-2">
            <div class="h-4 rounded bg-muted animate-pulse w-3/4" />
            <div class="h-3 rounded bg-muted animate-pulse w-1/2" />
          </div>
          <div class="h-5 w-16 rounded-lg bg-muted animate-pulse" />
        </div>
        <div class="grid grid-cols-3 gap-3 pt-3 border-t border-border/40">
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
        :can-edit="true"
        :can-delete="true"
        @click="openDetail(c)"
        @edit="openEdit(c)"
        @delete="confirmDelete(c)"
        @candidates="openCandidates(c)"
      />
    </div>

    <!-- ── Empty State ─────────────────────────────────────────────── -->
    <EmptyData
      v-else-if="!loading"
      :icon="ClipboardList"
      :title="t('companyPanel.tests.emptyTitle')"
      :description="t('companyPanel.tests.emptyDesc')"
    >
      <button
        v-if="searchQuery || statusFilter !== 'all'"
        class="flex items-center gap-2 px-4 py-2 rounded-xl border border-border text-sm font-semibold text-slate-800 hover:bg-primary/5 hover:border-primary/30 hover:text-primary transition-colors mt-2"
        @click="searchQuery = ''; statusFilter = 'all'"
      >
        <X class="w-3.5 h-3.5" /> {{ t('common.clearFilters') }}
      </button>
      <button
        v-else
        class="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 shadow-[0_2px_12px_0_oklch(0.58_0.21_42/0.18)] transition-all mt-2"
        @click="openCreate"
      >
        <Plus class="w-4 h-4" /> {{ t('companyPanel.tests.newCampaign') }}
      </button>
    </EmptyData>

    <!-- ── Success Toast ───────────────────────────────────────────── -->
    <Transition
      enter-from-class="opacity-0 translate-y-2"
      enter-active-class="transition-all duration-200"
      leave-to-class="opacity-0 translate-y-2"
      leave-active-class="transition-all duration-200"
    >
      <div v-if="showSuccessToast" class="fixed bottom-6 end-6 z-50 flex items-center gap-2 px-4 py-3 rounded-2xl bg-card border border-border text-slate-800 text-sm font-semibold shadow-[0_8px_32px_0_oklch(0.58_0.21_42/0.15)]">
        <CheckCircle class="w-4 h-4 text-emerald-500" /> {{ t('companyPanel.settings.savedSuccessfully') }}
      </div>
    </Transition>

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
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel,
  AlertDialogContent, AlertDialogDescription, AlertDialogFooter,
  AlertDialogHeader, AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { useLocale } from '@/composables/useLocale'
import { getCampaignsApi, deleteCampaignApi } from '@/utils/api'
import type { CampaignItem } from '@/utils/models'
import {
  Search, Plus, X, ClipboardList, PlayCircle, CalendarClock, CircleAlert, CheckCircle,
} from 'lucide-vue-next'

const { t } = useLocale()
const router = useRouter()

// ── UI state ──────────────────────────────────────────────────────────────
const searchQuery      = ref('')
const statusFilter     = ref('all')
const isDetailOpen     = ref(false)
const selectedCampaign = ref<CampaignCardData | null>(null)

// ── Candidates sheet ───────────────────────────────────────────────────────
const isCandidatesOpen = ref(false)
const candidatesTarget = ref<CampaignCardData | null>(null)

// ── Delete confirmation ───────────────────────────────────────────────────
const isDeleteConfirmOpen = ref(false)
const isDeleting          = ref(false)
const campaignToDelete    = ref<CampaignCardData | null>(null)
const showSuccessToast    = ref(false)

function openCandidates(c: CampaignCardData) {
  candidatesTarget.value = c
  isCandidatesOpen.value = true
}

// ── Fetch state ───────────────────────────────────────────────────────────
const loading    = ref(true)
const fetchError = ref<string | null>(null)

// ── Campaign data ──────────────────────────────────────────────────────────
const campaigns = ref<CampaignCardData[]>([])

function formatDate(iso: string): string {
  if (!iso) return '—'
  return new Date(iso).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })
}

function adaptCampaign(item: CampaignItem): CampaignCardData {
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

// ── Computed ──────────────────────────────────────────────────────────────
const activeCampaigns    = computed(() => campaigns.value.filter(c => c.status === 'Active').length)
const scheduledCampaigns = computed(() => campaigns.value.filter(c => c.status === 'Scheduled').length)

const filtered = computed(() => {
  let list = campaigns.value
  if (statusFilter.value !== 'all') list = list.filter(c => c.status === statusFilter.value)
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(c => c.name.toLowerCase().includes(q))
  }
  return list
})

// ── Actions ───────────────────────────────────────────────────────────────
function openDetail(c: CampaignCardData) {
  selectedCampaign.value = c
  isDetailOpen.value     = true
}

function openEdit(c: CampaignCardData) {
  router.push({ name: 'CompanyCampaignEdit', params: { id: c.id } })
}

function openCreate() {
  router.push({ name: 'CompanyCampaignNew' })
}

function confirmDelete(c: CampaignCardData) {
  campaignToDelete.value = c
  isDeleteConfirmOpen.value = true
  isDetailOpen.value = false
}

async function handleDelete() {
  if (!campaignToDelete.value) return
  isDeleting.value = true
  fetchError.value = null

  const { error } = await deleteCampaignApi(campaignToDelete.value.id)
  isDeleting.value = false

  if (error) {
    fetchError.value = error
    isDeleteConfirmOpen.value = false
  } else {
    isDeleteConfirmOpen.value = false
    campaignToDelete.value = null
    showSuccessToast.value = true
    setTimeout(() => { showSuccessToast.value = false }, 3000)
    loadCampaigns()
  }
}
</script>