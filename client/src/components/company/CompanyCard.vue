<template>
  <div
    @click="$emit('click')"
    class="p-5 rounded-xl border border-border bg-slate-50 hover:border-primary/30 hover:shadow-sm transition-all flex flex-col group cursor-pointer"
  >
    <!-- Header: logo + name + actions slot -->
    <div class="flex justify-between items-start mb-3 gap-3">
      <div class="flex items-center gap-3 min-w-0">

        <!-- Logo / Fallback initial -->
        <div class="w-11 h-11 rounded-xl border border-border bg-muted flex items-center justify-center shrink-0 overflow-hidden relative">
          <template v-if="company.logo && !imgError">
            <span v-if="imgLoading" class="absolute inset-0 flex items-center justify-center">
              <Loader2 class="w-4 h-4 text-muted-foreground animate-spin" />
            </span>
            <img
              :src="company.logo"
              :alt="company.name"
              class="w-full h-full object-cover transition-opacity duration-200"
              :class="imgLoading ? 'opacity-0' : 'opacity-100'"
              @load="imgLoading = false"
              @error="imgError = true"
            />
          </template>
          <!-- No logo or broken → letter avatar -->
          <span v-else class="text-slate-800 text-slate-800 text-sm select-none">
            {{ company.name.charAt(0).toUpperCase() }}
          </span>
        </div>

        <!-- Name + subtitle (description → website → address → dash) -->
        <div class="min-w-0">
          <h3 class="font-bold text-slate-800 truncate">{{ company.name }}</h3>
          <p class="text-xs text-muted-foreground truncate">
            {{ company.description ?? company.website ?? company.address ?? '—' }}
          </p>
        </div>
      </div>

      <!-- Dropdown actions injected by parent -->
      <slot name="actions" />
    </div>

    <!-- Contact pills: phone + email -->
    <div v-if="company.phone || company.email" class="flex flex-wrap gap-1.5 mb-3">
      <span v-if="company.phone"
        class="inline-flex items-center gap-1 text-[11px] font-medium text-muted-foreground bg-muted/60 rounded-md px-2 py-0.5 shrink-0">
        <Phone class="w-3 h-3 shrink-0" />{{ company.phone }}
      </span>
      <span v-if="company.email"
        class="inline-flex items-center gap-1 text-[11px] font-medium text-muted-foreground bg-muted/60 rounded-md px-2 py-0.5 min-w-0 truncate">
        <Mail class="w-3 h-3 shrink-0" />{{ company.email }}
      </span>
    </div>

    <!-- Footer: status + date -->
    <div class="mt-auto pt-3 border-t border-border/60">
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-1.5">
          <span class="w-1.5 h-1.5 rounded-full shrink-0"
            :class="company.isActive ? 'bg-emerald-500' : 'bg-amber-400'" />
          <span class="text-xs font-semibold"
            :class="company.isActive
              ? 'text-emerald-700 dark:text-emerald-400'
              : 'text-amber-700 dark:text-amber-400'">
            {{ company.isActive ? t('common.active') : t('common.inactive') }}
          </span>
        </div>
        <span class="text-[10px] text-muted-foreground">{{ formatDate(company.createdAt) }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { Company } from '@utils/models'
import { Mail, Phone, Loader2 } from 'lucide-vue-next'
import { useLocale } from '@/composables/useLocale'

defineProps<{ company: Company }>()
defineEmits<{ click: [] }>()

const { t } = useLocale()

const imgLoading = ref(true)
const imgError   = ref(false)

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}
</script>
