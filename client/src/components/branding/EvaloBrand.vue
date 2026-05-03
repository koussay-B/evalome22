<template>
  <div class="flex items-center gap-4"> <div
      :class="[
        'flex shrink-0 items-center justify-center rounded-[22px] border border-brand/15 bg-white shadow-md dark:bg-background/70',
        size === 'sm' ? 'h-9 w-9' : 
        size === 'lg' ? 'h-16 w-16' : 
        size === 'xl' ? 'h-24 w-24' : 'h-12 w-12',
      ]"
    >
      <EvaloMark
        :class="[
          'text-brand',
          size === 'sm' ? 'h-6 w-6' : 
          size === 'lg' ? 'h-11 w-11' : 
          size === 'xl' ? 'h-16 w-16' : 'h-8 w-8',
        ]"
      />
    </div>

    <div class="min-w-0" :class="compact ? 'space-y-0' : 'space-y-1'">
      <div
        :class="[
          'truncate font-black tracking-tighter text-slate-800',
          size === 'sm' ? 'text-base' : 
          size === 'lg' ? 'text-3xl' : 
          size === 'xl' ? 'text-5xl' : 'text-2xl',
        ]"
      >
        <span class="uppercase">EVALO</span><span class="text-brand">.</span><span>me</span>
      </div>
      <p
        v-if="showSubtitle && subtitle"
        :class="[
          'truncate font-semibold text-slate-400',
          size === 'xl' ? 'text-lg' : size === 'lg' ? 'text-sm' : 'text-xs'
        ]"
      >
        {{ subtitle }}
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import EvaloMark from './EvaloMark.vue'

withDefaults(defineProps<{
  subtitle?: string
  showSubtitle?: boolean
  size?: 'sm' | 'md' | 'lg' | 'xl'
  compact?: boolean
}>(), {
  subtitle: "Unlock your team's potential",
  showSubtitle: true,
  size: 'md',
  compact: false,
})
</script>