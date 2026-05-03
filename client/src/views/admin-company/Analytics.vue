<template>
  <div class="space-y-6">

    <!-- ── Header ─────────────────────────────────────────────────────────── -->
    <div>
      <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground mb-1.5">{{ t('companyPanel.analytics.subtitle') }}</p>
      <h1 class="text-3xl font-black tracking-tight text-foreground">{{ t('companyPanel.analytics.title') }}</h1>
    </div>

    <!-- ── KPI Strip ──────────────────────────────────────────────────────── -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="kpi in kpis" :key="kpi.label" class="rounded-2xl border border-border bg-card p-5">
        <div class="flex items-center gap-2 mb-3">
          <component :is="kpi.icon" class="w-3.5 h-3.5 text-muted-foreground" />
          <span class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ kpi.label }}</span>
        </div>
        <div class="text-3xl font-black text-foreground tracking-tight tabular-nums">
          <span v-if="statsLoading">—</span>
          <span v-else>{{ kpi.value }}</span>
        </div>
        <div class="flex items-center gap-1.5 mt-2">
          <TrendingUp v-if="kpi.trendUp" class="w-3 h-3 text-emerald-600" />
          <TrendingDown v-else class="w-3 h-3 text-amber-600" />
          <span class="text-xs font-medium" :class="kpi.trendUp ? 'text-emerald-600' : 'text-amber-600'">{{ kpi.trend }}</span>
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

          <AppAreaChart
            :data="liveStats?.attemptTrend ?? []"
            title="Attempt Activity"
            subtitle="Test submissions over time"
            color="#6366f1"
            :height="260"
            :show-range-selector="true"
          />

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
              <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground mb-4">{{ t('companyPanel.analytics.attemptsByStatus') }}</p>
              <div v-if="statsLoading" class="space-y-3">
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
                <p v-if="!totalAttemptCount && !statsLoading" class="text-xs text-muted-foreground italic">No data yet.</p>
              </div>
            </div>
          </div>
        </div>

        <!-- ════════════ Campaigns ════════════ -->
        <div v-else-if="activeTab === 'campaigns'" class="space-y-6">

          <AppBarChart
            :data="passRateBarData"
            :title="t('companyPanel.analytics.passRateByCampaign')"
            subtitle="Percentage of passing candidates per campaign"
            color="#6366f1"
            :height="Math.max(180, (liveStats?.passRateByCampaign?.length ?? 3) * 38)"
            :horizontal="true"
          />

          <div v-if="liveStats?.passRateByCampaign?.length">
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
                  <tr v-for="camp in liveStats.passRateByCampaign" :key="camp.campaignName" class="hover:bg-muted/20 transition-colors">
                    <td class="px-5 py-3.5 font-semibold text-foreground">{{ camp.campaignName }}</td>
                    <td class="px-5 py-3.5 text-right font-bold tabular-nums text-foreground">{{ camp.invitedCount }}</td>
                    <td class="px-5 py-3.5 text-right">
                      <span class="font-black tabular-nums" :class="camp.passRate >= 70 ? 'text-emerald-600' : camp.passRate >= 50 ? 'text-amber-600' : 'text-red-500'">
                        {{ Math.round(camp.passRate) }}%
                      </span>
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

          <div v-else-if="!statsLoading" class="flex flex-col items-center justify-center gap-3 py-16 text-center">
            <Megaphone class="w-10 h-10 text-muted-foreground/30" />
            <p class="text-sm font-medium text-muted-foreground">No campaign data available yet.</p>
            <p class="text-xs text-muted-foreground">Create a campaign with invited candidates to see stats here.</p>
          </div>
        </div>

        <!-- ════════════ Candidates ════════════ -->
        <div v-else-if="activeTab === 'candidates'" class="space-y-6">

          <AppAreaChart
            :data="liveStats?.attemptTrend ?? []"
            title="Candidate Activity"
            subtitle="Attempt submissions over time"
            color="#10b981"
            :height="220"
            :show-range-selector="true"
          />

          <div>
            <h2 class="text-[11px] font-bold uppercase tracking-widest text-foreground mb-4">{{ t('companyPanel.analytics.topCandidates') }}</h2>
            <div class="border border-border rounded-xl overflow-hidden">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-border bg-muted/30">
                    <th class="text-left px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">#</th>
                    <th class="text-left px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('companyPanel.analytics.candidate') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('companyPanel.analytics.attempts') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('companyPanel.analytics.avgScore') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('companyPanel.analytics.passRate') }}</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-border">
                  <tr v-for="(c, idx) in topCandidates" :key="c.id" class="hover:bg-muted/20 transition-colors">
                    <td class="px-5 py-3.5 text-sm font-bold tabular-nums text-muted-foreground">{{ idx + 1 }}</td>
                    <td class="px-5 py-3.5">
                      <div class="flex items-center gap-2.5">
                        <div class="w-7 h-7 rounded-full bg-foreground text-background flex items-center justify-center text-[10px] font-black shrink-0">{{ c.initials }}</div>
                        <div>
                          <p class="font-semibold text-foreground">{{ c.name }}</p>
                          <p class="text-xs text-muted-foreground">{{ c.email }}</p>
                        </div>
                      </div>
                    </td>
                    <td class="px-5 py-3.5 text-right font-semibold tabular-nums text-muted-foreground">{{ c.attempts }}</td>
                    <td class="px-5 py-3.5 text-right font-black tabular-nums" :class="c.avgScore >= 80 ? 'text-emerald-600' : c.avgScore >= 60 ? 'text-amber-600' : 'text-red-500'">
                      {{ c.avgScore }}%
                    </td>
                    <td class="px-5 py-3.5 text-right">
                      <span
                        class="text-[10px] font-bold uppercase tracking-wide px-2 py-0.5 rounded-md"
                        :class="c.passRate >= 70 ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-400' : 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-400'"
                      >{{ c.passRate }}%</span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useLocale } from '@/composables/useLocale'
import { Skeleton } from '@/components/ui/skeleton'
import { getCompanyStatsApi, type CompanyStats } from '@/utils/api/stats.api'
import AppAreaChart from '@/components/charts/AppAreaChart.vue'
import AppPieChart  from '@/components/charts/AppPieChart.vue'
import AppBarChart  from '@/components/charts/AppBarChart.vue'
import {
  ClipboardList, BarChart3, TrendingUp, TrendingDown, Users,
  LayoutDashboard, Megaphone,
} from 'lucide-vue-next'

const { t } = useLocale()
const activeTab = ref('overview')

// ── Live stats ────────────────────────────────────────────────────────────────
const statsLoading = ref(true)
const liveStats    = ref<CompanyStats | null>(null)

onMounted(async () => {
  const { data } = await getCompanyStatsApi()
  liveStats.value    = data ?? null
  statsLoading.value = false
})

// ── Tabs ──────────────────────────────────────────────────────────────────────
const tabs = [
  { id: 'overview',   label: 'Overview',   icon: LayoutDashboard },
  { id: 'campaigns',  label: 'Campaigns',  icon: Megaphone },
  { id: 'candidates', label: 'Candidates', icon: Users },
]

// ── KPIs (live) ───────────────────────────────────────────────────────────────
const kpis = computed(() => {
  const c = liveStats.value?.attemptStatusCounts
  const totalAttempts = c ? c.submitted + c.inProgress + c.timedOut + c.abandoned : 0
  return [
    {
      label:   t('companyPanel.analytics.kpis.totalAttempts'),
      value:   totalAttempts.toLocaleString(),
      trend:   'All-time total',
      trendUp: true,
      icon:    ClipboardList,
    },
    {
      label:   t('companyPanel.analytics.kpis.avgPassRate'),
      value:   liveStats.value?.passRate != null ? liveStats.value.passRate + '%' : '—',
      trend:   'Submitted attempts',
      trendUp: (liveStats.value?.passRate ?? 0) >= 60,
      icon:    TrendingUp,
    },
    {
      label:   t('companyPanel.analytics.kpis.avgScore'),
      value:   liveStats.value?.avgScore != null ? liveStats.value.avgScore + '%' : '—',
      trend:   'Average across all tests',
      trendUp: (liveStats.value?.avgScore ?? 0) >= 60,
      icon:    BarChart3,
    },
    {
      label:   t('companyPanel.analytics.kpis.activeCampaigns'),
      value:   liveStats.value?.activeCampaigns?.toLocaleString() ?? '—',
      trend:   'Running now',
      trendUp: true,
      icon:    Users,
    },
  ]
})

// ── Attempt status (live) ─────────────────────────────────────────────────────
const totalAttemptCount = computed(() => {
  const c = liveStats.value?.attemptStatusCounts
  if (!c) return 0
  return c.submitted + c.inProgress + c.timedOut + c.abandoned
})

const attemptStatusItems = computed(() => {
  const c = liveStats.value?.attemptStatusCounts ?? { submitted: 0, inProgress: 0, timedOut: 0, abandoned: 0 }
  const total = totalAttemptCount.value || 1
  return [
    { label: 'Submitted',   count: c.submitted,  pct: Math.round(c.submitted  / total * 100), dot: 'bg-emerald-500', bar: 'bg-emerald-500' },
    { label: 'In Progress', count: c.inProgress, pct: Math.round(c.inProgress / total * 100), dot: 'bg-sky-500',     bar: 'bg-sky-500' },
    { label: 'Timed Out',   count: c.timedOut,   pct: Math.round(c.timedOut   / total * 100), dot: 'bg-amber-400',   bar: 'bg-amber-400' },
    { label: 'Abandoned',   count: c.abandoned,  pct: Math.round(c.abandoned  / total * 100), dot: 'bg-red-400',     bar: 'bg-red-400' },
  ]
})

// ── Charts ────────────────────────────────────────────────────────────────────
const passFailPieData = computed(() => {
  const pf = liveStats.value?.passFailRatio ?? { passed: 0, failed: 0 }
  return [
    { label: 'Passed', value: pf.passed, color: '#10b981' },
    { label: 'Failed', value: pf.failed, color: '#f43f5e' },
  ]
})

const passRateBarData = computed(() =>
  (liveStats.value?.passRateByCampaign ?? []).map(c => ({
    label: c.campaignName,
    value: Math.round(c.passRate),
    color: c.passRate >= 70 ? '#10b981' : c.passRate >= 50 ? '#f59e0b' : '#f43f5e',
  }))
)

// ── Static: top candidates (placeholder — no backend endpoint yet) ────────────
const topCandidates = [
  { id: 1, name: 'Alice Martin',  email: 'alice@techcorp.com',  initials: 'AM', attempts: 5, avgScore: 91, passRate: 100 },
  { id: 2, name: 'Eva Moreau',    email: 'eva@techcorp.com',    initials: 'EM', attempts: 4, avgScore: 86, passRate: 100 },
  { id: 3, name: 'Hugo Bernard',  email: 'hugo@techcorp.com',   initials: 'HB', attempts: 3, avgScore: 79, passRate: 67  },
  { id: 4, name: 'Bob Dupont',    email: 'bob@techcorp.com',    initials: 'BD', attempts: 6, avgScore: 74, passRate: 83  },
  { id: 5, name: 'Clara Reyes',   email: 'clara@techcorp.com',  initials: 'CR', attempts: 2, avgScore: 68, passRate: 50  },
]
</script>
