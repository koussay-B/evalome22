<template>
  <div class="min-h-[80vh] flex flex-col items-center justify-center text-center space-y-6">
    <div class="relative">
      <h1 class="text-[12rem] font-black text-primary/10 leading-none select-none">404</h1>
      <div class="absolute inset-0 flex items-center justify-center">
        <h2 class="text-4xl font-black text-foreground tracking-tight">PAGE NOT FOUND</h2>
      </div>
    </div>
    <p class="text-muted-foreground text-lg max-w-md mx-auto font-medium">
      The page you're looking for doesn't exist or has been moved to a new dimension.
    </p>
    <Button as-child size="lg" class="font-bold rounded-2xl px-12 shadow-xl shadow-primary/20">
      <router-link to="/">Return Home</router-link>
    </Button>
  </div>
</template>

<script setup lang="ts">
import { Button } from '@/components/ui/button';
</script>
