<template>
  <div>
    <p class="text-[10px] font-bold uppercase tracking-widest text-muted-foreground mb-3">
      {{ t('users.activity') }}
    </p>

    <div v-if="loading" class="grid grid-cols-2 gap-2">
      <Skeleton v-for="i in 4" :key="i" class="h-16 rounded-xl" />
    </div>

    <div v-else-if="cards.length" class="grid grid-cols-2 gap-2">
      <div
        v-for="card in cards"
        :key="card.label"
        class="flex flex-col items-center gap-1 p-3 rounded-xl bg-muted/30 border border-border text-center"
        :class="card.spanClass"
      >
        <component :is="card.icon" class="w-4 h-4 text-muted-foreground" />
        <span class="text-xl text-slate-800 tabular-nums" :class="card.valueClass">{{ card.value }}</span>
        <span class="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground">{{ card.label }}</span>
      </div>
    </div>

    <p v-else class="text-xs text-muted-foreground italic">
      {{ t('users.noStats') }}
    </p>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Skeleton } from '@/components/ui/skeleton'
import { useLocale } from '@/composables/useLocale'
import type { ManagedUserStats } from '@utils/models'
import {
  Activity,
  Award,
  BriefcaseBusiness,
  FileText,
  Shield,
  TrendingUp,
  UserCheck,
  Users,
} from 'lucide-vue-next'

const props = defineProps<{
  role?: string | null
  stats?: ManagedUserStats | null
  loading?: boolean
}>()

const { t } = useLocale()

const cards = computed(() => {
  const stats = props.stats ?? {}

  switch (props.role) {
    case 'Condidat':
      return [
        { label: t('users.stats.attempts'), value: stats.totalAttempts ?? 0, icon: Activity, valueClass: 'text-slate-800' },
        { label: t('users.stats.passed'), value: stats.passedAttempts ?? 0, icon: TrendingUp, valueClass: 'text-emerald-600' },
        { label: t('users.stats.activeCampaigns'), value: stats.activeCampaigns ?? 0, icon: BriefcaseBusiness, valueClass: 'text-slate-800' },
        { label: t('users.stats.certificates'), value: stats.certificatesEarned ?? 0, icon: Award, valueClass: 'text-amber-600' },
      ]
    case 'Formateur':
      return [
        { label: t('users.stats.campaignsCreated'), value: stats.campaignsCreated ?? 0, icon: BriefcaseBusiness, valueClass: 'text-slate-800' },
        { label: t('users.stats.questionnairesCreated'), value: stats.questionnairesCreated ?? 0, icon: FileText, valueClass: 'text-primary' },
        { label: t('users.stats.managedCandidates'), value: stats.totalCandidatesManaged ?? 0, icon: Users, valueClass: 'text-slate-800', spanClass: 'col-span-2' },
      ]
    case 'CompanyAdmin':
      return [
        { label: t('users.stats.employees'), value: stats.employeesCount ?? 0, icon: Users, valueClass: 'text-slate-800' },
        { label: t('users.stats.activeUsers'), value: stats.activeUsers ?? 0, icon: UserCheck, valueClass: 'text-emerald-600' },
        { label: t('users.stats.activeCampaigns'), value: stats.activeCampaigns ?? 0, icon: BriefcaseBusiness, valueClass: 'text-slate-800' },
        { label: t('users.stats.formateurs'), value: stats.formateursCount ?? 0, icon: Shield, valueClass: 'text-primary' },
      ]
    case 'Admin':
      return [
        { label: t('users.stats.totalUsers'), value: stats.totalUsers ?? 0, icon: Users, valueClass: 'text-slate-800' },
        { label: t('users.stats.activeUsers'), value: stats.activeUsers ?? 0, icon: UserCheck, valueClass: 'text-emerald-600' },
        { label: t('users.stats.companies'), value: stats.companiesCount ?? 0, icon: BriefcaseBusiness, valueClass: 'text-slate-800' },
        { label: t('users.stats.managementAccounts'), value: stats.managementAccounts ?? 0, icon: Shield, valueClass: 'text-primary' },
      ]
    default:
      return []
  }
})
</script>
