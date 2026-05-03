<template>
  <div class="space-y-6">

    <!-- ── Header ─────────────────────────────────────────────────────────── -->
    <div>
      <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground mb-1.5">{{ t('analytics.insights') }}</p>
      <h1 class="text-3xl font-black tracking-tight text-foreground">{{ t('analytics.title') }}</h1>
    </div>

    <!-- ── KPI stripe ─────────────────────────────────────────────────────── -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <div
        v-for="kpi in kpis"
        :key="kpi.label"
        class="rounded-2xl border border-border bg-card p-5"
      >
        <div class="flex items-center gap-2 mb-3">
          <component :is="kpi.icon" class="w-3.5 h-3.5 text-muted-foreground" />
          <span class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ kpi.label }}</span>
        </div>
        <div class="text-3xl font-black text-foreground tracking-tight tabular-nums">
          <span v-if="statsLoading">—</span>
          <span v-else>{{ kpi.value }}</span>
        </div>
        <div class="flex items-center gap-1.5 mt-2">
          <TrendingUp v-if="kpi.up" class="w-3 h-3 text-emerald-600" />
          <TrendingDown v-else class="w-3 h-3 text-amber-600" />
          <span class="text-xs font-medium" :class="kpi.up ? 'text-emerald-600' : 'text-amber-600'">{{ kpi.change }}</span>
        </div>
      </div>
    </div>

    <!-- ── Tabs ───────────────────────────────────────────────────────────── -->
    <div>
      <!-- Pill tab bar -->
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

      <!-- Tab content -->
      <div class="pt-6">

        <!-- ════════════════════ Overview ════════════════════ -->
        <div v-if="activeTab === 'overview'" class="space-y-6">

          <!-- Full-width attempt trend -->
          <AppAreaChart
            :data="attemptTrend"
            title="Attempt Activity"
            subtitle="Test attempts over time — last 30 days"
            color="#f97316"
            :height="260"
            :show-range-selector="true"
          />

          <!-- Donut + horizontal bar -->
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <AppPieChart
              :data="rolePieData"
              title="User Distribution"
              subtitle="Users by role"
              :size="170"
              cutout="62%"
              :show-total="true"
            />
            <AppBarChart
              :data="passRateBarData"
              title="Pass Rate by Company"
              subtitle="Percentage of passed attempts"
              color="#f97316"
              :height="200"
              :horizontal="true"
            />
          </div>

          <!-- Attempt status + role grid -->
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">

            <!-- Attempt status breakdown (live) -->
            <div class="rounded-2xl border border-border bg-card p-5">
              <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground mb-4">{{ t('analytics.overviewTab.attemptsByStatus') }}</p>
              <div v-if="statsLoading" class="space-y-3">
                <Skeleton v-for="i in 4" :key="i" class="h-8 w-full rounded-lg" />
              </div>
              <div v-else class="space-y-4">
                <div v-for="item in attemptStatusItems" :key="item.label">
                  <div class="flex items-center justify-between text-sm mb-1.5">
                    <div class="flex items-center gap-2">
                      <span class="w-2 h-2 rounded-full shrink-0" :class="item.dot" />
                      <span class="font-medium text-foreground text-xs">{{ item.label }}</span>
                    </div>
                    <div class="flex items-center gap-2">
                      <span class="font-bold text-foreground tabular-nums text-xs">{{ item.count.toLocaleString() }}</span>
                      <span class="text-[10px] text-muted-foreground w-8 text-right tabular-nums">{{ item.pct }}%</span>
                    </div>
                  </div>
                  <div class="w-full h-1.5 bg-muted rounded-full overflow-hidden">
                    <div class="h-full rounded-full transition-all duration-700" :class="item.bar" :style="{ width: item.pct + '%' }" />
                  </div>
                </div>
                <p v-if="!totalAttemptCount && !statsLoading" class="text-xs text-muted-foreground italic">No attempt data yet.</p>
              </div>
            </div>

            <!-- Role distribution grid (live) -->
            <div class="rounded-2xl border border-border bg-card p-5">
              <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground mb-4">{{ t('analytics.overviewTab.userDistributionByRole') }}</p>
              <div class="grid grid-cols-2 gap-3">
                <div
                  v-for="role in roleDistribution"
                  :key="role.key"
                  class="flex flex-col items-center gap-2 py-4 px-3 rounded-xl bg-muted/30 border border-border hover:bg-muted/50 transition-colors"
                >
                  <div :class="role.iconBg" class="w-9 h-9 rounded-xl flex items-center justify-center">
                    <component :is="role.icon" :class="role.iconColor" class="w-4 h-4" />
                  </div>
                  <div class="text-2xl font-black text-foreground">
                    {{ statsLoading ? '—' : (liveStats?.roleDistribution?.[role.value] ?? role.count) }}
                  </div>
                  <span :class="role.chip" class="text-[10px] font-bold uppercase tracking-wide px-2 py-0.5 rounded-md">
                    {{ t(`users.roles.${role.key}`) }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- ════════════════════ Users ════════════════════ -->
        <div v-else-if="activeTab === 'users'" class="space-y-6">

          <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <AppPieChart
              :data="rolePieData"
              title="Role Distribution"
              subtitle="All registered users by role"
              :size="170"
              cutout="62%"
              :show-total="true"
            />

            <div class="rounded-2xl border border-border bg-card p-5">
              <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground mb-4">{{ t('analytics.usersTab.activeVsInactive') }}</p>
              <div class="divide-y divide-border">
                <div v-for="seg in userSegments" :key="seg.label" class="flex items-center gap-4 py-3.5 hover:bg-muted/20 transition-colors -mx-2 px-2 rounded-lg">
                  <div class="w-9 h-9 rounded-xl flex items-center justify-center shrink-0" :class="seg.iconBg">
                    <component :is="seg.icon" class="w-4 h-4" :class="seg.iconColor" />
                  </div>
                  <div class="flex-1 min-w-0">
                    <div class="flex items-center justify-between mb-1">
                      <span class="text-sm font-semibold text-foreground">{{ seg.label }}</span>
                      <span class="text-sm font-black tabular-nums text-foreground">{{ seg.count }}</span>
                    </div>
                    <div class="w-full h-1.5 bg-muted rounded-full overflow-hidden">
                      <div class="h-full rounded-full transition-all duration-700" :class="seg.bar" :style="{ width: seg.pct + '%' }" />
                    </div>
                  </div>
                  <span class="text-xs font-bold tabular-nums shrink-0" :class="seg.pctColor">{{ seg.pct }}%</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Users by role table (live) -->
          <div>
            <h2 class="text-[11px] font-bold uppercase tracking-widest text-foreground mb-4">{{ t('analytics.usersTab.usersByRole') }}</h2>
            <div class="border border-border rounded-xl overflow-hidden">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-border bg-muted/30">
                    <th class="text-left px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('analytics.usersTab.role') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('analytics.usersTab.count') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground hidden sm:table-cell">{{ t('analytics.usersTab.share') }}</th>
                    <th class="px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground hidden md:table-cell">{{ t('analytics.usersTab.distribution') }}</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-border">
                  <tr v-for="role in roleDistribution" :key="role.key" class="hover:bg-muted/20 transition-colors">
                    <td class="px-5 py-3.5">
                      <div class="flex items-center gap-2.5">
                        <div class="w-7 h-7 rounded-lg flex items-center justify-center shrink-0" :class="role.iconBg">
                          <component :is="role.icon" class="w-3.5 h-3.5" :class="role.iconColor" />
                        </div>
                        <span class="font-semibold text-foreground">{{ t(`users.roles.${role.key}`) }}</span>
                      </div>
                    </td>
                    <td class="px-5 py-3.5 text-right font-bold tabular-nums text-foreground">
                      {{ statsLoading ? '—' : (liveStats?.roleDistribution?.[role.value] ?? role.count) }}
                    </td>
                    <td class="px-5 py-3.5 text-right font-medium tabular-nums text-muted-foreground hidden sm:table-cell">
                      {{ totalUsers > 0 ? Math.round((liveStats?.roleDistribution?.[role.value] ?? role.count) / totalUsers * 100) : 0 }}%
                    </td>
                    <td class="px-5 py-3.5 hidden md:table-cell">
                      <div class="w-full h-1.5 bg-muted rounded-full overflow-hidden">
                        <div
                          class="h-full rounded-full transition-all duration-700"
                          :class="role.iconColor.replace('text-', 'bg-')"
                          :style="{ width: (totalUsers > 0 ? (liveStats?.roleDistribution?.[role.value] ?? role.count) / totalUsers * 100 : 0) + '%' }"
                        />
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Top candidates -->
          <div>
            <h2 class="text-[11px] font-bold uppercase tracking-widest text-foreground mb-4">{{ t('analytics.usersTab.topCandidates') }}</h2>
            <div class="border border-border rounded-xl overflow-hidden">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-border bg-muted/30">
                    <th class="text-left px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">#</th>
                    <th class="text-left px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('analytics.usersTab.candidate') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('analytics.usersTab.avgScore') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground hidden sm:table-cell">{{ t('analytics.usersTab.attempts') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground hidden md:table-cell">{{ t('analytics.usersTab.passRate') }}</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-border">
                  <tr v-for="(p, i) in topCandidates" :key="p.name" class="hover:bg-muted/20 transition-colors">
                    <td class="px-5 py-3.5 text-sm font-bold tabular-nums text-muted-foreground">{{ i + 1 }}</td>
                    <td class="px-5 py-3.5">
                      <div class="flex items-center gap-2.5">
                        <div class="w-7 h-7 rounded-full bg-primary text-primary-foreground flex items-center justify-center text-[10px] font-black shrink-0">{{ p.initials }}</div>
                        <div>
                          <p class="font-semibold text-foreground">{{ p.name }}</p>
                          <p class="text-xs text-muted-foreground">{{ p.company }}</p>
                        </div>
                      </div>
                    </td>
                    <td class="px-5 py-3.5 text-right font-black tabular-nums" :class="p.score >= 80 ? 'text-emerald-600' : p.score >= 60 ? 'text-amber-600' : 'text-red-500'">{{ p.score }}%</td>
                    <td class="px-5 py-3.5 text-right font-medium tabular-nums text-muted-foreground hidden sm:table-cell">{{ p.attempts }}</td>
                    <td class="px-5 py-3.5 text-right font-medium tabular-nums text-muted-foreground hidden md:table-cell">{{ p.passRate }}%</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- ════════════════════ Companies ════════════════════ -->
        <div v-else-if="activeTab === 'companies'" class="space-y-6">

          <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <AppRadarChart
              :data="companyRadarData"
              title="Company Performance"
              subtitle="Pass rate by company"
              label="Pass Rate"
              color="#f97316"
              suffix="%"
              :height="280"
            />
            <AppBarChart
              :data="companyMembersData"
              title="Members per Company"
              subtitle="Total headcount"
              color="#fb923c"
              :height="240"
              :horizontal="true"
            />
          </div>

          <div>
            <h2 class="text-[11px] font-bold uppercase tracking-widest text-foreground mb-4">{{ t('analytics.companiesTab.companyPerformance') }}</h2>
            <div class="border border-border rounded-xl overflow-hidden">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-border bg-muted/30">
                    <th class="text-left px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('analytics.companiesTab.company') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('analytics.companiesTab.members') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('analytics.companiesTab.campaigns') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground hidden sm:table-cell">{{ t('analytics.companiesTab.attempts') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('analytics.companiesTab.passRate') }}</th>
                    <th class="px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground hidden md:table-cell">{{ t('analytics.companiesTab.trend') }}</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-border">
                  <tr v-for="c in companyPerformance" :key="c.name" class="hover:bg-muted/20 transition-colors">
                    <td class="px-5 py-3.5">
                      <div class="flex items-center gap-2.5">
                        <div class="w-8 h-8 rounded-lg bg-muted flex items-center justify-center text-xs font-black text-foreground shrink-0">{{ c.name.slice(0, 2).toUpperCase() }}</div>
                        <div>
                          <p class="font-semibold text-foreground">{{ c.name }}</p>
                          <p class="text-xs text-muted-foreground">{{ c.industry }}</p>
                        </div>
                      </div>
                    </td>
                    <td class="px-5 py-3.5 text-right font-bold tabular-nums text-foreground">{{ c.members }}</td>
                    <td class="px-5 py-3.5 text-right font-medium tabular-nums text-muted-foreground">{{ c.campaigns }}</td>
                    <td class="px-5 py-3.5 text-right font-medium tabular-nums text-muted-foreground hidden sm:table-cell">{{ c.attempts }}</td>
                    <td class="px-5 py-3.5 text-right">
                      <span class="font-black tabular-nums" :class="c.passRate >= 70 ? 'text-emerald-600' : c.passRate >= 50 ? 'text-amber-600' : 'text-red-500'">{{ c.passRate }}%</span>
                    </td>
                    <td class="px-5 py-3.5 hidden md:table-cell">
                      <div class="flex items-center gap-1" :class="c.up ? 'text-emerald-600' : 'text-amber-600'">
                        <TrendingUp v-if="c.up" class="w-3.5 h-3.5" />
                        <TrendingDown v-else class="w-3.5 h-3.5" />
                        <span class="text-xs font-semibold">{{ c.trend }}</span>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- ════════════════════ Campaigns ════════════════════ -->
        <div v-else-if="activeTab === 'campaigns'" class="space-y-6">

          <!-- Campaign KPIs -->
          <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
            <div v-for="stat in campaignKpis" :key="stat.label" class="rounded-2xl border border-border bg-card px-5 py-5 hover:shadow-sm transition-shadow">
              <div class="flex items-center gap-2 mb-3">
                <component :is="stat.icon" class="w-3.5 h-3.5 text-muted-foreground" />
                <span class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ stat.label }}</span>
              </div>
              <div class="text-3xl font-black text-foreground tracking-tight">{{ stat.value }}</div>
              <span class="text-xs font-medium text-muted-foreground mt-1 block">{{ stat.sub }}</span>
            </div>
          </div>

          <!-- Area chart + pass/fail donut -->
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <AppAreaChart
              :data="attemptTrend"
              title="Attempt Trend"
              subtitle="Submissions over time"
              color="#f97316"
              :height="240"
              :show-range-selector="true"
            />
            <AppPieChart
              :data="passFail"
              title="Pass / Fail"
              subtitle="Overall pass ratio"
              :size="160"
              cutout="62%"
              :show-total="true"
            />
          </div>

          <!-- Attempt funnel (AppBarChart) -->
          <AppBarChart
            :data="attemptFunnelBar"
            title="Attempt Funnel"
            subtitle="Candidates at each stage"
            :height="180"
            :horizontal="true"
          />

          <!-- Campaign table -->
          <div>
            <h2 class="text-[11px] font-bold uppercase tracking-widest text-foreground mb-4">{{ t('analytics.campaignsTab.recentCampaigns') }}</h2>
            <div class="border border-border rounded-xl overflow-hidden">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-border bg-muted/30">
                    <th class="text-left px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('analytics.campaignsTab.campaign') }}</th>
                    <th class="text-left px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground hidden sm:table-cell">{{ t('analytics.campaignsTab.company') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('analytics.campaignsTab.invited') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground hidden md:table-cell">{{ t('analytics.campaignsTab.completed') }}</th>
                    <th class="text-right px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('analytics.campaignsTab.passRate') }}</th>
                    <th class="text-center px-5 py-3 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('analytics.campaignsTab.status') }}</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-border">
                  <tr v-for="c in recentCampaigns" :key="c.name" class="hover:bg-muted/20 transition-colors">
                    <td class="px-5 py-3.5">
                      <p class="font-semibold text-foreground">{{ c.name }}</p>
                      <p class="text-xs text-muted-foreground">{{ c.questionnaire }}</p>
                    </td>
                    <td class="px-5 py-3.5 text-sm text-muted-foreground hidden sm:table-cell">{{ c.company }}</td>
                    <td class="px-5 py-3.5 text-right font-bold tabular-nums text-foreground">{{ c.invited }}</td>
                    <td class="px-5 py-3.5 text-right font-medium tabular-nums text-muted-foreground hidden md:table-cell">{{ c.completed }}</td>
                    <td class="px-5 py-3.5 text-right">
                      <span class="font-black tabular-nums" :class="c.passRate >= 70 ? 'text-emerald-600' : c.passRate >= 50 ? 'text-amber-600' : 'text-red-500'">{{ c.passRate }}%</span>
                    </td>
                    <td class="px-5 py-3.5 text-center">
                      <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[11px] font-bold uppercase tracking-wide" :class="c.statusClass">
                        <span class="w-1.5 h-1.5 rounded-full" :class="c.dotClass" />{{ c.status }}
                      </span>
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
import { getAdminStatsApi, type AdminStats } from '@/utils/api/stats.api'
import AppAreaChart  from '@/components/charts/AppAreaChart.vue'
import AppPieChart   from '@/components/charts/AppPieChart.vue'
import AppRadarChart from '@/components/charts/AppRadarChart.vue'
import AppBarChart   from '@/components/charts/AppBarChart.vue'
import {
  LayoutDashboard, Users, Building2, Megaphone,
  TrendingUp, TrendingDown, ClipboardList, BookOpen,
  ShieldCheck, UserCog, GraduationCap, User,
  Activity, CheckCircle2, Clock3, XCircle,
} from 'lucide-vue-next'
import { UserRole, getRoleAvatarStyles, getRoleChipStyles } from '@/utils/roles'

const { t } = useLocale()
const activeTab = ref('overview')

// ── Live stats ────────────────────────────────────────────────────────────────
const statsLoading = ref(true)
const liveStats    = ref<AdminStats | null>(null)

onMounted(async () => {
  const { data } = await getAdminStatsApi()
  liveStats.value    = data ?? null
  statsLoading.value = false
})

// ── Tabs ──────────────────────────────────────────────────────────────────────
const tabs = computed(() => [
  { id: 'overview',  label: t('analytics.tabs.overview'),  icon: LayoutDashboard },
  { id: 'users',     label: t('analytics.tabs.users'),     icon: Users },
  { id: 'companies', label: t('analytics.tabs.companies'), icon: Building2 },
  { id: 'campaigns', label: t('analytics.tabs.campaigns'), icon: Megaphone },
])

// ── KPIs (live) ───────────────────────────────────────────────────────────────
const kpis = computed(() => [
  {
    label:  t('analytics.kpis.totalAttempts'),
    value:  liveStats.value?.totalAttempts?.toLocaleString() ?? '—',
    change: '+12% this month', up: true, icon: ClipboardList,
  },
  {
    label:  t('analytics.kpis.avgPassRate'),
    value:  liveStats.value?.avgPassRate != null ? liveStats.value.avgPassRate + '%' : '—',
    change: 'All-time average', up: true, icon: BookOpen,
  },
  {
    label:  t('analytics.kpis.avgScore'),
    value:  liveStats.value?.avgScore != null ? liveStats.value.avgScore + '%' : '—',
    change: 'Submitted attempts', up: true, icon: TrendingUp,
  },
  {
    label:  t('analytics.kpis.activeCampaigns'),
    value:  liveStats.value?.totalCampaigns?.toLocaleString() ?? '—',
    change: 'Total campaigns', up: true, icon: Building2,
  },
])

// ── Attempt trend (live) ──────────────────────────────────────────────────────
const attemptTrend = computed(() => liveStats.value?.attemptTrend ?? [])

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
    { label: 'In Progress', count: c.inProgress, pct: Math.round(c.inProgress / total * 100), dot: 'bg-primary',     bar: 'bg-primary' },
    { label: 'Timed Out',   count: c.timedOut,   pct: Math.round(c.timedOut   / total * 100), dot: 'bg-amber-400',   bar: 'bg-amber-400' },
    { label: 'Abandoned',   count: c.abandoned,  pct: Math.round(c.abandoned  / total * 100), dot: 'bg-red-400',     bar: 'bg-red-400' },
  ]
})

// ── Role distribution (live + static fallback) ────────────────────────────────
const roleDistribution = computed(() => [
  { key: 'admin',        value: 'Admin',        count: 2,   icon: ShieldCheck,   iconColor: 'text-red-600',     iconBg: getRoleAvatarStyles(UserRole.Admin),        chip: getRoleChipStyles(UserRole.Admin)        },
  { key: 'companyAdmin', value: 'CompanyAdmin', count: 24,  icon: UserCog,       iconColor: 'text-primary',     iconBg: getRoleAvatarStyles(UserRole.CompanyAdmin), chip: getRoleChipStyles(UserRole.CompanyAdmin) },
  { key: 'formateur',    value: 'Formateur',    count: 58,  icon: GraduationCap, iconColor: 'text-orange-500',  iconBg: getRoleAvatarStyles(UserRole.Formateur),    chip: getRoleChipStyles(UserRole.Formateur)    },
  { key: 'condidat',     value: 'Condidat',     count: 228, icon: User,          iconColor: 'text-emerald-600', iconBg: getRoleAvatarStyles(UserRole.Condidat),     chip: getRoleChipStyles(UserRole.Condidat)     },
])

const rolePieData = computed(() => roleDistribution.value.map(r => ({
  label: t(`users.roles.${r.key}`),
  value: liveStats.value?.roleDistribution?.[r.value] ?? r.count,
  color: r.iconColor === 'text-red-600'    ? '#ef4444'
       : r.iconColor === 'text-primary'    ? '#f97316'
       : r.iconColor === 'text-orange-500' ? '#fb923c'
       : '#10b981',
})))

const totalUsers = computed(() => rolePieData.value.reduce((s, r) => s + r.value, 0))

// ── Pass rates (live) ─────────────────────────────────────────────────────────
const passRateBarData = computed(() =>
  (liveStats.value?.passRates ?? []).map(r => ({
    label: r.companyName,
    value: Math.round(r.passRate),
    color: r.passRate >= 70 ? '#10b981' : r.passRate >= 50 ? '#f59e0b' : '#f43f5e',
  }))
)

// ── Company radar (live) ──────────────────────────────────────────────────────
const companyRadarData = computed(() =>
  passRateBarData.value.slice(0, 6).map(c => ({ axis: c.label, value: c.value }))
)

// ── Pass/Fail pie (static fallback) ──────────────────────────────────────────
const passFail = computed(() => [
  { label: 'Passed', value: 929,  color: '#10b981' },
  { label: 'Failed', value: 458,  color: '#f43f5e' },
])

// ── Static data (tables / fallbacks) ─────────────────────────────────────────
const userSegments = [
  { label: 'Active (last 30 days)',  count: 198, pct: 63, icon: Activity,  iconColor: 'text-emerald-600', iconBg: 'bg-emerald-100 dark:bg-emerald-900/30', bar: 'bg-emerald-500', pctColor: 'text-emerald-600' },
  { label: 'Inactive (30–90 days)', count: 74,  pct: 24, icon: Clock3,    iconColor: 'text-amber-600',   iconBg: 'bg-amber-100 dark:bg-amber-900/30',    bar: 'bg-amber-400',   pctColor: 'text-amber-600' },
  { label: 'Dormant (90+ days)',    count: 40,  pct: 13, icon: XCircle,   iconColor: 'text-red-500',     iconBg: 'bg-red-100 dark:bg-red-900/30',        bar: 'bg-red-400',     pctColor: 'text-red-500' },
]

const topCandidates = [
  { name: 'Alice Dupont',   initials: 'AD', company: 'EduWorld',   score: 94, attempts: 6, passRate: 100 },
  { name: 'Karim Bensalem', initials: 'KB', company: 'TechCorp',   score: 91, attempts: 4, passRate: 100 },
  { name: 'Marie Leclerc',  initials: 'ML', company: 'HealthPlus', score: 88, attempts: 5, passRate: 80  },
  { name: 'Omar El Fassi',  initials: 'OE', company: 'FinanceHub', score: 85, attempts: 8, passRate: 75  },
  { name: 'Sara Fontaine',  initials: 'SF', company: 'EduWorld',   score: 83, attempts: 3, passRate: 100 },
]

const companyPerformance = [
  { name: 'EduWorld',   industry: 'Education',  members: 68, campaigns: 12, attempts: 421, passRate: 82, up: true,  trend: '+5%' },
  { name: 'TechCorp',   industry: 'Technology', members: 54, campaigns: 9,  attempts: 318, passRate: 78, up: true,  trend: '+2%' },
  { name: 'FinanceHub', industry: 'Finance',    members: 47, campaigns: 7,  attempts: 276, passRate: 65, up: false, trend: '-3%' },
  { name: 'RetailZone', industry: 'Retail',     members: 39, campaigns: 5,  attempts: 193, passRate: 61, up: false, trend: '-1%' },
  { name: 'HealthPlus', industry: 'Healthcare', members: 32, campaigns: 4,  attempts: 157, passRate: 54, up: true,  trend: '+4%' },
]

const companyMembersData = computed(() =>
  companyPerformance.map(c => ({ label: c.name, value: c.members, color: '#fb923c' }))
)

const campaignKpis = [
  { label: 'Total',     value: '38', sub: 'All time',    icon: Megaphone    },
  { label: 'Active',    value: '14', sub: 'Running now', icon: Activity     },
  { label: 'Completed', value: '21', sub: 'Fully done',  icon: CheckCircle2 },
  { label: 'Avg. Size', value: '28', sub: 'Candidates',  icon: Users        },
]

const attemptFunnelBar = [
  { label: 'Invited',   value: 1842, color: '#f97316' },
  { label: 'Started',   value: 1654, color: '#0ea5e9' },
  { label: 'Completed', value: 1387, color: '#fb923c' },
  { label: 'Passed',    value: 929,  color: '#10b981' },
]

const recentCampaigns = [
  { name: 'Spring Hiring 2026', questionnaire: 'JS Fundamentals',   company: 'TechCorp',   invited: 40, completed: 33, passRate: 79, status: 'Active',    statusClass: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400', dotClass: 'bg-emerald-500' },
  { name: 'Q1 Assessment',      questionnaire: 'Finance Basics',    company: 'FinanceHub', invited: 28, completed: 28, passRate: 68, status: 'Closed',    statusClass: 'bg-muted text-muted-foreground', dotClass: 'bg-border' },
  { name: 'Retail Onboarding',  questionnaire: 'Product Knowledge', company: 'RetailZone', invited: 22, completed: 19, passRate: 61, status: 'Active',    statusClass: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400', dotClass: 'bg-emerald-500' },
  { name: 'Health Cert. 2026',  questionnaire: 'Medical Ethics',    company: 'HealthPlus', invited: 35, completed: 12, passRate: 58, status: 'Active',    statusClass: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400', dotClass: 'bg-emerald-500' },
  { name: 'EduWorld Feb Batch', questionnaire: 'Pedagogy 101',      company: 'EduWorld',   invited: 50, completed: 50, passRate: 84, status: 'Closed',    statusClass: 'bg-muted text-muted-foreground', dotClass: 'bg-border' },
  { name: 'Dev Bootcamp Eval',  questionnaire: 'Algorithms',        company: 'TechCorp',   invited: 18, completed: 0,  passRate: 0,  status: 'Scheduled', statusClass: 'bg-primary/10 text-primary dark:bg-primary/15 dark:text-orange-300', dotClass: 'bg-primary' },
]
</script>
