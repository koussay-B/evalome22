<template>
  <div class="space-y-8 max-w-5xl">
    <!-- Header -->
    <div>
      <p class="text-[10px] font-semibold uppercase tracking-[0.25em] text-primary/60 mb-1">{{ t('settings.subtitle') }}</p>
      <h1 class="text-3xl text-slate-800 tracking-tight text-foreground">{{ t('settings.title') }}</h1>
    </div>

    <MyAccountSection @saved="toast" />

    <!-- ── Platform Settings ────────────────────────────────────────────── -->
    <div class="rounded-2xl border border-border/60 bg-slate-50 shadow-sm overflow-hidden">
      <div class="px-6 py-4 border-b border-border/40 bg-slate-50">
        <h2 class="text-lg font-semibold text-slate-800 uppercase tracking-wider text-foreground">{{ t('settings.general') }}</h2>
        <p class="text-[11px] text-muted-foreground mt-0.5">{{ t('settings.platformNameDesc') }}</p>
      </div>
      <div class="divide-y divide-border/40">
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.platformName') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.platformNameDesc') }}</p>
          </div>
          <Input v-model="settings.platformName" class="w-52 h-9 text-sm shrink-0 rounded-xl" />
        </div>
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800  text-foreground tracking-tight">{{ t('settings.defaultLang') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.defaultLangDesc') }}</p>
          </div>
          <div class="flex gap-1.5 shrink-0">
            <button v-for="lang in ['FR', 'EN', 'AR']" :key="lang" @click="settings.lang = lang"
              class="px-3 py-1.5 rounded-xl border text-[11px] font-semibold tracking-wider transition-all"
              :class="settings.lang === lang ? 'bg-[oklch(65%_0.18_51.057)] text-white border-[oklch(65%_0.18_51.057)]' : 'bg-white border-border/60 text-muted-foreground hover:bg-[oklch(97.5%_0.02_51.057)] hover:text-[oklch(65%_0.18_51.057)]'">
              {{ lang }}
            </button>
          </div>
        </div>
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.timezone') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.timezoneDesc') }}</p>
          </div>
          <div class="flex items-center gap-2 px-3 py-1.5 rounded-xl border border-border/60 text-xs font-semibold text-muted-foreground bg-[oklch(97.5%_0.02_51.057)] shrink-0">
            <Clock class="w-3.5 h-3.5" />
            <span>UTC+01:00 — Paris</span>
          </div>
        </div>
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.maintenance') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.maintenanceDesc') }}</p>
          </div>
          <ToggleSwitch v-model="settings.maintenance" />
        </div>
      </div>
    </div>

    <!-- ── Registration ─────────────────────────────────────────────────── -->
    <div class="rounded-2xl border border-border/60 bg-slate-50 shadow-sm overflow-hidden">
      <div class="px-6 py-4 border-b border-border/40 bg-slate-50">
        <h2 class="text-lg font-semibold text-slate-800 uppercase tracking-wider text-foreground">{{ t('settings.registration') }}</h2>
        <p class="text-[11px] text-muted-foreground mt-0.5">{{ t('settings.publicRegistrationDesc') }}</p>
      </div>
      <div class="divide-y divide-border/40">
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.publicRegistration') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.publicRegistrationDesc') }}</p>
          </div>
          <ToggleSwitch v-model="settings.publicRegistration" />
        </div>
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.emailVerification') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.emailVerificationDesc') }}</p>
          </div>
          <ToggleSwitch v-model="settings.emailVerification" />
        </div>
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.defaultRole') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.defaultRoleDesc') }}</p>
          </div>
          <div class="flex gap-1.5 shrink-0">
            <button v-for="role in ['Candidat', 'Formateur']" :key="role" @click="settings.defaultRole = role"
              class="px-3 py-1.5 rounded-xl border text-[11px] font-semibold tracking-wider transition-all"
              :class="settings.defaultRole === role ? 'bg-[oklch(65%_0.18_51.057)] text-white border-[oklch(65%_0.18_51.057)]' : 'bg-white border-border/60 text-muted-foreground hover:bg-[oklch(97.5%_0.02_51.057)] hover:text-[oklch(65%_0.18_51.057)]'">
              {{ role }}
            </button>
          </div>
        </div>
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.maxAttempts') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.maxAttemptsDesc') }}</p>
          </div>
          <div class="flex items-center gap-2 shrink-0">
            <button @click="settings.maxAttempts = Math.max(1, settings.maxAttempts - 1)"
              class="w-8 h-8 rounded-xl border border-border/60 flex items-center justify-center text-foreground hover:bg-[oklch(97.5%_0.02_51.057)] transition-colors">−</button>
            <span class="w-6 text-center text-sm font-semibold text-foreground tracking-tight">{{ settings.maxAttempts }}</span>
            <button @click="settings.maxAttempts = Math.min(10, settings.maxAttempts + 1)"
              class="w-8 h-8 rounded-xl border border-border/60 flex items-center justify-center text-foreground hover:bg-[oklch(97.5%_0.02_51.057)] transition-colors">+</button>
          </div>
        </div>
      </div>
    </div>

    <!-- ── Security ───────────────────────────────────────────────────── -->
    <div class="rounded-2xl border border-border/60 bg-slate-50 shadow-sm overflow-hidden">
      <div class="px-6 py-4 border-b border-border/40 bg-slate-50">
        <h2 class="text-lg font-semibold text-slate-800 uppercase tracking-wider text-foreground">{{ t('settings.security') }}</h2>
        <p class="text-[11px] text-muted-foreground mt-0.5">{{ t('settings.sessionTimeoutDesc') }}</p>
      </div>
      <div class="divide-y divide-border/40">
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.sessionTimeout') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.sessionTimeoutDesc') }}</p>
          </div>
          <div class="flex gap-1.5 shrink-0">
            <button v-for="t in ['30m', '1h', '4h', '24h']" :key="t" @click="settings.sessionTimeout = t"
              class="px-3 py-1.5 rounded-xl border text-[11px] font-semibold tracking-wider transition-all"
              :class="settings.sessionTimeout === t ? 'bg-[oklch(65%_0.18_51.057)] text-white border-[oklch(65%_0.18_51.057)]' : 'bg-white border-border/60 text-muted-foreground hover:bg-[oklch(97.5%_0.02_51.057)] hover:text-[oklch(65%_0.18_51.057)]'">
              {{ t }}
            </button>
          </div>
        </div>
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.jwtExpiry') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.jwtExpiryDesc') }}</p>
          </div>
          <div class="flex gap-1.5 shrink-0">
            <button v-for="t in ['1d', '7d', '30d']" :key="t" @click="settings.jwtExpiry = t"
              class="px-3 py-1.5 rounded-xl border text-[11px] font-semibold tracking-wider transition-all"
              :class="settings.jwtExpiry === t ? 'bg-[oklch(65%_0.18_51.057)] text-white border-[oklch(65%_0.18_51.057)]' : 'bg-white border-border/60 text-muted-foreground hover:bg-[oklch(97.5%_0.02_51.057)] hover:text-[oklch(65%_0.18_51.057)]'">
              {{ t }}
            </button>
          </div>
        </div>
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.forceHttps') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.forceHttpsDesc') }}</p>
          </div>
          <ToggleSwitch v-model="settings.forceHttps" />
        </div>
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.allowTabSwitch') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.allowTabSwitchDesc') }}</p>
          </div>
          <ToggleSwitch v-model="settings.allowTabSwitch" />
        </div>
      </div>
    </div>

    <!-- ── AI Models ─────────────────────────────────────────────────── -->
    <div class="rounded-2xl border border-border/60 bg-slate-50 shadow-sm overflow-hidden">
      <div class="px-6 py-4 border-b border-border/40 bg-slate-50 flex items-center justify-between gap-4">
        <div>
          <h2 class="text-lg font-semibold text-slate-800 uppercase tracking-wider text-foreground">{{ t('settings.aiModels') }}</h2>
          <p class="text-[11px] text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.aiModelsDesc') }}</p>
        </div>
        <button
          @click="openAiModelDialog(null)"
          class="shrink-0 flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-[oklch(65%_0.18_51.057)] text-white text-[11px] font-semibold hover:opacity-90 transition-all shadow-sm"
        >
          <Plus class="w-3.5 h-3.5" />
          {{ t('settings.addAiModel') }}
        </button>
      </div>

      <!-- Loading -->
      <div v-if="aiLoading" class="px-6 py-8 flex justify-center">
        <Loader2 class="w-5 h-5 animate-spin text-muted-foreground" />
      </div>

      <!-- Empty -->
      <div v-else-if="aiModels.length === 0" class="px-6 py-10 text-center">
        <Bot class="w-8 h-8 mx-auto text-muted-foreground/40 mb-2" />
        <p class="text-sm font-semibold text-foreground">{{ t('settings.aiModelNoModels') }}</p>
        <p class="text-xs text-muted-foreground mt-0.5">{{ t('settings.aiModelNoModelsDesc') }}</p>
      </div>

      <!-- Model list -->
      <div v-else class="divide-y divide-border/40">
        <div
          v-for="model in aiModels"
          :key="model.id"
          class="flex items-center gap-4 px-6 py-4"
        >
          <!-- Provider badge -->
          <div class="shrink-0 w-9 h-9 rounded-xl flex items-center justify-center text-[10px] font-bold border tracking-wider"
            :class="providerBadgeClass(model.provider)" 
          >
            {{ providerShort(model.provider) }}
          </div>

          <!-- Info -->
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2">
              <p class="text-sm font-semibold text-foreground truncate tracking-tight">{{ model.name }}</p>
              <span v-if="model.isDefault" class="shrink-0 px-1.5 py-0.5 rounded-lg text-[10px] font-bold bg-[oklch(97.5%_0.02_51.057)] text-[oklch(65%_0.18_51.057)] border border-[oklch(65%_0.18_51.057_/_0.1)] uppercase tracking-wider">
                {{ t('settings.aiModelDefault') }}
              </span>
              <span v-if="!model.isEnabled" class="shrink-0 px-1.5 py-0.5 rounded-md text-[10px]  bg-muted text-muted-foreground uppercase tracking-wider">
                off
              </span>
            </div>
            <p class="text-[11px] font-medium text-muted-foreground truncate">{{ model.modelIdentifier }} · {{ model.apiKeyMasked }}</p>
          </div>

          <!-- Actions -->
          <div class="shrink-0 flex items-center gap-1">
            <button
              v-if="!model.isDefault"
              @click="setDefaultModel(model.id)"
              class="p-1.5 rounded-xl text-xs text-muted-foreground hover:text-[oklch(65%_0.18_51.057)] hover:bg-[oklch(97.5%_0.02_51.057)] transition-all"
              :title="t('settings.aiModelSetDefault')"
            >
              <Star class="w-3.5 h-3.5" />
            </button>
            <button
              @click="openAiModelDialog(model)"
              class="p-1.5 rounded-xl text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-all"
              :title="t('settings.aiModelEdit')"
            >
              <Pencil class="w-3.5 h-3.5" />
            </button>
            <button
              @click="confirmDeleteModel(model)"
              class="p-1.5 rounded-xl text-muted-foreground hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/30 transition-all"
              :title="t('settings.aiModelDelete')"
            >
              <Trash2 class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- ── AI Model Dialog ─────────────────────────────────────────────── -->
    <Teleport to="body">
      <Transition enter-from-class="opacity-0" enter-active-class="transition-opacity duration-150"
        leave-to-class="opacity-0" leave-active-class="transition-opacity duration-150">
        <div v-if="aiDialogOpen" class="fixed inset-0 z-50 bg-black/40 flex items-center justify-center p-4" @click.self="aiDialogOpen = false">
          <div class="bg-white rounded-2xl border border-border shadow-xl w-full max-w-md p-6 space-y-5">

            <div>
              <h3 class="text-base font-semibold text-slate-800 text-foreground tracking-tight">{{ aiDialogEditing ? t('settings.aiModelEdit') : t('settings.addAiModel') }}</h3>
            </div>

            <!-- Name -->
            <div class="space-y-1.5">
              <label class="text-[11px] font-semibold text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('settings.aiModelName') }}</label>
              <Input v-model="aiForm.name" placeholder="e.g. GPT-4o Production" class="h-9 rounded-xl" />
            </div>

            <!-- Provider -->
            <div class="space-y-1.5">
              <label class="text-xs  uppercase tracking-wider text-muted-foreground">{{ t('settings.aiModelProvider') }}</label>
              <div class="grid grid-cols-3 gap-2">
                <button v-for="p in PROVIDERS" :key="p.value"
                  @click="selectProvider(p.value)"
                  class="py-2 rounded-xl border text-[11px] font-semibold transition-all"
                  :class="aiForm.provider === p.value ? 'border-[oklch(65%_0.18_51.057)] bg-[oklch(97.5%_0.02_51.057)] text-[oklch(65%_0.18_51.057)] ring-1 ring-[oklch(65%_0.18_51.057)]' : 'border-border/60 text-muted-foreground hover:border-[oklch(65%_0.18_51.057_/_0.3)]'"
                >
                  {{ p.label }}
                </button>
              </div>
            </div>

            <!-- Model Identifier -->
            <div class="space-y-1.5">
              <label class="text-[11px] font-semibold text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('settings.aiModelIdentifier') }}</label>
              <Input v-model="aiForm.modelIdentifier" :placeholder="t('settings.aiModelIdentifierHint')" class="h-9 rounded-xl font-mono text-xs" />
              <!-- Known models quick-select -->
              <div v-if="knownModels.length" class="flex flex-wrap gap-1.5 pt-0.5">
                <span class="text-[10px] text-muted-foreground self-center">{{ t('settings.aiModelKnownModels') }}:</span>
                <button
                  v-for="m in knownModels"
                  :key="m"
                  @click="aiForm.modelIdentifier = m"
                  class="px-2 py-0.5 rounded-md border text-[10px] font-mono transition-all"
                  :class="aiForm.modelIdentifier === m
                    ? 'border-[oklch(65%_0.18_51.057)] bg-[oklch(97.5%_0.02_51.057)] text-[oklch(65%_0.18_51.057)]'
                    : 'border-border text-muted-foreground hover:border-primary/40 hover:text-foreground'"
                >{{ m }}</button>
              </div>
            </div>

            <!-- API Key -->
            <div class="space-y-1.5">
              <label class="text-[11px] font-semibold text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('settings.aiModelApiKey') }}</label>
              <Input v-model="aiForm.apiKey" type="password" placeholder="sk-…" class="h-9 rounded-xl font-mono" />
              <p class="text-[10px] text-amber-600 dark:text-amber-400 flex items-center gap-1">
                <AlertTriangle class="w-3 h-3" /> {{ t('settings.aiModelApiKeyHint') }}
              </p>
            </div>

            <!-- Base URL -->
            <div class="space-y-1.5">
              <label class="text-[11px] font-semibold text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('settings.aiModelBaseUrl') }}</label>
              <Input v-model="aiForm.apiBaseUrl" placeholder="https://api.openai.com" class="h-9 rounded-xl font-mono text-[10px]" />
              <p v-if="aiForm.provider !== 'Other'" class="text-[10px] text-muted-foreground">
                {{ t('settings.aiModelBaseUrlAutoFilled') }} 
              </p>
            </div>

            <!-- Toggles -->
            <div class="flex gap-6">
              <label class="flex items-center gap-2.5 cursor-pointer select-none group">
                <input type="checkbox" v-model="aiForm.isDefault" class="w-4 h-4 rounded accent-[oklch(65%_0.18_51.057)]" />
                <span class="text-sm font-semibold text-slate-800 tracking-tight group-hover:text-primary transition-colors">{{ t('settings.aiModelDefault') }}</span>
              </label>
              <label class="flex items-center gap-2.5 cursor-pointer select-none group">
                <input type="checkbox" v-model="aiForm.isEnabled" class="w-4 h-4 rounded accent-[oklch(65%_0.18_51.057)]" />
                <span class="text-sm font-semibold text-slate-800 tracking-tight group-hover:text-primary transition-colors">{{ t('settings.aiModelEnabled') }}</span>
              </label>
            </div>

            <!-- Test Connection -->
            <div class="rounded-2xl border border-border bg-[oklch(97.5%_0.02_51.057_/_0.5)] p-3 space-y-2">
              <div class="flex items-center justify-between gap-3">
                <p class="text-xs font-semibold text-slate-800 tracking-tight">{{ t('settings.aiModelTestConnection') }}</p>
                <button
                  @click="testConnection"
                  :disabled="testingConnection || !aiForm.modelIdentifier.trim() || !aiForm.apiKey.trim()"
                  class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl border text-[11px] font-semibold transition-all disabled:opacity-40"
                  :class="testResult?.success
                    ? 'border-emerald-400 text-emerald-700 dark:text-emerald-400 bg-emerald-50/60 dark:bg-emerald-950/30'
                    : testResult && !testResult.success
                      ? 'border-red-400 text-red-600 dark:text-red-400 bg-red-50/60 dark:bg-red-950/30'
                      : 'border-border text-muted-foreground hover:text-foreground hover:border-primary/40'"
                >
                  <Loader2 v-if="testingConnection" class="w-3 h-3 animate-spin" />
                  <Wifi v-else class="w-3 h-3" />
                  {{ testingConnection ? t('settings.aiModelTesting') : t('settings.aiModelTestConnection') }}
                </button>
              </div>
              <div v-if="testResult" class="flex items-start gap-1.5 text-xs" :class="testResult.success ? 'text-emerald-700 dark:text-emerald-400' : 'text-red-500'">
                <CheckCircle2 v-if="testResult.success" class="w-3.5 h-3.5 mt-0.5 shrink-0" />
                <XCircle v-else class="w-3.5 h-3.5 mt-0.5 shrink-0" />
                <span>{{ testResult.success ? t('settings.aiModelTestSuccess') : testResult.message }}</span>
              </div>
            </div>

            <!-- Error -->
            <p v-if="aiFormError" class="text-xs text-red-500 flex items-center gap-1.5">
              <CircleAlert class="w-3.5 h-3.5" /> {{ aiFormError }}
            </p>

            <!-- Footer -->
            <div class="flex gap-3 pt-1">
              <button
                @click="saveAiModel"
                :disabled="aiFormSaving"
                class="flex-1 py-2.5 rounded-xl bg-[oklch(65%_0.18_51.057)] text-white text-sm font-semibold hover:opacity-90 transition-all disabled:opacity-60 flex items-center justify-center gap-2"
              >
                <Loader2 v-if="aiFormSaving" class="w-4 h-4 animate-spin" />
                {{ t('common.save') }} 
              </button>
              <button
                @click="aiDialogOpen = false"
                class="px-4 py-2.5 rounded-xl border border-border text-sm font-semibold text-foreground hover:bg-muted/50 transition-colors"
              >
                {{ t('common.cancel') }}
              </button>
            </div>
          </div>
        </div>
      </Transition>

      <!-- Delete Confirm Dialog -->
      <Transition enter-from-class="opacity-0" enter-active-class="transition-opacity duration-150"
        leave-to-class="opacity-0" leave-active-class="transition-opacity duration-150">
        <div v-if="deleteConfirmModel" class="fixed inset-0 z-50 bg-black/40 flex items-center justify-center p-4" @click.self="deleteConfirmModel = null">
          <div class="bg-background rounded-2xl border border-border shadow-xl w-full max-w-sm p-6 space-y-5">
            <div class="space-y-1">
              <h3 class="text-base font-semibold text-foreground tracking-tight">{{ t('settings.aiModelDelete') }}</h3>
              <p class="text-sm text-muted-foreground tracking-tight">{{ t('settings.aiModelDeleteConfirm') }}</p>
              <p class="text-sm font-semibold text-foreground">{{ deleteConfirmModel.name }}</p>
            </div>
            <div class="flex gap-3">
              <button @click="executeDeleteModel" :disabled="aiFormSaving"
                class="flex-1 py-2.5 rounded-xl bg-red-600 text-white text-sm font-semibold hover:bg-red-700 transition-colors disabled:opacity-60 flex items-center justify-center gap-2">
                <Loader2 v-if="aiFormSaving" class="w-4 h-4 animate-spin" />
                {{ t('common.delete') }}
              </button>
              <button @click="deleteConfirmModel = null"
                class="px-4 py-2.5 rounded-xl border border-border text-sm font-semibold text-foreground hover:bg-muted/50 transition-colors">
                {{ t('common.cancel') }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ── Notifications ──────────────────────────────────────────────── -->
    <div class="rounded-2xl border border-border/60 bg-slate-50 shadow-sm overflow-hidden">
      <div class="px-6 py-4 border-b border-border/40 bg-slate-50">
        <h2 class="text-lg font-semibold text-slate-800 uppercase tracking-wider text-foreground">{{ t('settings.notifications') }}</h2>
        <p class="text-[11px] text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.inviteEmailsDesc') }}</p>
      </div>
      <div class="divide-y divide-border/40">
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.inviteEmails') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.inviteEmailsDesc') }}</p>
          </div>
          <ToggleSwitch v-model="settings.inviteEmails" />
        </div>
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.resultEmails') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.resultEmailsDesc') }}</p>
          </div>
          <ToggleSwitch v-model="settings.resultEmails" />
        </div>
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.reminderEmails') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.reminderEmailsDesc') }}</p>
          </div>
          <ToggleSwitch v-model="settings.reminderEmails" />
        </div>
        <div class="flex items-center justify-between gap-6 px-6 py-5">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.adminDigest') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.adminDigestDesc') }}</p>
          </div>
          <ToggleSwitch v-model="settings.adminDigest" />
        </div>
      </div>
    </div>

    <!-- ── Danger Zone ────────────────────────────────────────────────── -->
    <div class="rounded-2xl border border-red-200 dark:border-red-900/40 bg-slate-50 shadow-sm overflow-hidden">
      <div class="px-6 py-4 border-b border-red-100 dark:border-red-900/30 bg-red-50/30">
        <h2 class="text-lg font-semibold text-slate-800 uppercase tracking-wider text-red-600 dark:text-red-400">{{ t('settings.dangerZone') }}</h2>
        <p class="text-[13px] text-red-600/70 mt-0.5 tracking-tight">{{ t('settings.dangerResetDesc') }}</p>
      </div>
      <div class="divide-y divide-red-100 dark:divide-red-900/30">
        <div class="flex items-start justify-between px-6 py-5 gap-6">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.dangerReset') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.dangerResetDesc') }}</p>
          </div>
          <button class="shrink-0 px-3 py-1.5 rounded-xl border border-red-300 dark:border-red-800 text-red-600 text-[10px] font-bold uppercase tracking-wider hover:bg-red-50 dark:hover:bg-red-900/30 transition-colors">
            {{ t('settings.reset') }}
          </button>
        </div>
        <div class="flex items-start justify-between px-6 py-5 gap-6">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.dangerArchive') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">{{ t('settings.dangerArchiveDesc') }}</p>
          </div>
          <button class="shrink-0 px-3 py-1.5 rounded-xl border border-red-300 dark:border-red-800 text-red-600 text-[10px] font-bold uppercase tracking-wider hover:bg-red-50 dark:hover:bg-red-900/30 transition-colors">
            {{ t('settings.archive') }}
          </button>
        </div>
        <div class="flex items-start justify-between px-6 py-5 gap-6 bg-red-50/20 dark:bg-red-950/20">
          <div>
            <p class="text-sm text-slate-800 text-foreground tracking-tight">{{ t('settings.dangerWipe') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5 tracking-tight">
              {{ t('settings.dangerWipeDesc') }}
              <span class="font-semibold text-red-700">{{ t('settings.dangerWipeWarning') }}</span>
            </p>
          </div>
          <button class="shrink-0 px-3 py-1.5 rounded-xl bg-red-600 text-white text-[10px] font-bold uppercase tracking-wider hover:bg-red-700 transition-colors shadow-sm">
            {{ t('settings.wipe') }}
          </button>
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
      <div v-if="saved" class="fixed bottom-6 inset-e-6 z-50 flex items-center gap-2 px-4 py-3 rounded-xl bg-[oklch(65%_0.18_51.057)] text-white text-sm font-semibold shadow-lg">
        <Check class="w-4 h-4" /> {{ t('settings.savedSuccessfully') }} 
      </div>
    </Transition>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted, onUnmounted } from 'vue'
import { useLocale } from '@/composables/useLocale'
import { Input } from '@/components/ui/input'
import MyAccountSection from '@/components/common/MyAccountSection.vue'
import ToggleSwitch from '@/components/campaign/builder/ToggleSwitch.vue'
import {
  Clock, Check, Plus, Bot, Star, Pencil, Trash2, Loader2, AlertTriangle,
  CircleAlert, Wifi, CheckCircle2, XCircle,
} from 'lucide-vue-next'
import type { AiModelItem, AiProvider, TestAiModelResult } from '@/utils/models'
import {
  getAiModelsApi, createAiModelApi, updateAiModelApi,
  deleteAiModelApi, setDefaultAiModelApi, testAiModelApi,
} from '@/utils/api'

// ── Provider constants ─────────────────────────────────────────────────────
const PROVIDERS = [
  { value: 'OpenAI',      label: 'OpenAI' },
  { value: 'Anthropic',   label: 'Anthropic' },
  { value: 'Google',      label: 'Google' },
  { value: 'OpenRouter',  label: 'OpenRouter' },
  { value: 'Other',       label: 'Other' },
] as const

const PROVIDER_BASE_URLS: Record<string, string> = {
  OpenAI:     'https://api.openai.com',
  Anthropic:  'https://api.anthropic.com',
  Google:     'https://generativelanguage.googleapis.com/v1beta/openai',
  OpenRouter: 'https://openrouter.ai/api/v1',
  Other:      '', 
}

const PROVIDER_MODELS: Record<string, string[]> = {
  OpenAI:     ['gpt-4o', 'gpt-4o-mini', 'gpt-4-turbo', 'o1', 'o3-mini'],
  Anthropic:  ['claude-opus-4-6', 'claude-sonnet-4-6', 'claude-haiku-4-5', 'claude-3-5-sonnet-20241022'],
  Google:     ['gemini-2.0-flash', 'gemini-2.0-flash-exp', 'gemini-1.5-pro', 'gemini-1.5-flash'],
  OpenRouter: ['openai/gpt-4o', 'anthropic/claude-opus-4-6', 'google/gemini-2.0-flash', 'meta-llama/llama-3.3-70b-instruct'],
  Other:      [],
}

const { t } = useLocale()

function toast() {
  saved.value = true
  clearTimeout(timeout)
  timeout = setTimeout(() => { saved.value = false }, 2500)
}

// ── Settings ───────────────────────────────────────────────────────────────
const saved = ref(false)
const settings = reactive({
  platformName:      'EVALO.me',
  lang:              'FR',
  maintenance:       false,
  publicRegistration: false,
  emailVerification: true,
  defaultRole:       'Candidat',
  maxAttempts:       1,
  sessionTimeout:    '1h',
  jwtExpiry:         '7d',
  forceHttps:        true,
  allowTabSwitch:    false,
  inviteEmails:      true,
  resultEmails:      true,
  reminderEmails:    false,
  adminDigest:       true,
})

let timeout: ReturnType<typeof setTimeout>

watch(settings, () => {
  saved.value = true
  clearTimeout(timeout)
  timeout = setTimeout(() => { saved.value = false }, 2500)
}, { deep: true })

onUnmounted(() => clearTimeout(timeout))

// ── AI Models ──────────────────────────────────────────────────────────────
const aiModels    = ref<AiModelItem[]>([])
const aiLoading   = ref(false)

async function loadAiModels() {
  aiLoading.value = true
  const { data } = await getAiModelsApi()
  if (data) aiModels.value = data
  aiLoading.value = false
}

onMounted(loadAiModels)

// Provider helpers
function providerShort(p: string) {
  const map: Record<string, string> = {
    OpenAI: 'OAI', Anthropic: 'ANT', Google: 'GGL', OpenRouter: 'ORT', Other: 'OTH',
  }
  return map[p] ?? p.slice(0, 3).toUpperCase()
}
function providerBadgeClass(p: string) {
  if (p === 'OpenAI')     return 'border-emerald-300 dark:border-emerald-800 bg-emerald-50/60 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-400'
  if (p === 'Anthropic')  return 'border-orange-300 dark:border-orange-800 bg-orange-50/60 dark:bg-orange-950/30 text-orange-700 dark:text-orange-400'
  if (p === 'Google')     return 'border-blue-300 dark:border-blue-800 bg-blue-50/60 dark:bg-blue-950/30 text-blue-700 dark:text-blue-400'
  if (p === 'OpenRouter') return 'border-purple-300 dark:border-purple-800 bg-purple-50/60 dark:bg-purple-950/30 text-purple-700 dark:text-purple-400'
  return 'border-sky-300 dark:border-sky-800 bg-sky-50/60 dark:bg-sky-950/30 text-sky-700 dark:text-sky-400'
}

// ── AI Model dialog ────────────────────────────────────────────────────────
const aiDialogOpen    = ref(false)
const aiDialogEditing = ref<AiModelItem | null>(null)
const aiFormSaving    = ref(false)
const aiFormError     = ref('')

// Test connection state
const testingConnection = ref(false)
const testResult        = ref<TestAiModelResult | null>(null)

const aiForm = reactive({
  name:            '',
  provider:        'OpenAI' as AiProvider,
  modelIdentifier: '',
  apiKey:          '',
  apiBaseUrl:      '' as string | undefined,
  isDefault:       false,
  isEnabled:       true,
})

const knownModels = computed(() => PROVIDER_MODELS[aiForm.provider] ?? [])

function selectProvider(p: AiProvider) {
  aiForm.provider        = p
  aiForm.modelIdentifier = ''
  aiForm.apiBaseUrl      = PROVIDER_BASE_URLS[p] ?? ''
  testResult.value       = null
}

function openAiModelDialog(model: AiModelItem | null) {
  aiDialogEditing.value  = model
  aiFormError.value      = ''
  testResult.value       = null
  if (model) {
    aiForm.name            = model.name
    aiForm.provider        = model.provider
    aiForm.modelIdentifier = model.modelIdentifier
    aiForm.apiKey          = model.apiKeyMasked   // shows masked; user overwrites if needed
    aiForm.apiBaseUrl      = model.apiBaseUrl ?? ''
    aiForm.isDefault       = model.isDefault
    aiForm.isEnabled       = model.isEnabled
  } else {
    aiForm.name            = ''
    aiForm.provider        = 'OpenAI'
    aiForm.modelIdentifier = ''
    aiForm.apiKey          = ''
    aiForm.apiBaseUrl      = PROVIDER_BASE_URLS['OpenAI']
    aiForm.isDefault       = false
    aiForm.isEnabled       = true
  }
  aiDialogOpen.value = true
}

async function testConnection() {
  testingConnection.value = true
  testResult.value        = null
  const { data, error } = await testAiModelApi({
    provider:        aiForm.provider,
    modelIdentifier: aiForm.modelIdentifier.trim(),
    apiKey:          aiForm.apiKey.trim(),
    apiBaseUrl:      aiForm.apiBaseUrl?.trim() || null,
    modelId:         aiDialogEditing.value?.id,
  })
  testResult.value       = data ?? { success: false, message: error ?? 'Unknown error' }
  testingConnection.value = false
}

async function saveAiModel() {
  if (!aiForm.name.trim() || !aiForm.modelIdentifier.trim() || !aiForm.apiKey.trim()) {
    aiFormError.value = 'Name, Model ID and API Key are required.'
    return
  }
  aiFormSaving.value = true
  aiFormError.value  = ''

  const payload = {
    name:            aiForm.name.trim(),
    provider:        aiForm.provider,
    modelIdentifier: aiForm.modelIdentifier.trim(),
    apiKey:          aiForm.apiKey.trim(),
    apiBaseUrl:      aiForm.apiBaseUrl?.trim() || null,
    isDefault:       aiForm.isDefault,
    isEnabled:       aiForm.isEnabled,
  }

  const { data, error } = aiDialogEditing.value
    ? await updateAiModelApi(aiDialogEditing.value.id, payload)
    : await createAiModelApi(payload)

  if (error || !data) {
    aiFormError.value = error ?? 'Failed to save model.'
    aiFormSaving.value = false
    return
  }

  if (aiDialogEditing.value) {
    const idx = aiModels.value.findIndex(m => m.id === aiDialogEditing.value!.id)
    if (idx >= 0) aiModels.value[idx] = data
  } else {
    aiModels.value.push(data)
  }

  // If newly set as default, clear others
  if (data.isDefault)
    aiModels.value = aiModels.value.map(m => ({ ...m, isDefault: m.id === data.id }))

  aiDialogOpen.value = false
  aiFormSaving.value = false
  toast()
}

// ── Delete ─────────────────────────────────────────────────────────────────
const deleteConfirmModel = ref<AiModelItem | null>(null)

function confirmDeleteModel(model: AiModelItem) {
  deleteConfirmModel.value = model
}

async function executeDeleteModel() {
  if (!deleteConfirmModel.value) return
  aiFormSaving.value = true
  const { error } = await deleteAiModelApi(deleteConfirmModel.value.id)
  if (!error) {
    aiModels.value = aiModels.value.filter(m => m.id !== deleteConfirmModel.value!.id)
    deleteConfirmModel.value = null
    toast()
  }
  aiFormSaving.value = false
}

// ── Set Default ───────────────────────────────────────────────────────────
async function setDefaultModel(id: number) {
  const { data } = await setDefaultAiModelApi(id)
  if (data) {
    aiModels.value = aiModels.value.map(m => ({ ...m, isDefault: m.id === id }))
    toast()
  }
}
</script>
