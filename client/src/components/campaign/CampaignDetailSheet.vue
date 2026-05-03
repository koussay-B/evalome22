<template>

  <!-- Candidates sheet (stacked) -->
  <CampaignCandidatesSheet
    v-if="campaign"
    v-model:open="isCandidatesOpen"
    :campaign-id="campaign.id"
    :campaign-name="campaign.name"
  />

  <Sheet v-model:open="isOpen">
    <SheetContent :side="sheetSide" class="sm:max-w-md flex flex-col p-0">
      <template v-if="campaign">
        <!-- Header -->
        <SheetHeader class="px-6 pt-6 pb-5 border-b border-border shrink-0">
          <div class="flex items-start gap-4">
            <div class="w-12 h-12 rounded-2xl bg-primary/10 text-primary flex items-center justify-center shrink-0 mt-0.5">
              <ClipboardList class="w-6 h-6" />
            </div>
            <div class="min-w-0 flex-1">
              <SheetTitle class="text-xl text-slate-800 leading-tight">{{ campaign.name }}</SheetTitle>
              <SheetDescription class="mt-1.5">
                <CampaignStatusBadge :status="campaign.status" />
              </SheetDescription>
            </div>
          </div>
        </SheetHeader>

        <!-- Body -->
        <div class="flex-1 overflow-y-auto flex flex-col gap-6 p-6">

          <!-- Dates -->
          <div class="space-y-3">
            <p class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('campaignDetail.timeline') }}</p>
            <div class="grid grid-cols-2 gap-3">
              <div class="rounded-xl border border-border bg-muted/40 p-3 space-y-0.5">
                <p class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">{{ t('campaignDetail.start') }}</p>
                <p class="text-sm font-bold text-foreground">{{ campaign.startDate }}</p>
              </div>
              <div class="rounded-xl border border-border bg-muted/40 p-3 space-y-0.5">
                <p class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">{{ t('campaignDetail.end') }}</p>
                <p class="text-sm font-bold text-foreground">{{ campaign.endDate }}</p>
              </div>
            </div>
          </div>

          <!-- Stats grid -->
          <div class="space-y-3">
            <p class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('campaignDetail.statsSection') }}</p>
            <div class="grid grid-cols-2 gap-3 rounded-xl border border-border overflow-hidden">
              <div class="p-4 text-center border-e border-b border-border">
                <div class="text-2xl text-slate-800 text-foreground">{{ campaign.invited }}</div>
                <div class="text-[10px] font-bold uppercase tracking-wide text-muted-foreground mt-1">{{ t('campaignDetail.invited') }}</div>
              </div>
              <div class="p-4 text-center border-b border-border">
                <div class="text-2xl text-slate-800 text-foreground">{{ campaign.completed }}</div>
                <div class="text-[10px] font-bold uppercase tracking-wide text-muted-foreground mt-1">{{ t('campaignDetail.completed') }}</div>
              </div>
              <div class="p-4 text-center border-e border-border">
                <div class="text-2xl text-slate-800" :class="rateColor(campaign.passRate)">{{ campaign.passRate }}%</div>
                <div class="text-[10px] font-bold uppercase tracking-wide text-muted-foreground mt-1">{{ t('campaignDetail.passRate') }}</div>
              </div>
              <div class="p-4 text-center">
                <div class="text-2xl text-slate-800 text-foreground">{{ campaign.avgScore }}%</div>
                <div class="text-[10px] font-bold uppercase tracking-wide text-muted-foreground mt-1">{{ t('campaignDetail.avgScore') }}</div>
              </div>
            </div>
          </div>

          <!-- Pass rate bar -->
          <div class="space-y-2">
            <div class="flex justify-between text-xs">
              <span class="font-bold text-muted-foreground uppercase tracking-wider">{{ t('campaignDetail.passRateSection') }}</span>
              <span class="font-bold text-foreground">{{ campaign.passRate }}%</span>
            </div>
            <div class="h-2.5 rounded-full bg-muted overflow-hidden">
              <div
                class="h-full rounded-full transition-all duration-700"
                :class="barColor(campaign.passRate)"
                :style="{ width: campaign.passRate + '%' }"
              />
            </div>
            <p class="text-[11px] text-muted-foreground">
              {{ campaign.completed }} {{ t('campaignDetail.of') }} {{ campaign.invited }} {{ t('campaignDetail.candidatesCompleted') }}
              <template v-if="campaign.invited > 0">
                ({{ Math.round((campaign.completed / campaign.invited) * 100) }}% {{ t('campaignDetail.completionPct') }})
              </template>
            </p>
          </div>

          <!-- Questionnaires -->
          <div class="space-y-2">
            <p class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('campaignDetail.questionnaireSection') }}</p>
            <div v-if="campaignQuestionnaires.length > 0" class="space-y-2">
              <button
                v-for="cq in campaignQuestionnaires"
                :key="cq.id"
                class="w-full flex items-center gap-4 px-4 py-3.5 rounded-xl border border-border hover:border-primary/40 hover:bg-primary/5 transition-all text-start group"
                @click="openQuestionnaire(cq.questionnaireId)"
              >
                <div class="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
                  <FileText class="w-5 h-5 text-primary" />
                </div>
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-bold text-slate-800truncate">{{ cq.label || cq.questionnaireTitle }}</p>
                  <p v-if="cq.label" class="text-xs text-muted-foreground mt-0.5 truncate">{{ cq.questionnaireTitle }}</p>
                </div>
                <ArrowRight class="w-4 h-4 text-muted-foreground group-hover:text-primary group-hover:translate-x-0.5 transition-all shrink-0" />
              </button>
            </div>
            <div v-else class="flex items-start gap-3 px-4 py-3.5 rounded-xl border border-dashed border-border bg-muted/20">
              <div class="w-9 h-9 rounded-lg bg-muted flex items-center justify-center shrink-0 mt-0.5">
                <FileQuestion class="w-4 h-4 text-muted-foreground" />
              </div>
              <div>
                <p class="text-sm font-semibold text-foreground">{{ t('campaignDetail.noQuestionnaire') }}</p>
                <p class="text-xs text-muted-foreground mt-0.5">{{ t('campaignDetail.noQuestionnaireDesc') }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex flex-wrap gap-2 px-6 py-5 border-t border-border shrink-0">
          <!-- Edit -->
          <button
            v-if="canEdit"
            class="flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors"
            @click="$emit('edit', campaign)"
          >
            <Pencil class="w-4 h-4" /> {{ t('campaignDetail.editBtn') }}
          </button>

          <!-- Candidates -->
          <button
            class="flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800hover:bg-muted/50 transition-colors"
            @click="openCandidates"
          >
            <Users class="w-4 h-4" /> {{ t('campaignDetail.candidatesBtn') }}
          </button>

          <!-- Questions shortcut (no-edit view) -->
          <button
            v-if="campaignQuestionnaires.length > 0 && !canEdit"
            class="flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800hover:bg-muted/50 transition-colors"
            @click="openQuestionnaire(campaignQuestionnaires[0]!.questionnaireId)"
          >
            <FileText class="w-4 h-4" /> {{ t('campaignDetail.questionsBtn') }}
          </button>

          <!-- Delete -->
          <button
            v-if="canDelete"
            class="flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg border border-red-200 dark:border-red-900 text-sm font-semibold text-red-600 hover:bg-red-50 dark:hover:bg-red-950/30 transition-colors"
            @click="$emit('delete', campaign)"
          >
            <Trash2 class="w-4 h-4" />
          </button>

          <!-- Close -->
          <button
            class="px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800hover:bg-muted/50 transition-colors"
            @click="isOpen = false"
          >
            {{ t('campaignDetail.closeBtn') }}
          </button>
        </div>
      </template>
    </SheetContent>
  </Sheet>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { getCampaignQuestionnairesApi } from '@/utils/api'
import type { CampaignQuestionnaireItem } from '@/utils/models'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet'
import CampaignStatusBadge from './CampaignStatusBadge.vue'
import CampaignCandidatesSheet from './CampaignCandidatesSheet.vue'
import { useLocale } from '@/composables/useLocale'
import type { CampaignCardData } from './CampaignCard.vue'
import { ClipboardList, Pencil, Trash2, FileText, ArrowRight, FileQuestion, Users } from 'lucide-vue-next'

const props = defineProps<{
  open:       boolean
  campaign:   CampaignCardData | null
  canEdit?:   boolean
  canDelete?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:open', v: boolean): void
  (e: 'edit',   campaign: CampaignCardData): void
  (e: 'delete', campaign: CampaignCardData): void
}>()

// ── Router — navigate to questionnaire builder ────────────────────────────
const router = useRouter()
const route  = useRoute()

const questionnaireRouteName = computed(() =>
  route.path.startsWith('/formateur') ? 'FormateurQuestionnaire' : 'CompanyQuestionnaire'
)

// ── Questionnaires list ───────────────────────────────────────────────────
const campaignQuestionnaires = ref<CampaignQuestionnaireItem[]>([])

watch(() => props.open, async (open) => {
  if (open && props.campaign) {
    const { data } = await getCampaignQuestionnairesApi(props.campaign.id)
    campaignQuestionnaires.value = data ?? []
  } else if (!open) {
    campaignQuestionnaires.value = []
  }
})

function openQuestionnaire(qId: number) {
  isOpen.value = false
  router.push({ name: questionnaireRouteName.value, params: { id: qId } })
}

const { t, locale } = useLocale()
const isRtl         = computed(() => locale.value === 'ar')
const sheetSide     = computed<'right' | 'left'>(() => isRtl.value ? 'left' : 'right')

const isOpen = computed({
  get: () => props.open,
  set: (v) => emit('update:open', v),
})

// ── Candidates sheet ───────────────────────────────────────────────────────
const isCandidatesOpen = ref(false)

function openCandidates() {
  isCandidatesOpen.value = true
}

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
