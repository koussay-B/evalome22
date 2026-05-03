<template>
  <Sheet v-model:open="isOpen">
    <SheetContent :side="sheetSide" class="sm:max-w-md flex flex-col p-0">
      <SheetHeader class="px-6 pt-6 pb-5 border-b border-border shrink-0">
        <SheetTitle class="text-xl text-slate-800">
          {{ isEditing ? (isSubtheme ? 'Edit Subtheme' : 'Edit Theme') : (isSubtheme ? 'Add Subtheme' : 'Add Theme') }}
        </SheetTitle>
        <SheetDescription class="mt-1">
          {{ isEditing ? 'Update the details below.' : (isSubtheme ? 'Add a new subtheme.' : 'Create a new theme and optionally add subthemes.') }}
        </SheetDescription>
      </SheetHeader>

      <form class="flex flex-col flex-1 overflow-y-auto" @submit.prevent="handleSave">
        <div class="flex flex-col gap-6 p-6 flex-1">

          <!-- Name -->
          <div class="space-y-2">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
              Name *
            </label>
            <div class="relative">
              <Layers class="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <Input
                v-model="form.name"
                placeholder="e.g. Development"
                class="ps-9 h-10"
                required
                :disabled="saving"
              />
            </div>
          </div>

          <!-- Description -->
          <div class="space-y-2">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
              Description
            </label>
            <Input
              v-model="form.description"
              placeholder="Short description (optional)"
              class="h-10"
              :disabled="saving"
            />
          </div>

          <!-- Subthemes section — only for root themes -->
          <div v-if="!isSubtheme" class="space-y-3 pt-5 border-t border-border">
            <div class="flex items-center justify-between">
              <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
                Subthemes
              </label>
              <button
                type="button"
                class="text-xs flex items-center gap-1 font-semibold text-primary hover:text-primary/80 transition-colors"
                :disabled="saving"
                @click="addSubtheme"
              >
                <Plus class="w-3.5 h-3.5" /> Add
              </button>
            </div>

            <!-- Existing subthemes (when editing) -->
            <div v-if="existingSubthemes.length" class="space-y-2">
              <p class="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground">Existing</p>
              <div
                v-for="sub in existingSubthemes"
                :key="sub.id"
                class="flex items-center gap-2"
              >
                <div class="flex-1 relative">
                  <Input
                    v-model="sub.name"
                    class="h-9 text-sm"
                    :disabled="saving"
                    required
                  />
                </div>
                <button
                  v-if="canDeleteSubthemes"
                  type="button"
                  class="p-1.5 rounded-md text-muted-foreground hover:bg-red-50 hover:text-red-600 dark:hover:bg-red-950 dark:hover:text-red-400 transition-colors shrink-0"
                  :disabled="saving"
                  @click="markSubthemeDeleted(sub)"
                >
                  <X class="w-4 h-4" />
                </button>
              </div>
            </div>

            <!-- New subthemes to add -->
            <div v-if="newSubthemes.length" class="space-y-2">
              <p v-if="existingSubthemes.length" class="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground">New</p>
              <div
                v-for="(sub, i) in newSubthemes"
                :key="i"
                class="flex items-center gap-2"
              >
                <Input
                  v-model="sub.name"
                  placeholder="Subtheme name"
                  class="h-9 text-sm"
                  :disabled="saving"
                  required
                />
                <button
                  type="button"
                  class="p-1.5 rounded-md text-muted-foreground hover:bg-red-50 hover:text-red-600 dark:hover:bg-red-950 dark:hover:text-red-400 transition-colors shrink-0"
                  :disabled="saving"
                  @click="removeNewSubtheme(i)"
                >
                  <X class="w-4 h-4" />
                </button>
              </div>
            </div>

            <p v-if="!existingSubthemes.length && !newSubthemes.length" class="text-xs text-muted-foreground italic py-1">
              No subthemes added yet.
            </p>
          </div>

          <!-- Error -->
          <p v-if="formError" class="text-sm text-red-500 font-medium">{{ formError }}</p>
        </div>

        <!-- Actions -->
        <div class="flex gap-3 px-6 py-5 border-t border-border shrink-0">
          <button
            type="submit"
            class="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors disabled:opacity-60"
            :disabled="saving"
          >
            <Loader2 v-if="saving" class="w-4 h-4 animate-spin" />
            <Save v-else class="w-4 h-4" />
            {{ saving ? 'Saving…' : 'Save' }}
          </button>
          <button
            type="button"
            class="px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors"
            :disabled="saving"
            @click="isOpen = false"
          >
            Cancel
          </button>
        </div>
      </form>
    </SheetContent>
  </Sheet>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet'
import { Input } from '@/components/ui/input'
import { useTopicStore } from '@/store/topicStore'
import { useLocale } from '@/composables/useLocale'
import type { ThemeItem } from '@utils/models'
import { Plus, X, Save, Loader2, Layers } from 'lucide-vue-next'

// ── Props / Emits ───────────────────────────────────────────────────────────
const props = defineProps<{
  open: boolean
  /** If provided, we're editing this theme/subtheme */
  theme?: ThemeItem | null
  /** If provided, this form creates/updates a subtheme under this parent */
  parentId?: number | null
  /** Whether to show the subtheme management section (only root themes) */
  isSubtheme?: boolean
  /** Whether the current user can delete existing subthemes */
  canDeleteSubthemes?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:open', v: boolean): void
  (e: 'saved'): void
}>()

// ── Locals ───────────────────────────────────────────────────────────────────
const store = useTopicStore()
const { locale } = useLocale()
const isRtl = computed(() => locale.value === 'ar')
const sheetSide = computed<'right' | 'left'>(() => isRtl.value ? 'left' : 'right')

const isOpen = computed({
  get: () => props.open,
  set: (v) => emit('update:open', v),
})

const isEditing = computed(() => !!props.theme)
const saving = ref(false)
const formError = ref<string | null>(null)

// ── Form state ────────────────────────────────────────────────────────────────
const form = reactive({ name: '', description: '' })

// Subtheme tracking (for root themes)
interface SubEdit { id: number; name: string }
interface SubNew  { name: string }

const existingSubthemes = ref<SubEdit[]>([])
const newSubthemes      = ref<SubNew[]>([])
const deletedSubthemeIds = ref<number[]>([])

// Reset form whenever sheet opens
watch(() => props.open, (val) => {
  if (!val) return
  formError.value = null
  saving.value    = false

  if (props.theme) {
    form.name        = props.theme.name
    form.description = props.theme.description ?? ''
    existingSubthemes.value  = (props.theme.children ?? []).map(c => ({ id: c.id, name: c.name }))
    newSubthemes.value       = []
    deletedSubthemeIds.value = []
  } else {
    form.name        = ''
    form.description = ''
    existingSubthemes.value  = []
    newSubthemes.value       = []
    deletedSubthemeIds.value = []
  }
})

// ── Subtheme helpers ──────────────────────────────────────────────────────────
function addSubtheme() {
  newSubthemes.value.push({ name: '' })
}

function removeNewSubtheme(i: number) {
  newSubthemes.value.splice(i, 1)
}

function markSubthemeDeleted(sub: SubEdit) {
  deletedSubthemeIds.value.push(sub.id)
  existingSubthemes.value = existingSubthemes.value.filter(s => s.id !== sub.id)
}

// ── Save ──────────────────────────────────────────────────────────────────────
async function handleSave() {
  if (!form.name.trim()) return
  saving.value    = true
  formError.value = null

  const payload = {
    name:        form.name.trim(),
    description: form.description.trim() || null,
  }

  if (isEditing.value && props.theme) {
    await saveEdit(props.theme, payload)
  } else {
    await saveCreate(payload)
  }

  saving.value = false
}

async function saveCreate(payload: { name: string; description: string | null }) {
  if (props.isSubtheme && props.parentId) {
    // Creating a subtheme
    const { error } = await store.createSubTheme(props.parentId, payload)
    if (error) { formError.value = error; return }
  } else {
    // Creating a root theme
    const { data: created, error } = await store.createTheme(payload)
    if (error || !created) { formError.value = error ?? 'Failed to create'; return }

    // Create each new subtheme
    for (const sub of newSubthemes.value) {
      if (!sub.name.trim()) continue
      const { error: subErr } = await store.createSubTheme(created.id, { name: sub.name.trim() })
      if (subErr) { formError.value = subErr; return }
    }
  }
  isOpen.value = false
  emit('saved')
}

async function saveEdit(theme: ThemeItem, payload: { name: string; description: string | null }) {
  const parentId = props.isSubtheme ? (theme.parentId ?? null) : null

  const { error } = await (props.isSubtheme && theme.parentId
    ? store.updateSubTheme(theme.id, theme.parentId, { ...payload, parentId: theme.parentId })
    : store.updateTheme(theme.id, { ...payload, parentId }))

  if (error) { formError.value = error; return }

  if (!props.isSubtheme) {
    // Update existing subthemes
    for (const sub of existingSubthemes.value) {
      const original = theme.children?.find(c => c.id === sub.id)
      if (original && original.name !== sub.name && sub.name.trim()) {
        const { error: e } = await store.updateSubTheme(sub.id, theme.id, { name: sub.name.trim(), parentId: theme.id })
        if (e) { formError.value = e; return }
      }
    }

    // Delete removed subthemes (if allowed)
    if (props.canDeleteSubthemes) {
      for (const delId of deletedSubthemeIds.value) {
        const { error: e } = await store.deleteSubTheme(delId, theme.id)
        if (e) { formError.value = e; return }
      }
    }

    // Create new subthemes
    for (const sub of newSubthemes.value) {
      if (!sub.name.trim()) continue
      const { error: e } = await store.createSubTheme(theme.id, { name: sub.name.trim(), parentId: theme.id })
      if (e) { formError.value = e; return }
    }
  }

  isOpen.value = false
  emit('saved')
}
</script>
