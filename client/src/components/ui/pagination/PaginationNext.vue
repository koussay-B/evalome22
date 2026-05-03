<script setup lang="ts">
import type { HTMLAttributes } from 'vue'
import { ChevronRight, ChevronLeft } from 'lucide-vue-next'
import { cn } from '@/lib/utils'
import { useLocale } from '@/composables/useLocale'

const props = defineProps<{
  class?: HTMLAttributes['class']
  href?: string
}>()

const emit = defineEmits<{ click: [e: MouseEvent] }>()
const { t, isRtl } = useLocale()
</script>

<template>
  <a
    :href="href ?? '#'"
    :aria-label="t('pagination.next')"
    :class="cn(
      'inline-flex items-center justify-center gap-1 rounded-lg text-sm font-semibold h-9 px-3 transition-colors text-foreground hover:bg-muted',
      props.class,
    )"
    @click="emit('click', $event)"
  >
    <span class="hidden sm:inline" :class="isRtl ? 'order-2' : 'order-1'">{{ t('pagination.next') }}</span>
    <component :is="isRtl ? ChevronLeft : ChevronRight" class="w-4 h-4" :class="isRtl ? 'order-1' : 'order-2'" />
  </a>
</template>
