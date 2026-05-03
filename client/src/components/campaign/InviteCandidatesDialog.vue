<template>
  <Dialog v-model:open="isOpen">
    <DialogContent class="sm:max-w-lg flex flex-col max-h-[90vh] p-0 gap-0">

      <!-- Header -->
      <DialogHeader class="px-6 pt-6 pb-4 border-b border-border shrink-0">
        <DialogTitle class="text-lg text-slate-800">{{ t('campaignCandidates.inviteDialogTitle') }}</DialogTitle>
        <DialogDescription>{{ t('campaignCandidates.inviteDialogDesc') }}</DialogDescription>
      </DialogHeader>

      <!-- Search -->
      <div class="px-6 py-4 border-b border-border shrink-0">
        <div class="relative">
          <Search class="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <Input
            v-model="search"
            :placeholder="t('campaignCandidates.searchPlaceholder')"
            class="ps-9 h-9"
          />
        </div>
      </div>

      <!-- Candidate list -->
      <div class="flex-1 overflow-y-auto px-4 py-3">

        <!-- Loading -->
        <div v-if="loadingUsers" class="space-y-2 py-2">
          <div v-for="i in 5" :key="i" class="flex items-center gap-3 p-2">
            <div class="w-4 h-4 rounded bg-muted animate-pulse shrink-0" />
            <div class="w-9 h-9 rounded-full bg-muted animate-pulse shrink-0" />
            <div class="flex-1 space-y-1.5">
              <div class="h-3.5 w-1/2 rounded bg-muted animate-pulse" />
              <div class="h-3 w-2/3 rounded bg-muted animate-pulse" />
            </div>
          </div>
        </div>

        <!-- Empty -->
        <div v-else-if="filteredCandidates.length === 0" class="py-10 text-center text-sm text-muted-foreground">
          {{ t('campaignCandidates.noCandidatesFound') }}
        </div>

        <!-- List -->
        <div v-else class="space-y-1">
          <div
            v-for="c in filteredCandidates"
            :key="c.id"
            :class="[
              'flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors select-none',
              c.alreadyInvited
                ? 'opacity-50 cursor-not-allowed'
                : selectedIds.includes(c.id)
                  ? 'bg-primary/10 border border-primary/30 cursor-pointer'
                  : 'hover:bg-muted/50 cursor-pointer',
            ]"
            @click="!c.alreadyInvited && toggleCandidate(c.id, !selectedIds.includes(c.id))"
          >
            <!-- Checkbox -->
            <Checkbox
              :checked="selectedIds.includes(c.id)"
              :disabled="c.alreadyInvited"
              @update:checked="toggleCandidate(c.id, $event)"
              @click.stop
            />

            <!-- Avatar -->
            <div class="w-9 h-9 rounded-full bg-primary/10 text-primary flex items-center justify-center text-xs font-bold shrink-0 overflow-hidden">
              <img v-if="c.imageUrl" :src="c.imageUrl" :alt="c.name" class="w-full h-full object-cover" />
              <span v-else>{{ initials(c.name) }}</span>
            </div>

            <!-- Info -->
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-slate-800truncate">{{ c.name }}</p>
              <p class="text-xs text-muted-foreground truncate">{{ c.email }}</p>
            </div>

            <!-- Already invited badge -->
            <span
              v-if="c.alreadyInvited"
              class="text-[10px] font-bold uppercase tracking-wide px-2 py-0.5 rounded-full bg-muted text-muted-foreground shrink-0"
            >
              {{ t('campaignCandidates.alreadyInvited') }}
            </span>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="px-6 py-4 border-t border-border shrink-0 flex items-center justify-between gap-3">
        <!-- Selection count -->
        <p class="text-xs text-muted-foreground">
          <template v-if="selectedIds.length > 0">
            {{ selectedIds.length }} {{ t('campaignCandidates.candidatesInSheet').toLowerCase() }} {{ t('common.of').toLowerCase() }} {{ selectableCount }} {{ t('common.status').toLowerCase() }}
          </template>
        </p>

        <div class="flex gap-2">
          <button
            class="px-4 py-2 rounded-lg border border-border text-sm font-semibold text-slate-800hover:bg-muted/50 transition-colors"
            @click="isOpen = false"
          >
            {{ t('common.cancel') }}
          </button>
          <button
            :disabled="selectedIds.length === 0 || inviting"
            class="flex items-center gap-2 px-4 py-2 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            @click="handleInvite"
          >
            <Loader2 v-if="inviting" class="w-3.5 h-3.5 animate-spin" />
            <UserPlus v-else class="w-3.5 h-3.5" />
            {{
              inviting
                ? t('campaignCandidates.inviting')
                : t('campaignCandidates.inviteSelected').replace('{n}', String(selectedIds.length))
            }}
          </button>
        </div>
      </div>

    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Checkbox } from '@/components/ui/checkbox'
import { useLocale } from '@/composables/useLocale'
import { getAvailableCandidatesApi, inviteCandidatesApi } from '@/utils/api'
import { Search, UserPlus, Loader2 } from 'lucide-vue-next'

// ── Props & emits ──────────────────────────────────────────────────────────

const props = defineProps<{
  open:            boolean
  campaignId:      number
  alreadyInvited:  number[]   // candidateIds already in the campaign
}>()

const emit = defineEmits<{
  (e: 'update:open', v: boolean): void
  (e: 'invited'): void
}>()

const { t } = useLocale()

// ── Local open binding ─────────────────────────────────────────────────────

const isOpen = computed({
  get: () => props.open,
  set: (v) => emit('update:open', v),
})

// ── Candidate list ─────────────────────────────────────────────────────────

interface CandidateOption {
  id:             number
  name:           string
  email:          string
  imageUrl?:      string | null
  alreadyInvited: boolean
}

const allCandidates = ref<CandidateOption[]>([])
const loadingUsers  = ref(false)

async function loadCandidates() {
  loadingUsers.value = true
  const { data } = await getAvailableCandidatesApi()
  if (data) {
    allCandidates.value = data.map(u => ({
      id:             u.id,
      name:           u.userName,
      email:          u.email ?? '',
      imageUrl:       u.imageUrl,
      alreadyInvited: props.alreadyInvited.includes(u.id),
    }))
  }
  loadingUsers.value = false
}

// Reload when dialog opens
watch(() => props.open, (v) => {
  if (v) {
    search.value      = ''
    selectedIds.value = []
    loadCandidates()
  }
})

// ── Search + filter ────────────────────────────────────────────────────────

const search = ref('')

const filteredCandidates = computed(() => {
  const q = search.value.trim().toLowerCase()
  if (!q) return allCandidates.value
  return allCandidates.value.filter(
    c => c.name.toLowerCase().includes(q) || c.email.toLowerCase().includes(q)
  )
})

const selectableCount = computed(() => allCandidates.value.filter(c => !c.alreadyInvited).length)

// ── Selection ──────────────────────────────────────────────────────────────

const selectedIds = ref<number[]>([])

function toggleCandidate(id: number, checked: boolean | 'indeterminate') {
  if (checked === true) {
    if (!selectedIds.value.includes(id)) selectedIds.value.push(id)
  } else {
    selectedIds.value = selectedIds.value.filter(x => x !== id)
  }
}

// ── Submit ─────────────────────────────────────────────────────────────────

const inviting   = ref(false)

async function handleInvite() {
  if (selectedIds.value.length === 0) return
  inviting.value = true
  const { error } = await inviteCandidatesApi(props.campaignId, selectedIds.value)
  inviting.value = false
  if (!error) {
    isOpen.value = false
    emit('invited')
  }
}

// ── Helpers ────────────────────────────────────────────────────────────────

function initials(name: string) {
  return name.split(' ').filter(Boolean).map(n => n[0]).join('').toUpperCase().slice(0, 2) || '?'
}
</script>
