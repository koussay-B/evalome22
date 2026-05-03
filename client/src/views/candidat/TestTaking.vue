<template>
  <!-- Leave confirmation dialog -->
  <AlertDialog v-model:open="showLeaveDialog">
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>{{ t('testTaking.leaveWarningTitle') }}</AlertDialogTitle>
        <AlertDialogDescription>{{ t('testTaking.leaveWarningDesc') }}</AlertDialogDescription>
      </AlertDialogHeader>
      <AlertDialogFooter>
        <AlertDialogCancel @click="cancelLeave">{{ t('common.cancel') }}</AlertDialogCancel>
        <AlertDialogAction
          class="bg-red-600 hover:bg-red-700 text-white border-none"
          @click="confirmLeave"
        >
          {{ t('testTaking.leaveBtn') }}
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>

  <!-- Submit confirmation dialog -->
  <AlertDialog v-model:open="showSubmitDialog">
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>{{ t('testTaking.confirmSubmitTitle') }}</AlertDialogTitle>
        <AlertDialogDescription>{{ t('testTaking.confirmSubmitDesc') }}</AlertDialogDescription>
      </AlertDialogHeader>
      <AlertDialogFooter>
        <AlertDialogCancel>{{ t('common.cancel') }}</AlertDialogCancel>
        <AlertDialogAction @click="doSubmit">{{ t('testTaking.submitBtn') }}</AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>

  <!-- Main layout -->
  <div class="min-h-screen flex flex-col bg-background">

    <!-- Top bar -->
    <header class="sticky top-0 z-10 border-b border-border bg-background/95 backdrop-blur px-6 py-3 flex items-center gap-4">
      <!-- Title -->
      <div class="flex-1 min-w-0">
        <p class="text-xs text-muted-foreground font-semibold uppercase tracking-wider">{{ t('testTaking.testLabel') }}</p>
        <h1 class="text-sm  text-slate-800 truncate">{{ sessionData?.questions?.[currentIdx]?.title ? `Q${currentIdx + 1} / ${questions.length}` : '...' }}</h1>
      </div>

      <!-- Tab switch warning -->
      <div
        v-if="tabWarnings > 0"
        class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400 text-xs "
      >
        <AlertTriangle class="w-3.5 h-3.5" />
        {{ t('testTaking.tabSwitchWarning').replace('{n}', String(tabWarnings)) }}
      </div>

      <!-- Timer: countdown (per-question or global duration) -->
      <div
        v-if="sessionData?.showTimer && activeTimeLeft !== null"
        :class="timerClass"
        class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg font-mono  text-sm tabular-nums"
      >
        <Clock class="w-3.5 h-3.5" />
        {{ formattedTime }}
      </div>

      <!-- Timer: elapsed (showTimer=true but no countdown active) -->
      <div
        v-else-if="sessionData?.showTimer"
        class="bg-muted text-muted-foreground flex items-center gap-1.5 px-3 py-1.5 rounded-lg font-mono  text-sm tabular-nums"
      >
        <Clock class="w-3.5 h-3.5" />
        {{ formattedElapsed }}
      </div>

      <!-- Progress -->
      <span class="text-xs text-muted-foreground font-semibold hidden sm:block">
        {{ t('testTaking.questionOf').replace('{cur}', String(currentIdx + 1)).replace('{total}', String(questions.length)) }}
      </span>

      <!-- Submit -->
      <button
        :disabled="submitting"
        class="flex items-center gap-2 px-4 py-2 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        @click="showSubmitDialog = true"
      >
        <Loader2 v-if="submitting" class="w-3.5 h-3.5 animate-spin" />
        <CheckCircle v-else class="w-3.5 h-3.5" />
        {{ submitting ? t('testTaking.submitting') : t('testTaking.submitBtn') }}
      </button>
    </header>

    <!-- Progress bar -->
    <div class="h-1 bg-muted">
      <div
        class="h-full bg-primary transition-all duration-500"
        :style="{ width: `${((currentIdx + 1) / Math.max(questions.length, 1)) * 100}%` }"
      />
    </div>

    <!-- Body -->
    <div class="flex-1 flex flex-col items-center justify-start px-4 py-8 max-w-2xl mx-auto w-full">

      <!-- Loading skeleton -->
      <div v-if="loading" class="w-full space-y-4">
        <div class="h-6 rounded bg-muted animate-pulse w-3/4" />
        <div class="h-4 rounded bg-muted animate-pulse w-1/2" />
        <div class="space-y-3 mt-6">
          <div v-for="i in 4" :key="i" class="h-14 rounded-xl bg-muted animate-pulse" />
        </div>
      </div>

      <!-- Error -->
      <div
        v-else-if="loadError"
        class="flex flex-col items-center gap-3 py-16 text-center"
      >
        <CircleAlert class="w-10 h-10 text-red-500" />
        <p class=" text-slate-800">{{ loadError }}</p>
        <button
          class="px-4 py-2 rounded-lg border border-border text-sm font-semibold hover:bg-muted/50"
          @click="router.push({ name: 'CandidatTests' })"
        >{{ t('candidatTests.title') }}</button>
      </div>

      <!-- Question card -->
      <div v-else-if="currentQuestion" class="w-full space-y-6">

        <!-- Question text -->
        <div class="space-y-2">
          <p class="text-[10px]  uppercase tracking-widest text-muted-foreground">
            {{ t('testTaking.questionOf').replace('{cur}', String(currentIdx + 1)).replace('{total}', String(questions.length)) }}
            · {{ typeLabel(currentQuestion.type) }}
          </p>
          <h2 class="text-xl  text-slate-800 leading-snug">{{ currentQuestion.title }}</h2>
        </div>

        <!-- Hint -->
        <div v-if="currentQuestion.hint">
          <button
            class="text-xs text-muted-foreground hover:text-slate-800 font-semibold flex items-center gap-1"
            @click="showHint = !showHint"
          >
            <ChevronDown :class="showHint ? 'rotate-180' : ''" class="w-3.5 h-3.5 transition-transform" />
            {{ t('testTaking.hint') }}
          </button>
          <p v-if="showHint" class="mt-2 text-sm text-muted-foreground bg-muted/50 rounded-lg px-4 py-3 leading-relaxed">
            {{ currentQuestion.hint }}
          </p>
        </div>

        <!-- Choices -->
        <div class="space-y-3">
          <button
            v-for="choice in currentQuestion.choices"
            :key="choice.id"
            :class="[
              'w-full flex items-center gap-4 px-4 py-4 rounded-xl border text-start transition-all',
              isSelected(choice.id)
                ? 'border-primary bg-primary/10 text-slate-800'
                : 'border-border hover:border-primary/40 hover:bg-muted/30 text-slate-800',
            ]"
            @click="toggleChoice(choice.id)"
          >
            <!-- Radio / Checkbox indicator -->
            <span
              :class="[
                'shrink-0 flex items-center justify-center rounded-full border-2 transition-colors',
                currentQuestion.type === 'QCM' ? 'rounded-md w-5 h-5' : 'rounded-full w-5 h-5',
                isSelected(choice.id)
                  ? 'border-primary bg-primary text-primary-foreground'
                  : 'border-muted-foreground',
              ]"
            >
              <Check v-if="isSelected(choice.id)" class="w-3 h-3" />
            </span>
            <span class="text-sm font-medium">{{ choice.text }}</span>
          </button>
        </div>

        <!-- Navigation -->
        <div class="flex items-center justify-between pt-2">
          <button
            v-if="sessionData?.allowNavigationBack && currentIdx > 0"
            class="flex items-center gap-2 px-4 py-2 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors"
            @click="currentIdx--; showHint = false"
          >
            <ChevronLeft class="w-4 h-4" /> {{ t('testTaking.prev') }}
          </button>
          <div v-else />

          <button
            v-if="currentIdx < questions.length - 1"
            class="flex items-center gap-2 px-4 py-2 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors"
            @click="currentIdx++; showHint = false"
          >
            {{ t('testTaking.next') }} <ChevronRight class="w-4 h-4" />
          </button>
          <button
            v-else
            class="flex items-center gap-2 px-4 py-2 rounded-lg bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 transition-colors"
            @click="showSubmitDialog = true"
          >
            {{ t('testTaking.submitBtn') }} <CheckCircle class="w-4 h-4" />
          </button>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter, onBeforeRouteLeave } from 'vue-router'
import { useUiStore } from '@/store/uiStore'
import type { TerminationReason } from '@/utils/models'
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel,
  AlertDialogContent, AlertDialogDescription, AlertDialogFooter,
  AlertDialogHeader, AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { useLocale } from '@/composables/useLocale'
import { submitAnswerApi, submitAttemptApi, reportTabSwitchApi } from '@/utils/api'
import type { StartAttemptResponse, AttemptQuestion } from '@/utils/models'
import { Clock, CheckCircle, AlertTriangle, ChevronDown, ChevronLeft, ChevronRight, Check, CircleAlert, Loader2 } from 'lucide-vue-next'

const { t } = useLocale()
const route  = useRouter()
const router = useRouter()
const currentRoute = useRoute()
const uiStore = useUiStore()

const attemptId = computed(() => Number(currentRoute.params.id))

// ── Session data ────────────────────────────────────────────────────────────
const sessionData = ref<StartAttemptResponse | null>(null)
const loading     = ref(true)
const loadError   = ref<string | null>(null)

const questions = computed<AttemptQuestion[]>(() => sessionData.value?.questions ?? [])

onMounted(() => {
  uiStore.sidebarOpen = false   // collapse sidebar during test
  // Read session data stored by CandidatTests before navigation
  const raw = sessionStorage.getItem(`attempt_${attemptId.value}`)
  if (raw) {
    try {
      sessionData.value = JSON.parse(raw) as StartAttemptResponse
      loading.value     = false
      startTimer()
      setupAntiCheat()
      // Start per-question timer for the first question
      const firstQ = sessionData.value.questions[0]
      if (sessionData.value.showTimer && firstQ?.timeLimitSeconds) {
        startQuestionTimer(firstQ.timeLimitSeconds)
      }
    } catch {
      loadError.value = 'Failed to load test session'
      loading.value   = false
    }
  } else {
    loadError.value = 'Test session not found. Please start the test again.'
    loading.value   = false
  }
})

// ── Questions navigation ────────────────────────────────────────────────────
const currentIdx      = ref(0)
const showHint        = ref(false)
const currentQuestion = computed<AttemptQuestion | null>(() => questions.value[currentIdx.value] ?? null)

// ── Answers ─────────────────────────────────────────────────────────────────
const answers = ref<Map<number, number[]>>(new Map())

function isSelected(choiceId: number): boolean {
  return answers.value.get(currentQuestion.value?.questionId ?? -1)?.includes(choiceId) ?? false
}

let answerDebounce: ReturnType<typeof setTimeout> | null = null

function toggleChoice(choiceId: number) {
  const qId = currentQuestion.value?.questionId
  if (qId == null) return
  const type = currentQuestion.value?.type

  let selected = [...(answers.value.get(qId) ?? [])]

  if (type === 'QCU' || type === 'TrueFalse') {
    selected = [choiceId]
  } else {
    const idx = selected.indexOf(choiceId)
    if (idx === -1) selected.push(choiceId)
    else selected.splice(idx, 1)
  }

  answers.value.set(qId, selected)

  // Debounced API save
  if (answerDebounce) clearTimeout(answerDebounce)
  answerDebounce = setTimeout(() => {
    submitAnswerApi(attemptId.value, qId, selected, 0)
  }, 500)
}

// ── Timer ────────────────────────────────────────────────────────────────────
const timeLeft         = ref<number | null>(null)   // null = no global countdown
const questionTimeLeft = ref<number | null>(null)   // null = no per-question limit
const elapsedTime      = ref(0)                      // always counting up
let timerInterval:         ReturnType<typeof setInterval> | null = null
let questionTimerInterval: ReturnType<typeof setInterval> | null = null

function storeTerminationReason(reason: TerminationReason) {
  sessionStorage.setItem(`reason_${attemptId.value}`, reason)
}

function clearQuestionTimer() {
  if (questionTimerInterval) { clearInterval(questionTimerInterval); questionTimerInterval = null }
  questionTimeLeft.value = null
}

function startQuestionTimer(limitSeconds: number) {
  clearQuestionTimer()
  questionTimeLeft.value = limitSeconds
  questionTimerInterval = setInterval(() => {
    if (questionTimeLeft.value === null) return
    questionTimeLeft.value--
    if (questionTimeLeft.value <= 0) {
      clearQuestionTimer()
      // auto-advance to next question, or submit if on last
      if (currentIdx.value < questions.value.length - 1) {
        currentIdx.value++
        showHint.value = false
      } else {
        storeTerminationReason('TimerExpired')
        doSubmit(true)
      }
    }
  }, 1000)
}

function startTimer() {
  const hasDuration = !!sessionData.value?.durationMinutes
  if (hasDuration) {
    timeLeft.value = (sessionData.value!.durationMinutes!) * 60
  }
  // Always tick every second (for elapsed time + global countdown enforcement)
  timerInterval = setInterval(() => {
    elapsedTime.value++
    if (timeLeft.value !== null) {
      timeLeft.value--
      if (timeLeft.value <= 0) {
        clearInterval(timerInterval!)
        clearQuestionTimer()
        storeTerminationReason('TimerExpired')
        doSubmit(true)
      }
    }
  }, 1000)
}

// Watch current question changes → start/stop per-question timer
watch(currentIdx, (idx) => {
  const q = questions.value[idx]
  if (sessionData.value?.showTimer && q?.timeLimitSeconds) {
    startQuestionTimer(q.timeLimitSeconds)
  } else {
    clearQuestionTimer()
  }
})

// Active timer seconds to display: prefer question-level, then global, then elapsed
const activeTimeLeft = computed(() => {
  if (questionTimeLeft.value !== null) return questionTimeLeft.value
  return timeLeft.value
})

// Countdown timer display (question-level or global)
const formattedTime = computed(() => {
  const tl = activeTimeLeft.value
  if (tl === null) return ''
  const m = Math.floor(tl / 60).toString().padStart(2, '0')
  const s = (tl % 60).toString().padStart(2, '0')
  return `${m}:${s}`
})

// Elapsed time display (when showTimer=true but no countdown active)
const formattedElapsed = computed(() => {
  const m = Math.floor(elapsedTime.value / 60).toString().padStart(2, '0')
  const s = (elapsedTime.value % 60).toString().padStart(2, '0')
  return `${m}:${s}`
})

const timerClass = computed(() => {
  const tl = activeTimeLeft.value
  if (tl === null) return 'bg-muted text-muted-foreground'
  if (tl > 120) return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-400'
  if (tl > 60)  return 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-400'
  return 'bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-400 animate-pulse'
})

// ── Anti-cheat ───────────────────────────────────────────────────────────────
const tabWarnings = ref(0)

// Debounce: visibilitychange + window blur both fire on a tab switch → count only once per 500ms
let tabSwitchFiredAt = 0

function recordTabSwitch() {
  const now = Date.now()
  if (now - tabSwitchFiredAt < 500) return   // same event pair — skip duplicate
  tabSwitchFiredAt = now
  tabWarnings.value++
  reportTabSwitchApi(attemptId.value)
  const max = sessionData.value?.tabSwitchMaxCount ?? 3
  if (max > 0 && tabWarnings.value > max) {
    storeTerminationReason('TabSwitchLimit')
    doSubmit(true)
  }
}

function handleVisibilityChange() {
  if (document.visibilityState === 'hidden') recordTabSwitch()
}

function handleWindowBlur() {
  recordTabSwitch()
}

function handleBeforeUnload(e: BeforeUnloadEvent) {
  e.preventDefault()
  e.returnValue = ''
}

function setupAntiCheat() {
  document.addEventListener('visibilitychange', handleVisibilityChange)
  window.addEventListener('blur', handleWindowBlur)
  window.addEventListener('beforeunload', handleBeforeUnload)
  window.addEventListener('pagehide', handlePageHide)
}

function cleanupAntiCheat() {
  document.removeEventListener('visibilitychange', handleVisibilityChange)
  window.removeEventListener('blur', handleWindowBlur)
  window.removeEventListener('beforeunload', handleBeforeUnload)
  window.removeEventListener('pagehide', handlePageHide)
}

function handlePageHide() {
  if (submitted.value || submitting.value) return
  const token = localStorage.getItem('token')
  const apiUrl = (import.meta.env.VITE_API_URL || '').replace(/\/$/, '')
  if (!token || !apiUrl) return

  // Fire-and-forget background submission using keepalive
  fetch(`${apiUrl}/attempt/${attemptId.value}/submit`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    keepalive: true
  })
}

// ── Leave guard ──────────────────────────────────────────────────────────────
const showLeaveDialog = ref(false)
let leaveResolve: ((v: boolean) => void) | null = null

onBeforeRouteLeave((_to, _from, next) => {
  if (submitted.value) return next()
  showLeaveDialog.value = true
  leaveResolve = (confirm) => {
    if (confirm) {
      cleanupAntiCheat()
      next()
    } else {
      next(false)
    }
  }
})

function cancelLeave()  { showLeaveDialog.value = false; leaveResolve?.(false) }
async function confirmLeave() {
  showLeaveDialog.value = false
  // Automatically submit progress before leaving
  await doSubmit(true)
  // Note: doSubmit calls router.push which will resolve the navigation guard
}

// ── Submit ───────────────────────────────────────────────────────────────────
const showSubmitDialog = ref(false)
const submitting       = ref(false)
const submitted        = ref(false)

async function doSubmit(auto = false) {
  if (submitting.value || submitted.value) return
  if (!auto) showSubmitDialog.value = false

  if (answerDebounce)        { clearTimeout(answerDebounce); answerDebounce = null }
  if (timerInterval)         { clearInterval(timerInterval); timerInterval = null }
  clearQuestionTimer()

  submitting.value = true
  const { data, error } = await submitAttemptApi(attemptId.value)
  submitting.value = false

  if (error || !data) return

  submitted.value = true
  cleanupAntiCheat()
  sessionStorage.removeItem(`attempt_${attemptId.value}`)
  // Always show the questionnaire result first; user can then return to campaign
  router.push({ name: 'CandidatTestResult', params: { id: attemptId.value } })
}

// ── Helpers ──────────────────────────────────────────────────────────────────
function typeLabel(type: string): string {
  if (type === 'QCM') return t('testTaking.multipleChoice')
  if (type === 'TrueFalse') return t('testTaking.trueFalse')
  return t('testTaking.singleChoice')
}

onUnmounted(() => {
  uiStore.sidebarOpen = true    // restore sidebar
  cleanupAntiCheat()
  if (timerInterval) clearInterval(timerInterval)
  clearQuestionTimer()
  if (answerDebounce) clearTimeout(answerDebounce)
})
</script>
