<template>
  <!-- Invite dialog -->
  <InviteCandidatesDialog
    v-model:open="isInviteOpen"
    :campaign-id="campaignId"
    :already-invited="alreadyInvitedIds"
    @invited="handleInvited"
  />

  <!-- Remove confirmation -->
  <AlertDialog v-model:open="isRemoveOpen">
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>{{ t('campaignCandidates.removeConfirmTitle') }}</AlertDialogTitle>
        <AlertDialogDescription>{{ t('campaignCandidates.removeConfirmDesc') }}</AlertDialogDescription>
      </AlertDialogHeader>
      <AlertDialogFooter>
        <AlertDialogCancel @click="isRemoveOpen = false">{{ t('common.cancel') }}</AlertDialogCancel>
        <AlertDialogAction
          class="bg-red-600 hover:bg-red-700 text-white border-none"
          @click="confirmRemove"
        >
          {{ t('campaignCandidates.removeBtn') }}
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>

  <!-- Main sheet -->
  <Sheet v-model:open="isOpen">
    <SheetContent :side="sheetSide" class="sm:max-w-md flex flex-col p-0">

      <!-- Header -->
      <SheetHeader class="px-6 pt-6 pb-5 border-b border-border shrink-0">
        <div class="flex items-start gap-4">
          <div class="w-12 h-12 rounded-2xl bg-primary/10 text-primary flex items-center justify-center shrink-0 mt-0.5">
            <Users class="w-6 h-6" />
          </div>
          <div class="min-w-0 flex-1">
            <SheetTitle class="text-xl text-slate-800 leading-tight">
              {{ t('campaignCandidates.sheetTitle') }}
              <span v-if="candidates.length > 0" class="ms-2 text-sm font-bold text-muted-foreground">({{ candidates.length }})</span>
            </SheetTitle>
            <SheetDescription class="mt-1 truncate">{{ campaignName }}</SheetDescription>
          </div>
        </div>
      </SheetHeader>

      <!-- Body -->
      <div class="flex-1 overflow-y-auto flex flex-col">

        <!-- Error -->
        <div
          v-if="loadError"
          class="mx-6 mt-4 flex items-center gap-2 px-4 py-3 rounded-lg bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 text-sm text-red-700 dark:text-red-400"
        >
          <CircleAlert class="w-4 h-4 shrink-0" />
          {{ t('campaignCandidates.loadError') }}
          <button class="ms-auto text-xs font-bold underline" @click="loadCandidates">{{ t('common.retry') }}</button>
        </div>

        <!-- Loading skeletons -->
        <div v-if="loading" class="p-6 space-y-3">
          <div v-for="i in 5" :key="i" class="flex items-center gap-3 p-3 rounded-xl border border-border">
            <div class="w-9 h-9 rounded-full bg-muted animate-pulse shrink-0" />
            <div class="flex-1 space-y-1.5">
              <div class="h-3.5 w-1/2 rounded bg-muted animate-pulse" />
              <div class="h-3 w-2/3 rounded bg-muted animate-pulse" />
            </div>
            <div class="h-5 w-16 rounded-full bg-muted animate-pulse" />
          </div>
        </div>

        <!-- Empty state -->
        <div
          v-else-if="candidates.length === 0"
          class="flex-1 flex flex-col items-center justify-center px-6 py-12 text-center gap-3"
        >
          <div class="w-14 h-14 rounded-2xl bg-muted flex items-center justify-center">
            <UserX class="w-7 h-7 text-muted-foreground" />
          </div>
          <p class="font-bold text-foreground">{{ t('campaignCandidates.emptyTitle') }}</p>
          <p class="text-sm text-muted-foreground leading-relaxed max-w-xs">{{ t('campaignCandidates.emptyDesc') }}</p>
        </div>

        <!-- Candidates list -->
        <div v-else class="p-4 space-y-2">
          <div
            v-for="c in candidates"
            :key="c.campaignCandidateId"
            class="flex items-center gap-3 px-4 py-3 rounded-xl border border-border bg-secondary/50 hover:border-border/80 transition-colors group"
          >
            <!-- Avatar -->
            <div class="w-9 h-9 rounded-full bg-primary/10 text-primary flex items-center justify-center text-xs font-bold shrink-0 overflow-hidden">
              <img v-if="c.candidateImageUrl" :src="c.candidateImageUrl" :alt="c.candidateName" class="w-full h-full object-cover" />
              <span v-else>{{ initials(c.candidateName) }}</span>
            </div>

            <!-- Info -->
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-slate-800truncate">{{ c.candidateName }}</p>
              <p class="text-xs text-muted-foreground truncate">{{ c.candidateEmail }}</p>
            </div>

            <!-- Status badge -->
            <span :class="statusBadgeClass(c.status)" class="text-[10px] font-bold uppercase tracking-wide px-2 py-0.5 rounded-full shrink-0">
              {{ statusLabel(c.status) }}
            </span>

            <!-- Remove button (visible on hover, only for Invited/Expired) -->
            <button
              v-if="c.status === 'Invited' || c.status === 'Expired'"
              class="p-1.5 rounded-lg text-muted-foreground hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-950/30 transition-colors opacity-0 group-hover:opacity-100 focus:opacity-100 shrink-0"
              @click="askRemove(c)"
            >
              <X class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="px-6 py-5 border-t border-border shrink-0 flex gap-2">
        <button
          class="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors"
          @click="isInviteOpen = true"
        >
          <UserPlus class="w-4 h-4" /> {{ t('campaignCandidates.inviteBtn') }}
        </button>
        <button
          class="px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800hover:bg-muted/50 transition-colors"
          @click="isOpen = false"
        >
          {{ t('campaignDetail.closeBtn') }}
        </button>
      </div>

    </SheetContent>
  </Sheet>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet'
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog'
import InviteCandidatesDialog from './InviteCandidatesDialog.vue'
import { useLocale } from '@/composables/useLocale'
import { getCampaignCandidatesApi, removeCampaignCandidateApi } from '@/utils/api'
import type { CampaignCandidateItem, CampaignCandidateStatus } from '@/utils/models'
import { Users, UserPlus, UserX, X, CircleAlert } from 'lucide-vue-next'

// ── Props & emits ──────────────────────────────────────────────────────────

const props = defineProps<{
  open:         boolean
  campaignId:   number
  campaignName: string
}>()

const emit = defineEmits<{
  (e: 'update:open', v: boolean): void
}>()

const { t, isRtl } = useLocale()
const sheetSide = computed<'right' | 'left'>(() => isRtl.value ? 'left' : 'right')

// ── Open binding ───────────────────────────────────────────────────────────

const isOpen = computed({
  get: () => props.open,
  set: (v) => emit('update:open', v),
})

// ── Data ───────────────────────────────────────────────────────────────────

const candidates = ref<CampaignCandidateItem[]>([])
const loading    = ref(false)
const loadError  = ref(false)

const alreadyInvitedIds = computed(() => candidates.value.map(c => c.candidateId))

async function loadCandidates() {
  loading.value   = true
  loadError.value = false
  const { data, error } = await getCampaignCandidatesApi(props.campaignId)
  if (error || !data) {
    loadError.value = true
  } else {
    candidates.value = data
  }
  loading.value = false
}

watch(() => props.open, (v) => {
  if (v) loadCandidates()
})

// ── Invite dialog ──────────────────────────────────────────────────────────

const isInviteOpen = ref(false)

function handleInvited() {
  loadCandidates()
}

// ── Remove ─────────────────────────────────────────────────────────────────

const isRemoveOpen  = ref(false)
const removingItem  = ref<CampaignCandidateItem | null>(null)

function askRemove(c: CampaignCandidateItem) {
  removingItem.value  = c
  isRemoveOpen.value  = true
}

async function confirmRemove() {
  if (!removingItem.value) return
  isRemoveOpen.value = false
  await removeCampaignCandidateApi(props.campaignId, removingItem.value.candidateId)
  removingItem.value = null
  loadCandidates()
}

// ── Helpers ────────────────────────────────────────────────────────────────

function initials(name: string) {
  return name.split(' ').filter(Boolean).map(n => n[0]).join('').toUpperCase().slice(0, 2) || '?'
}

function statusLabel(status: CampaignCandidateStatus): string {
  const map: Record<CampaignCandidateStatus, string> = {
    Invited:    t('campaignCandidates.statusInvited'),
    InProgress: t('campaignCandidates.statusInProgress'),
    Completed:  t('campaignCandidates.statusCompleted'),
    Expired:    t('campaignCandidates.statusExpired'),
    Withdrawn:  t('campaignCandidates.statusWithdrawn'),
  }
  return map[status] ?? status
}

function statusBadgeClass(status: CampaignCandidateStatus): string {
  const map: Record<CampaignCandidateStatus, string> = {
    Invited:    'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300',
    InProgress: 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300',
    Completed:  'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300',
    Expired:    'bg-muted text-muted-foreground',
    Withdrawn:  'bg-red-100 text-red-600 dark:bg-red-900/40 dark:text-red-400',
  }
  return map[status] ?? 'bg-muted text-muted-foreground'
}
</script>
