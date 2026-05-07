<template>
  <div class="space-y-6">
    <div>
      <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">
        {{ t('candidatTests.subtitle') }}
      </p>
      <h1 class="text-3xl text-slate-800 tracking-tight">{{ t('candidatTests.title') }}</h1>
    </div>

    <div
      v-if="fetchError"
      class="flex items-center gap-3 px-4 py-3 rounded-xl bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 text-sm text-red-700 dark:text-red-400"
    >
      <CircleAlert class="w-4 h-4 shrink-0" />
      {{ fetchError }}
      <button class="ms-auto text-xs font-bold underline" @click="load">{{ t('common.retry') }}</button>
    </div>

    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <div v-for="i in 6" :key="i" class="rounded-xl border border-border bg-secondary p-5 space-y-4">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-muted animate-pulse shrink-0" />
          <div class="flex-1 space-y-2">
            <div class="h-4 rounded bg-muted animate-pulse w-3/4" />
            <div class="h-3 rounded bg-muted animate-pulse w-1/2" />
          </div>
          <div class="h-5 w-16 rounded-md bg-muted animate-pulse" />
        </div>
        <div class="h-9 rounded-lg bg-muted animate-pulse" />
      </div>
    </div>

    <EmptyData
      v-else-if="campaigns.length === 0 && !fetchError"
      :icon="ClipboardList"
      :title="t('candidatTests.noCampaigns')"
      :description="t('candidatTests.noCampaignsDesc')"
    />

    <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <button
        v-for="c in campaigns"
        :key="c.campaignId"
        class="rounded-xl border border-border bg-gray-50 p-5 flex flex-col gap-4 hover:border-primary/30 hover:shadow-sm transition-all text-start"
        @click="router.push({ name: 'CandidatCampaignDetail', params: { id: c.campaignId } })"
      >
        <div class="flex items-start gap-3">
          <div
            class="w-11 h-11 rounded-xl flex items-center justify-center shrink-0"
            :class="statusIconShell(c.testStatus)"
          >
            <component :is="statusIcon(c.testStatus)" class="w-5 h-5" />
          </div>

          <div class="flex-1 min-w-0">
            <h3 class="text-slate-800 truncate">{{ c.name }}</h3>
            <p class="text-xs text-muted-foreground mt-0.5 flex items-center gap-1">
              <Calendar class="w-3 h-3 shrink-0" />
              {{ formatDateTime(c.startDate) }} -> {{ formatDateTime(c.endDate) }}
            </p>
          </div>

          <span
            :class="statusBadge(c.testStatus)"
            class="text-[10px] uppercase tracking-wide px-2 py-0.5 rounded-full shrink-0"
          >
            {{ statusLabel(c.testStatus) }}
          </span>
        </div>

        <div class="grid grid-cols-2 gap-2">
          <div class="rounded-lg border border-border/70 bg-white/70 px-3 py-2">
            <p class="text-[10px] uppercase tracking-wide text-muted-foreground">
              {{ t('candidatTests.campaignStatus') }}
            </p>
            <p class="text-xs font-semibold text-slate-700 mt-0.5">
              {{ statusLabel(c.status) }}
            </p>
          </div>

          <div class="rounded-lg border border-border/70 bg-white/70 px-3 py-2">
            <p class="text-[10px] uppercase tracking-wide text-muted-foreground">
              {{ t('candidatTests.content') }}
            </p>
            <p class="text-xs font-semibold text-slate-700 mt-0.5 flex items-center gap-1.5">
              <FileText class="w-3.5 h-3.5" />
              {{ c.questionnairesCount }} {{ t('candidatTests.questionnaires') }}
            </p>
          </div>
        </div>

        <div class="flex items-center justify-center gap-1 text-sm font-semibold text-primary w-full mt-1">
          {{ t('candidatTests.openCampaign') }}
          <ChevronRight class="w-4 h-4" />
        </div>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import EmptyData from '@/components/common/EmptyData.vue'
import { useLocale } from '@/composables/useLocale'
import { getMyCampaignsApi } from '@/utils/api'
import type { MyCampaignItem } from '@/utils/models'
import {
  AlertTriangle,
  Calendar,
  CheckCircle2,
  ChevronRight,
  CircleAlert,
  ClipboardList,
  Clock3,
  FileText,
  PlayCircle,
  XCircle,
} from 'lucide-vue-next'

const { t } = useLocale()
const router = useRouter()

const loading = ref(true)
const fetchError = ref<string | null>(null)
const campaigns = ref<MyCampaignItem[]>([])

async function load() {
  loading.value = true
  fetchError.value = null

  const { data, error } = await getMyCampaignsApi()
  if (error || !data) {
    fetchError.value = error ?? 'Failed to load tests'
  } else {
    campaigns.value = data
  }

  loading.value = false
}

function statusLabel(s: string): string {
  const map: Record<string, string> = {
    Active: t('candidatTests.available'),
    Scheduled: t('candidatTests.upcoming'),
    Closed: t('candidatTests.expired'),
    Passed: t('candidatTests.passedBadge'),
    TimedOut: t('campaignDetail.timedOut'),
    Abandoned: t('campaignDetail.abandoned'),
    Invited: t('candidatTests.upcoming'),
    InProgress: t('candidatTests.inProgress'),
    Completed: t('candidatTests.completed'),
    Expired: t('candidatTests.expired'),
    Withdrawn: t('candidatTests.withdrawn'),
  }
  return map[s] ?? s
}

function statusBadge(s: string): string {
  const map: Record<string, string> = {
    Active: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300',
    Scheduled: 'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300',
    Closed: 'bg-muted text-muted-foreground',
    Passed: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300',
    TimedOut: 'bg-red-100 text-red-600 dark:bg-red-900/40 dark:text-red-400',
    Abandoned: 'bg-red-100 text-red-600 dark:bg-red-900/40 dark:text-red-400',
    Invited: 'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300',
    InProgress: 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300',
    Completed: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300',
    Expired: 'bg-muted text-muted-foreground',
    Withdrawn: 'bg-red-100 text-red-600 dark:bg-red-900/40 dark:text-red-400',
  }
  return map[s] ?? 'bg-muted text-muted-foreground'
}

function statusIcon(s: string) {
  const map: Record<string, unknown> = {
    Active: PlayCircle,
    Scheduled: Clock3,
    Closed: XCircle,
    Passed: CheckCircle2,
    TimedOut: AlertTriangle,
    Abandoned: XCircle,
    InProgress: PlayCircle,
    Completed: CheckCircle2,
    Expired: XCircle,
  }
  return map[s] ?? ClipboardList
}

function statusIconShell(s: string): string {
  const map: Record<string, string> = {
    Active: 'bg-emerald-50 text-emerald-600',
    Scheduled: 'bg-sky-50 text-sky-600',
    Closed: 'bg-slate-100 text-slate-500',
    Passed: 'bg-emerald-50 text-emerald-600',
    TimedOut: 'bg-red-50 text-red-600',
    Abandoned: 'bg-red-50 text-red-600',
    InProgress: 'bg-amber-50 text-amber-600',
    Completed: 'bg-emerald-50 text-emerald-600',
    Expired: 'bg-slate-100 text-slate-500',
  }
  return map[s] ?? 'bg-primary/10 text-primary'
}

function formatDateTime(value: string): string {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value

  return date.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

onMounted(load)
</script>
