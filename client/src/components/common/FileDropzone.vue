<template>
  <div class="space-y-1.5">
    <!-- Drop Zone -->
    <div
      @dragover.prevent="isDragging = true"
      @dragleave.prevent="isDragging = false"
      @drop.prevent="onDrop"
      @click="open()"
      class="relative flex flex-col items-center justify-center gap-3 rounded-xl border-2 border-dashed transition-colors cursor-pointer select-none overflow-hidden"
      :class="[
        heightMap[height],
        isDragging
          ? 'border-primary bg-primary/5'
          : hasFiles
            ? 'border-border bg-muted/30'
            : 'border-border hover:border-muted-foreground/50 hover:bg-muted/20',
      ]"
    >
      <input
        ref="fileInput"
        type="file"
        :accept="accept"
        :multiple="multiple"
        class="sr-only"
        @change="onFileChange"
      />

      <!-- Single image preview -->
      <template v-if="!multiple && singlePreview">
        <img :src="singlePreview" alt="Preview" class="h-24 w-24 object-contain rounded-xl border border-border bg-background shadow-sm" />
        <p class="text-xs text-muted-foreground font-medium truncate max-w-[70%] text-center">{{ files[0]?.name }}</p>
        <button
          type="button"
          @click.stop="clear()"
          class="absolute top-2 inset-e-2 p-1 rounded-md bg-background border border-border text-muted-foreground hover:text-slate-800 hover:bg-muted transition-colors"
        >
          <X class="w-3.5 h-3.5" />
        </button>
      </template>

      <!-- Multiple files list -->
      <template v-else-if="multiple && files.length > 0">
        <div class="w-full px-3 space-y-1.5 max-h-36 overflow-y-auto py-2">
          <div
            v-for="(file, i) in files"
            :key="i"
            class="flex items-center gap-2 text-xs bg-background rounded-lg px-3 py-2 border border-border"
          >
            <FileUp class="w-3.5 h-3.5 text-muted-foreground shrink-0" />
            <span class="flex-1 truncate font-medium text-slate-800">{{ file.name }}</span>
            <span class="text-muted-foreground shrink-0">{{ fmtSize(file.size) }}</span>
            <button type="button" @click.stop="removeFile(i)" class="text-muted-foreground hover:text-slate-800 transition-colors">
              <X class="w-3 h-3" />
            </button>
          </div>
        </div>
        <p class="text-xs text-muted-foreground">{{ files.length }} {{ files.length === 1 ? 'file' : 'files' }} selected</p>
        <button
          type="button"
          @click.stop="clear()"
          class="absolute top-2 inset-e-2 p-1 rounded-md bg-background border border-border text-muted-foreground hover:text-slate-800 hover:bg-muted transition-colors"
        >
          <X class="w-3.5 h-3.5" />
        </button>
      </template>

      <!-- Empty state -->
      <template v-else>
        <div class="w-12 h-12 rounded-xl bg-muted flex items-center justify-center">
          <ImageUp v-if="isImageAccept" class="w-6 h-6 text-muted-foreground" />
          <FileUp v-else class="w-6 h-6 text-muted-foreground" />
        </div>
        <div class="text-center space-y-1 px-4">
          <p class="text-sm font-semibold text-slate-800">
            {{ isDragging ? dropNowLabel : dropLabel }}
          </p>
          <p class="text-xs text-muted-foreground">{{ computedHint }}</p>
        </div>
      </template>
    </div>

    <!-- Size / type error -->
    <p v-if="errorMsg" class="text-xs text-red-500 font-medium">{{ errorMsg }}</p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { X, ImageUp, FileUp } from 'lucide-vue-next'

// ── Props ──────────────────────────────────────────────────────────────────
const props = withDefaults(defineProps<{
  /** MIME types the input should accept, e.g. "image/png,image/jpeg" or "*" */
  accept?: string
  /** Allow selecting multiple files */
  multiple?: boolean
  /** Max size per file in megabytes */
  maxSizeMb?: number
  /** Height preset of the drop zone */
  height?: 'sm' | 'md' | 'lg'
  /** Label shown in the empty state */
  dropLabel?: string
  /** Label shown while dragging over */
  dropNowLabel?: string
  /** Override the auto-generated hint line (accepted types + size) */
  hint?: string
}>(), {
  accept: 'image/*',
  multiple: false,
  maxSizeMb: 2,
  height: 'md',
  dropLabel: 'Drop your file here, or click to browse',
  dropNowLabel: 'Drop it!',
})

const emit = defineEmits<{ 'update:modelValue': [files: File[]] }>()

// ── State ──────────────────────────────────────────────────────────────────
const fileInput    = ref<HTMLInputElement | null>(null)
const isDragging   = ref(false)
const files        = ref<File[]>([])
const singlePreview = ref<string | null>(null)
const errorMsg     = ref<string | null>(null)

// ── Computed ───────────────────────────────────────────────────────────────
const hasFiles     = computed(() => files.value.length > 0)
const isImageAccept = computed(() => props.accept.includes('image'))

const heightMap: Record<string, string> = { sm: 'h-32', md: 'h-44', lg: 'h-56' }

const computedHint = computed(() => {
  if (props.hint) return props.hint
  const raw = props.accept === '*' || props.accept === '*/*' ? [] :
    props.accept.split(',').map(t => {
      const part = t.trim().split('/')[1]
      return part && part !== '*' ? part.toUpperCase() : null
    }).filter(Boolean) as string[]
  const typePart = raw.length ? raw.join(', ') : 'All files'
  const sizePart = `max ${props.maxSizeMb} MB`
  return `${typePart} — ${sizePart}`
})

// ── Helpers ────────────────────────────────────────────────────────────────
function fmtSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function generatePreview(file: File) {
  if (!file.type.startsWith('image/')) { singlePreview.value = null; return }
  const reader = new FileReader()
  reader.onload = e => { singlePreview.value = e.target?.result as string }
  reader.readAsDataURL(file)
}

function addFiles(incoming: FileList | File[]) {
  errorMsg.value = null
  const arr = Array.from(incoming)
  const oversized = arr.find(f => f.size > props.maxSizeMb * 1024 * 1024)
  if (oversized) {
    errorMsg.value = `"${oversized.name}" exceeds the ${props.maxSizeMb} MB limit`
    return
  }
  if (props.multiple) {
    files.value = [...files.value, ...arr]
  } else {
    if (arr[0]) {
      files.value = [arr[0]]
      generatePreview(arr[0])
    }
  }
  emit('update:modelValue', files.value)
}

// ── Event handlers ─────────────────────────────────────────────────────────
function onFileChange(e: Event) {
  const f = (e.target as HTMLInputElement).files
  if (f?.length) addFiles(f)
  if (fileInput.value) fileInput.value.value = '' // allow re-selecting same file
}

function onDrop(e: DragEvent) {
  isDragging.value = false
  const f = e.dataTransfer?.files
  if (f?.length) addFiles(f)
}

function removeFile(i: number) {
  files.value = files.value.filter((_, idx) => idx !== i)
  if (!props.multiple) singlePreview.value = null
  emit('update:modelValue', files.value)
}

// ── Public API ─────────────────────────────────────────────────────────────
function clear() {
  files.value = []
  singlePreview.value = null
  errorMsg.value = null
  if (fileInput.value) fileInput.value.value = ''
  emit('update:modelValue', [])
}

function open() {
  fileInput.value?.click()
}

defineExpose({ clear, open })
</script>
