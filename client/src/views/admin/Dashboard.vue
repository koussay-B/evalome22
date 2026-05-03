<template>
  <div class="space-y-6">

    <!-- ── Header ─────────────────────────────────────────────────────────── -->
    <div class="flex items-end justify-between">
      <div>
        <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">{{ t('dashboard.overview') }}</p>
        <h1 class="text-3xl text-slate-800 tracking-tight text-foreground">{{ t('dashboard.title') }}</h1>
      </div>
      <span class="text-xs font-medium text-muted-foreground bg-muted px-3 py-1.5 rounded-full border border-border">{{ today }}</span>
    </div>

    <!-- ── KPI Cards ───────────────────────────────────────────────────────── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3">
      <div
        v-for="stat in kpiCards"
        :key="stat.label"
        class="rounded-xl border border-stone-200/60 bg-slate-50 p-3.5 flex items-center gap-4 hover:shadow-md transition-all duration-300"
      >
        <div
          class="w-11 h-11 rounded-xl flex items-center justify-center shrink-0"
          :class="stat.iconBg || 'bg-[oklch(97.5%_0.02_51.057_/_0.925)]'"
        >
          <component
            :is="stat.icon"
            class="w-5 h-5 stroke-[1.8px]"
            :class="stat.iconColor || 'text-[oklch(65%_0.18_51.057)]'"
          />
        </div>

        <div class="flex flex-col min-w-0">
          <span class="text-xl font-semibold text-slate-900 tracking-tight tabular-nums leading-none">
            <span v-if="statsLoading" class="opacity-30">—</span>
            <span v-else>{{ stat.value }}</span>
          </span>

          <p class="text-[10px] text-slate-400 uppercase tracking-widest mt-1.5 truncate">
            {{ stat.label }}
          </p>
        </div>
      </div>
    </div>

    <!-- ── Bar Chart (members per company) + Line Chart (member trend) ────── -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">

      <div class="rounded-2xl border border-border bg-card p-5">
        <div class="flex items-center justify-between mb-5">
          <div>
            <p class="text-[10px] font-bold uppercase tracking-[0.2em] text-primary/60 leading-none mb-1.5">{{ t('dashboard.titles.membersPerCompany') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">{{ memberRoleSubtitle }}</p>
          </div>
          <div class="flex items-center gap-0.5 rounded-lg border border-border p-0.5 bg-background">
            <button
              v-for="r in MEMBER_ROLE_FILTERS"
              :key="r.key"
              @click="activeMemberRole = r.key"
              class="px-2.5 py-1 rounded-md text-xs font-bold transition-all"
              :class="activeMemberRole === r.key
                ? 'bg-foreground text-background shadow-sm'
                : 'text-muted-foreground hover:text-slate-800'"
            >
              {{ r.label }}
            </button>
          </div>
        </div>

        <div :style="{ height: '240px' }">
          <Bar v-if="hasCompanyMembersData" :data="memberBarChartData" :options="memberBarChartOpts" />
          <div v-else class="h-full flex items-center justify-center text-xs text-muted-foreground italic">
            No data available
          </div>
        </div>

        <div v-if="hasCompanyMembersData" class="flex items-center justify-between mt-4 pt-3 border-t border-border">
          <div class="flex items-center gap-1.5 text-xs text-muted-foreground">
            <span class="w-2.5 h-2.5 rounded-sm shrink-0" style="background-color: #f97316" />
            <span>{{ memberRoleSubtitle }}</span>
          </div>
          <span class="text-xs text-muted-foreground tabular-nums">
            Max: <strong class="text-slate-800">{{ memberMaxValue.toLocaleString() }}</strong>
          </span>
        </div>
      </div>

      <!-- Member growth line chart -->
      <div class="rounded-2xl border border-border bg-card p-5">
        <div class="flex items-center justify-between mb-5">
          <div>
            <p class="text-[10px] font-bold uppercase tracking-[0.2em] text-primary/60 leading-none mb-1.5">{{ t('dashboard.titles.memberEvolution') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">{{ t('dashboard.titles.memberTrendDesc') }}</p>
          </div>
          <div class="flex items-center gap-0.5 rounded-lg border border-border p-0.5 bg-background">
            <button
              v-for="r in MEMBER_TREND_RANGES"
              :key="r.days"
              @click="activeMemberTrendRange = r.days"
              class="px-2.5 py-1 rounded-md text-xs font-bold transition-all"
              :class="activeMemberTrendRange === r.days
                ? 'bg-foreground text-background shadow-sm'
                : 'text-muted-foreground hover:text-slate-800'"
            >
              {{ r.label }}
            </button>
          </div>
        </div>

        <div :style="{ height: '240px' }">
          <Line v-if="memberTrendFiltered.length" :data="memberTrendChartData" :options="memberTrendChartOpts" />
          <div v-else class="h-full flex items-center justify-center text-xs text-muted-foreground italic">
            No data available
          </div>
        </div>

        <div v-if="memberTrendFiltered.length" class="flex items-center justify-between mt-4 pt-3 border-t border-border">
          <div class="flex items-center gap-1.5 text-xs text-muted-foreground">
            <TrendingUp class="w-3.5 h-3.5 text-emerald-500" />
            <span>Current: <strong class="text-slate-800">{{ memberTrendCurrent.toLocaleString() }}</strong></span>
          </div>
          <span class="text-xs text-muted-foreground">Last {{ activeMemberTrendRange }}D</span>
        </div>
      </div>
    </div>

    <!-- Mini stats horizontal -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
      <div
        v-for="mini in miniStats"
        :key="mini.label"
        class="rounded-2xl border border-border bg-slate-50 px-5 py-4 flex items-center gap-4"
      >
        <div :class="mini.iconBg" class="w-10 h-10 rounded-xl flex items-center justify-center shrink-0">
          <component :is="mini.icon" :class="mini.iconColor" class="w-4.5 h-4.5" />
        </div>
        <div class="min-w-0">
          <p class="text-[11px] text-muted-foreground uppercase tracking-widest">{{ mini.label }}</p>
          <p class="text-2xl text-foreground tabular-nums mt-0.5">
            <span v-if="statsLoading">—</span>
            <span v-else>{{ mini.value }}</span>
          </p>
        </div>
      </div>
    </div>

    <!-- ── Row 3: Donut + Bar Chart + Attempt Status ──────────────────────── -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">

      <!-- Role distribution donut -->
      <AppPieChart
        :data="rolePieData"
        :title="t('dashboard.titles.roleDistribution')"
        subtitle="All registered users by role"
        :size="160"
        cutout="62%"
        :show-total="true"
      />

      <!-- Pass rates bar chart -->
      <AppBarChart
        :data="passRateBarData"
        :title="t('dashboard.titles.passRates')"
        subtitle="By company"
        color="#f97316"
        :height="200"
        :horizontal="false"
      />
    <!-- Campagnes par entreprise -->
    <AppBarChart
      :data="campaignsPerCompanyData"
      :title="t('dashboard.titles.campaignsPerCompany')"
      subtitle="Nombre de campagnes actives"
      color="#6366f1"
      :height="200"
      :horizontal="false"
    />

      
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useLocale } from '@/composables/useLocale'
import { useUserStore } from '@/store/userStore'
import { Skeleton } from '@/components/ui/skeleton'
import { getAdminStatsApi, type AdminStats } from '@/utils/api/stats.api'
import AppPieChart from '@/components/charts/AppPieChart.vue'
import AppBarChart from '@/components/charts/AppBarChart.vue'
import { Bar, Line } from 'vue-chartjs'
import {
  Chart as ChartJS, CategoryScale, LinearScale,
  BarElement, BarController, Tooltip,
  PointElement, LineElement, LineController, Filler,
} from 'chart.js'
import {
  Users, Building2, ClipboardList, BookOpen,
  TrendingUp, Target, BarChart3, Award, Zap,
} from 'lucide-vue-next'
import { useNotificationStore } from '@/store/notificationStore'

ChartJS.register(
  CategoryScale, LinearScale, BarElement, BarController, Tooltip,
  PointElement, LineElement, LineController, Filler
)

const { t } = useLocale()
const userStore = useUserStore()
const notificationStore = useNotificationStore()
const today = new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })

// ── Live stats ───────────────────────────────────────────────────────────────
const statsLoading = ref(true)
const liveStats    = ref<AdminStats | null>(null)

onMounted(async () => {
  userStore.list.fetchPage({ pageSize: 5, pageNumber: 1, orderBy: 'username' })
  const { data, error } = await getAdminStatsApi()

  console.log('=== ADMIN STATS DEBUG ===')
  console.log('error:', error)
  console.log('data:', data)
  console.log('passRates:', data?.passRates)
  console.log('campaignsPerCompany:', data?.campaignsPerCompany)
  console.log('companyMembers:', data?.companyMembers)
  console.log('full data:', JSON.stringify(data, null, 2))
  console.log('=========================')

  liveStats.value = data ?? null
  statsLoading.value = false
})

// ── Campaigns per company ────────────────────────────────────────────────────
const campaignsPerCompanyData = computed(() =>
  (liveStats.value?.campaignsPerCompany ?? []).map(r => ({
    label: r.companyName,
    value: r.count,
  }))
)

// ── KPI cards ────────────────────────────────────────────────────────────────
const kpiCards = computed(() => [
  {
    label:     t('dashboard.stats.companies'),
    value:     liveStats.value?.totalCompanies ?? '–',
    icon:      Building2,
    iconBg:    'bg-primary/12',
    iconColor: 'text-primary',
    trend: '+2', trendUp: true,
  },
  {
    label:     t('dashboard.stats.totalUsers'),
    value:     liveStats.value?.totalUsers ?? '–',
    icon:      Users,
    iconBg:    'bg-orange-100 dark:bg-orange-950/30',
    iconColor: 'text-orange-600 dark:text-orange-400',
    trend: '+8%', trendUp: true,
  },
  {
    label:     t('dashboard.stats.questionnaires'),
    value:     liveStats.value?.totalQuestionnaires ?? '–',
    icon:      BookOpen,
    iconBg:    'bg-amber-100 dark:bg-amber-900/30',
    iconColor: 'text-amber-600 dark:text-amber-400',
    trend: '+5', trendUp: true,
  },
  {
    label:     t('dashboard.stats.activeCampaigns'),
    value:     liveStats.value?.totalCampaigns ?? '–',
    icon:      ClipboardList,
    iconBg:    'bg-emerald-100 dark:bg-emerald-900/30',
    iconColor: 'text-emerald-600 dark:text-emerald-400',
    trend: '+3', trendUp: true,
  },
  {
    label:     'Online Users',
    value:     liveStats.value?.onlineUsersCount ?? 0,
    icon:      Zap,
    iconBg:    'bg-indigo-100 dark:bg-indigo-900/30',
    iconColor: 'text-indigo-600 dark:text-indigo-400',
    trend: 'Live', trendUp: true,
  },
])

// ── Mini stats ────────────────────────────────────────────────────────────────
const miniStats = computed(() => [
  {
    label:     'Total Attempts',
    value:     liveStats.value?.totalAttempts?.toLocaleString() ?? '–',
    icon:      Target,
    iconBg:    'bg-primary/12',
    iconColor: 'text-primary',
  },
  {
    label:     'Avg Pass Rate',
    value:     liveStats.value?.avgPassRate != null ? liveStats.value.avgPassRate + '%' : '–',
    icon:      BarChart3,
    iconBg:    'bg-emerald-100 dark:bg-emerald-900/30',
    iconColor: 'text-emerald-600 dark:text-emerald-400',
  },
  {
    label:     'Avg Score',
    value:     liveStats.value?.avgScore != null ? liveStats.value.avgScore + '%' : '–',
    icon:      Award,
    iconBg:    'bg-amber-100 dark:bg-amber-900/30',
    iconColor: 'text-amber-600 dark:text-amber-400',
  },
])

// ── Member trend line chart ──────────────────────────────────────────────────
const MEMBER_TREND_RANGES = [
  { label: '7D',  days: 7  },
  { label: '30D', days: 30 },
  { label: '90D', days: 90 },
] as const

const activeMemberTrendRange = ref<7 | 30 | 90>(30)

const memberTrendFiltered = computed(() => {
  const trend = liveStats.value?.memberTrend ?? []
  return trend.slice(-activeMemberTrendRange.value)
})

const memberTrendCurrent = computed(() => liveStats.value?.totalUsers ?? 0)

const memberTrendChartData = computed(() => ({
  labels: memberTrendFiltered.value.map(d =>
    new Date(d.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
  ),
  datasets: [{
    label:           'Members',
    fill:            true,
    tension:         0.4,
    data:            memberTrendFiltered.value.map(d => d.count),
    borderColor:     '#10b981',
    backgroundColor: (ctx: any) => {
      const chart = ctx.chart
      const { ctx: c, chartArea } = chart
      if (!chartArea) return 'transparent'
      const g = c.createLinearGradient(0, chartArea.top, 0, chartArea.bottom)
      g.addColorStop(0, 'rgba(16,185,129,0.32)')
      g.addColorStop(1, 'rgba(16,185,129,0.00)')
      return g
    },
    borderWidth:      2,
    pointRadius:      memberTrendFiltered.value.length < 15 ? 3 : 1,
    pointHoverRadius: 5,
  }],
}))

const memberTrendChartOpts = computed(() => ({
  responsive:          true,
  maintainAspectRatio: false,
  interaction: { mode: 'index' as const, intersect: false },
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: '#1a1a1a',
      titleColor:      '#ffffff',
      bodyColor:       '#a1a1aa',
      borderColor:     '#27272a',
      borderWidth:     1,
      padding:         10,
      displayColors:   false,
      callbacks: {
        label: (ctx: any) => ` ${ctx.raw.toLocaleString()} members`,
      },
    },
  },
  scales: {
    x: {
      grid:   { display: false },
      border: { display: false },
      ticks:  { color: '#71717a', font: { size: 11 }, maxRotation: 0, autoSkipPadding: 16 },
    },
    y: {
      grid:        { color: 'rgba(128,128,128,0.07)' },
      border:      { display: false },
      beginAtZero: true,
      ticks:       { color: '#71717a', font: { size: 11 }, precision: 0 },
    },
  },
}))

// ── Members per company bar chart ────────────────────────────────────────────
type MemberRoleKey = 'all' | 'condidat' | 'formateur' | 'companyAdmin'

const MEMBER_ROLE_FILTERS: { key: MemberRoleKey; label: string; subtitle: string }[] = [
  { key: 'all',          label: t('common.all'),      subtitle: 'Total members per company' },
  { key: 'condidat',     label: t('users.roles.condidat'),  subtitle: 'Candidats per company' },
  { key: 'formateur',    label: t('users.roles.formateur'), subtitle: 'Formateurs per company' },
  { key: 'companyAdmin', label: t('users.roles.companyAdmin'),     subtitle: 'Company admins per company' },
]

const activeMemberRole = ref<MemberRoleKey>('all')

const memberRoleSubtitle = computed(() =>
  MEMBER_ROLE_FILTERS.find(r => r.key === activeMemberRole.value)?.subtitle ?? ''
)

const companyMembersBarData = computed(() => {
  const rows = (liveStats.value?.companyMembers ?? []).map(c => {
    const value = activeMemberRole.value === 'all'
      ? c.total
      : (c[activeMemberRole.value] ?? 0)
    return { label: c.companyName, value }
  })
  return rows.sort((a, b) => b.value - a.value)
})

const hasCompanyMembersData = computed(() => companyMembersBarData.value.some(d => d.value > 0))
const memberMaxValue = computed(() => Math.max(...companyMembersBarData.value.map(d => d.value), 1))

const memberBarChartData = computed(() => ({
  labels: companyMembersBarData.value.map(d => d.label),
  datasets: [{
    data:                 companyMembersBarData.value.map(d => d.value),
    backgroundColor:      '#f97316cc',
    hoverBackgroundColor: '#f97316',
    borderRadius:         6,
    borderSkipped:        false,
    borderWidth:          0,
    barThickness:         18,
  }],
}))

const memberBarChartOpts = computed(() => ({
  indexAxis:           'y' as const,
  responsive:          true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: '#1a1a1a',
      titleColor:      '#ffffff',
      bodyColor:       '#a1a1aa',
      borderColor:     '#27272a',
      borderWidth:     1,
      padding:         10,
      displayColors:   false,
      callbacks: {
        label: (ctx: any) => ` ${ctx.raw.toLocaleString()} members`,
      },
    },
  },
  scales: {
    x: {
      grid:        { display: false, color: 'rgba(128,128,128,0.07)' },
      border:      { display: false },
      beginAtZero: true,
      ticks:       { color: '#71717a', font: { size: 11 }, precision: 0 },
    },
    y: {
      grid:   { display: true, color: 'rgba(128,128,128,0.07)' },
      border: { display: false },
      ticks:  { color: '#71717a', font: { size: 11 } },
    },
  },
}))

// ── Role distribution donut ──────────────────────────────────────────────────
const rolePieData = computed(() => {
  const rd = liveStats.value?.roleDistribution ?? {}
  const COLORS: Record<string, string> = {
    Admin:        '#ef4444',
    CompanyAdmin: '#f97316',
    Formateur:    '#fb923c',
    Condidat:     '#10b981',
  }
  return Object.entries(rd).map(([label, value]) => ({
    label: t(`users.roles.${label.charAt(0).toLowerCase() + label.slice(1)}`),
    value: value as number,
    color: COLORS[label] ?? '#f97316',
  }))
})

// ── Pass rates bar chart ─────────────────────────────────────────────────────
const passRateBarData = computed(() =>
  (liveStats.value?.passRates ?? []).map(r => ({
    label: r.companyName,
    value: Math.round(r.passRate),
    color: r.passRate >= 70 ? '#10b981' : r.passRate >= 50 ? '#f59e0b' : '#f43f5e',
  }))
)

</script>.