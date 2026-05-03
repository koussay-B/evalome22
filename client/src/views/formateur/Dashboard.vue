<template>
  <div class="space-y-6">

    <!-- ── Header ─────────────────────────────────────────────────────────── -->
    <div class="flex items-center justify-between gap-4">
      <div>
        <p class="text-[10px] text-slate-800 uppercase tracking-[0.25em] text-primary/60 mb-1">Formateur Panel</p>
        <h1 class="text-3xl text-slate-800 tracking-tight ">Dashboard</h1>
      </div>
      <span class="text-xs font-medium text-muted-foreground bg-muted px-3 py-1.5 rounded-full border border-border shrink-0">{{ today }}</span>
    </div>

    <!-- ── KPI Cards ───────────────────────────────────────────────────────── -->
    <div class="grid grid-cols-2 lg:grid-cols-5 gap-3">
  <div
    v-for="card in kpiCards"
    :key="card.label"
    class="relative rounded-2xl border border-stone-200/60 bg-slate-50 p-3.5 flex items-center gap-3.5 hover:shadow-md transition-all duration-300 group"
  >
    <div :class="card.iconBg" class="w-11 h-11 bg-slate-50 rounded-[18px] flex items-center justify-center shrink-0 shadow-sm transition-transform group-hover:scale-105">
      <component :is="card.icon" :class="card.iconColor" class="w-5.5 h-5.5" />
    </div>

    <div class="min-w-0 flex-1">
      <div class="text-2xl  text-slate-800 tracking-tight tabular-nums">
        <span v-if="loading">—</span>
        <span v-else>{{ card.value }}</span>
      </div>
      
      <p class="text-[10px] text-slate-800 uppercase tracking-[0.12em] text-slate-400 mt-0.5 truncate">
        {{ card.label }}
      </p>
    </div>
  </div>
</div>
    <!-- ── Area Chart + Score Cards ──────────────────────────────────────── -->
    <div class="grid grid-cols-1 lg:grid-cols-[1fr_280px] gap-4">
      <AppAreaChart
        :data="stats?.attemptTrend ?? []"
        title="Attempt Activity"
        subtitle="Test activity in your company — last 30 days"
        color="#6366f1"
        :height="240"
        :show-range-selector="true"
      />
      <div class="flex flex-col gap-4">
        <!-- Avg Score -->
        <div class="flex-1 rounded-2xl border border-indigo-100 dark:border-indigo-800/40 bg-indigo-50 dark:bg-indigo-900/20 p-4 flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-indigo-100 dark:bg-indigo-900/40 flex items-center justify-center shrink-0">
            <BarChart3 class="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
          </div>
          <div>
            <p class="text-[11px] text-slate-800 uppercase tracking-widest text-muted-foreground">Avg Score</p>
            <p class="text-2xl text-slate-800 text-slate-800 tabular-nums mt-0.5">
              <span v-if="loading">—</span>
              <span v-else>{{ stats?.avgScore ?? 0 }}%</span>
            </p>
          </div>
        </div>

        <!-- Pass Rate -->
        <div class="flex-1 rounded-2xl border border-emerald-100 dark:border-emerald-800/40 bg-emerald-50 dark:bg-emerald-900/20 p-4 flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-emerald-100 dark:bg-emerald-900/40 flex items-center justify-center shrink-0">
            <Target class="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
          </div>
          <div>
            <p class="text-[11px] text-slate-800 uppercase tracking-widest text-muted-foreground">Pass Rate</p>
            <p class="text-2xl text-slate-800 text-slate-800 tabular-nums mt-0.5">
              <span v-if="loading">—</span>
              <span v-else>{{ stats?.passRate ?? 0 }}%</span>
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- ── Pass/Fail Donut + Attempt Status ──────────────────────────────── -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">

      <AppPieChart
        :data="passFailPieData"
        title="Pass / Fail Ratio"
        subtitle="All submitted attempts on your campaigns"
        :size="150"
        cutout="62%"
        :show-total="true"
      />

      <div class="rounded-2xl border border-border bg-card p-5">
        <p class="text-[11px] text-slate-800 uppercase tracking-widest text-muted-foreground mb-1">Attempt Status</p>
        <p class="text-sm font-semibold text-slate-800 mb-5">All-time breakdown</p>
        <div v-if="loading" class="space-y-3">
          <Skeleton v-for="i in 4" :key="i" class="h-7 w-full rounded-lg" />
        </div>
        <div v-else class="space-y-4">
          <div v-for="s in attemptStatusItems" :key="s.label">
            <div class="flex items-center justify-between mb-1.5">
              <div class="flex items-center gap-2">
                <span class="w-2 h-2 rounded-full shrink-0" :class="s.dot" />
                <span class="text-xs font-medium text-slate-800">{{ s.label }}</span>
              </div>
              <div class="flex items-center gap-2">
                <span class="text-xs  text-slate-800 tabular-nums">{{ s.count.toLocaleString() }}</span>
                <span class="text-[10px] text-muted-foreground w-8 text-right tabular-nums">{{ s.pct }}%</span>
              </div>
            </div>
            <div class="w-full h-1.5 bg-muted rounded-full overflow-hidden">
              <div class="h-full rounded-full transition-all duration-700" :class="s.bar" :style="{ width: s.pct + '%' }" />
            </div>
          </div>
          <p v-if="!totalAttempts && !loading" class="text-xs text-muted-foreground italic">No attempts yet.</p>
        </div>
      </div>
    </div>

    <!-- ── Campaign Pass Rates ────────────────────────────────────────────── -->
    <div class="rounded-2xl border border-border bg-card p-5">
      <div class="flex items-center justify-between mb-5">
        <div>
          <p class="text-[11px] text-slate-800 uppercase tracking-widest text-muted-foreground mb-1">My Campaigns</p>
          <p class="text-sm font-semibold text-slate-800">Pass rate per campaign</p>
        </div>
        <router-link
          to="/formateur/tests"
          class="flex items-center gap-1 text-xs text-muted-foreground hover:text-slate-800 transition-colors font-medium"
        >
          View all <ArrowRight class="w-3 h-3" />
        </router-link>
      </div>

      <div v-if="loading" class="space-y-3">
        <Skeleton v-for="i in 4" :key="i" class="h-10 w-full rounded-lg" />
      </div>
      <div v-else-if="stats?.passRateByCampaign?.length" class="space-y-4">
        <div v-for="camp in stats.passRateByCampaign.slice(0, 6)" :key="camp.campaignName">
          <div class="flex items-center justify-between mb-1.5">
            <div class="flex items-center gap-2 min-w-0">
              <span
                class="w-1.5 h-1.5 rounded-full shrink-0"
                :class="camp.status === 'Active' ? 'bg-emerald-500' : camp.status === 'Scheduled' ? 'bg-sky-500' : 'bg-muted-foreground/40'"
              />
              <span class="text-xs font-medium text-slate-800 truncate">{{ camp.campaignName }}</span>
            </div>
            <div class="flex items-center gap-3 shrink-0 ml-3">
              <span class="text-[10px] text-muted-foreground tabular-nums">({{ camp.invitedCount }})</span>
              <span
                class="text-slate-800 tabular-nums text-xs w-10 text-right"
                :class="camp.passRate >= 70 ? 'text-emerald-600' : camp.passRate >= 50 ? 'text-amber-600' : 'text-red-500'"
              >{{ Math.round(camp.passRate) }}%</span>
            </div>
          </div>
          <div class="w-full h-1.5 bg-muted rounded-full overflow-hidden">
            <div
              class="h-full rounded-full transition-all duration-700"
              :class="camp.passRate >= 70 ? 'bg-emerald-500' : camp.passRate >= 50 ? 'bg-amber-400' : 'bg-red-400'"
              :style="{ width: Math.round(camp.passRate) + '%' }"
            />
          </div>
        </div>
      </div>
      <div v-else class="flex flex-col items-center justify-center gap-2 py-10 text-center">
        <ClipboardList class="w-8 h-8 text-muted-foreground/30" />
        <p class="text-sm text-muted-foreground">No campaigns with candidates yet.</p>
        <router-link to="/formateur/tests" class="text-xs text-muted-foreground underline hover:text-slate-800">
          Create your first campaign →
        </router-link>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Skeleton } from '@/components/ui/skeleton'
import { getFormateurStatsApi, type FormateurStats } from '@/utils/api/stats.api'
import AppAreaChart from '@/components/charts/AppAreaChart.vue'
import AppPieChart  from '@/components/charts/AppPieChart.vue'
import {
  ClipboardList, BarChart3, Users, FileText,
  Activity, Target, ArrowRight, Zap
} from 'lucide-vue-next'

const today = new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })

const loading = ref(true)
const stats   = ref<FormateurStats | null>(null)

onMounted(async () => {
  const { data } = await getFormateurStatsApi()
  stats.value   = data ?? null
  loading.value = false
})

// ── KPI cards ─────────────────────────────────────────────────────────────────
const kpiCards = computed(() => [
  {
    label:     'My Campaigns',
    value:     stats.value?.totalCampaigns ?? '–',
    icon:      ClipboardList,
    iconBg:    'bg-indigo-100 dark:bg-indigo-900/30',
    iconColor: 'text-indigo-600 dark:text-indigo-400',
    badge:     null as string | null,
  },
  {
    label:     'Active Campaigns',
    value:     stats.value?.activeCampaigns ?? '–',
    icon:      Activity,
    iconBg:    'bg-emerald-100 dark:bg-emerald-900/30',
    iconColor: 'text-emerald-600 dark:text-emerald-400',
    badge:     stats.value?.activeCampaigns ? 'Live' : null as string | null,
  },
  {
    label:     'Questionnaires',
    value:     stats.value?.totalQuestionnaires ?? '–',
    icon:      FileText,
    iconBg:    'bg-violet-100 dark:bg-violet-900/30',
    iconColor: 'text-violet-600 dark:text-violet-400',
    badge:     null as string | null,
  },
  {
    label:     'Total Candidates',
    value:     stats.value?.totalCandidates ?? '–',
    icon:      Users,
    iconBg:    'bg-amber-100 dark:bg-amber-900/30',
    iconColor: 'text-amber-600 dark:text-amber-400',
    badge:     null as string | null,
  },
  {
    label:     'Online Users',
    value:     stats.value?.onlineUsersCount ?? 0,
    icon:      Zap,
    iconBg:    'bg-sky-100 dark:bg-sky-900/30',
    iconColor: 'text-sky-600 dark:text-sky-400',
    badge:     'Live',
  },
])

// ── Charts ────────────────────────────────────────────────────────────────────
const passFailPieData = computed(() => {
  const pf = stats.value?.passFailRatio ?? { passed: 0, failed: 0 }
  return [
    { label: 'Passed', value: pf.passed, color: '#10b981' },
    { label: 'Failed', value: pf.failed, color: '#f43f5e' },
  ]
})

const totalAttempts = computed(() => {
  const c = stats.value?.attemptStatusCounts
  if (!c) return 0
  return c.submitted + c.inProgress + c.timedOut + c.abandoned
})

const attemptStatusItems = computed(() => {
  const c = stats.value?.attemptStatusCounts ?? { submitted: 0, inProgress: 0, timedOut: 0, abandoned: 0 }
  const total = totalAttempts.value || 1
  return [
    { label: 'Submitted',   count: c.submitted,  pct: Math.round(c.submitted  / total * 100), dot: 'bg-emerald-500', bar: 'bg-emerald-500' },
    { label: 'In Progress', count: c.inProgress, pct: Math.round(c.inProgress / total * 100), dot: 'bg-sky-500',     bar: 'bg-sky-500' },
    { label: 'Abandoned',   count: c.abandoned,  pct: Math.round(c.abandoned  / total * 100), dot: 'bg-red-400',     bar: 'bg-red-400' },
  ]
})
</script>
