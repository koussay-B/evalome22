<script setup lang="ts">
import type { HTMLAttributes } from 'vue'
import { cn } from '@/lib/utils'
import { useLocale } from '@/composables/useLocale'

const props = defineProps<{ class?: HTMLAttributes['class'] }>()
const { t } = useLocale()
</script>

<template>
  <nav
    role="navigation"
    :aria-label="t('common.pagination')"
    :class="cn('mx-auto flex w-full justify-center', props.class)"
  >
    <slot />
  </nav>
</template>
