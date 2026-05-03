<template>
  <div class="space-y-6">

    <!-- ── Header ─────────────────────────────────────────────────────────── -->
    <div>
      <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground mb-1.5">Candidat Panel</p>
      <h1 class="text-3xl font-black tracking-tight text-foreground">My </h1>
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

          <!-- Attempt activity area chart -->
          <AppAreaChart
            :data="stats?.attemptTrend ?? []"
            title="My Attempt Activity"
            subtitle="Test submissions over the last 30 days"
            color="#6366f1"
            :height="260"
            :show-range-selector="true"
          />

          <!-- Pass/Fail donut + score gauge side by side -->
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <AppPieChart
              :data="passFailPieData"
              title="Pass / Fail Ratio"
              subtitle="All my submitted attempts"
              :size="160"
              cutout="62%"
              :show-total="true"
            />

            <!-- Score summary card -->
            <div class="rounded-2xl border border-border bg-card p-5 flex flex-col justify-between gap-5">
              <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">Score Summary</p>
              <div class="space-y-4">
                <div>
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-xs font-medium text-foreground flex items-center gap-1.5">
                      <span class="w-2 h-2 rounded-full bg-emerald-500 inline-block" /> Avg Score
                    </span>
                    <span class="text-xs font-black tabular-nums text-emerald-600">
                      <span v-if="loading">—</span>
                      <span v-else>{{ stats?.avgScore ?? 0 }}%</span>
                    </span>
                  </div>
                  <div class="w-full h-2 bg-muted rounded-full overflow-hidden">
                    <div
                      class="h-full bg-emerald-500 rounded-full transition-all duration-700"
                      :style="{ width: (stats?.avgScore ?? 0) + '%' }"
                    />
                  </div>
                </div>
                <div>
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-xs font-medium text-foreground flex items-center gap-1.5">
                      <span class="w-2 h-2 rounded-full bg-indigo-500 inline-block" /> Pass Rate
                    </span>
                    <span class="text-xs font-black tabular-nums text-indigo-600">
                      <span v-if="loading">—</span>
                      <span v-else>{{ stats?.passRate ?? 0 }}%</span>
                    </span>
                  </div>
                  <div class="w-full h-2 bg-muted rounded-full overflow-hidden">
                    <div
                      class="h-full bg-indigo-500 rounded-full transition-all duration-700"
                      :style="{ width: (stats?.passRate ?? 0) + '%' }"
                    />
                  </div>
                </div>
                <div>
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-xs font-medium text-foreground flex items-center gap-1.5">
                      <span class="w-2 h-2 rounded-full bg-amber-400 inline-block" /> Completion Rate
                    </span>
                    <span class="text-xs font-black tabular-nums text-amber-600">
                      <span v-if="loading">—</span>
                      <span v-else>{{ completionRate }}%</span>
                    </span>
                  </div>
                  <div class="w-full h-2 bg-muted rounded-full overflow-hidden">
                    <div
                      class="h-full bg-amber-400 rounded-full transition-all duration-700"
                      :style="{ width: completionRate + '%' }"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- ════════════ Results ════════════ -->
        <div v-else-if="activeTab === 'results'" class="space-y-6">

          <!-- Bar chart of scores by campaign -->
          <AppBarChart
            :data="scoreBarData"
            title="Score per Campaign"
            subtitle="Your latest results per test"
            color="#6366f1"
            :height="Math.max(180, (stats?.scoresByCampaign?.length ?? 3) * 42)"
            :horizontal="true"
          />

          <!-- Results table -->
          <div v-if="stats?.scoresByCampaign?.length">
            <h2 class="text-[11px] font-bold uppercase tracking-widest text-foreground mb-4">All Results</h2>
            <div class="border border-border rounded-xl overflow-hidden">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-border bg-muted/30">
                    <th class="text-left px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">Campaign</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">Score</th>
                    <th class="text-center px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">Result</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground hidden sm:table-cell">Date</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-border">
                  <tr v-for="r in stats.scoresByCampaign" :key="r.campaignName + r.submittedAt" class="hover:bg-muted/20 transition-colors">
                    <td class="px-5 py-3.5 font-semibold text-foreground">{{ r.campaignName }}</td>
                    <td class="px-5 py-3.5 text-right">
                      <span
                        class="font-black tabular-nums"
                        :class="r.score >= 80 ? 'text-emerald-600' : r.score >= 60 ? 'text-amber-600' : 'text-red-500'"
                      >{{ Math.round(r.score) }}%</span>
                    </td>
                    <td class="px-5 py-3.5 text-center">
                      <span
                        class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[11px] font-bold uppercase tracking-wide"
                        :class="r.passed
                          ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400'
                          : 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400'"
                      >
                        <CheckCircle2 v-if="r.passed" class="w-3 h-3" />
                        <XCircle v-else class="w-3 h-3" />
                        {{ r.passed ? 'Passed' : 'Failed' }}
                      </span>
                    </td>
                    <td class="px-5 py-3.5 text-right text-muted-foreground text-xs hidden sm:table-cell">{{ formatDate(r.submittedAt) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <div v-else-if="!loading" class="flex flex-col items-center justify-center gap-3 py-16 text-center">
            <ClipboardList class="w-10 h-10 text-muted-foreground/30" />
            <p class="text-sm font-medium text-muted-foreground">No completed tests yet.</p>
            <router-link to="/candidat/tests" class="text-xs text-muted-foreground underline hover:text-foreground">
              Go to my tests →
            </router-link>
          </div>
        </div>

        <!-- ════════════ Progress ════════════ -->
        <div v-else-if="activeTab === 'progress'" class="space-y-6">

          <!-- Radar chart: my stats -->
          <AppRadarChart
            :data="radarData"
            title="My Performance Overview"
            subtitle="A snapshot of your test activity and achievements"
            :height="320"
          />

          <!-- Achievement cards -->
          <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
            <div
              v-for="ach in achievements"
              :key="ach.label"
              class="rounded-2xl border border-border bg-card p-5 flex flex-col items-center gap-3 text-center"
            >
              <div :class="ach.iconBg" class="w-12 h-12 rounded-2xl flex items-center justify-center">
                <component :is="ach.icon" :class="ach.iconColor" class="w-6 h-6" />
              </div>
              <div>
                <div class="text-2xl font-black text-foreground tabular-nums">
                  <span v-if="loading">—</span>
                  <span v-else>{{ ach.value }}</span>
                </div>
                <p class="text-xs font-medium text-muted-foreground mt-0.5">{{ ach.label }}</p>
              </div>
            </div>
          </div>

          <!-- Certificates section -->
          <div class="rounded-2xl border border-border bg-card p-5">
            <div class="flex items-center justify-between mb-4">
              <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">Certificates Earned</p>
              <router-link to="/candidat/certificates" class="flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground font-medium">
                View all <ArrowRight class="w-3 h-3" />
              </router-link>
            </div>
            <div v-if="loading">
              <Skeleton class="h-16 w-full rounded-xl" />
            </div>
            <div v-else-if="(stats?.certificatesEarned ?? 0) > 0" class="flex items-center gap-4 p-4 rounded-xl bg-amber-50 dark:bg-amber-900/20 border border-amber-100 dark:border-amber-800/40">
              <div class="w-12 h-12 rounded-xl bg-amber-100 dark:bg-amber-900/40 flex items-center justify-center shrink-0">
                <Award class="w-6 h-6 text-amber-600 dark:text-amber-400" />
              </div>
              <div>
                <p class="text-lg font-black text-foreground">{{ stats!.certificatesEarned }} Certificate{{ stats!.certificatesEarned > 1 ? 's' : '' }} Earned</p>
                <p class="text-xs text-muted-foreground">Keep going — every pass adds to your profile!</p>
              </div>
            </div>
            <div v-else class="flex items-center gap-3 p-4 rounded-xl bg-muted/30 border border-border">
              <Award class="w-8 h-8 text-muted-foreground/40 shrink-0" />
              <div>
                <p class="text-sm font-semibold text-foreground">No certificates yet</p>
                <p class="text-xs text-muted-foreground">Complete and pass tests to earn certificates.</p>
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
import { getCandidatStatsApi, type CandidatStats } from '@/utils/api/stats.api'
import AppAreaChart  from '@/components/charts/AppAreaChart.vue'
import AppPieChart   from '@/components/charts/AppPieChart.vue'
import AppBarChart   from '@/components/charts/AppBarChart.vue'
import AppRadarChart from '@/components/charts/AppRadarChart.vue'
import {
  ClipboardList, BarChart3, TrendingUp, TrendingDown, Award, Target,
  CheckCircle2, XCircle, ArrowRight, LayoutDashboard, ClipboardCheck,
} from 'lucide-vue-next'

// ── Data ──────────────────────────────────────────────────────────────────────
const loading   = ref(true)
const stats     = ref<CandidatStats | null>(null)
const activeTab = ref('overview')

onMounted(async () => {
  const { data } = await getCandidatStatsApi()
  stats.value   = data ?? null
  loading.value = false
})

// ── Tabs ──────────────────────────────────────────────────────────────────────
const tabs = [
  { id: 'overview',  label: 'Overview',  icon: LayoutDashboard },
  { id: 'results',   label: 'Results',   icon: ClipboardCheck },
  { id: 'progress',  label: 'Progress',  icon: Award },
]

// ── KPIs ──────────────────────────────────────────────────────────────────────
const kpis = computed(() => [
  {
    label:   'Campaigns',
    value:   stats.value?.totalCampaigns?.toLocaleString() ?? '—',
    trend:   'Invited to',
    trendUp: true,
    icon:    ClipboardList,
  },
  {
    label:   'Completed',
    value:   stats.value?.completedAttempts?.toLocaleString() ?? '—',
    trend:   'Tests submitted',
    trendUp: true,
    icon:    ClipboardCheck,
  },
  {
    label:   'Pass Rate',
    value:   stats.value?.passRate != null ? stats.value.passRate + '%' : '—',
    trend:   'Overall success',
    trendUp: (stats.value?.passRate ?? 0) >= 60,
    icon:    Target,
  },
  {
    label:   'Avg Score',
    value:   stats.value?.avgScore != null ? stats.value.avgScore + '%' : '—',
    trend:   'Across all tests',
    trendUp: (stats.value?.avgScore ?? 0) >= 60,
    icon:    BarChart3,
  },
])

// ── Pass / Fail donut ─────────────────────────────────────────────────────────
const passFailPieData = computed(() => {
  const pf = stats.value?.passFailRatio ?? { passed: 0, failed: 0 }
  return [
    { label: 'Passed', value: pf.passed, color: '#10b981' },
    { label: 'Failed', value: pf.failed, color: '#f43f5e' },
  ]
})

// ── Completion rate ───────────────────────────────────────────────────────────
const completionRate = computed(() => {
  const total     = stats.value?.totalCampaigns ?? 0
  const completed = stats.value?.completedAttempts ?? 0
  return total > 0 ? Math.round(completed / total * 100) : 0
})

// ── Score bar chart ───────────────────────────────────────────────────────────
const scoreBarData = computed(() =>
  (stats.value?.scoresByCampaign ?? []).map(r => ({
    label: r.campaignName,
    value: Math.round(r.score),
    color: r.passed ? '#10b981' : '#f43f5e',
  }))
)

// ── Radar data ────────────────────────────────────────────────────────────────
const radarData = computed(() => ({
  labels: ['Campaigns', 'Completed', 'Passed', 'Certificates', 'Avg Score'],
  datasets: [{
    label: 'My Stats',
    data: [
      stats.value?.totalCampaigns      ?? 0,
      stats.value?.completedAttempts   ?? 0,
      stats.value?.passFailRatio?.passed ?? 0,
      stats.value?.certificatesEarned  ?? 0,
      stats.value?.avgScore            ?? 0,
    ],
    color: '#6366f1',
  }],
}))

// ── Achievement cards ─────────────────────────────────────────────────────────
const achievements = computed(() => [
  {
    label:     'Tests Taken',
    value:     stats.value?.completedAttempts ?? '—',
    icon:      ClipboardList,
    iconBg:    'bg-indigo-100 dark:bg-indigo-900/30',
    iconColor: 'text-indigo-600 dark:text-indigo-400',
  },
  {
    label:     'Tests Passed',
    value:     stats.value?.passFailRatio?.passed ?? '—',
    icon:      CheckCircle2,
    iconBg:    'bg-emerald-100 dark:bg-emerald-900/30',
    iconColor: 'text-emerald-600 dark:text-emerald-400',
  },
  {
    label:     'Certificates',
    value:     stats.value?.certificatesEarned ?? '—',
    icon:      Award,
    iconBg:    'bg-amber-100 dark:bg-amber-900/30',
    iconColor: 'text-amber-600 dark:text-amber-400',
  },
  {
    label:     'Avg Score',
    value:     stats.value?.avgScore != null ? stats.value.avgScore + '%' : '—',
    icon:      BarChart3,
    iconBg:    'bg-sky-100 dark:bg-sky-900/30',
    iconColor: 'text-sky-600 dark:text-sky-400',
  },
])

// ── Helpers ───────────────────────────────────────────────────────────────────
function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}
</script>
