<template>
  <div class="space-y-6 max-w-2xl mx-auto">

    <!-- Loading skeleton -->
    <div v-if="loading" class="space-y-6">
      <div class="h-40 rounded-2xl bg-muted animate-pulse" />
      <div class="h-32 rounded-2xl bg-muted animate-pulse" />
    </div>

    <!-- Error -->
    <div
      v-else-if="loadError"
      class="flex flex-col items-center gap-3 py-16 text-center"
    >
      <CircleAlert class="w-10 h-10 text-red-500" />
      <p class="font-bold text-slate-800">{{ loadError }}</p>
      <button
        class="px-4 py-2 rounded-lg border border-border text-sm font-semibold hover:bg-muted/50 transition-colors"
        @click="router.push({ name: 'CandidatTests' })"
      >{{ t('testResult.backToTests') }}</button>
    </div>

    <template v-else-if="result">

      <!-- Termination reason banner -->
      <div
        v-if="terminationReason"
        class="flex items-center gap-2.5 px-4 py-2.5 rounded-xl border text-sm font-medium"
        :class="{
          'bg-amber-50 border-amber-200/80 text-amber-800 dark:bg-amber-950/30 dark:border-amber-900 dark:text-amber-400': terminationReason === 'TabSwitchLimit',
          'bg-red-50 border-red-200/80 text-red-800 dark:bg-red-950/30 dark:border-red-900 dark:text-red-400': terminationReason === 'TimerExpired',
          'bg-muted/60 border-border text-muted-foreground': terminationReason === 'UserQuit',
        }"
      >
        <component
          :is="terminationReason === 'TimerExpired' ? Clock : terminationReason === 'TabSwitchLimit' ? AlertTriangle : LogOut"
          class="w-4 h-4 shrink-0"
        />
        <p>{{ t(`testResult.reason_${terminationReason}`) }}</p>
      </div>

      <!-- Hero banner -->
      <div
        :class="result.passed
          ? 'bg-emerald-50 dark:bg-emerald-950/30 border-emerald-200/80 dark:border-emerald-900'
          : 'bg-red-50 dark:bg-red-950/30 border-red-200/80 dark:border-red-900'"
        class="rounded-2xl border p-8 flex flex-col items-center text-center gap-5"
      >

        <!-- Icon badge -->
        <div
          :class="result.passed
            ? 'bg-emerald-100 dark:bg-emerald-900/40 text-emerald-600 dark:text-emerald-400'
            : 'bg-red-100 dark:bg-red-900/40 text-red-600 dark:text-red-400'"
          class="w-16 h-16 rounded-full flex items-center justify-center ring-4 ring-white/60 dark:ring-white/10"
        >
          <CheckCircle v-if="result.passed" class="w-8 h-8" />
          <XCircle v-else class="w-8 h-8" />
        </div>

        <!-- Title block -->
        <div>
  <p v-if="result.questionnaireName" class="text-[10px] font-bold uppercase tracking-[0.2em] text-stone-400 mb-2">
    {{ result.questionnaireName }}
  </p>

  <h1
    :class="result.passed ? 'text-emerald-600' : 'text-rose-600'"
    class="text-xl font-semibold tracking-tight"
  >
    {{ result.passed ? t('testResult.passedTitle') : t('testResult.failedTitle') }}
  </h1>

  <p class="text-stone-400 mt-1 text-[11px] font-medium">
    {{ t('testResult.submittedOn') }} {{ formatDate(result.submittedAt) }}
  </p>
</div>
        <!-- Score stats -->
        <div class="flex items-center">
          <div class="text-center px-6">
            <div
              :class="result.passed ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-600 dark:text-red-400'"
              class="text-4xl font-bold"
            >
              {{ Math.round(result.percentage) }}%
            </div>
            <div class="text-[10px] text-muted-foreground font-semibold uppercase tracking-wider mt-1">{{ t('testResult.percentage') }}</div>
          </div>
          <div
            :class="result.passed ? 'bg-emerald-200/60' : 'bg-red-200/60'"
            class="h-10 w-px"
          />
          <div class="text-center px-6">
            <div class="text-4xl font-bold text-slate-800 dark:text-slate-100">{{ Math.round(result.rawScore) }}</div>
            <div class="text-[10px] text-muted-foreground font-semibold uppercase tracking-wider mt-1">{{ t('testResult.score') }}</div>
          </div>
          <div
            :class="result.passed ? 'bg-emerald-200/60' : 'bg-red-200/60'"
            class="h-10 w-px"
          />
          <div class="text-center px-6">
            <div class="text-4xl font-bold text-slate-800 dark:text-slate-100">{{ Math.round(result.maxPossibleScore) }}</div>
            <div class="text-[10px] text-muted-foreground font-semibold uppercase tracking-wider mt-1">{{ t('testResult.maxScore') }}</div>
          </div>
        </div>

        <!-- Progress bar -->
        <div class="w-full max-w-xs">
          <div
            :class="result.passed ? 'bg-emerald-100 dark:bg-emerald-900/30' : 'bg-red-100 dark:bg-red-900/30'"
            class="h-2 rounded-full overflow-hidden"
          >
            <div
              :class="result.passed ? 'bg-emerald-500' : 'bg-red-500'"
              class="h-full rounded-full transition-all duration-1000"
              :style="{ width: result.percentage + '%' }"
            />
          </div>
        </div>

        <!-- Attempt counter -->
        <p
          v-if="result.maxAttempts !== undefined && result.maxAttempts !== -1"
          :class="canRetry ? 'text-emerald-600 dark:text-emerald-400' : 'text-muted-foreground'"
          class="text-xs font-medium flex items-center gap-1.5"
        >
          <RefreshCcw class="w-3 h-3 shrink-0" />
          {{ t('testResult.attemptInfo')
              .replace('{used}', String(attemptsUsed))
              .replace('{max}', String(result.maxAttempts)) }}
          <span v-if="result.remainingAttempts && result.remainingAttempts > 0" class="font-semibold">
            ({{ result.remainingAttempts }} {{ t('testResult.attemptsLeft') }})
          </span>
        </p>
        <p
          v-else-if="result.maxAttempts === -1"
          class="text-xs text-muted-foreground flex items-center gap-1.5"
        >
          <RefreshCcw class="w-3 h-3 shrink-0" />
          {{ t('testResult.unlimitedAttempts') }}
        </p>

      </div>

      <!-- AI Feedback -->
      <div class="space-y-3">

        <!-- Shimmer placeholders while generating -->
        <template v-if="generatingReport">
          <div class="rounded-xl border border-border overflow-hidden">
            <div class="px-5 pt-4 pb-3">
              <div class="h-2.5 w-24 rounded-full bg-muted animate-pulse mb-3" />
              <div class="space-y-2">
                <div class="h-2.5 rounded-full bg-muted animate-pulse" />
                <div class="h-2.5 w-4/5 rounded-full bg-muted animate-pulse" />
                <div class="h-2.5 w-3/5 rounded-full bg-muted animate-pulse" />
              </div>
            </div>
            <div class="px-5 pb-4 pt-3 border-t border-border">
              <div class="h-2.5 w-28 rounded-full bg-muted animate-pulse mb-3" />
              <div class="space-y-2">
                <div class="h-2.5 rounded-full bg-muted animate-pulse" />
                <div class="h-2.5 w-5/6 rounded-full bg-muted animate-pulse" />
                <div class="h-2.5 w-2/3 rounded-full bg-muted animate-pulse" />
              </div>
            </div>
          </div>
        </template>

        <!-- AI Strengths -->
        <div v-if="!generatingReport && aiStrengths" class="rounded-xl border border-emerald-200/70 dark:border-emerald-900 bg-slate-50 dark:bg-emerald-950/20 p-5 space-y-2.5">
          <div class="flex items-center gap-2">
            <span class="inline-flex items-center gap-1.5 text-[11px] font-semibold uppercase tracking-wider text-emerald-700 dark:text-emerald-400 bg-emerald-100 dark:bg-emerald-900/50 px-2.5 py-1 rounded-full">
              <Sparkles class="w-3 h-3" />
              {{ t('testResult.aiStrengths') }}
            </span>
          </div>
          <p class="text-sm text-slate-700 dark:text-slate-300 leading-relaxed">{{ aiStrengths }}</p>
        </div>

        <!-- AI Recommendations -->
        <div v-if="!generatingReport && aiRecommendations" class="rounded-xl border border-border bg-slate-50 dark:bg-muted/10 p-5 space-y-2.5">
          <div class="flex items-center gap-2">
            <span class="inline-flex items-center gap-1.5 text-[11px] font-semibold uppercase tracking-wider text-muted-foreground bg-muted px-2.5 py-1 rounded-full">
              <Sparkles class="w-3 h-3" />
              {{ t('testResult.aiRecommendations') }}
            </span>
          </div>
          <p class="text-sm text-slate-700 dark:text-slate-300 leading-relaxed">{{ aiRecommendations }}</p>
        </div>

      </div>

      <!-- Theme breakdown -->
      <div v-if="themeScores && Object.keys(themeScores).length > 0" class="space-y-2.5">
        <p class="text-[11px] font-semibold uppercase tracking-widest text-muted-foreground">{{ t('testResult.themes') }}</p>
        <div class="rounded-xl border border-border overflow-hidden">
          <div
            v-for="([theme, score], i) in Object.entries(themeScores)"
            :key="theme"
            :class="i % 2 === 0 ? 'bg-secondary/40' : 'bg-transparent'"
            class="flex items-center justify-between px-4 py-3 border-b border-border/50 last:border-b-0"
          >
            <span class="text-sm font-medium text-slate-800 dark:text-slate-200">{{ theme }}</span>
            <div class="flex items-center gap-3">
              <div class="w-24 h-1.5 rounded-full bg-muted overflow-hidden">
                <div
                  :class="score >= 60 ? 'bg-emerald-500' : score >= 40 ? 'bg-amber-500' : 'bg-red-500'"
                  class="h-full rounded-full transition-all duration-700"
                  :style="{ width: score + '%' }"
                />
              </div>
              <span
                :class="score >= 60 ? 'text-emerald-600 dark:text-emerald-400' : score >= 40 ? 'text-amber-600 dark:text-amber-400' : 'text-red-600 dark:text-red-400'"
                class="text-sm font-semibold w-10 text-end"
              >{{ Math.round(score) }}%</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Actions -->
      <div class="flex flex-wrap gap-2.5">

        <!-- Certificate -->
        <button
          v-if="result.passed"
          class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors"
          @click="router.push({ name: 'CandidatCertificates' })"
        >
          <Award class="w-4 h-4" /> {{ t('testResult.viewCertificate') }}
        </button>

        <!-- Retry -->
        <div class="flex flex-col gap-1.5">
          <button
            v-if="!result.passed && canRetry"
            :disabled="starting"
            class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/85 transition-colors disabled:opacity-50"
            @click="retryTest"
          >
            <Loader2 v-if="starting" class="w-4 h-4 animate-spin" />
            <RefreshCcw v-else class="w-4 h-4" />
            {{ t('testResult.retry') }}
            <span v-if="result.remainingAttempts && result.remainingAttempts > 0" class="text-xs opacity-75">
              ({{ result.remainingAttempts }} {{ t('testResult.attemptsLeft') }})
            </span>
          </button>
          <p v-if="retryError" class="text-xs text-red-500 font-medium flex items-center gap-1">
            <CircleAlert class="w-3 h-3" />
            {{ retryError }}
          </p>
        </div>

        <!-- Generate AI Report -->
        <button
          v-if="!result.passed && !canRetry && !hasAiReport && !generatingReport"
          :disabled="generatingReport"
          class="flex items-center gap-2 px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800 dark:text-slate-200 hover:bg-muted/50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          @click="generateReport"
        >
          <Sparkles class="w-4 h-4" /> {{ t('testResult.generateReport') }}
        </button>

        <!-- Back to campaign -->
        <button
          v-if="result.campaignId"
          class="flex items-center gap-2 px-4 py-2.5 rounded-lg border border-primary/40 text-sm font-semibold text-slate-800 dark:text-slate-200 hover:bg-primary/10 transition-colors"
          @click="router.push({ name: 'CandidatCampaignDetail', params: { id: result.campaignId } })"
        >
          <ArrowLeft class="w-4 h-4" /> {{ t('testResult.backToCampaign') }}
        </button>

        <!-- Back to tests -->
        <button
          class="flex items-center gap-2 px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800 dark:text-slate-200 hover:bg-muted/50 transition-colors"
          @click="router.push({ name: 'CandidatTests' })"
        >
          <ArrowLeft class="w-4 h-4" /> {{ t('testResult.backToTests') }}
        </button>

      </div>

    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useLocale } from '@/composables/useLocale'
import { getAttemptResultApi, generateReportApi, startAttemptApi } from '@/utils/api'
import type { AttemptResultItem, TerminationReason } from '@/utils/models'
import {
  CheckCircle, XCircle, CircleAlert, Award, ArrowLeft,
  RefreshCcw, Sparkles, AlertTriangle, Clock, LogOut, Loader2,
} from 'lucide-vue-next'

const { t } = useLocale()
const route  = useRoute()
const router = useRouter()

const attemptId = computed(() => Number(route.params.id))

const loading         = ref(true)
const loadError       = ref<string | null>(null)
const result          = ref<AttemptResultItem | null>(null)
const generatingReport = ref(false)
const starting        = ref(false)
const retryError      = ref<string | null>(null)

// AI content — starts from result, updated after on-demand generation
const aiStrengths       = ref<string | null>(null)
const aiRecommendations = ref<string | null>(null)

// Termination reason stored by TestTaking before navigating
const terminationReason = ref<TerminationReason | null>(null)

const hasAiReport = computed(() => !!(aiStrengths.value || aiRecommendations.value))

const attemptsUsed = computed(() => {
  if (!result.value) return 0
  const max  = result.value.maxAttempts ?? 1
  const rem  = result.value.remainingAttempts ?? 0
  if (max === -1) return 0
  return max - rem
})

// Can retry: failed + remainingAttempts > 0 (or unlimited)
const canRetry = computed(() => {
  if (!result.value || result.value.passed) return false
  const rem = result.value.remainingAttempts
  return rem === -1 || (rem !== undefined && rem > 0)
})

const themeScores = computed<Record<string, number> | null>(() => {
  const raw = result.value?.report?.themeScores
  if (!raw || typeof raw !== 'object') return null
  return raw
})

onMounted(async () => {
  // Read termination reason from sessionStorage (written by TestTaking)
  const reasonRaw = sessionStorage.getItem(`reason_${attemptId.value}`)
  if (reasonRaw) {
    terminationReason.value = reasonRaw as TerminationReason
    sessionStorage.removeItem(`reason_${attemptId.value}`)
  }

  const { data, error } = await getAttemptResultApi(attemptId.value)
  if (error || !data) {
    loadError.value = error ?? 'Failed to load results'
  } else {
    result.value          = data
    aiStrengths.value     = data.report?.aiStrengths ?? null
    aiRecommendations.value = data.report?.aiRecommendations ?? null
  }
  loading.value = false
})

async function generateReport() {
  if (!result.value) return
  generatingReport.value = true
  const { data } = await generateReportApi(attemptId.value)
  generatingReport.value = false
  if (data) {
    aiStrengths.value       = data.aiStrengths
    aiRecommendations.value = data.aiRecommendations
  }
}

async function retryTest() {
  if (!result.value || starting.value) return
  const campaignId       = result.value.campaignId
  const questionnaireId  = result.value.questionnaireId ?? undefined

  starting.value = true
  retryError.value = null

  const { data, error } = await startAttemptApi(campaignId, questionnaireId)
  
  if (error || !data) {
    retryError.value = error ?? 'Failed to start attempt'
    starting.value = false
    return
  }

  sessionStorage.setItem(`attempt_${data.attemptId}`, JSON.stringify(data))
  router.push({ name: 'CandidatTestTaking', params: { id: data.attemptId } })
}

function formatDate(iso?: string | null): string {
  if (!iso) return '—'
  return new Date(iso).toLocaleDateString(undefined, {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}
</script>
