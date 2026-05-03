<template>
  <Dialog v-model:open="isOpen">
    <DialogContent class="sm:max-w-md p-0 gap-0 overflow-hidden">

      <!-- Header -->
      <DialogHeader class="px-5 pt-5 pb-4 border-b border-border">
        <DialogTitle class="text-base font-bold">
          {{ t('employees.selectCompany') }}
        </DialogTitle>
        <DialogDescription class="text-sm text-muted-foreground mt-0.5">
          {{ t('employees.selectCompanyDesc') }}
        </DialogDescription>
      </DialogHeader>

      <!-- Search -->
      <div class="px-4 py-3 border-b border-border">
        <div class="relative">
          <Search
            class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground pointer-events-none"
          />
          <Input
            v-model="searchQuery"
            :placeholder="t('common.search') + '…'"
            class="ps-9 h-9 text-sm"
            @input="onSearch"
          />
        </div>
      </div>

      <!-- Company list -->
      <div class="overflow-y-auto max-h-[300px]">

        <!-- Loading -->
        <div v-if="companyStore.list.isLoading" class="flex items-center justify-center py-12">
          <Loader2 class="w-5 h-5 animate-spin text-muted-foreground" />
        </div>

        <!-- Empty -->
        <div
          v-else-if="!companyStore.list.currentItems.length"
          class="py-12 text-center text-sm text-muted-foreground"
        >
          {{ t('companies.emptyTitle') }}
        </div>

        <!-- Items -->
        <button
          v-for="company in companyStore.list.currentItems"
          :key="company.id"
          type="button"
          @click="togglePending(company)"
          class="w-full flex items-center gap-3 px-4 py-3 hover:bg-muted/50 transition-colors text-start border-b border-border/40 last:border-0"
          :class="pending?.id === company.id ? 'bg-primary/5' : ''"
        >
          <!-- Logo -->
          <div
            class="w-9 h-9 rounded-md bg-muted flex items-center justify-center overflow-hidden shrink-0 border border-border"
          >
            <img
              v-if="company.logo"
              :src="company.logo"
              class="w-full h-full object-cover"
              alt=""
            />
            <Building2 v-else class="w-4 h-4 text-muted-foreground" />
          </div>

          <!-- Info -->
          <div class="flex-1 min-w-0">
            <p class="text-sm font-semibold text-slate-800 truncate">{{ company.name }}</p>
            <p v-if="company.email" class="text-xs text-muted-foreground truncate">
              {{ company.email }}
            </p>
          </div>

          <!-- Check or status dot -->
          <Check
            v-if="pending?.id === company.id"
            class="w-4 h-4 text-primary shrink-0"
          />
          <span
            v-else
            class="w-2 h-2 rounded-full shrink-0"
            :class="company.isActive ? 'bg-emerald-500' : 'bg-muted-foreground/30'"
          />
        </button>
      </div>

      <!-- Footer -->
      <div class="flex items-center justify-end gap-2 px-4 py-3 border-t border-border bg-muted/20">
        <button
          type="button"
          @click="isOpen = false"
          class="px-3.5 py-2 rounded-lg border border-border text-sm font-medium hover:bg-muted/50 transition-colors"
        >
          {{ t('common.cancel') }}
        </button>
        <button
          type="button"
          @click="confirm"
          :disabled="!pending"
          class="px-3.5 py-2 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 disabled:opacity-40 transition-colors"
        >
          {{ t('common.confirm') }}
        </button>
      </div>

    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { useLocale }       from '@/composables/useLocale'
import { useCompanyStore } from '@/store/companyStore'
import type { Company }    from '@utils/models'
import { Building2, Search, Check, Loader2 } from 'lucide-vue-next'

// ── Props / Emits ─────────────────────────────────────────────────────────────

const props = defineProps<{
  modelValue?: Company | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: Company]
}>()

const isOpen = defineModel<boolean>('open', { default: false })

// ── Composables ───────────────────────────────────────────────────────────────

const { t }        = useLocale()
const companyStore = useCompanyStore()

// ── State ─────────────────────────────────────────────────────────────────────

const searchQuery = ref('')
const pending     = ref<Company | null>(props.modelValue ?? null)

// Sync pending + fetch when dialog opens; clear search when it closes
watch(isOpen, (open) => {
  if (open) {
    pending.value = props.modelValue ?? null
    if (!companyStore.list.hasData) {
      companyStore.list.fetchPage({ pageSize: 100 })
    }
  } else {
    searchQuery.value = ''
  }
})

onMounted(() => {
  if (!companyStore.list.hasData) {
    companyStore.list.fetchPage({ pageSize: 100 })
  }
})

// ── Search (debounced) ────────────────────────────────────────────────────────

let _timer: ReturnType<typeof setTimeout>

function onSearch() {
  clearTimeout(_timer)
  _timer = setTimeout(() => {
    companyStore.list.fetchPage({
      search:   searchQuery.value || undefined,
      pageSize: 100,
    })
  }, 350)
}

// ── Handlers ──────────────────────────────────────────────────────────────────

function togglePending(company: Company) {
  pending.value = pending.value?.id === company.id ? null : company
}

function confirm() {
  if (!pending.value) return
  emit('update:modelValue', pending.value)
  isOpen.value = false
}
</script>
