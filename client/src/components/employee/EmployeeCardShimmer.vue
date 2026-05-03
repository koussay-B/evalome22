<template>
  <div class="p-5 rounded-xl border border-border bg-secondary/20 flex flex-col">
    <!-- Header -->
    <div class="flex items-start gap-3 mb-4">
      <Skeleton class="w-10 h-10 rounded-full shrink-0" />
      <div class="flex-1 space-y-2 pt-0.5">
        <Skeleton class="h-4 w-2/5 rounded" />
        <Skeleton class="h-3 w-3/5 rounded" />
      </div>
    </div>

    <!-- Footer -->
    <div class="mt-auto pt-3 border-t border-border/60">
      <div class="flex items-center justify-between gap-2">
        <Skeleton class="h-5 w-16 rounded-md" />
        <Skeleton class="h-3 w-24 rounded" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Skeleton } from '@/components/ui/skeleton'
</script>
