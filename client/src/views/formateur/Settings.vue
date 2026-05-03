<template>
  <div class="space-y-8 max-w-5xl">

    <!-- Header -->
    <div>
      <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">{{ t('settings.subtitle') }}</p>
      <h1 class="text-3xl text-slate-800 tracking-tight text-slate-800">{{ t('companyPanel.settings.myAccount') }}</h1>
    </div>

    <MyAccountSection @saved="toast" />


    <!-- ── Notifications ─────────────────────────────────────────────────── -->
    <div class="rounded-xl border border-border bg-gray-50">
      <div class="px-6 py-5 border-b border-border">
        <h2 class="text-sm text-slate-800 uppercase tracking-wider text-slate-800">{{ t('companyPanel.settings.notifications') }}</h2>
        <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.notificationsDesc') }}</p>
      </div>
      <div class="divide-y divide-border">
        <div class="flex items-center justify-between px-6 py-4 border-t border-border">
          <div>
            <p class="text-sm font-semibold text-slate-800">{{ t('companyPanel.settings.resultEmails') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.resultEmailsDesc') }}</p>
          </div>
          <ToggleSwitch v-model="notif.results" />
        </div>
        <div class="flex items-center justify-between px-6 py-4">
          <div>
            <p class="text-sm font-semibold text-slate-800">{{ t('companyPanel.settings.reminderEmails') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.reminderEmailsDesc') }}</p>
          </div>
          <ToggleSwitch v-model="notif.reminders" />
        </div>
      </div>
    </div>

    <!-- ── Security ──────────────────────────────────────────────────────── -->
    <div class="rounded-xl border border-border bg-gray-50">
      <div class="px-6 py-5 border-b border-border">
        <h2 class="text-sm text-slate-800 uppercase tracking-wider text-slate-800">{{ t('companyPanel.settings.security') }}</h2>
        <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.securityDesc') }}</p>
      </div>
      <div class="divide-y divide-border">
        <div class="flex items-center justify-between px-6 py-4">
          <div>
            <p class="text-sm font-semibold text-slate-800">{{ t('settings.allowTabSwitch') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">{{ t('settings.allowTabSwitchDesc') }}</p>
          </div>
          <ToggleSwitch v-model="security.allowTabSwitch" />
        </div>
      </div>
    </div>
    
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
