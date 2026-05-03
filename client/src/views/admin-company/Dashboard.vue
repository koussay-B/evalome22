<template>
  <div class="space-y-7">

    <!-- ── Header ─────────────────────────────────────────────────────────── -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-6">
      <div class="flex items-center gap-5">
        <div class="w-14 h-14 rounded-2xl border border-border bg-muted flex items-center justify-center shrink-0 overflow-hidden shadow-[0_2px_12px_0_oklch(0.58_0.21_42/0.10)]">
          <img v-if="companyLogo" :src="companyLogo" class="w-full h-full object-contain" />
          <span v-else class="text-2xl text-slate-800 text-primary">{{ companyName.charAt(0) }}</span>
        </div>
        <div>
          <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1 leading-none">{{ t('companyPanel.dashboard.overview') }}</p>
          <h1 class="text-3xl text-slate-800 tracking-tight text-slate-800 leading-none">{{ companyName }}</h1>
        </div>
      </div>
      
      <!-- Theme Dropdown -->
      <div class="flex items-center gap-3 bg-card border border-border p-1.5 rounded-2xl shadow-sm min-w-[220px]">
        <div class="w-9 h-9 rounded-xl bg-primary/8 flex items-center justify-center shrink-0">
          <Layers class="w-4 h-4 text-primary" />
        </div>
        <div class="flex-1 min-w-0 pr-1">
          <Select :model-value="selectedThemeId === null ? 'all' : String(selectedThemeId)" @update:model-value="v => selectedThemeId = v === 'all' ? null : Number(v)">
            <SelectTrigger class="border-none bg-transparent h-8 p-0 focus:ring-0 shadow-none hover:bg-transparent text-slate-800 font-semibold text-xs">
              <SelectValue :placeholder="t('common.all')" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all" class="text-xs font-bold">{{ t('common.all') }}</SelectItem>
              <SelectItem
                v-for="theme in availableThemes"
                :key="theme.id"
                :value="String(theme.id)"
                class="text-xs font-semibold"
              >
                {{ theme.name }}
              </SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>
    </div>

    <!-- KPI Cards -->
    <div class="grid grid-cols-4 lg:grid-cols-5 gap-4">
      <div
        v-for="card in kpiCards"
        :key="card.label"
        class="rounded-2xl border border-border bg-card p-5 flex items-start gap-4 hover:shadow-[0_4px_24px_0_oklch(0.58_0.21_42/0.10)] hover:-translate-y-0.5 transition-all duration-300 group relative overflow-hidden shadow-sm"
      >
        <div :class="[card.iconBg, 'w-12 h-12 rounded-xl flex items-center justify-center shrink-0 transition-transform group-hover:scale-110']">
          <component :is="card.icon" :class="card.iconColor" class="w-6 h-6" />
        </div>

        <div class="flex-1 min-w-0">
          <div class="text-2xl text-slate-800 text-slate-800 tracking-tight tabular-nums flex items-baseline gap-1">
            <span v-if="statsLoading" class="animate-pulse text-muted-foreground">···</span>
            <span v-else>{{ card.value }}</span>
            
            <div v-if="card.badge" class="ml-auto flex items-center gap-1 px-2 py-0.5 rounded-full bg-primary/10 text-primary border border-primary/20 scale-75">
              <span class="text-[9px] text-slate-800 uppercase tracking-wider">{{ card.badge }}</span>
            </div>
          </div>

          <p class="text-[10px] font-bold uppercase tracking-widest text-muted-foreground mt-1 truncate">
            {{ card.label }}
          </p>
        </div>
      </div>
    </div>

    <!-- ── Area Chart + Pass/Fail Donut ──────────────────────────────────── -->
    <div class="grid grid-cols-1 lg:grid-cols-[1fr_360px] gap-6">
      <AppAreaChart
        :data="liveStats?.attemptTrend ?? []"
        :title="selectedThemeName ? `Activity: ${selectedThemeName}` : 'Global Attempt Activity'"
        subtitle="Test submissions overview"
        :color="activeColor"
        :height="280"
        :show-range-selector="true"
        class="!rounded-3xl shadow-sm"
      />

      <AppPieChart
        :data="passFailPieData"
        :title="selectedThemeName ? `Results: ${selectedThemeName}` : 'Pass / Fail Ratio'"
        subtitle="Performance breakdown"
        :size="180"
        cutout="65%"
        :show-total="true"
        class="!rounded-3xl shadow-sm"
      />
    </div>

    <!-- ── Middle Section: Distribution & Activity ── -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
      <!-- Role Distribution -->
      <div class="lg:col-span-4">
        <AppPieChart
          :data="rolePieData"
          :title="t('companyPanel.dashboard.titles.roles')"
          subtitle="Employees by role"
          :size="160" 
          cutout="65%"
          :show-total="true"
          class="!rounded-3xl shadow-sm h-full"
        />
      </div>

      <!-- Creator Activity -->
      <div class="lg:col-span-8">
        <CreatorActivityBarChart
          :data="creatorActivityChartData"
          :title="t('companyPanel.dashboard.titles.creatorActivity')"
          :subtitle="creatorActivitySubtitle"
          :height="320"
          class="!rounded-3xl shadow-sm"
        >
          <template #actions>
            <div class="flex gap-2">
              <Select :model-value="creatorRoleFilter" @update:model-value="setCreatorRoleFilter">
                <SelectTrigger class="h-8 min-w-[120px] rounded-xl border-border bg-background px-3 text-[10px] font-bold text-slate-800 shadow-none">
                  <SelectValue placeholder="Role" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem v-for="option in creatorRoleOptions" :key="option.value" :value="option.value" class="text-xs font-semibold">
                    {{ option.label }}
                  </SelectItem>
                </SelectContent>
              </Select>
              <Select :model-value="creatorPeriodFilter" @update:model-value="setCreatorPeriodFilter">
                <SelectTrigger class="h-8 min-w-[120px] rounded-xl border-border bg-background px-3 text-[10px] font-bold text-slate-800 shadow-none">
                  <SelectValue placeholder="Periode" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem v-for="option in creatorPeriodOptions" :key="option.value" :value="option.value" class="text-xs font-semibold">
                    {{ option.label }}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>
          </template>
        </CreatorActivityBarChart>
      </div>
    </div>

    <!-- ── Bottom Section: Status & Score ── -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <!-- Attempt Status -->
      <div class="rounded-3xl border border-border bg-card p-6 shadow-sm flex flex-col justify-center h-full">
        <div class="flex items-center justify-between mb-6">
          <div>
            <p class="text-[10px] font-bold uppercase tracking-[0.2em] text-primary/60 leading-none">{{ t('companyPanel.dashboard.titles.attemptStatus') }}</p>
            <p class="text-xs font-bold text-slate-800 mt-2">{{ selectedThemeName || t('common.all') }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-primary/8 flex items-center justify-center shrink-0">
            <Zap class="w-5 h-5 text-primary" />
          </div>
        </div>
        <div v-if="statsLoading">
          <Skeleton class="h-10 w-full rounded-xl" />
        </div>
        <div v-else class="space-y-4">
          <div v-for="s in filteredAttemptStatusItems.filter(i => i.count > 0)" :key="s.label" class="group/item">
            <div class="flex items-center justify-between mb-2">
              <div class="flex items-center gap-2">
                <div class="w-2.5 h-2.5 rounded-full ring-2 ring-background shadow-sm" :class="s.dot" />
                <span class="text-xs font-bold text-slate-600">
                  {{ s.label === 'In Progress' ? t('companyCandidates.statusInProgress') : s.label }}
                </span>
              </div>
              <div class="flex items-center gap-2">
                <span class="text-sm font-bold text-slate-800 tabular-nums">{{ s.count.toLocaleString() }}</span>
                <span class="text-[10px] text-slate-800 font-bold bg-muted px-2 py-0.5 rounded-lg tabular-nums">{{ s.pct }}%</span>
              </div>
            </div>
            <div class="w-full h-2 bg-muted/50 rounded-full overflow-hidden">
              <div class="h-full rounded-full transition-all duration-1000" :class="s.bar" :style="{ width: s.pct + '%' }" />
            </div>
          </div>
        </div>
      </div>

      <!-- Average Score Card -->
      <div
        class="rounded-3xl border p-6 flex items-center gap-6 transition-all duration-300 hover:shadow-md h-full"
        :class="[activeThemeStyle!.bg, activeThemeStyle!.border]"
      >
        <div class="w-16 h-16 rounded-2xl flex items-center justify-center shrink-0 shadow-sm" :class="activeThemeStyle!.iconBg">
          <BarChart3 class="w-8 h-8" :class="activeThemeStyle!.text" />
        </div>
        <div>
          <p class="text-[10px] font-bold uppercase tracking-[0.2em] opacity-70 mb-2" :class="activeThemeStyle!.text">{{ t('companyPanel.dashboard.titles.avgScore') }}</p>
          <div class="flex items-baseline gap-1">
            <span class="text-5xl font-black tabular-nums tracking-tight" :class="activeThemeStyle!.text">
              {{ statsLoading ? '···' : filteredAvgScore }}
            </span>
            <span class="text-xl font-bold opacity-80" :class="activeThemeStyle!.text">%</span>
          </div>
          <p class="text-xs font-medium opacity-60 mt-2" :class="activeThemeStyle!.text">{{ t('companyPanel.dashboard.titles.avgScoreDesc') }}</p>
        </div>
      </div>
    </div>
  </div>
    

    
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useLocale } from '@/composables/useLocale'
import { useUserStore } from '@/store/userStore'
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { getMyCompanyApi } from '@/utils/api/company.api'
import {
  getCompanyCreatorActivityStatsApi,
  getCompanyStatsApi,
  type CompanyStats,
  type CreatorActivityItem,
  type CreatorActivityPeriod,
} from '@/utils/api/stats.api'
import { getRootThemesApi } from '@/utils/api/theme.api'
import { getCampaignsApi } from '@/utils/api/campaign.api'
import type { ThemeItem, CampaignItem } from '@/utils/models'
import AppAreaChart from '@/components/charts/AppAreaChart.vue'
import AppPieChart  from '@/components/charts/AppPieChart.vue'
import CreatorActivityBarChart from '@/components/charts/CreatorActivityBarChart.vue'
import {
  ArrowRight, Users, ClipboardList, BarChart3, GraduationCap,
  Target, Zap, Layers,
} from 'lucide-vue-next'
import { UserRole, getRoleChipStyles } from '@/utils/roles'

const { t } = useLocale()
const userStore = useUserStore()

const companyName  = ref('My Company')
const companyLogo  = ref<string | null>(null)
const statsLoading = ref(true)
const liveStats    = ref<CompanyStats | null>(null)
const creatorActivityLoading = ref(true)
const creatorActivity = ref<CreatorActivityItem[]>([])

type CreatorRoleFilter = 'all' | 'CompanyAdmin' | 'Formateur'

const creatorRoleFilter = ref<CreatorRoleFilter>('all')
const creatorPeriodFilter = ref<CreatorActivityPeriod>('all')

const creatorRoleOptions: { label: string; value: CreatorRoleFilter }[] = [
  { label: t('users.roles.all'), value: 'all' },
  { label: t('users.roles.companyAdmin'), value: 'CompanyAdmin' },
  { label: t('users.roles.formateur'), value: 'Formateur' },
]

const creatorPeriodOptions: { label: string; value: CreatorActivityPeriod }[] = [
  { label: t('common.all'), value: 'all' },
  { label: '30 ' + t('common.daysShort'), value: '30d' },
  { label: '90 ' + t('common.daysShort'), value: '90d' },
  { label: '12 m', value: '12m' },
]

// ── Theme grouping state ──────────────────────────────────────────────────────
const availableThemes = ref<ThemeItem[]>([])
const allCampaigns    = ref<CampaignItem[]>([])
const selectedThemeId = ref<number | null>(null)

const THEME_STYLES = [
  { id: 'indigo', border: 'border-indigo-200 dark:border-indigo-800/50', bg: 'bg-indigo-50 dark:bg-indigo-900/20', text: 'text-indigo-600 dark:text-indigo-400', chart: '#6366f1', iconBg: 'bg-indigo-100 dark:bg-indigo-900/40' },
  { id: 'emerald', border: 'border-emerald-200 dark:border-emerald-800/50', bg: 'bg-emerald-50 dark:bg-emerald-900/20', text: 'text-emerald-600 dark:text-emerald-400', chart: '#10b981', iconBg: 'bg-emerald-100 dark:bg-emerald-900/40' },
  { id: 'amber', border: 'border-amber-200 dark:border-amber-800/50', bg: 'bg-amber-50 dark:bg-amber-900/20', text: 'text-amber-600 dark:text-amber-400', chart: '#f59e0b', iconBg: 'bg-amber-100 dark:bg-amber-900/40' },
  { id: 'rose', border: 'border-rose-200 dark:border-rose-800/50', bg: 'bg-rose-50 dark:bg-rose-900/20', text: 'text-rose-600 dark:text-rose-400', chart: '#f43f5e', iconBg: 'bg-rose-100 dark:bg-rose-900/40' },
  { id: 'sky', border: 'border-sky-200 dark:border-sky-800/50', bg: 'bg-sky-50 dark:bg-sky-900/20', text: 'text-sky-600 dark:text-sky-400', chart: '#0ea5e9', iconBg: 'bg-sky-100 dark:bg-sky-900/40' },
  { id: 'violet', border: 'border-violet-200 dark:border-violet-800/50', bg: 'bg-violet-50 dark:bg-violet-900/20', text: 'text-violet-600 dark:text-violet-400', chart: '#8b5cf6', iconBg: 'bg-violet-100 dark:bg-violet-900/40' },
]

const activeThemeStyle = computed(() => {
  if (selectedThemeId.value === null) return THEME_STYLES[0] // default indigo
  const idx = availableThemes.value.findIndex(t => t.id === selectedThemeId.value)
  if (idx === -1) return THEME_STYLES[0]
  return THEME_STYLES[idx % THEME_STYLES.length]
})

const activeColor = computed(() => activeThemeStyle.value!.chart)

const selectedThemeName = computed(() => 
  availableThemes.value.find(t => t.id === selectedThemeId.value)?.name || null
)

onMounted(async () => {
  userStore.list.fetchPage({ pageSize: 5, pageNumber: 1, orderBy: 'username', includeCompany: false })
  const [companyRes, statsRes, themesRes, campaignsRes, creatorActivityRes] = await Promise.all([
    getMyCompanyApi(), 
    getCompanyStatsApi(),
    getRootThemesApi(),
    getCampaignsApi(),
    getCompanyCreatorActivityStatsApi(creatorPeriodFilter.value),
  ])
  
  if (companyRes.data) {
    companyName.value = companyRes.data.name
    companyLogo.value = companyRes.data.logo ?? null
  }
  
  availableThemes.value = themesRes.data ?? []
  allCampaigns.value    = campaignsRes.data ?? []
  liveStats.value       = statsRes.data ?? null
  creatorActivity.value = creatorActivityRes.data?.items ?? []
  statsLoading.value    = false
  creatorActivityLoading.value = false
})

async function loadCreatorActivity(period: CreatorActivityPeriod) {
  creatorActivityLoading.value = true
  const { data } = await getCompanyCreatorActivityStatsApi(period)
  creatorActivity.value = data?.items ?? []
  creatorActivityLoading.value = false
}

function setCreatorRoleFilter(value: unknown) {
  creatorRoleFilter.value = String(value) as CreatorRoleFilter
}

function setCreatorPeriodFilter(value: unknown) {
  const period = String(value) as CreatorActivityPeriod
  creatorPeriodFilter.value = period
  loadCreatorActivity(period)
}

// ── Filtering logic ───────────────────────────────────────────────────────────
const campaignThemeMap = computed(() => {
  const map = new Map<string, number | null>()
  allCampaigns.value.forEach(c => map.set(c.name, c.themeId ?? null))
  return map
})

const filteredPassRateByCampaign = computed(() => {
  const all = liveStats.value?.passRateByCampaign ?? []
  if (selectedThemeId.value === null) return all
  return all.filter(c => campaignThemeMap.value.get(c.campaignName) === selectedThemeId.value)
})

const filteredPassRate = computed(() => {
  const items = filteredPassRateByCampaign.value
  if (!items.length) return selectedThemeId.value === null ? (liveStats.value?.passRate ?? 0) : 0
  const sum = items.reduce((acc, curr) => acc + curr.passRate, 0)
  return Math.round(sum / items.length)
})

const filteredAvgScore = computed(() => {
  // If we had campaign-level avg scores, we would aggregate them here.
  // Since we only have global avgScore in CompanyStats, we'll return global if "All" is selected.
  // For specific themes, we'll use the average passRate as a proxy or keep global if data is missing.
  if (selectedThemeId.value === null) return liveStats.value?.avgScore ?? 0
  
  // Basic heuristic: if we have campaigns, use their average pass rate weighted slightly or just return 0 if no campaigns
  if (!filteredPassRateByCampaign.value.length) return 0
  return filteredPassRate.value // Using pass rate as placeholder for score if detailed score per campaign isn't in api
})

// ── KPI cards ─────────────────────────────────────────────────────────────────
const kpiCards = computed(() => [
  {
    label:     t('companyPanel.dashboard.stats.members'),
    value:     liveStats.value?.totalMembers ?? '–',
    icon:      Users,
    iconBg:    'bg-sky-50 dark:bg-sky-950/30',
    iconColor: 'text-sky-500 dark:text-sky-400',
    badge:     null as string | null,
  },
  {
    label:     t('companyPanel.dashboard.stats.formateurs'),
    value:     liveStats.value?.totalFormateurs ?? '–',
    icon:      GraduationCap,
    iconBg:    'bg-violet-100 dark:bg-violet-900/30',
    iconColor: 'text-violet-600 dark:text-violet-400',
    badge:     null as string | null,
  },
  {
    label:     t('companyPanel.dashboard.stats.activeCampaigns'),
    value:     selectedThemeId.value === null 
      ? (liveStats.value?.activeCampaigns ?? '–')
      : filteredPassRateByCampaign.value.filter(c => c.invitedCount > 0).length,
    icon:      ClipboardList,
    iconBg:    'bg-emerald-100 dark:bg-emerald-900/30',
    iconColor: 'text-emerald-600 dark:text-emerald-400',
    badge:     'Live' as string | null,
  },
  {
    label:     t('companyPanel.dashboard.stats.passRate'),
    value:     filteredPassRate.value + '%',
    icon:      BarChart3,
    iconBg:    'bg-amber-100 dark:bg-amber-900/30',
    iconColor: 'text-amber-600 dark:text-amber-400',
    badge:     null as string | null,
  },
  {
    label:     'Online Users',
    value:     liveStats.value?.onlineUsersCount ?? 0,
    icon:      Zap,
    iconBg:    'bg-indigo-100 dark:bg-indigo-900/30',
    iconColor: 'text-indigo-600 dark:text-indigo-400',
    badge:     'Live' as string | null,
  },
])

// ── Charts ────────────────────────────────────────────────────────────────────
const creatorRoleLabels: Record<string, string> = {
  all: t('users.roles.all'),
  CompanyAdmin: t('users.roles.companyAdmin'),
  Formateur: t('users.roles.formateur'),
}

const creatorActivitySubtitle = computed(() => {
  const roleLabel = creatorRoleLabels[creatorRoleFilter.value] ?? creatorRoleLabels.all
  const periodLabel = creatorPeriodOptions.find(option => option.value === creatorPeriodFilter.value)?.label
    ?? 'Toute la periode'

  return `${roleLabel} - ${periodLabel}`
})

const creatorActivityChartData = computed(() => {
  if (creatorActivityLoading.value) return []

  return creatorActivity.value
    .filter(item => creatorRoleFilter.value === 'all' || item.role === creatorRoleFilter.value)
    .map(item => ({
      label: item.userName,
      role: creatorRoleLabels[item.role] ?? item.role,
      questionsCount: item.questionsCount,
      campaignsCount: item.campaignsCount,
    }))
})

const passFailPieData = computed(() => {
  const pf = liveStats.value?.passFailRatio ?? { passed: 0, failed: 0 }
  // Note: Backend doesn't give passFailRatio per theme. 
  // We can approximate it from campaign pass rates if needed, but for now we show global or 0.
  if (selectedThemeId.value === null) {
      return [
        { label: t('testResult.passedBadge'), value: pf.passed, color: '#10b981' },
        { label: t('testResult.failedBadge'), value: pf.failed, color: '#f43f5e' },
      ]
  }
  
  const totalInvited = filteredPassRateByCampaign.value.reduce((acc, curr) => acc + curr.invitedCount, 0)
  const passed = Math.round(totalInvited * (filteredPassRate.value / 100))
  const failed = totalInvited - passed
  
  return [
    { label: 'Passed', value: passed, color: '#10b981' },
    { label: 'Failed', value: failed, color: '#f43f5e' },
  ]
})

const rolePieData = computed(() => {
  const rd = liveStats.value?.roleDistribution ?? {}
  const COLORS: Record<string, string> = {
    CompanyAdmin: '#0ea5e9', Formateur: '#8b5cf6', Condidat: '#10b981',
  }
  return Object.entries(rd).map(([label, value]) => ({
    label: t(`users.roles.${label.charAt(0).toLowerCase() + label.slice(1)}`),
    value: value as number,
    color: COLORS[label] ?? '#f97316',
  }))
})

const totalAttempts = computed(() => {
  const c = liveStats.value?.attemptStatusCounts
  if (!c) return 0
  return c.submitted + c.inProgress + c.timedOut + c.abandoned
})

const filteredAttemptStatusItems = computed(() => {
  const c = liveStats.value?.attemptStatusCounts ?? { submitted: 0, inProgress: 0, timedOut: 0, abandoned: 0 }
  const total = totalAttempts.value || 1
  
  // If no theme selected, show global
  if (selectedThemeId.value === null) {
    return [
      { label: 'Submitted', count: c.submitted, pct: Math.round(c.submitted / total * 100), dot: 'bg-emerald-500', bar: 'bg-emerald-500' },
      { label: 'In Progress', count: c.inProgress, pct: Math.round(c.inProgress / total * 100), dot: 'bg-sky-500', bar: 'bg-sky-500' },
      { label: 'Timed Out', count: c.timedOut, pct: Math.round(c.timedOut / total * 100), dot: 'bg-amber-400', bar: 'bg-amber-400' },
      { label: 'Abandoned', count: c.abandoned, pct: Math.round(c.abandoned / total * 100), dot: 'bg-red-400', bar: 'bg-red-400' },
    ]
  }
  
  // Approximate breakdown for theme
  const themeTotal = filteredPassRateByCampaign.value.reduce((acc, curr) => acc + curr.invitedCount, 0) || 1
  const ratio = themeTotal / total
  
  return [
    { label: 'In Progress', count: Math.round(c.inProgress * ratio), pct: Math.round(c.inProgress / total * 100), dot: 'bg-sky-500',     bar: 'bg-sky-500' },
     ]
})










</script>

<style scoped>
.no-scrollbar::-webkit-scrollbar {
  display: none;
}
.no-scrollbar {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
</style>
