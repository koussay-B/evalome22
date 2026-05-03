<template>
  <div class="p-5 rounded-xl border border-border bg-secondary flex flex-col">
    <!-- Header -->
    <div class="flex items-start gap-3 mb-4">
      <Skeleton class="w-10 h-10 rounded-xl shrink-0" />
      <div class="flex-1 space-y-2 pt-0.5">
        <Skeleton class="h-4 w-3/5 rounded" />
        <Skeleton class="h-3 w-2/5 rounded" />
      </div>
    </div>

    <!-- Footer -->
    <div class="mt-auto pt-3 border-t border-border/60 space-y-2.5">
      <div class="flex items-center justify-between">
        <Skeleton class="h-3 w-14 rounded" />
        <Skeleton class="h-3 w-20 rounded" />
      </div>
      <Skeleton class="h-3 w-3/5 rounded" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { Skeleton } from '@/components/ui/skeleton'
</script>
