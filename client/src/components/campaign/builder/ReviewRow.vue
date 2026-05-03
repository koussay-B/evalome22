<template>
  <div class="flex items-center justify-between px-4 py-2.5 gap-4">
    <span class="text-xs font-medium text-muted-foreground shrink-0">{{ label }}</span>
    <span
      class="text-xs font-bold text-end"
      :class="danger ? 'text-red-600 dark:text-red-400' : highlight ? 'text-emerald-600 dark:text-emerald-400' : 'text-foreground'"
    >
      {{ value }}
    </span>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  label: string
  value: string
  highlight?: boolean
  danger?: boolean
}>()
</script>
