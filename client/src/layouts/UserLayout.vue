<template>
  <div class="min-h-screen bg-background flex flex-col transition-colors duration-300">
    <!-- Topbar -->
    <header class="bg-card/50 backdrop-blur-md shadow-sm h-16 flex items-center px-8 justify-between border-b sticky top-0 z-50 transition-colors duration-300">
      <div class="flex items-center gap-4">
        <EvaloBrand :show-subtitle="false" size="lg" compact />
      </div>
      <div class="flex items-center gap-6">
        <ThemeToggle />
        <div class="flex items-center gap-3 bg-secondary/30 pl-4 py-1 pr-1 rounded-full border border-border/50">
          <span class="text-sm font-bold text-muted-foreground">{{ authStore.state.user?.userName }}</span>
          <Button variant="secondary" size="sm" class="rounded-full shadow-sm font-bold" @click="logout">
            Logout
          </Button>
        </div>
      </div>
    </header>

    <main class="flex-1 p-8 max-w-7xl mx-auto w-full transition-all duration-500">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/store/authStore';
import { useRouter } from 'vue-router';
import { Button } from '@/components/ui/button';
import ThemeToggle from '@/components/common/ThemeToggle.vue';
import EvaloBrand from '@/components/branding/EvaloBrand.vue';

const authStore = useAuthStore();
const router = useRouter();

const logout = () => {
  authStore.logout();
  router.push({ name: 'Login' });
};
</script>
