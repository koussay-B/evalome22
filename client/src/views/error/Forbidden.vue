<template>
  <div class="min-h-[80vh] flex flex-col items-center justify-center text-center space-y-8 p-8 border-4 border-dashed border-destructive/20 rounded-[3rem] bg-destructive/5 m-8">
    <div class="w-24 h-24 rounded-3xl bg-destructive/10 flex items-center justify-center text-destructive border border-destructive/20 shadow-2xl shadow-destructive/10 animate-bounce">
      <ShieldAlert class="w-12 h-12" />
    </div>
    <div class="space-y-2">
      <h1 class="text-5xl font-black text-destructive tracking-tighter italic">403: ACCESS DENIED</h1>
      <p class="text-muted-foreground text-xl font-bold max-w-md mx-auto">
        You don't have the necessary clearance to access this sector.
      </p>
    </div>
    <div class="flex gap-4">
      <Button variant="outline" @click="router.back()" class="font-bold rounded-2xl px-8 border-destructive/20 text-destructive hover:bg-destructive/10">Go Back</Button>
      <Button as-child size="lg" class="bg-destructive hover:bg-destructive/90 text-destructive-foreground font-bold rounded-2xl px-12 shadow-xl shadow-destructive/20">
        <router-link to="/">Emergency Exit</router-link>
      </Button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Button } from '@/components/ui/button';
import { ShieldAlert } from 'lucide-vue-next';
import { useRouter } from 'vue-router';

const router = useRouter();
</script>

