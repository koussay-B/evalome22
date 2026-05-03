<template>
  <div class="space-y-8 max-w-5xl">

    <!-- Header -->
    <div>
      <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">{{ t('settings.subtitle') }}</p>
      <h1 class="text-3xl text-slate-800 tracking-tight text-slate-800">{{ t('companyPanel.settings.myAccount') }}</h1>
    </div>

    <MyAccountSection initial-name="John Doe" initial-email="user@example.com" @saved="toast" />


    

    <!-- Saved Toast -->
    <Transition
      enter-from-class="opacity-0 translate-y-2"
      enter-active-class="transition-all duration-200"
      leave-to-class="opacity-0 translate-y-2"
      leave-active-class="transition-all duration-200"
    >
      <div v-if="showSaved" class="fixed bottom-6 inset-e-6 z-50 flex items-center gap-2 px-4 py-3 rounded-xl bg-foreground text-background text-sm font-semibold shadow-lg">
        <CheckCircle class="w-4 h-4" /> {{ t('settings.savedSuccessfully') }}
      </div>
    </Transition>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onUnmounted } from 'vue'
import { useLocale } from '@/composables/useLocale'
import MyAccountSection from '@/components/common/MyAccountSection.vue'
import ToggleSwitch from '@/components/campaign/builder/ToggleSwitch.vue'
import { CheckCircle } from 'lucide-vue-next'

const { t } = useLocale()



// ── Settings  ──────────────────────────────────────────────────────────────
const notif    = reactive({ invite: true, results: true, reminders: false })
const security = reactive({ allowTabSwitch: false, maxAttempts: '3' })

const showSaved = ref(false)
let timeout: ReturnType<typeof setTimeout>

// ── Actions ────────────────────────────────────────────────────────────────
function toast() {
  showSaved.value = true
  clearTimeout(timeout)
  timeout = setTimeout(() => { showSaved.value = false }, 2500)
}

onUnmounted(() => clearTimeout(timeout))
</script>
