<template>
  <div
    class="group relative rounded-xl border border-border bg-secondary hover:border-primary/30 hover:shadow-sm transition-all cursor-pointer"
    :class="isDragging ? 'opacity-50 scale-[0.98]' : ''"
    @click="$emit('edit', question)"
  >
    <!-- Drag handle -->
    <div
      class="absolute start-0 top-0 bottom-0 flex items-center ps-2 cursor-grab active:cursor-grabbing opacity-0 group-hover:opacity-100 transition-opacity"
      :title="t('questionCard.dragToReorder')"
    >
      <GripVertical class="w-4 h-4 text-muted-foreground" />
    </div>

    <div class="ps-7 pe-4 py-4 flex items-start gap-4">

      <!-- Order badge -->
      <div class="w-7 h-7 rounded-full bg-muted border border-border flex items-center justify-center shrink-0 mt-0.5">
        <span class="text-[10px] text-slate-800 text-muted-foreground tabular-nums">{{ order }}</span>
      </div>

      <!-- Content -->
      <div class="flex-1 min-w-0 space-y-2">
        <div class="flex items-start justify-between gap-3">
          <p class="text-sm font-semibold text-slate-800leading-snug line-clamp-2">
            {{ question.title }}
          </p>
          <!-- Actions dropdown -->
          <div class="opacity-0 group-hover:opacity-100 focus-within:opacity-100 transition-opacity shrink-0">
            <DropdownMenu>
              <DropdownMenuTrigger as-child>
                <button class="p-1.5 rounded-lg text-muted-foreground hover:bg-muted transition-colors" @click.stop>
                  <MoreHorizontal class="w-4 h-4" />
                </button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" class="w-36">
                <DropdownMenuLabel class="text-xs">{{ t('questionCard.actionsLabel') }}</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem class="gap-2 cursor-pointer" @click.stop="$emit('edit', question)">
                  <Pencil class="w-3.5 h-3.5" /> {{ t('questionCard.edit') }}
                </DropdownMenuItem>
                <DropdownMenuItem
                  class="gap-2 cursor-pointer text-red-600 focus:text-red-600 focus:bg-red-50 dark:focus:bg-red-950/30"
                  @click.stop="$emit('delete', question)"
                >
                  <Trash2 class="w-3.5 h-3.5" /> {{ t('questionCard.remove') }}
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>

        <!-- Meta row -->
        <div class="flex flex-wrap items-center gap-2">
          <!-- Type badge -->
          <span :class="typeStyle.bg" class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[10px] font-bold uppercase tracking-wide">
            <component :is="typeStyle.icon" class="w-3 h-3" />
            {{ typeStyle.label }}
          </span>

          <!-- Theme badge -->
          <span v-if="themeName" class="flex items-center gap-1 text-[10px] font-bold text-muted-foreground">
            <Tag class="w-3 h-3" />
            {{ themeName }}
          </span>

          <!-- Complexity badge -->
          <span :class="complexityStyle.color" class="text-[10px] font-bold uppercase tracking-wide">
            {{ question.complexity }}
          </span>

          <!-- Points -->
          <span class="flex items-center gap-1 text-[10px] font-bold text-muted-foreground">
            <Star class="w-3 h-3" />
            {{ question.points }}pt{{ question.points !== 1 ? 's' : '' }}
          </span>

          <!-- Time limit -->
          <span v-if="question.timeLimitSeconds" class="flex items-center gap-1 text-[10px] font-bold text-muted-foreground">
            <Clock class="w-3 h-3" />
            {{ question.timeLimitSeconds }}s
          </span>

          <!-- Mandatory badge -->
          <span v-if="!isMandatory" class="text-[10px] font-bold text-amber-600 dark:text-amber-400">{{ t('questionCard.optional') }}</span>

          <!-- Weight -->
          <span v-if="weight !== 1" class="text-[10px] font-bold text-muted-foreground">
            ×{{ weight }} {{ t('questionCard.weight') }}
          </span>
        </div>

        <!-- Choices preview (QCU / QCM) -->
        <div v-if="question.type !== 'TrueFalse' && question.choices.length > 0" class="flex flex-wrap gap-1.5 pt-1">
          <span
            v-for="choice in sortedChoices"
            :key="choice.id"
            class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-medium border transition-colors"
            :class="
              choice.isCorrect
                ? 'bg-emerald-50 dark:bg-emerald-950/30 border-emerald-200 dark:border-emerald-800 text-emerald-700 dark:text-emerald-400'
                : 'bg-muted/50 border-border/60 text-muted-foreground'
            "
          >
            <CheckCircle v-if="choice.isCorrect" class="w-2.5 h-2.5" />
            {{ choice.text }}
          </span>
        </div>

        <!-- TrueFalse preview -->
        <div v-if="question.type === 'TrueFalse'" class="flex gap-2 pt-1">
          <span
            v-for="choice in question.choices"
            :key="choice.id"
            class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold border"
            :class="
              choice.isCorrect
                ? 'bg-emerald-50 dark:bg-emerald-950/30 border-emerald-200 dark:border-emerald-800 text-emerald-700 dark:text-emerald-400'
                : 'bg-muted/50 border-border/60 text-muted-foreground'
            "
          >
            <CheckCircle v-if="choice.isCorrect" class="w-2.5 h-2.5" />
            {{ choice.text }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import {
  DropdownMenu, DropdownMenuTrigger, DropdownMenuContent,
  DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator,
} from '@/components/ui/dropdown-menu'
import { GripVertical, MoreHorizontal, Pencil, Trash2, Star, Clock, CheckCircle, CircleDot, CheckSquare, ToggleLeft, Tag } from 'lucide-vue-next'
import { useLocale } from '@/composables/useLocale'
import type { QuestionItem } from '@/utils/models'

const { t } = useLocale()

const props = defineProps<{
  question:    QuestionItem
  order:       number
  weight?:     number
  isMandatory?: boolean
  isDragging?: boolean
  themeName?:  string
}>()

defineEmits<{
  (e: 'edit',   q: QuestionItem): void
  (e: 'delete', q: QuestionItem): void
}>()

const sortedChoices = computed(() =>
  [...props.question.choices].sort((a, b) => a.order - b.order)
)

const typeStyle = computed(() => {
  switch (props.question.type) {
    case 'QCU':      return { label: t('questionCard.typeQcu'),      bg: 'bg-primary/10 text-primary',                                                icon: CircleDot }
    case 'QCM':      return { label: t('questionCard.typeQcm'),      bg: 'bg-sky-100 dark:bg-sky-900/30 text-sky-700 dark:text-sky-400',            icon: CheckSquare }
    case 'TrueFalse':return { label: t('questionCard.typeTrueFalse'), bg: 'bg-violet-100 dark:bg-violet-900/30 text-violet-700 dark:text-violet-400', icon: ToggleLeft }
  }
})

const complexityStyle = computed(() => {
  switch (props.question.complexity) {
    case 'Beginner':     return { color: 'text-emerald-600 dark:text-emerald-400' }
    case 'Intermediate': return { color: 'text-sky-600 dark:text-sky-400' }
    case 'Advanced':     return { color: 'text-amber-600 dark:text-amber-400' }
    case 'Expert':       return { color: 'text-red-600 dark:text-red-400' }
  }
})
</script>
