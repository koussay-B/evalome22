<template>
  <Sheet v-model:open="isOpen">
    <SheetContent :side="sheetSide" class="sm:max-w-md flex flex-col p-0">
      <template v-if="theme">
        <!-- Header -->
        <SheetHeader class="px-6 pt-6 pb-5 border-b border-border shrink-0">
          <div class="flex items-center gap-4">
            <div class="w-14 h-14 rounded-2xl bg-primary/10 text-primary flex items-center justify-center shrink-0">
              <Layers class="w-7 h-7" />
            </div>
            <div class="min-w-0 flex-1">
              <div class="flex items-center gap-2 flex-wrap">
                <SheetTitle class="text-xl text-slate-800 leading-tight">{{ theme.name }}</SheetTitle>
                <span
                  v-if="showMineBadge && isOwner"
                  class="px-2 py-0.5 rounded-full text-[10px] font-bold bg-primary/10 text-primary border border-primary/20 shrink-0"
                >
                  Mine
                </span>
                <span
                  v-if="isSubtheme"
                  class="px-2 py-0.5 rounded-full text-[10px] font-bold bg-secondary text-muted-foreground border border-border shrink-0"
                >
                  Subtheme
                </span>
              </div>
              <SheetDescription class="mt-1 text-sm line-clamp-2">
                {{ theme.description || 'No description provided.' }}
              </SheetDescription>
            </div>
          </div>
        </SheetHeader>

        <!-- Body -->
        <div class="flex-1 overflow-y-auto flex flex-col gap-6 p-6">

          <!-- Subthemes list (only for root themes) -->
          <div v-if="!isSubtheme" class="space-y-3">
            <div class="flex items-center justify-between">
              <p class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
                Subthemes
              </p>
              <span class="px-2 py-0.5 rounded-full bg-muted text-xs font-semibold text-muted-foreground">
                {{ theme.children?.length ?? 0 }}
              </span>
            </div>

            <div v-if="theme.children?.length" class="flex flex-wrap gap-2">
              <button
                v-for="sub in theme.children"
                :key="sub.id"
                class="group/sub px-3 py-1.5 text-sm font-medium rounded-lg bg-secondary text-secondary-foreground border border-border/50 hover:border-primary/40 hover:bg-primary/5 transition-all flex items-center gap-1.5"
                @click="$emit('subtheme-click', sub)"
              >
                {{ sub.name }}
                <ChevronRight class="w-3 h-3 opacity-0 group-hover/sub:opacity-100 transition-opacity text-primary" />
              </button>
            </div>

            <div v-else class="flex items-center gap-2 px-3 py-3 rounded-lg bg-muted/40 border border-dashed border-border">
              <p class="text-xs text-muted-foreground italic">No subthemes yet.</p>
            </div>
          </div>

          <!-- Stats -->
          <div class="grid grid-cols-2 gap-3">
            <div class="rounded-xl border border-border bg-muted/40 p-4 text-center">
              <div class="text-2xl text-slate-800 text-slate-800">{{ isSubtheme ? '—' : (theme.children?.length ?? 0) }}</div>
              <div class="text-xs text-muted-foreground mt-1">{{ isSubtheme ? 'Parent Theme' : 'Subthemes' }}</div>
              <div v-if="isSubtheme" class="text-xs font-semibold text-primary mt-0.5 truncate">{{ parentName }}</div>
            </div>
            <div class="rounded-xl border border-border bg-muted/40 p-4 text-center">
              <div class="text-2xl text-slate-800 text-slate-800">{{ theme.questions?.length ?? 0 }}</div>
              <div class="text-xs text-muted-foreground mt-1">Questions</div>
            </div>
          </div>



          <!-- Permission note (for non-owned themes in Formateur view) -->
          <div
            v-if="showPermissionNote"
            class="flex items-start gap-3 px-4 py-3 rounded-xl bg-muted/40 border border-border/60"
          >
            <Info class="w-4 h-4 text-muted-foreground shrink-0 mt-0.5" />
            <p class="text-xs text-muted-foreground leading-relaxed">
              This {{ isSubtheme ? 'subtheme' : 'theme' }} was created by another team member. You can view it but cannot edit it.
            </p>
          </div>
        </div>

        <!-- Footer actions -->
        <div class="flex gap-3 px-6 py-5 border-t border-border shrink-0">
          <button
            v-if="canEdit"
            class="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors"
            @click="$emit('edit', theme)"
          >
            <Pencil class="w-4 h-4" /> Edit
          </button>
          <button
            v-if="canDelete"
            class="flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg border border-red-200 dark:border-red-900 text-sm font-semibold text-red-600 hover:bg-red-50 dark:hover:bg-red-950/30 transition-colors"
            :class="canEdit ? '' : 'flex-1'"
            @click="$emit('delete', theme)"
          >
            <Trash2 class="w-4 h-4" />
            <span v-if="!canEdit">Delete</span>
          </button>
          <button
            class="px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors"
            :class="!canEdit && !canDelete ? 'flex-1' : ''"
            @click="isOpen = false"
          >
            Close
          </button>
        </div>
      </template>
    </SheetContent>
  </Sheet>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet'
import { useLocale } from '@/composables/useLocale'
import type { ThemeItem } from '@utils/models'
import { Layers, Pencil, Trash2, Plus, Info, ChevronRight, ClipboardList } from 'lucide-vue-next'

const props = defineProps<{
  open: boolean
  theme: ThemeItem | null
  /** Parent name — shown inside stat box when viewing a subtheme */
  parentName?: string
  /** Current user's ID for ownership check */
  currentUserId?: number
  /** Show the "Mine" badge */
  showMineBadge?: boolean
  /** Whether the current user can edit this theme */
  canEdit?: boolean
  /** Whether the current user can delete this theme */
  canDelete?: boolean
  /** True when displaying a subtheme (hides the subtheme list section) */
  isSubtheme?: boolean
  /** Show the "view only" permission note */
  showPermissionNote?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:open', v: boolean): void
  (e: 'edit', theme: ThemeItem): void
  (e: 'delete', theme: ThemeItem): void
  (e: 'subtheme-click', sub: ThemeItem): void
  (e: 'add-questionnaire', theme: ThemeItem): void
}>()

const { locale } = useLocale()
const isRtl = computed(() => locale.value === 'ar')
const sheetSide = computed<'right' | 'left'>(() => isRtl.value ? 'left' : 'right')

const isOpen = computed({
  get: () => props.open,
  set: (v) => emit('update:open', v),
})

const isOwner = computed(() =>
  props.currentUserId != null && props.theme?.createdById === props.currentUserId,
)
</script>
