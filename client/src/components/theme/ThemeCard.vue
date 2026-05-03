<template>
  <div
    class="p-5 rounded-xl border border-border bg-gray-50 dark:bg-card hover:border-primary/30 hover:shadow-sm transition-all flex flex-col group cursor-pointer relative select-none"
    @click="$emit('click')"
  >
    <!-- "Mine" badge (Formateur view) -->
    <span
      v-if="showMineBadge && isOwner"
      class="absolute top-3 end-3 px-2 py-0.5 rounded-full text-[10px] font-bold bg-primary/10 text-primary border border-primary/20 z-10"
    >
      {{ t('formateur.themes.tabMine') }}
    </span>

    <!-- Actions dropdown -->
    <div
      v-if="canEdit || canDelete"
      class="absolute top-3 opacity-0 group-hover:opacity-100 focus-within:opacity-100 transition-opacity z-10"
      :class="showMineBadge && isOwner ? 'end-14' : 'end-3'"
    >
      <DropdownMenu>
        <DropdownMenuTrigger as-child>
          <button
            class="p-1.5 rounded-lg text-muted-foreground hover:bg-muted transition-colors"
            @click.stop
          >
            <MoreHorizontal class="w-4 h-4" />
          </button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" class="w-44">
          <DropdownMenuLabel class="text-xs">{{ t('common.actions') }}</DropdownMenuLabel>
          <DropdownMenuSeparator />
          <DropdownMenuItem
            v-if="canEdit"
            class="gap-2 cursor-pointer"
            @click.stop="$emit('edit')"
          >
            <Pencil class="w-3.5 h-3.5" /> {{ t('common.edit') }}
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

   <div class="flex items-start gap-2 mb-2.5" :class="(canEdit || canDelete || (showMineBadge && isOwner)) ? 'pe-8' : ''">
  <div class="w-6.5 h-6.5 rounded-lg bg-orange-100/50 text-orange-600 flex items-center justify-center shrink-0">
    <Layers class="w-4 h-4" />
  </div>
  <div class="min-w-0">
    <h3 class="text-slate-800 text-sm truncate leading-tight">{{ theme.name }}</h3>
    <p class="text-[10px] text-slate-400 line-clamp-1 mt-0.5">
      {{ theme.description || t('themes.noDescription') }}
    </p>
  </div>
</div>

<div class="mt-auto pt-2 border-t border-stone-200/60 space-y-1.5">
  <div class="text-[9px] text-slate-800 uppercase tracking-tighter  flex items-center justify-center">
   
  </div>

  <div v-if="theme.children?.length" class="flex flex-wrap gap-1">
    <button
      v-for="sub in theme.children.slice(0, 3)" 
      :key="sub.id"
      class="px-2 py-0 text-[10px] font-bold rounded-md bg-white text-slate-600 border border-stone-200/60 hover:border-orange-200 hover:bg-orange-50 transition-all"
      @click.stop="$emit('subtheme-click', sub)"
    >
      {{ sub.name }}
    </button>
    <span
      v-if="theme.children.length > 3"
      class="px-1.5 py-0 text-[10px] font-bold rounded-md bg-stone-50 text-stone-400 border border-dashed border-stone-200"
    >
      +{{ theme.children.length - 3 }}
    </span>
  </div>
  <p v-else class="text-[9px] text-slate-300 italic">{{ t('themes.noSubthemesYet') }}</p>
</div>
    <!-- Question count -->
    <div v-if="questionCount > 0" class="mt-2 flex items-center gap-1.5 text-xs text-muted-foreground">
      <BookOpen class="w-3 h-3" />
      <span>{{ questionCount }} {{ t('themes.statQuestions') }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ThemeItem } from '@utils/models'
import {
  DropdownMenu, DropdownMenuTrigger, DropdownMenuContent,
  DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator,
} from '@/components/ui/dropdown-menu'
import { MoreHorizontal, Pencil, Trash2, Layers, BookOpen } from 'lucide-vue-next'
import { computed } from 'vue'
import { useLocale } from '@/composables/useLocale'

const { t } = useLocale()

const props = defineProps<{
  theme: ThemeItem
  /** Current user's ID — used to compute isOwner */
  currentUserId?: number
  /** Show the "Mine" badge when isOwner is true */
  showMineBadge?: boolean
  /** Whether the current user can edit this theme */
  canEdit?: boolean
  /** Whether the current user can delete this theme */
  canDelete?: boolean
}>()

defineEmits<{
  (e: 'click'): void
  (e: 'edit'): void
  (e: 'delete'): void
  (e: 'subtheme-click', sub: ThemeItem): void
}>()

const isOwner = computed(() =>
  props.currentUserId != null && props.theme.createdById === props.currentUserId,
)

const questionCount = computed(() => props.theme.questions?.length ?? 0)
</script>
