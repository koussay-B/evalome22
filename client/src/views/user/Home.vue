<template>
  <div class="space-y-6">

    <!-- ── Header ───────────────────────────────────────── -->
    <div class="flex items-center justify-between gap-4">
      <div>
        <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">
          Candidat Panel
        </p>
        <h1 class="text-3xl text-slate-800 tracking-tight">
          Dashboard
        </h1>
      </div>
      <span class="text-xs font-medium text-muted-foreground bg-muted px-3 py-1.5 rounded-full border border-border shrink-0">
        {{ today }}
      </span>
    </div>

    <!-- ── KPI Cards ─────────────────────────────────────── -->
    <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-3">
      <div
        v-for="card in kpiCards"
        :key="card.label"
        class="rounded-xl border border-stone-200 bg-slate-50 p-3 flex items-center gap-4 hover:shadow-sm transition-all duration-300"
      >
        <div 
          class="w-12 h-12 rounded-xl flex items-center bg-slate-50 justify-center shrink-0 border border-stone-100"
          :class="card.iconBg || 'bg-stone-50'"
        >
          <component 
            :is="card.icon" 
            class="w-6 h-6 stroke-[1.8px]" 
            :class="card.iconColor || 'text-stone-600'"
          />
        </div>

        <div class="flex flex-col min-w-0">
          <span class="text-2xl font-medium text-slate-900 tracking-tight leading-tight">
            {{ loading ? '—' : card.value }}
          </span>

          <p class="text-[10px] font-medium text-slate-400 uppercase truncate">
            {{ card.label }}
          </p>
        </div>
      </div>
    </div>

    <!-- ── Charts (Same Level) ───────────────────────── -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">

      <!-- Area Chart -->
      <AppAreaChart
        :data="stats?.attemptTrend ?? []"
        title="My Attempt Activity"
        subtitle="Test submissions over the last 30 days"
        color="#6366f1"
        :height="240"
        :show-range-selector="true"
      />

      <!-- Pie Chart -->
      <AppPieChart
        :data="passFailPieData"
        title="Pass / Fail Ratio"
        subtitle="All my submitted attempts"
        :size="150"
        cutout="62%"
        :show-total="true"
      />

    </div>

    <!-- ── Stats Cards (Below Charts) ───────────────────── -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">

      <div class="flex-1 rounded-2xl border border-indigo-100 dark:border-indigo-800/40 bg-indigo-50 dark:bg-indigo-900/20 p-4 flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl bg-indigo-100 dark:bg-indigo-900/40 flex items-center justify-center shrink-0">
          <BarChart3 class="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
        </div>
        <div>
          <p class="text-[11px] uppercase tracking-widest text-muted-foreground">Avg Score</p>
          <p class="text-2xl text-slate-800 tabular-nums mt-0.5">
            <span v-if="loading">—</span>
            <span v-else>{{ stats?.avgScore ?? 0 }}%</span>
          </p>
        </div>
      </div>

      <div class="flex-1 rounded-2xl border border-emerald-100 dark:border-emerald-800/40 bg-emerald-50 dark:bg-emerald-900/20 p-4 flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl bg-emerald-100 dark:bg-emerald-900/40 flex items-center justify-center shrink-0">
          <Target class="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
        </div>
        <div>
          <p class="text-[11px] uppercase tracking-widest text-muted-foreground">Pass Rate</p>
          <p class="text-2xl text-slate-800 tabular-nums mt-0.5">
            <span v-if="loading">—</span>
            <span v-else>{{ stats?.passRate ?? 0 }}%</span>
          </p>
        </div>
      </div>s

    </div>

  </div>
</template>
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Skeleton } from '@/components/ui/skeleton'
import { getCandidatStatsApi, type CandidatStats } from '@/utils/api/stats.api'
import { getMyCampaignsApi } from '@/utils/api/attempt.api'
import type { MyCampaignItem } from '@/utils/models'
import AppAreaChart from '@/components/charts/AppAreaChart.vue'
import AppPieChart  from '@/components/charts/AppPieChart.vue'
import AppBarChart  from '@/components/charts/AppBarChart.vue'
import {
  ClipboardList, BarChart3, Award, Target, ArrowRight,
  CheckCircle2, XCircle, ClipboardCheck, TrendingUp,
} from 'lucide-vue-next'

const today = new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })

// ── Stats ─────────────────────────────────────────────────────────────────────
const loading = ref(true)
const stats   = ref<CandidatStats | null>(null)

// ── Campaigns ─────────────────────────────────────────────────────────────────
const campaignsLoading = ref(true)
const campaigns        = ref<MyCampaignItem[]>([])

onMounted(async () => {
  const [statsRes, campsRes] = await Promise.all([
    getCandidatStatsApi(),
    getMyCampaignsApi(),
  ])
  stats.value     = statsRes.data ?? null
  campaigns.value = campsRes.data ?? []
  loading.value         = false
  campaignsLoading.value = false
})

// ── KPI Cards ─────────────────────────────────────────────────────────────────
const kpiCards = computed(() => [
  {
    label:     'Campaigns',
    value:     stats.value?.totalCampaigns ?? '–',
    icon:      ClipboardList,
    iconBg:    'bg-indigo-100 dark:bg-indigo-900/30',
    iconColor: 'text-indigo-600 dark:text-indigo-400',
    badge:     null as string | null,
  },
  {
    label:     'Completed',
    value:     stats.value?.completedAttempts ?? '–',
    icon:      ClipboardCheck,
    iconBg:    'bg-sky-100 dark:bg-sky-900/30',
    iconColor: 'text-sky-600 dark:text-sky-400',
    badge:     null as string | null,
  },
  {
    label:     'Certificates',
    value:     stats.value?.certificatesEarned ?? '–',
    icon:      Award,
    iconBg:    'bg-amber-100 dark:bg-amber-900/30',
    iconColor: 'text-amber-600 dark:text-amber-400',
    badge:      null as string | null,
  },
  {
    label:     'Pass Rate',
    value:     stats.value?.passRate != null ? stats.value.passRate + '%' : '–',
    icon:      Target,
    iconBg:    'bg-emerald-100 dark:bg-emerald-900/30',
    iconColor: 'text-emerald-600 dark:text-emerald-400',
    badge:     (stats.value?.passRate ?? 0) >= 70 ? 'Great' : null as string | null,
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

// ── Score bar chart ───────────────────────────────────────────────────────────
const scoreBarData = computed(() =>
  (stats.value?.scoresByCampaign ?? []).map(r => ({
    label: r.campaignName,
    value: Math.round(r.score),
    color: r.passed ? '#10b981' : '#f43f5e',
  }))
)

// ── Quick links ───────────────────────────────────────────────────────────────
const quickLinks = [
  {
    to:        '/candidat/tests',
    label:     'My Tests',
    desc:      'View all campaigns you are invited to',
    icon:      ClipboardList,
    iconBg:    'bg-indigo-100 dark:bg-indigo-900/30',
    iconColor: 'text-indigo-600 dark:text-indigo-400',
  },
  {
    to:        '/candidat/certificates',
    label:     'Certificates',
    desc:      'Download and share your earned certificates',
    icon:      Award,
    iconBg:    'bg-amber-100 dark:bg-amber-900/30',
    iconColor: 'text-amber-600 dark:text-amber-400',
  },
  {
    to:        '/candidat/analytics',
    label:     'My Analytics',
    desc:      'Detailed view of your scores and progress',
    icon:      BarChart3,
    iconBg:    'bg-emerald-100 dark:bg-emerald-900/30',
    iconColor: 'text-emerald-600 dark:text-emerald-400',
  },
]

// ── Helpers ───────────────────────────────────────────────────────────────────
function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}
</script>
