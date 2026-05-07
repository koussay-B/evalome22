<template>
  <div class="space-y-6 max-w-3xl mx-auto">

    <!-- Loading -->
    <div v-if="loading" class="space-y-4">
      <div class="h-28 rounded-2xl bg-muted animate-pulse" />
      <div v-for="i in 3" :key="i" class="h-28 rounded-xl bg-muted animate-pulse" />
    </div>

    <!-- Error -->
    <div
      v-else-if="loadError"
      class="flex flex-col items-center gap-3 py-16 text-center"
    >
      <CircleAlert class="w-10 h-10 text-red-500" />
      <p class="font-bold text-slate-800">{{ loadError }}</p>
      <button
        class="px-4 py-2 rounded-lg border border-border text-sm font-semibold"
        @click="router.push({ name: 'CandidatTests' })"
      >{{ t('candidatTests.title') }}</button>
    </div>

    <template v-else-if="campaign">

      <!-- Back -->
      <button
        class="flex items-center gap-1.5 text-sm text-muted-foreground hover:text-slate-800 transition-colors"
        @click="router.push({ name: 'CandidatTests' })"
      >
        <ArrowLeft class="w-4 h-4" />
        {{ t('candidatTests.title') }}
      </button>

      <!-- Campaign header -->
      <div class="rounded-2xl border border-border bg-gray-50 p-6 space-y-3">
        <div class="flex items-start gap-4">
          <div class="w-12 h-12 rounded-xl bg-primary/10 text-primary flex items-center justify-center shrink-0">
            <ClipboardList class="w-6 h-6" />
          </div>
          <div class="flex-1 min-w-0">
            <h1 class="text-2xl  text-slate-800">{{ campaign.name }}</h1>
            <p v-if="campaign.description" class="text-sm text-muted-foreground mt-1">{{ campaign.description }}</p>
            <div class="flex flex-wrap items-center gap-3 mt-2">
              <span class="flex items-center gap-1 text-xs text-muted-foreground">
                <Calendar class="w-3.5 h-3.5" />
                {{ formatDateTime(campaign.startDate) }} → {{ formatDateTime(campaign.endDate) }}
              </span>
              <span :class="statusBadge(campaign.status)" class="text-[10px]  uppercase tracking-wide px-2 py-0.5 rounded-full">
                {{ campaign.status }}
              </span>
              <span :class="candidateBadge(campaign.candidateStatus)" class="text-[10px]  uppercase tracking-wide px-2 py-0.5 rounded-full">
                {{ statusLabel(campaign.candidateStatus) }}
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- Questionnaires list -->
      <div class="space-y-3 ">
        <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">
          {{ t('campaignDetail.questionnaires') }} ({{ campaign.questionnaires.length }})
        </p>

        <div
          v-for="q in campaign.questionnaires"
          :key="q.questionnaireId"
          class="rounded-xl border border-border bg-slate-50 p-5 flex flex-col gap-4"
        >
          <!-- Header row -->
          <div class="flex items-start gap-3">
            <div class="w-10 h-10 rounded-xl bg-muted flex items-center justify-center shrink-0 text-muted-foreground">
              <FileText class="w-5 h-5" />
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 flex-wrap">
                <h3 class=" text-slate-800">{{ q.label || q.title }}</h3>
                <span v-if="q.label" class="text-xs text-muted-foreground">({{ q.title }})</span>
                <span
                  v-if="q.isRequired"
                  class="text-[10px]  uppercase tracking-wide px-1.5 py-0.5 rounded bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400"
                >{{ t('campaignDetail.required') }}</span>
              </div>
              <!-- Meta info -->
              <div class="flex flex-wrap gap-3 mt-1.5">
                <span class="flex items-center gap-1 text-xs text-muted-foreground">
                  <HelpCircle class="w-3.5 h-3.5" />
                  {{ q.questionCount }} {{ t('campaignDetail.questions') }}
                </span>
                <span v-if="q.durationMinutes" class="flex items-center gap-1 text-xs text-muted-foreground">
                  <Clock class="w-3.5 h-3.5" />
                  {{ q.durationMinutes }} {{ t('campaignDetail.minutes') }}
                </span>
                <span class="flex items-center gap-1 text-xs text-muted-foreground">
                  <Target class="w-3.5 h-3.5" />
                  {{ t('campaignDetail.passScore') }}: {{ q.passingScore }}%
                </span>
                <!-- Attempts counter -->
                <span class="flex items-center gap-1 text-xs font-semibold" :class="q.remainingAttempts > 0 || q.maxAttempts === -1 ? 'text-emerald-600' : 'text-muted-foreground'">
                  <RefreshCcw class="w-3.5 h-3.5" />
                  {{ q.maxAttempts === -1 ? '∞' : q.remainingAttempts }} {{ t('campaignDetail.attemptsLeft') }}
                </span>
              </div>
            </div>
            <!-- Attempt status badge -->
            <span :class="attemptBadge(q.attemptStatus)" class="text-[10px]  uppercase tracking-wide px-2 py-0.5 rounded-full shrink-0">
              {{ attemptStatusLabel(q.attemptStatus) }}
            </span>
          </div>

          <!-- Score (if completed) -->
          <div v-if="q.attemptStatus === 'Submitted' && q.score !== null && q.score !== undefined" class="flex items-center gap-2">
            <span
              :class="q.passed ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-600 dark:text-red-400'"
              class="text-2xl font-black"
            >{{ Math.round(q.score) }}%</span>
            <span
              :class="q.passed
                ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300'
                : 'bg-red-100 text-red-600 dark:bg-red-900/40 dark:text-red-400'"
              class="text-[10px]  uppercase tracking-wide px-2 py-0.5 rounded-full"
            >{{ q.passed ? t('candidatTests.passedBadge') : t('candidatTests.failedBadge') }}</span>
          </div>

            <!-- Actions -->
          <div class="flex flex-col gap-2">
            <div class="flex gap-3">
              <!-- Start / Continue -->
              <button
                v-if="q.attemptStatus === 'NotStarted' || q.attemptStatus === 'InProgress'"
                :disabled="campaign.status !== 'Active' || starting === q.questionnaireId || isExpired"
                class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                @click="startQuestionnaire(q)"
              >
                <Loader2 v-if="starting === q.questionnaireId" class="w-4 h-4 animate-spin" />
                <PlayCircle v-else class="w-4 h-4" />
                {{ q.attemptStatus === 'InProgress' ? t('candidatTests.continueTest') : t('candidatTests.startTest') }}
              </button>

              <!-- View result -->
              <button
                v-if="q.attemptStatus === 'Submitted' && q.attemptId"
                class="flex items-center gap-2 px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors"
                @click="router.push({ name: 'CandidatTestResult', params: { id: q.attemptId } })"
              >
                <BarChart2 class="w-4 h-4" /> {{ t('candidatTests.viewResults') }}
              </button>

              <!-- Notices -->
              <p
                v-if="q.attemptStatus === 'NotStarted' && (campaign.status !== 'Active' || isExpired)"
                class="text-xs text-muted-foreground self-center"
              >
                <template v-if="isExpired">{{ t('candidatTests.expired') }}</template>
                <template v-else>
                  {{ campaign.status === 'Scheduled' ? t('candidatTests.notYetAvailable') : t('candidatTests.campaignClosed') }}
                </template>
              </p>
            </div>

            <!-- Error message if start fails -->
            <p v-if="startError && starting === null" class="text-xs text-red-500 font-medium flex items-center gap-1">
              <CircleAlert class="w-3 h-3" />
              {{ startError }}
            </p>
          </div>
        </div>

        <!-- Empty -->
        <div
          v-if="campaign.questionnaires.length === 0"
          class="text-center py-8 text-muted-foreground text-sm"
        >
          {{ t('campaignDetail.noQuestionnaires') }}
        </div>
      </div>

    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useLocale } from '@/composables/useLocale'
import { getCampaignDetailApi, startAttemptApi } from '@/utils/api'
import type { MyCampaignDetail, CampaignQuestionnaireForCandidate } from '@/utils/models'
import {
  ArrowLeft, ClipboardList, Calendar, FileText, HelpCircle,
  Clock, Target, PlayCircle, BarChart2, CircleAlert, Loader2,
  RefreshCcw,
} from 'lucide-vue-next'

const { t } = useLocale()
const route  = useRoute()
const router = useRouter()

const campaignId = Number(route.params.id)

const loading   = ref(true)
const loadError = ref<string | null>(null)
const campaign  = ref<MyCampaignDetail | null>(null)
const starting  = ref<number | null>(null)
const startError = ref<string | null>(null)

const isExpired = computed(() => {
  if (!campaign.value) return false
  const until = new Date(campaign.value.endDate)
  if (Number.isNaN(until.getTime())) return false
  // Check if current date is strictly after AvailableUntil
  return new Date() > until
})

onMounted(async () => {
  const { data, error } = await getCampaignDetailApi(campaignId)
  if (error || !data) {
    loadError.value = error ?? 'Failed to load campaign'
  } else {
    campaign.value = data
  }
  loading.value = false
})

async function startQuestionnaire(q: CampaignQuestionnaireForCandidate) {
  if (q.attemptStatus === 'InProgress' && q.attemptId) {
    // Resume: navigate directly if we already have the session data
    const cached = sessionStorage.getItem(`attempt_${q.attemptId}`)
    if (cached) {
      router.push({ name: 'CandidatTestTaking', params: { id: q.attemptId } })
      return
    }
  }

  starting.value = q.questionnaireId
  startError.value = null
  const { data, error } = await startAttemptApi(campaignId, q.questionnaireId)
  starting.value = null

  if (error || !data) {
    startError.value = error ?? 'Failed to start'
    return
  }

  sessionStorage.setItem(`attempt_${data.attemptId}`, JSON.stringify(data))
  router.push({ name: 'CandidatTestTaking', params: { id: data.attemptId } })
}

function statusLabel(s: string): string {
  const map: Record<string, string> = {
    Invited:    t('candidatTests.upcoming'),
    InProgress: t('candidatTests.inProgress'),
    Completed:  t('candidatTests.completed'),
    Expired:    t('candidatTests.expired'),
    Withdrawn:  t('candidatTests.withdrawn'),
  }
  return map[s] ?? s
}

function attemptStatusLabel(s: string): string {
  const map: Record<string, string> = {
    NotStarted: t('campaignDetail.notStarted'),
    InProgress: t('candidatTests.inProgress'),
    Submitted:  t('candidatTests.completed'),
    TimedOut:   t('campaignDetail.timedOut'),
    Abandoned:  t('campaignDetail.abandoned'),
  }
  return map[s] ?? s
}

function statusBadge(s: string): string {
  const map: Record<string, string> = {
    Active:    'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300',
    Scheduled: 'bg-primary/10 text-primary dark:bg-primary/15 dark:text-orange-300',
    Closed:    'bg-muted text-muted-foreground',
  }
  return map[s] ?? 'bg-muted text-muted-foreground'
}

function candidateBadge(s: string): string {
  const map: Record<string, string> = {
    Invited:    'bg-primary/10 text-primary dark:bg-primary/15 dark:text-orange-300',
    InProgress: 'bg-orange-100 text-orange-700 dark:bg-orange-950/40 dark:text-orange-300',
    Completed:  'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300',
    Expired:    'bg-muted text-muted-foreground',
  }
  return map[s] ?? 'bg-muted text-muted-foreground'
}

function attemptBadge(s: string): string {
  const map: Record<string, string> = {
    NotStarted: 'bg-primary/10 text-primary dark:bg-primary/15 dark:text-orange-300',
    InProgress: 'bg-orange-100 text-orange-700 dark:bg-orange-950/40 dark:text-orange-300',
    Submitted:  'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300',
    TimedOut:   'bg-red-100 text-red-600 dark:bg-red-900/40 dark:text-red-400',
    Abandoned:  'bg-red-100 text-red-600 dark:bg-red-900/40 dark:text-red-400',
  }
  return map[s] ?? 'bg-muted text-muted-foreground'
}

function formatDateTime(value: string): string {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value

  return date.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
</script>
