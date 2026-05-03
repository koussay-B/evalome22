<template>
  <div class="flex flex-col items-center gap-5 py-16 px-8 text-center">
    <!-- Icon -->
    <div class="w-14 h-14 rounded-2xl bg-muted flex items-center justify-center">
      <component :is="icon" class="w-7 h-7 text-muted-foreground" />
    </div>

    <!-- Text -->
    <div class="space-y-1.5 max-w-xs">
      <h3 class="text-sm font-bold text-foreground">{{ title }}</h3>
      <p v-if="description" class="text-sm text-muted-foreground leading-relaxed">
        {{ description }}
      </p>
    </div>

    <!-- Actions slot -->
    <div v-if="$slots.default" class="flex items-center gap-2 flex-wrap justify-center">
      <slot />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Component } from 'vue'

defineProps<{
  icon: Component
  title: string
  description?: string
}>()
</script>
