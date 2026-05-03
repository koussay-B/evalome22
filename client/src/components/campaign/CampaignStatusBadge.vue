<template>
  <span :class="classes" class="inline-flex items-center gap-1 text-[10px] font-bold uppercase tracking-wide px-2 py-1 rounded-md shrink-0">
    <span class="w-1.5 h-1.5 rounded-full" :class="dotClass" />
    {{ label }}
  </span>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useLocale } from '@/composables/useLocale'

const props = defineProps<{ status: string }>()
const { t } = useLocale()

const map = computed<Record<string, { classes: string; dotClass: string; label: string }>>(() => ({
  Active:    { classes: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-400', dotClass: 'bg-emerald-500',      label: t('campaignBuilder.statusActive')    },
  Upcoming:  { classes: 'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-400',                dotClass: 'bg-sky-500',          label: t('campaignBuilder.statusScheduled') },
  Scheduled: { classes: 'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-400',                dotClass: 'bg-sky-500',          label: t('campaignBuilder.statusScheduled') },
  Closed:    { classes: 'bg-muted text-muted-foreground',                                              dotClass: 'bg-muted-foreground', label: t('campaignBuilder.statusClosed')    },
  Draft:     { classes: 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-400',        dotClass: 'bg-amber-500',        label: t('campaignBuilder.statusDraft')     },
}))

const entry   = computed(() => map.value[props.status] ?? map.value['Draft']!)
const classes  = computed(() => entry.value.classes)
const dotClass = computed(() => entry.value.dotClass)
const label    = computed(() => entry.value.label)
</script>
