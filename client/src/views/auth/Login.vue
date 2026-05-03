<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/store/authStore'
import { useRouter, RouterLink } from 'vue-router'
import { Home } from 'lucide-vue-next'

// Branding
import EvaloBrand from '@/components/branding/EvaloBrand.vue'

// Components
import ThemeToggle from '@/components/common/ThemeToggle.vue'
import LanguageSwitcher from '@/components/common/LanguageSwitcher.vue'
import LoginForm from '@/components/auth/LoginForm.vue'
import DevUserPanel from '@/components/auth/DevUserPanel.vue'

const authStore = useAuthStore()
const router = useRouter()

const loginFormRef = ref<InstanceType<typeof LoginForm> | null>(null)

async function handleLogin(form: { username: string; password: string }) {
  const result = await authStore.login(form)
  if (result.success) {
    router.push(result.redirectUrl || '/')
  }
}

async function handleDevFill({ username, password }: { username: string; password: string }) {
  loginFormRef.value?.fill(username, password)
  // Déclencher le login directement après avoir rempli les champs
  await handleLogin({ username, password })
}

const isDev = import.meta.env.DEV
</script>

<template>
  <div class="relative flex min-h-svh w-full items-center justify-center p-6 md:p-10 overflow-hidden bg-slate-50">
    
    <!-- ── Background Orbs ── -->
    <div class="absolute inset-0 z-0 overflow-hidden pointer-events-none">
      <div class="absolute -top-[10%] -left-[5%] w-[500px] h-[500px] rounded-full bg-primary/15 blur-[120px] animate-pulse"></div>
      <div class="absolute -bottom-[20%] -right-[10%] w-[600px] h-[600px] rounded-full bg-sky-600/20 blur-[140px]"></div>
      <!-- Subtle Grid Pattern -->
      <div class="absolute inset-0 opacity-[0.03]" 
           style="background-image: radial-gradient(#000 1px, transparent 1px); background-size: 32px 32px;">
      </div>
    </div>

    <!-- Top Actions (Home, Lang, Theme) -->
    <div class="absolute top-6 left-6 z-50">
       <RouterLink to="/" class="h-9 w-9 rounded-xl border border-white/40 bg-white/40 backdrop-blur-md flex items-center justify-center shadow-sm hover:bg-white/60 transition-all hover:scale-105">
         <Home class="h-4 w-4 text-slate-700" />
       </RouterLink>
    </div>
    <div class="absolute top-6 right-6 flex items-center gap-2 z-50">
      <div class="bg-white/40 backdrop-blur-md rounded-xl border border-white/40 p-1.5 flex gap-1 shadow-sm">
        <LanguageSwitcher />
        <ThemeToggle />
      </div>
    </div>

    <!-- ── LOGIN SECTION (Logo + 3D Card) ── -->
    <div class="w-full max-w-sm z-10 [perspective:1200px] flex flex-col items-center">
      
      <!-- 1. EL LOGO (EvaloBrand) -->
      <div class="mb-10 transition-transform duration-500 hover:scale-105 drop-shadow-2xl">
        <EvaloBrand :show-subtitle="false" size="lg" compact />
      </div>

      <!-- 2. THE 3D CARD -->
      <div class="w-full relative group transition-all duration-700 ease-out transform-gpu 
                  [transform:rotateX(10deg)_rotateY(-4deg)] 
                  hover:[transform:rotateX(0deg)_rotateY(0deg)]">
        
        <!-- Glow effect wra el card -->
        <div class="absolute -inset-2 bg-gradient-to-r from-primary/20 to-sky-400/20 rounded-[2.5rem] blur-2xl opacity-40 group-hover:opacity-70 transition-opacity duration-500"></div>

        <LoginForm
          class="relative shadow-[30px_30px_70px_rgba(0,0,0,0.08)] border border-white/60 rounded-[2rem] bg-white/80 backdrop-blur-xl"
          ref="loginFormRef"
          :loading="authStore.state.loading"
          :server-error="authStore.state.error"
          @submit="handleLogin"
        />
        
        <!-- Glass shine reflection -->
        <div class="absolute inset-0 rounded-[2rem] pointer-events-none bg-gradient-to-br from-white/30 via-transparent to-transparent opacity-40"></div>
      </div>
    </div>

    <!-- ── Dev User Panel ── -->
    <div class="absolute bottom-4 left-0 w-full z-[100]">
       <DevUserPanel v-if="isDev" @fill="handleDevFill" />
    </div>

  </div>
</template>

<style scoped>
.transform-gpu {
  will-change: transform;
}
</style>