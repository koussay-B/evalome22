<script setup lang="ts">
import type { HTMLAttributes } from 'vue'
import { MoreHorizontal } from 'lucide-vue-next'
import { cn } from '@/lib/utils'

const props = defineProps<{ class?: HTMLAttributes['class'] }>()
</script>

<template>
  <span
    aria-hidden="true"
    :class="cn('flex h-9 w-9 items-center justify-center text-muted-foreground', props.class)"
  >
    <MoreHorizontal class="w-4 h-4" />
  </span>
</template>
