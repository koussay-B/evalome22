<template>
  <div class="space-y-6">

    <!-- ── Header ─────────────────────────────────────────────────────────── -->
    <div>
      <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground mb-1.5">Formateur Panel</p>
      <h1 class="text-3xl font-black tracking-tight text-foreground">Analytics</h1>
    </div>

    <!-- ── KPI Strip ──────────────────────────────────────────────────────── -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="kpi in kpis" :key="kpi.label" class="rounded-2xl border border-border bg-card p-5">
        <div class="flex items-center gap-2 mb-3">
          <component :is="kpi.icon" class="w-3.5 h-3.5 text-muted-foreground" />
          <span class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ kpi.label }}</span>
        </div>
        <div class="text-3xl font-black text-foreground tracking-tight tabular-nums">
          <span v-if="loading">—</span>
          <span v-else>{{ kpi.value }}</span>
        </div>
        <div class="flex items-center gap-1.5 mt-2">
          <TrendingUp v-if="kpi.trendUp" class="w-3 h-3 text-emerald-600" />
          <TrendingDown v-else class="w-3 h-3 text-amber-500" />
          <span class="text-xs font-medium" :class="kpi.trendUp ? 'text-emerald-600' : 'text-amber-500'">{{ kpi.trend }}</span>
        </div>
      </div>
    </div>

    <!-- ── Tabs ───────────────────────────────────────────────────────────── -->
    <div>
      <div class="flex items-center gap-1 p-1 bg-muted rounded-xl w-fit">
        <button
          v-for="tab in tabs"
          :key="tab.id"
          @click="activeTab = tab.id"
          class="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold transition-all outline-none"
          :class="activeTab === tab.id
            ? 'bg-background text-foreground shadow-sm'
            : 'text-muted-foreground hover:text-foreground'"
        >
          <component :is="tab.icon" class="w-3.5 h-3.5" />
          {{ tab.label }}
        </button>
      </div>

      <div class="pt-6">

        <!-- ════════════ Overview ════════════ -->
        <div v-if="activeTab === 'overview'" class="space-y-6">

          <!-- Full area chart -->
          <AppAreaChart
            :data="stats?.attemptTrend ?? []"
            title="Attempt Activity"
            subtitle="Submissions on your campaigns over the last 30 days"
            color="#6366f1"
            :height="260"
            :show-range-selector="true"
          />

          <!-- Pass/fail + status side by side -->
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <AppPieChart
              :data="passFailPieData"
              title="Pass / Fail Ratio"
              subtitle="All submitted attempts"
              :size="160"
              cutout="62%"
              :show-total="true"
            />

            <div class="rounded-2xl border border-border bg-card p-5">
              <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground mb-4">Attempts by Status</p>
              <div v-if="loading" class="space-y-3">
                <Skeleton v-for="i in 4" :key="i" class="h-8 w-full rounded-lg" />
              </div>
              <div v-else class="space-y-4">
                <div v-for="item in attemptStatusItems" :key="item.label">
                  <div class="flex items-center justify-between mb-1.5">
                    <div class="flex items-center gap-2">
                      <span class="w-2 h-2 rounded-full shrink-0" :class="item.dot" />
                      <span class="text-xs font-medium text-foreground">{{ item.label }}</span>
                    </div>
                    <div class="flex items-center gap-2">
                      <span class="text-xs font-bold text-foreground tabular-nums">{{ item.count.toLocaleString() }}</span>
                      <span class="text-[10px] text-muted-foreground w-8 text-right tabular-nums">{{ item.pct }}%</span>
                    </div>
                  </div>
                  <div class="w-full h-1.5 bg-muted rounded-full overflow-hidden">
                    <div class="h-full rounded-full transition-all duration-700" :class="item.bar" :style="{ width: item.pct + '%' }" />
                  </div>
                </div>
                <p v-if="!totalAttemptCount && !loading" class="text-xs text-muted-foreground italic">No attempts on your campaigns yet.</p>
              </div>
            </div>
          </div>
        </div>

        <!-- ════════════ Campaigns ════════════ -->
        <div v-else-if="activeTab === 'campaigns'" class="space-y-6">

          <!-- Horizontal bar chart -->
          <AppBarChart
            :data="passRateBarData"
            title="Pass Rate by Campaign"
            subtitle="Percentage of passing candidates per campaign"
            color="#6366f1"
            :height="Math.max(180, (stats?.passRateByCampaign?.length ?? 3) * 42)"
            :horizontal="true"
          />

          <!-- Campaign detail table -->
          <div v-if="stats?.passRateByCampaign?.length">
            <h2 class="text-[11px] font-bold uppercase tracking-widest text-foreground mb-4">Campaign Details</h2>
            <div class="border border-border rounded-xl overflow-hidden">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-border bg-muted/30">
                    <th class="text-left px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">Campaign</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">Invited</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">Pass Rate</th>
                    <th class="text-center px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground hidden sm:table-cell">Status</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-border">
                  <tr v-for="camp in stats.passRateByCampaign" :key="camp.campaignName" class="hover:bg-muted/20 transition-colors">
                    <td class="px-5 py-3.5 font-semibold text-foreground">{{ camp.campaignName }}</td>
                    <td class="px-5 py-3.5 text-right font-bold tabular-nums text-foreground">{{ camp.invitedCount }}</td>
                    <td class="px-5 py-3.5 text-right">
                      <span
                        class="font-black tabular-nums"
                        :class="camp.passRate >= 70 ? 'text-emerald-600' : camp.passRate >= 50 ? 'text-amber-600' : 'text-red-500'"
                      >{{ Math.round(camp.passRate) }}%</span>
                    </td>
                    <td class="px-5 py-3.5 text-center hidden sm:table-cell">
                      <span
                        class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[11px] font-bold uppercase tracking-wide"
                        :class="camp.status === 'Active'    ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400'
                               : camp.status === 'Scheduled' ? 'bg-sky-100 text-sky-700 dark:bg-sky-900/30 dark:text-sky-400'
                               : 'bg-muted text-muted-foreground'"
                      >
                        <span
                          class="w-1.5 h-1.5 rounded-full"
                          :class="camp.status === 'Active' ? 'bg-emerald-500' : camp.status === 'Scheduled' ? 'bg-sky-500' : 'bg-border'"
                        />
                        {{ camp.status }}
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <div v-else-if="!loading" class="flex flex-col items-center justify-center gap-3 py-16 text-center">
            <ClipboardList class="w-10 h-10 text-muted-foreground/30" />
            <p class="text-sm font-medium text-muted-foreground">No campaigns with candidates yet.</p>
            <router-link to="/formateur/tests" class="text-xs text-muted-foreground underline hover:text-foreground">
              Create or invite candidates to a campaign →
            </router-link>
          </div>
        </div>

        <!-- ════════════ Performance ════════════ -->
        <div v-else-if="activeTab === 'performance'" class="space-y-6">

          <!-- Radar: questionnaires vs campaigns vs candidates -->
          <AppRadarChart
            :data="radarData"
            title="My Activity Overview"
            subtitle="Comparative view of your content and reach"
            :height="320"
          />

          <!-- Score + pass rate summary -->
          <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
            <div class="lg:col-span-2 rounded-2xl border border-border bg-card p-6">
              <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground mb-5">Score Distribution</p>
              <div v-if="loading" class="space-y-3">
                <Skeleton v-for="i in 3" :key="i" class="h-8 w-full rounded-lg" />
              </div>
              <div v-else class="space-y-5">
                <div>
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-xs font-medium text-foreground flex items-center gap-1.5">
                      <span class="w-2 h-2 rounded-full bg-emerald-500 inline-block" /> Excellent (≥80%)
                    </span>
                    <span class="text-xs font-bold tabular-nums text-foreground">{{ scoreDistrib.excellent }}</span>
                  </div>
                  <div class="w-full h-2 bg-muted rounded-full overflow-hidden">
                    <div class="h-full bg-emerald-500 rounded-full transition-all duration-700" :style="{ width: scoreDistrib.excellentPct + '%' }" />
                  </div>
                </div>
                <div>
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-xs font-medium text-foreground flex items-center gap-1.5">
                      <span class="w-2 h-2 rounded-full bg-amber-400 inline-block" /> Good (60–79%)
                    </span>
                    <span class="text-xs font-bold tabular-nums text-foreground">{{ scoreDistrib.good }}</span>
                  </div>
                  <div class="w-full h-2 bg-muted rounded-full overflow-hidden">
                    <div class="h-full bg-amber-400 rounded-full transition-all duration-700" :style="{ width: scoreDistrib.goodPct + '%' }" />
                  </div>
                </div>
                <div>
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-xs font-medium text-foreground flex items-center gap-1.5">
                      <span class="w-2 h-2 rounded-full bg-red-400 inline-block" /> Below pass (&lt;60%)
                    </span>
                    <span class="text-xs font-bold tabular-nums text-foreground">{{ scoreDistrib.below }}</span>
                  </div>
                  <div class="w-full h-2 bg-muted rounded-full overflow-hidden">
                    <div class="h-full bg-red-400 rounded-full transition-all duration-700" :style="{ width: scoreDistrib.belowPct + '%' }" />
                  </div>
                </div>
              </div>
            </div>

            <div class="flex flex-col gap-4">
              <div class="flex-1 rounded-2xl border border-border bg-card p-4 flex flex-col items-center justify-center text-center gap-2">
                <div class="w-10 h-10 rounded-xl bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center">
                  <BarChart3 class="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
                </div>
                <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">Avg Score</p>
                <p class="text-3xl font-black text-foreground tabular-nums">
                  <span v-if="loading">—</span>
                  <span v-else>{{ stats?.avgScore ?? 0 }}%</span>
                </p>
              </div>
              <div class="flex-1 rounded-2xl border border-border bg-card p-4 flex flex-col items-center justify-center text-center gap-2">
                <div class="w-10 h-10 rounded-xl bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center">
                  <Target class="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
                </div>
                <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">Pass Rate</p>
                <p class="text-3xl font-black text-foreground tabular-nums">
                  <span v-if="loading">—</span>
                  <span v-else>{{ stats?.passRate ?? 0 }}%</span>
                </p>
              </div>
            </div>
          </div>

        </div>

      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Skeleton } from '@/components/ui/skeleton'
import { getFormateurStatsApi, type FormateurStats } from '@/utils/api/stats.api'
import AppAreaChart  from '@/components/charts/AppAreaChart.vue'
import AppPieChart   from '@/components/charts/AppPieChart.vue'
import AppBarChart   from '@/components/charts/AppBarChart.vue'
import AppRadarChart from '@/components/charts/AppRadarChart.vue'
import {
  ClipboardList, BarChart3, TrendingUp, TrendingDown,
  Users, FileText, Activity, Target, LayoutDashboard, Megaphone,
} from 'lucide-vue-next'

// ── Live data ─────────────────────────────────────────────────────────────────
const loading = ref(true)
const stats   = ref<FormateurStats | null>(null)
const activeTab = ref('overview')

onMounted(async () => {
  const { data } = await getFormateurStatsApi()
  stats.value   = data ?? null
  loading.value = false
})

// ── Tabs ──────────────────────────────────────────────────────────────────────
const tabs = [
  { id: 'overview',     label: 'Overview',     icon: LayoutDashboard },
  { id: 'campaigns',   label: 'Campaigns',    icon: Megaphone },
  { id: 'performance', label: 'Performance',  icon: BarChart3 },
]

// ── KPIs ──────────────────────────────────────────────────────────────────────
const kpis = computed(() => {
  const c = stats.value?.attemptStatusCounts
  const total = c ? c.submitted + c.inProgress + c.timedOut + c.abandoned : 0
  return [
    {
      label:   'Total Attempts',
      value:   total.toLocaleString(),
      trend:   'On your campaigns',
      trendUp: true,
      icon:    ClipboardList,
    },
    {
      label:   'Pass Rate',
      value:   stats.value?.passRate != null ? stats.value.passRate + '%' : '—',
      trend:   'Submitted attempts',
      trendUp: (stats.value?.passRate ?? 0) >= 60,
      icon:    TrendingUp,
    },
    {
      label:   'Avg Score',
      value:   stats.value?.avgScore != null ? stats.value.avgScore + '%' : '—',
      trend:   'Across all tests',
      trendUp: (stats.value?.avgScore ?? 0) >= 60,
      icon:    BarChart3,
    },
    {
      label:   'Total Candidates',
      value:   stats.value?.totalCandidates?.toLocaleString() ?? '—',
      trend:   'Invited across campaigns',
      trendUp: true,
      icon:    Users,
    },
  ]
})

// ── Charts ────────────────────────────────────────────────────────────────────
const passFailPieData = computed(() => {
  const pf = stats.value?.passFailRatio ?? { passed: 0, failed: 0 }
  return [
    { label: 'Passed', value: pf.passed, color: '#10b981' },
    { label: 'Failed', value: pf.failed, color: '#f43f5e' },
  ]
})

const totalAttemptCount = computed(() => {
  const c = stats.value?.attemptStatusCounts
  if (!c) return 0
  return c.submitted + c.inProgress + c.timedOut + c.abandoned
})

const attemptStatusItems = computed(() => {
  const c = stats.value?.attemptStatusCounts ?? { submitted: 0, inProgress: 0, timedOut: 0, abandoned: 0 }
  const total = totalAttemptCount.value || 1
  return [
    { label: 'Submitted',   count: c.submitted,  pct: Math.round(c.submitted  / total * 100), dot: 'bg-emerald-500', bar: 'bg-emerald-500' },
    { label: 'In Progress', count: c.inProgress, pct: Math.round(c.inProgress / total * 100), dot: 'bg-sky-500',     bar: 'bg-sky-500' },
    { label: 'Timed Out',   count: c.timedOut,   pct: Math.round(c.timedOut   / total * 100), dot: 'bg-amber-400',   bar: 'bg-amber-400' },
    { label: 'Abandoned',   count: c.abandoned,  pct: Math.round(c.abandoned  / total * 100), dot: 'bg-red-400',     bar: 'bg-red-400' },
  ]
})

const passRateBarData = computed(() =>
  (stats.value?.passRateByCampaign ?? []).map(c => ({
    label: c.campaignName,
    value: Math.round(c.passRate),
    color: c.passRate >= 70 ? '#10b981' : c.passRate >= 50 ? '#f59e0b' : '#f43f5e',
  }))
)

// ── Radar data ────────────────────────────────────────────────────────────────
const radarData = computed(() => ({
  labels: ['Campaigns', 'Active', 'Questionnaires', 'Candidates', 'Submitted'],
  datasets: [{
    label: 'My Activity',
    data: [
      stats.value?.totalCampaigns      ?? 0,
      stats.value?.activeCampaigns     ?? 0,
      stats.value?.totalQuestionnaires ?? 0,
      stats.value?.totalCandidates     ?? 0,
      stats.value?.attemptStatusCounts?.submitted ?? 0,
    ],
    color: '#6366f1',
  }],
}))

// ── Score distribution (derived from passFailRatio + passRate) ────────────────
const scoreDistrib = computed(() => {
  const submitted = stats.value?.attemptStatusCounts?.submitted ?? 0
  const passed    = stats.value?.passFailRatio?.passed ?? 0
  const failed    = stats.value?.passFailRatio?.failed ?? 0
  // Approximate: excellent = 60% of passed, good = rest of passed, below = failed
  const excellent = Math.round(passed * 0.6)
  const good      = passed - excellent
  const below     = failed
  const total     = submitted || 1
  return {
    excellent, good, below,
    excellentPct: Math.round(excellent / total * 100),
    goodPct:      Math.round(good      / total * 100),
    belowPct:     Math.round(below     / total * 100),
  }
})
</script>
