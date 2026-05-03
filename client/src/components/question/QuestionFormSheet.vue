<template>
  <Sheet v-model:open="isOpen">
    <SheetContent :side="sheetSide" class="sm:max-w-xl flex flex-col p-0 gap-0">

      <!-- Header -->
      <SheetHeader class="px-6 pt-6 pb-5 border-b border-border shrink-0">
        <SheetTitle class="text-lg text-slate-800">
          {{ editing ? t('questionForm.titleEdit') : t('questionForm.titleNew') }}
        </SheetTitle>
        <SheetDescription>
          {{ editing ? t('questionForm.descEdit') : t('questionForm.descNew') }}
        </SheetDescription>
      </SheetHeader>

      <!-- Body -->
      <div class="flex-1 overflow-y-auto p-6 space-y-6">

        <!-- ── Question Type ───────────────────────────────────── -->
        <div class="space-y-2">
          <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('questionForm.typeLabel') }}</label>
          <div class="grid grid-cols-3 gap-2">
            <button
              v-for="qt in questionTypes"
              :key="qt.value"
              class="flex flex-col items-center gap-1.5 p-3 rounded-xl border transition-all"
              :class="
                form.type === qt.value
                  ? 'border-primary bg-primary/5 ring-1 ring-primary'
                  : 'border-border hover:border-primary/30 hover:bg-muted/30'
              "
              @click="setType(qt.value)"
            >
              <component :is="qt.icon" class="w-5 h-5" :class="form.type === qt.value ? 'text-primary' : 'text-muted-foreground'" />
              <span class="text-[11px] font-bold" :class="form.type === qt.value ? 'text-foreground' : 'text-muted-foreground'">
                {{ qt.label }}
              </span>
            </button>
          </div>
        </div>

        <!-- ── Title ──────────────────────────────────────────── -->
        <div class="space-y-2">
          <label class="text-sm font-semibold text-foreground">
            {{ t('questionForm.questionLabel') }} <span class="text-red-500">*</span>
          </label>
          <textarea
            v-model="form.title"
            :placeholder="t('questionForm.questionPlaceholder')"
            rows="3"
            class="w-full rounded-md border border-input bg-transparent px-3 py-2.5 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring resize-none transition-colors"
            :class="errors.title ? 'border-red-400 focus-visible:ring-red-400' : ''"
          />
          <p v-if="errors.title" class="text-xs text-red-500 flex items-center gap-1.5">
            <CircleAlert class="w-3.5 h-3.5" /> {{ errors.title }}
          </p>
        </div>

        <!-- ── Choices (QCU / QCM) ────────────────────────────── -->
        <div v-if="form.type === 'QCU' || form.type === 'QCM'" class="space-y-3">
          <div class="flex items-center justify-between">
            <label class="text-sm font-semibold text-foreground">
              {{ t('questionForm.choicesLabel') }} <span class="text-red-500">*</span>
            </label>
            <span class="text-[10px] text-muted-foreground font-medium">
              {{ form.type === 'QCU' ? t('questionForm.selectOneCorrect') : t('questionForm.selectAllCorrect') }}
            </span>
          </div>

          <p v-if="errors.choices" class="text-xs text-red-500 flex items-center gap-1.5">
            <CircleAlert class="w-3.5 h-3.5" /> {{ errors.choices }}
          </p>

          <div class="space-y-2">
            <div
              v-for="(choice, i) in form.choices"
              :key="i"
              class="flex items-center gap-2 group/choice"
            >
              <!-- Correct indicator button -->
              <button
                class="shrink-0 w-5 h-5 flex items-center justify-center transition-colors"
                :title="form.type === 'QCU' ? 'Mark as correct answer' : 'Toggle correct'"
                @click="toggleCorrect(i)"
              >
                <!-- QCU: radio style -->
                <div v-if="form.type === 'QCU'" class="w-4 h-4 rounded-full border-2 flex items-center justify-center transition-colors"
                  :class="choice.isCorrect ? 'border-emerald-500 bg-emerald-500' : 'border-border hover:border-emerald-400'"
                >
                  <div v-if="choice.isCorrect" class="w-1.5 h-1.5 rounded-full bg-white" />
                </div>
                <!-- QCM: checkbox style -->
                <div v-else class="w-4 h-4 rounded border-2 flex items-center justify-center transition-colors"
                  :class="choice.isCorrect ? 'border-emerald-500 bg-emerald-500' : 'border-border hover:border-emerald-400'"
                >
                  <Check v-if="choice.isCorrect" class="w-2.5 h-2.5 text-white" />
                </div>
              </button>

              <!-- Choice text -->
              <Input
                v-model="choice.text"
                :placeholder="`Choice ${i + 1}…`"
                class="h-9 flex-1 text-sm"
                :class="choice.isCorrect ? 'border-emerald-300 dark:border-emerald-700 bg-emerald-50/50 dark:bg-emerald-950/20' : ''"
              />

              <!-- Remove -->
              <button
                class="shrink-0 p-1.5 rounded-lg text-muted-foreground hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/30 opacity-0 group-hover/choice:opacity-100 transition-all"
                :disabled="form.choices.length <= 2"
                @click="removeChoice(i)"
              >
                <X class="w-3.5 h-3.5" />
              </button>
            </div>
          </div>

          <button
            class="flex items-center gap-2 text-sm font-semibold text-primary hover:text-primary/80 transition-colors"
            :disabled="form.choices.length >= 8"
            @click="addChoice"
          >
            <Plus class="w-3.5 h-3.5" /> {{ t('questionForm.addChoice') }}
          </button>
        </div>

        <!-- ── True / False ───────────────────────────────────── -->
        <div v-if="form.type === 'TrueFalse'" class="space-y-2">
          <label class="text-sm font-semibold text-foreground">{{ t('questionForm.correctAnswer') }}</label>
          <div class="grid grid-cols-2 gap-3">
            <button
              class="py-3 rounded-xl border-2 font-bold text-sm transition-all"
              :class="
                trueFalseAnswer === true
                  ? 'border-emerald-500 bg-emerald-50 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-400'
                  : 'border-border hover:border-emerald-300 text-muted-foreground hover:text-foreground'
              "
              @click="setTrueFalse(true)"
            >
              ✓ True
            </button>
            <button
              class="py-3 rounded-xl border-2 font-bold text-sm transition-all"
              :class="
                trueFalseAnswer === false
                  ? 'border-red-500 bg-red-50 dark:bg-red-950/30 text-red-700 dark:text-red-400'
                  : 'border-border hover:border-red-300 text-muted-foreground hover:text-foreground'
              "
              @click="setTrueFalse(false)"
            >
              ✗ False
            </button>
          </div>
          <p v-if="errors.choices" class="text-xs text-red-500 flex items-center gap-1.5">
            <CircleAlert class="w-3.5 h-3.5" /> {{ errors.choices }}
          </p>
        </div>

        <div class="border-t border-border" />

        <!-- ── Meta row ───────────────────────────────────────── -->
        <div class="grid grid-cols-2 gap-4">

          <!-- Theme -->
          <div class="space-y-1.5">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('questionForm.themeLabel') }}</label>
            <Select v-model="form.themeId">
              <SelectTrigger class="h-9">
                <SelectValue :placeholder="t('questionForm.themePlaceholder')" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="theme in themes" :key="theme.id" :value="String(theme.id)">
                  {{ theme.name }}
                </SelectItem>
              </SelectContent>
            </Select>
            <p v-if="errors.themeId" class="text-xs text-red-500">{{ errors.themeId }}</p>
          </div>

          <!-- Complexity -->
          <div class="space-y-1.5">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('questionForm.complexityLabel') }}</label>
            <Select v-model="form.complexity">
              <SelectTrigger class="h-9">
                <SelectValue :placeholder="t('questionForm.complexityPlaceholder')" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem
                  v-for="c in complexities"
                  :key="c.value"
                  :value="c.value"
                >
                  <span class="font-semibold" :class="c.color">{{ c.label }}</span>
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <!-- Points -->
          <div class="space-y-1.5">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('questionForm.pointsLabel') }}</label>
            <div class="flex items-center gap-2">
              <button
                class="w-8 h-9 rounded-md border border-border hover:bg-muted flex items-center justify-center font-bold transition-colors disabled:opacity-40"
                :disabled="form.points <= 0.5"
                @click="form.points = Math.max(0.5, form.points - 0.5)"
              >−</button>
              <Input v-model.number="form.points" type="number" min="0.5" step="0.5" class="h-9 text-center font-bold flex-1" />
              <button
                class="w-8 h-9 rounded-md border border-border hover:bg-muted flex items-center justify-center font-bold transition-colors disabled:opacity-40"
                :disabled="form.points >= 20"
                @click="form.points = Math.min(20, form.points + 0.5)"
              >+</button>
            </div>
          </div>

          <!-- Time limit -->
          <div class="space-y-1.5">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
              {{ t('questionForm.timeLimitLabel') }} <span class="text-muted-foreground font-normal normal-case">({{ t('questionForm.timeLimitOptional') }})</span>
            </label>
            <div class="flex items-center gap-2">
              <Input
                v-model.number="form.timeLimitSeconds"
                type="number"
                min="5"
                placeholder="—"
                class="h-9 flex-1 text-center"
              />
              <span class="text-xs text-muted-foreground shrink-0 font-medium">{{ t('questionForm.timeLimitUnit') }}</span>
            </div>
          </div>
        </div>

        <!-- ── Explanation & Hint ──────────────────────────────── -->
        <div class="space-y-3">
          <!-- Explanation -->
          <div class="space-y-1.5">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
              {{ t('questionForm.explanationLabel') }} <span class="text-muted-foreground font-normal normal-case">({{ t('questionForm.explanationHint') }})</span>
            </label>
            <textarea
              v-model="form.explanation"
              :placeholder="t('questionForm.explanationPlaceholder')"
              rows="2"
              class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring resize-none transition-colors"
            />
          </div>

          <!-- Hint -->
          <div class="space-y-1.5">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
              {{ t('questionForm.hintLabel') }} <span class="text-muted-foreground font-normal normal-case">({{ t('questionForm.hintHint') }})</span>
            </label>
            <Input v-model="form.hint" :placeholder="t('questionForm.hintPlaceholder')" class="h-9" />
          </div>
        </div>

      </div>

      <!-- Footer -->
      <div class="flex gap-3 px-6 py-5 border-t border-border shrink-0">
        <button
          class="flex-1 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors disabled:opacity-60 flex items-center justify-center gap-2"
          :disabled="isSaving"
          @click="save"
        >
          <Loader2 v-if="isSaving" class="w-4 h-4 animate-spin" />
          {{ isSaving ? t('questionForm.saving') : editing ? t('questionForm.saveChanges') : t('questionForm.addQuestion') }}
        </button>
        <button
          class="px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800hover:bg-muted/50 transition-colors"
          @click="isOpen = false"
        >
          {{ t('questionForm.cancel') }}
        </button>
      </div>

    </SheetContent>
  </Sheet>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet'
import { Input } from '@/components/ui/input'
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from '@/components/ui/select'
import { useLocale } from '@/composables/useLocale'
import { CircleDot, CheckSquare, ToggleLeft, Plus, X, Check, CircleAlert, Loader2 } from 'lucide-vue-next'
import type { QuestionItem, QuestionType, ComplexityLevel, ChoicePayload } from '@/utils/models'
import { createQuestionApi, updateQuestionApi } from '@/utils/api'

// ── Props & Emits ──────────────────────────────────────────────────────────
const props = defineProps<{
  open:       boolean
  editing?:   QuestionItem | null
  themes?:    { id: number; name: string }[]
}>()

const emit = defineEmits<{
  (e: 'update:open', v: boolean): void
  (e: 'saved', question: QuestionItem): void
}>()

// ── RTL support ────────────────────────────────────────────────────────────
const { t, locale } = useLocale()
const isRtl         = computed(() => locale.value === 'ar')
const sheetSide     = computed<'right' | 'left'>(() => isRtl.value ? 'left' : 'right')

// ── Sheet open state ───────────────────────────────────────────────────────
const isOpen = computed({
  get: () => props.open,
  set: (v) => emit('update:open', v),
})

// ── Themes ─────────────────────────────────────────────────────────────────
const themes = computed(() => props.themes ?? [])

// ── Config ─────────────────────────────────────────────────────────────────
const questionTypes = computed(() => [
  { value: 'QCU'       as QuestionType, label: t('questionForm.typeQcu'),      icon: CircleDot },
  { value: 'QCM'       as QuestionType, label: t('questionForm.typeQcm'),      icon: CheckSquare },
  { value: 'TrueFalse' as QuestionType, label: t('questionForm.typeTrueFalse'), icon: ToggleLeft },
])

const complexities = computed(() => [
  { value: 'Beginner'     as ComplexityLevel, label: t('questionForm.complexityBeginner'),     color: 'text-emerald-600 dark:text-emerald-400' },
  { value: 'Intermediate' as ComplexityLevel, label: t('questionForm.complexityIntermediate'), color: 'text-sky-600 dark:text-sky-400' },
  { value: 'Advanced'     as ComplexityLevel, label: t('questionForm.complexityAdvanced'),     color: 'text-amber-600 dark:text-amber-400' },
  { value: 'Expert'       as ComplexityLevel, label: t('questionForm.complexityExpert'),       color: 'text-red-600 dark:text-red-400' },
])


// ── Form state ─────────────────────────────────────────────────────────────
interface FormChoice { text: string; isCorrect: boolean; order: number; explanation?: string | null }

function defaultChoices(): FormChoice[] {
  return [
    { text: '', isCorrect: false, order: 0 },
    { text: '', isCorrect: false, order: 1 },
    { text: '', isCorrect: false, order: 2 },
    { text: '', isCorrect: false, order: 3 },
  ]
}

function defaultTrueFalseChoices(): FormChoice[] {
  return [
    { text: 'True',  isCorrect: false, order: 0 },
    { text: 'False', isCorrect: false, order: 1 },
  ]
}

const form = ref({
  title:            '',
  type:             'QCU' as QuestionType,
  complexity:       'Beginner' as ComplexityLevel,
  points:           1,
  timeLimitSeconds: null as number | null,
  themeId:          '' as string,
  explanation:      '',
  hint:             '',
  choices:          defaultChoices() as FormChoice[],
})

const errors = ref({ title: '', choices: '', themeId: '' })

// Track True/False answer selection
const trueFalseAnswer = ref<boolean | null>(null)

// ── Populate form when editing ─────────────────────────────────────────────
watch(() => props.editing, (q) => {
  if (!q) {
    resetForm()
    return
  }
  form.value.title            = q.title
  form.value.type             = q.type
  form.value.complexity       = q.complexity
  form.value.points           = q.points
  form.value.timeLimitSeconds = q.timeLimitSeconds ?? null
  form.value.themeId          = String(q.themeId)
  form.value.explanation      = q.explanation ?? ''
  form.value.hint             = q.hint ?? ''

  if (q.type === 'TrueFalse') {
    form.value.choices  = defaultTrueFalseChoices()
    const correct       = q.choices.find(c => c.isCorrect)
    trueFalseAnswer.value = correct ? correct.text === 'True' : null
    // sync true/false answer into choices
    if (trueFalseAnswer.value !== null) {
      form.value.choices[0]!.isCorrect = trueFalseAnswer.value === true
      form.value.choices[1]!.isCorrect = trueFalseAnswer.value === false
    }
  } else {
    form.value.choices = q.choices.map(c => ({
      text:        c.text,
      isCorrect:   c.isCorrect,
      order:       c.order,
      explanation: c.explanation ?? null,
    }))
    trueFalseAnswer.value = null
  }
}, { immediate: true })

// ── Reset when sheet closes ────────────────────────────────────────────────
watch(isOpen, (v) => { if (!v) resetForm() })

function resetForm() {
  form.value = {
    title:            '',
    type:             'QCU',
    complexity:       'Beginner',
    points:           1,
    timeLimitSeconds: null,
    themeId:          '',
    explanation:      '',
    hint:             '',
    choices:          defaultChoices(),
  }
  trueFalseAnswer.value = null
  errors.value = { title: '', choices: '', themeId: '' }
}

// ── Type switch ────────────────────────────────────────────────────────────
function setType(type: QuestionType) {
  form.value.type = type
  if (type === 'TrueFalse') {
    form.value.choices = defaultTrueFalseChoices()
    trueFalseAnswer.value = null
  } else if (form.value.choices.length < 2) {
    form.value.choices = defaultChoices()
  }
}

// ── Choices management ─────────────────────────────────────────────────────
function addChoice() {
  if (form.value.choices.length >= 8) return
  form.value.choices.push({ text: '', isCorrect: false, order: form.value.choices.length })
}

function removeChoice(i: number) {
  if (form.value.choices.length <= 2) return
  form.value.choices.splice(i, 1)
  form.value.choices.forEach((c, idx) => (c.order = idx))
}

function toggleCorrect(i: number) {
  if (form.value.type === 'QCU') {
    form.value.choices.forEach((c, idx) => (c.isCorrect = idx === i))
  } else {
    form.value.choices[i]!.isCorrect = !form.value.choices[i]!.isCorrect
  }
}

function setTrueFalse(answer: boolean) {
  trueFalseAnswer.value     = answer
  form.value.choices[0]!.isCorrect = answer === true   // True choice
  form.value.choices[1]!.isCorrect = answer === false  // False choice
}

// ── Validation ─────────────────────────────────────────────────────────────
function validate(): boolean {
  errors.value = { title: '', choices: '', themeId: '' }
  let ok = true

  if (!form.value.title.trim()) {
    errors.value.title = t('questionForm.questionRequired')
    ok = false
  }
  if (!form.value.themeId) {
    errors.value.themeId = t('questionForm.themeRequired')
    ok = false
  }

  if (form.value.type === 'TrueFalse') {
    if (trueFalseAnswer.value === null) {
      errors.value.choices = t('questionForm.choicesTfError')
      ok = false
    }
  } else {
    const filled   = form.value.choices.filter(c => c.text.trim().length > 0)
    const hasCorrect = form.value.choices.some(c => c.isCorrect)
    if (filled.length < 2) {
      errors.value.choices = t('questionForm.choicesMinError')
      ok = false
    } else if (!hasCorrect) {
      errors.value.choices = t('questionForm.choicesCorrectError')
      ok = false
    }
  }

  return ok
}

// ── Save ───────────────────────────────────────────────────────────────────
const isSaving = ref(false)

async function save() {
  if (!validate()) return

  isSaving.value = true
  try {
    const payload = {
      title:            form.value.title.trim(),
      type:             form.value.type,
      complexity:       form.value.complexity,
      points:           form.value.points,
      timeLimitSeconds: form.value.timeLimitSeconds || null,
      themeId:          Number(form.value.themeId),
      explanation:      form.value.explanation.trim() || null,
      hint:             form.value.hint.trim() || null,
      choices:          form.value.choices
        .filter(c => form.value.type === 'TrueFalse' || c.text.trim().length > 0)
        .map((c, idx): ChoicePayload => ({
          text:      c.text.trim(),
          isCorrect: c.isCorrect,
          order:     idx,
        })),
    }

    const { data, error } = props.editing
      ? await updateQuestionApi(props.editing.id, payload)
      : await createQuestionApi(payload)

    if (error || !data) {
      errors.value.title = error ?? t('questionForm.saveError')
      return
    }

    emit('saved', data)
    isOpen.value = false
  } finally {
    isSaving.value = false
  }
}
</script>
