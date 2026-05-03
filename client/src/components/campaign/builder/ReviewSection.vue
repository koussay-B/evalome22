<template>
  <div class="rounded-xl border border-border overflow-hidden">
    <div class="flex items-center gap-2.5 px-4 py-3 bg-muted/50 border-b border-border">
      <component :is="iconComponent" class="w-4 h-4 text-muted-foreground" />
      <p class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ title }}</p>
    </div>
    <div class="divide-y divide-border/60">
      <slot />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { ClipboardList, BarChart2, Users } from 'lucide-vue-next'

const props = defineProps<{
  title: string
  icon: 'clipboard' | 'chart' | 'users'
}>()

const iconMap = { clipboard: ClipboardList, chart: BarChart2, users: Users }
const iconComponent = computed(() => iconMap[props.icon])
</script>
