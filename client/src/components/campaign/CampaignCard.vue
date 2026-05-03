<template>
  <div
    class="p-5 rounded-2xl border border-gray-200 dark:border-border bg-gray-50 dark:bg-card hover:border-primary/30 hover:shadow-[0_4px_20px_0_oklch(0.58_0.21_42/0.08)] transition-all duration-300 flex flex-col gap-4 cursor-pointer group relative"
    @click="$emit('click')"
  >
    <!-- Optional actions dropdown -->
    <div
      v-if="canEdit || canDelete"
      class="absolute top-3 inset-e-3 opacity-0 group-hover:opacity-100 focus-within:opacity-100 transition-opacity z-10"
    >
      <DropdownMenu>
        <DropdownMenuTrigger as-child>
          <button
            class="p-1.5 rounded-lg text-muted-foreground hover:bg-primary/8 hover:text-primary transition-colors"
            @click.stop
          >
            <MoreHorizontal class="w-4 h-4" />
          </button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" class="w-44">
          <DropdownMenuLabel class="text-xs">{{ t('common.actions') }}</DropdownMenuLabel>
          <DropdownMenuSeparator />
          <DropdownMenuItem v-if="canEdit" class="gap-2 cursor-pointer" @click.stop="$emit('edit')">
            <Pencil class="w-3.5 h-3.5" /> {{ t('common.edit') }}
          </DropdownMenuItem>
          <DropdownMenuItem class="gap-2 cursor-pointer" @click.stop="$emit('candidates')">
            <Users class="w-3.5 h-3.5" /> {{ t('campaignDetail.candidatesBtn') }}
          </DropdownMenuItem>
          <DropdownMenuItem
            v-if="canDelete"
            class="gap-2 cursor-pointer text-red-600 focus:text-red-600 focus:bg-red-50 dark:focus:bg-red-950/30"
            @click.stop="$emit('delete')"
          >
            <Trash2 class="w-3.5 h-3.5" /> {{ t('common.delete') }}
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </div>

    <!-- Header: icon + name + status -->
    <div class="flex items-start justify-between gap-3" :class="(canEdit || canDelete) ? 'pe-8' : ''">
      <div class="flex items-center gap-3 min-w-0">
        <div class="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center shrink-0">
          <ClipboardList class="w-5 h-5" />
        </div>
        <div class="min-w-0">
          <h3 class=" text-slate-800 truncate">{{ campaign.name }}</h3>
          <p class="text-xs text-muted-foreground mt-0.5 flex items-center gap-1">
            <Calendar class="w-3 h-3 shrink-0" />
            {{ campaign.startDate }} → {{ campaign.endDate }}
          </p>
          <p class="text-[10px] text-muted-foreground/70 mt-1 flex items-center gap-1 font-semibold">
            <FileText class="w-3 h-3 shrink-0" />
            {{ campaign.questionnairesCount }} {{ t('companyPanel.tests.questionnairesCount') }}
          </p>
        </div>
      </div>
      <CampaignStatusBadge :status="campaign.status" />
    </div>
  </div>
</template>

<script setup lang="ts">
import {
  DropdownMenu, DropdownMenuTrigger, DropdownMenuContent,
  DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator,
} from '@/components/ui/dropdown-menu'
import CampaignStatusBadge from './CampaignStatusBadge.vue'
import { useLocale } from '@/composables/useLocale'
import { ClipboardList, Calendar, FileText, MoreHorizontal, Pencil, Trash2, Users } from 'lucide-vue-next'

const { t } = useLocale()

export interface CampaignCardData {
  id:                  number
  name:                string
  startDate:           string
  endDate:             string
  status:              string
  invited:             number
  completed:           number
  passRate:            number
  avgScore:            number
  questionnairesCount: number
}

defineProps<{
  campaign: CampaignCardData
  canEdit?: boolean
  canDelete?: boolean
}>()

defineEmits<{
  (e: 'click'): void
  (e: 'edit'): void
  (e: 'delete'): void
  (e: 'candidates'): void
}>()

function rateColor(rate: number) {
  if (rate >= 70) return 'text-emerald-600 dark:text-emerald-400'
  if (rate >= 50) return 'text-amber-600 dark:text-amber-400'
  return 'text-red-600 dark:text-red-400'
}

function barColor(rate: number) {
  if (rate >= 70) return 'bg-emerald-500'
  if (rate >= 50) return 'bg-amber-500'
  return 'bg-red-500'
}
</script>