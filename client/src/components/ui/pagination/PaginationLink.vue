<script setup lang="ts">
import type { HTMLAttributes } from 'vue'
import { cn } from '@/lib/utils'

const props = defineProps<{
  class?: HTMLAttributes['class']
  isActive?: boolean
  href?: string
}>()

const emit = defineEmits<{ click: [e: MouseEvent] }>()
</script>

<template>
  <a
    :href="href ?? '#'"
    :aria-current="isActive ? 'page' : undefined"
    :class="cn(
      'inline-flex items-center justify-center rounded-lg text-sm font-semibold h-9 w-9 transition-all duration-200',
      isActive
        ? 'bg-[oklch(55%_0.24_51.057)] text-white shadow-md' 
        : 'text-slate-600 hover:bg-[oklch(97.5%_0.02_51.057)] hover:text-[oklch(65%_0.18_51.057)]',
      props.class,
    )"
    @click="emit('click', $event)"
  >
    <slot />
  </a>
</template>
