<template>
  <div class="space-y-6 pb-10">

    <!-- ══ SUCCESS STATE ══════════════════════════════════════════════ -->
    <div v-if="isSuccess" class="flex flex-col items-center justify-center py-16 gap-8">
      <!-- Icon -->
      <div class="relative">
        <div class="w-24 h-24 rounded-full bg-emerald-100 dark:bg-emerald-900/40 flex items-center justify-center">
          <CheckCircle2 class="w-12 h-12 text-emerald-600 dark:text-emerald-400" />
        </div>
        <div
          class="absolute -bottom-1 -inset-e-1 w-8 h-8 rounded-full flex items-center justify-center text-white text-xs font-black"
          :class="
            createdStatus === 'Active'
              ? 'bg-emerald-600'
              : createdStatus === 'Scheduled'
                ? 'bg-sky-600'
                : 'bg-foreground'
          "
        >
          {{ createdStatus === 'Active' ? '✓' : createdStatus === 'Scheduled' ? '→' : '✎' }}
        </div>
      </div>

      <!-- Text -->
      <div class="text-center space-y-2 max-w-sm">
        <h2 class="text-2xl  text-slate-800">{{ t('campaignBuilder.successTitle') }}</h2>
        <p class="text-base font-semibold text-slate-800">{{ createdName }}</p>
        <p class="text-sm text-muted-foreground">
          {{ t('campaignBuilder.successSavedAs') }}
          <span class="text-slate-800" :class="
            createdStatus === 'Active' ? 'text-emerald-600 dark:text-emerald-400' :
            createdStatus === 'Scheduled' ? 'text-sky-600 dark:text-sky-400' : 'text-slate-800'
          ">{{ t('campaignBuilder.status' + createdStatus) }}</span>.
          {{ createdStatus === 'Draft' ? t('campaignBuilder.successDraftNote') : createdStatus === 'Scheduled' ? t('campaignBuilder.successScheduledNote') : t('campaignBuilder.successActiveNote') }}
        </p>
      </div>

      <!-- Next actions -->
      <div class="w-full max-w-sm space-y-3">
        <p class="text-xs  uppercase tracking-wider text-muted-foreground text-center">{{ t('campaignBuilder.successNext') }}</p>

        <button
          class="w-full flex items-center gap-4 px-5 py-4 rounded-xl border-2 border-primary bg-primary/5 hover:bg-primary/10 transition-colors text-start"
          @click="goToQuestionnaire"
        >
          <div class="w-10 h-10 rounded-lg bg-primary/15 flex items-center justify-center shrink-0">
            <FileText class="w-5 h-5 text-primary" />
          </div>
          <div>
            <p class="text-sm  text-slate-800">{{ t('campaignBuilder.successManageQ') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">{{ t('campaignBuilder.successManageQDesc') }}</p>
          </div>
          <ChevronRight class="w-4 h-4 text-muted-foreground ms-auto shrink-0" />
        </button>

        <button
          class="w-full flex items-center gap-4 px-5 py-4 rounded-xl border border-border hover:bg-muted/50 transition-colors text-start"
          @click="goToTests"
        >
          <div class="w-10 h-10 rounded-lg bg-muted flex items-center justify-center shrink-0">
            <ClipboardList class="w-5 h-5 text-muted-foreground" />
          </div>
          <div>
            <p class="text-sm text-slate-800 text-slate-800">{{ t('campaignBuilder.successBackToTests') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">{{ t('campaignBuilder.successBackToTestsDesc') }}</p>
          </div>
          <ChevronRight class="w-4 h-4 text-muted-foreground ms-auto shrink-0" />
        </button>
      </div>
    </div>

    <!-- ── Page Header (hidden when success) ───────────────────────── -->
    <template v-else>

    <!-- ── Loading skeleton (edit mode fetching) ───────────────────── -->
    <div v-if="loadingCampaign" class="space-y-4">
      <div class="h-8 w-48 rounded-lg bg-muted animate-pulse" />
      <div class="h-64 rounded-xl border border-border bg-muted/30 animate-pulse" />
    </div>

    <template v-else>

    <!-- ── Page Header ─────────────────────────────────────────────── -->
    <div class="flex items-center gap-4">
      <button
        class="p-2 rounded-lg border border-border hover:bg-muted/50 transition-colors shrink-0"
        @click="router.back()"
      >
        <ArrowLeft class="w-4 h-4" />
      </button>
      <div>
        <p class="text-[11px] text-slate-800 uppercase tracking-widest text-muted-foreground mb-0.5">
          {{ t('campaignBuilder.subtitle') }}
        </p>
        <h1 class="text-2xl font-black tracking-tight text-slate-800">
          {{ isEdit ? t('campaignBuilder.titleEdit') : t('campaignBuilder.titleNew') }}
        </h1>
      </div>
      <!-- Quick save button -->
      <button
        class="ms-auto flex items-center gap-2 px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors shrink-0 disabled:opacity-50"
        :disabled="!form.name.trim()"
        @click="saveDialogOpen = true"
      >
        <Save class="w-4 h-4" />
        {{ t('common.save') }}
      </button>
    </div>

    <!-- ── Stepper ──────────────────────────────────────────────────── -->
    <div class="flex items-center">
      <template v-for="(step, i) in steps" :key="step.key">
        <button
          type="button"
          class="flex items-center gap-2.5 shrink-0 rounded-lg px-1 py-0.5 transition-colors hover:bg-muted/50 focus:outline-none"
          @click="currentStep = i"
        >
          <div
            class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-black transition-all"
            :class="
              currentStep > i
                ? 'bg-primary text-primary-foreground'
                : currentStep === i
                  ? 'bg-foreground text-background'
                  : 'bg-muted text-muted-foreground border border-border'
            "
          >
            <Check v-if="currentStep > i" class="w-4 h-4" />
            <span v-else>{{ i + 1 }}</span>
          </div>
          <div class="hidden sm:block text-start">
            <p
              class="text-xs text-slate-800 transition-colors"
              :class="
                currentStep === i
                  ? 'text-slate-800'
                  : currentStep > i
                    ? 'text-primary'
                    : 'text-muted-foreground'
              "
            >
              {{ step.label }}
            </p>
            <p class="text-[10px] text-muted-foreground">{{ step.sublabel }}</p>
          </div>
        </button>
        <div
          v-if="i < steps.length - 1"
          class="flex-1 h-px mx-4 transition-colors"
          :class="currentStep > i ? 'bg-primary' : 'bg-border'"
        />
      </template>
    </div>

    <!-- ── Content Card ─────────────────────────────────────────────── -->
    <div class="rounded-xl border border-border bg-card">

      <!-- ════════════════════════════════════════════════════════════
           STEP 1 — Campaign Info
           ════════════════════════════════════════════════════════════ -->
      <div v-if="currentStep === 0" class="p-8 space-y-8">

        <div class="space-y-1">
          <h2 class="text-lg font-black text-slate-800">{{ t('campaignBuilder.step1Title') }}</h2>
          <p class="text-sm text-muted-foreground">{{ t('campaignBuilder.step1Desc') }}</p>
        </div>

        <!-- Name -->
        <div class="space-y-2">
          <label class="text-sm font-semibold text-slate-800">
            {{ t('campaignBuilder.nameLabel') }} <span class="text-red-500">*</span>
          </label>
          <Input
            v-model="form.name"
            :placeholder="t('campaignBuilder.namePlaceholder')"
            class="h-10"
            :class="errors.name ? 'border-red-400 focus-visible:ring-red-400' : ''"
          />
          <p v-if="errors.name" class="text-xs text-red-500 flex items-center gap-1.5">
            <CircleAlert class="w-3.5 h-3.5" /> {{ errors.name }}
          </p>
        </div>

        <!-- Description -->
        <div class="space-y-2">
          <label class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.descLabel') }}</label>
          <textarea
            v-model="form.description"
            :placeholder="t('campaignBuilder.descPlaceholder')"
            rows="3"
            class="w-full rounded-md border border-input bg-transparent px-3 py-2.5 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring resize-none transition-colors"
          />
        </div>

        <!-- Theme / Category -->
        <div class="space-y-2">
          <label class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.themeLabel') }}</label>
          <button
            type="button"
            class="w-full flex items-center gap-2 h-10 px-3 rounded-md border border-input bg-background text-sm hover:bg-accent transition-colors text-start"
            @click="themePickerOpen = true"
          >
            <Folder class="w-4 h-4 text-muted-foreground shrink-0" />
            <span :class="selectedThemeName ? 'text-slate-800' : 'text-muted-foreground'" class="flex-1 truncate">
              {{ selectedThemeName || t('campaignBuilder.themeAllPlaceholder') }}
            </span>
            <X v-if="form.themeId" class="w-3.5 h-3.5 text-muted-foreground hover:text-slate-800 shrink-0" @click.stop="form.themeId = ''" />
            <ChevronDown v-else class="w-4 h-4 text-muted-foreground shrink-0" />
          </button>
          <p class="text-xs text-muted-foreground">{{ t('campaignBuilder.themeHint') }}</p>
        </div>

        <!-- Date range -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div class="space-y-2">
            <label class="text-sm font-semibold text-slate-800 flex items-center gap-1.5">
              <CalendarDays class="w-3.5 h-3.5 text-muted-foreground" />
              {{ t('campaignBuilder.startLabel') }} <span class="text-red-500">*</span>
            </label>
            <Input
              v-model="form.availableFrom"
              type="datetime-local"
              class="h-10"
              :class="errors.availableFrom ? 'border-red-400' : ''"
              :min="getNowLocal()"
              @input="validateStep(0)"
            />
            <p v-if="errors.availableFrom" class="text-xs text-red-500 flex items-center gap-1.5">
              <CircleAlert class="w-3.5 h-3.5" /> {{ errors.availableFrom }}
            </p>
          </div>
          <div class="space-y-2">
            <label class="text-sm font-semibold text-slate-800 flex items-center gap-1.5">
              <CalendarDays class="w-3.5 h-3.5 text-muted-foreground" />
              {{ t('campaignBuilder.endLabel') }} <span class="text-red-500">*</span>
            </label>
            <Input
              v-model="form.availableUntil"
              type="datetime-local"
              class="h-10"
              :class="errors.availableUntil ? 'border-red-400' : ''"
              :min="form.availableFrom || getNowLocal()"
              @input="validateStep(0)"
            />
            <p v-if="errors.availableUntil" class="text-xs text-red-500 flex items-center gap-1.5">
              <CircleAlert class="w-3.5 h-3.5" /> {{ errors.availableUntil }}
            </p>
          </div>
        </div>

        <!-- Duration badge -->
        <div
          v-if="campaignDuration"
          class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-muted/50 border border-border/60 w-fit"
        >
          <Clock class="w-3.5 h-3.5 text-muted-foreground" />
          <span class="text-xs text-muted-foreground font-medium">
            {{ t('campaignBuilder.windowLabel') }}: <span class="text-slate-800 text-slate-800">{{ campaignDuration }}</span>
          </span>
        </div>

        <div class="border-t border-border" />

        <!-- Invite settings -->
        <div class="space-y-4">
          <p class="text-sm text-slate-800 text-slate-800">{{ t('campaignBuilder.inviteSettings') }}</p>

          <!-- Send invite toggle -->
          <div class="flex items-center justify-between p-4 rounded-xl border border-border bg-muted/30">
            <div class="flex items-start gap-3">
              <div class="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0 mt-0.5">
                <Mail class="w-4 h-4 text-primary" />
              </div>
              <div>
                <p class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.sendInviteLabel') }}</p>
                <p class="text-xs text-muted-foreground mt-0.5">
                  {{ t('campaignBuilder.sendInviteDesc') }}
                </p>
              </div>
            </div>
            <ToggleSwitch v-model="form.sendInviteEmail" />
          </div>

          <!-- Reminder -->
          <div
            v-if="form.sendInviteEmail"
            class="flex items-center justify-between p-4 rounded-xl border border-border bg-muted/30"
          >
            <div class="flex items-start gap-3">
              <div class="w-9 h-9 rounded-lg bg-amber-100 dark:bg-amber-900/30 flex items-center justify-center shrink-0 mt-0.5">
                <BellRing class="w-4 h-4 text-amber-600 dark:text-amber-400" />
              </div>
              <div>
                <p class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.reminderLabel') }}</p>
                <p class="text-xs text-muted-foreground mt-0.5">
                  {{ t('campaignBuilder.reminderDesc') }}
                </p>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <Input
                v-model.number="form.reminderMinutesBeforeStart"
                type="number"
                min="1"
                placeholder="15"
                class="h-8 w-20 text-center"
                :class="errors.reminderMinutesBeforeStart ? 'border-red-400' : ''"
                @input="validateStep(0)"
              />
              <span class="text-xs text-muted-foreground font-medium shrink-0">{{ t('campaignBuilder.reminderUnit') }}</span>
            </div>
          </div>
          <p v-if="errors.reminderMinutesBeforeStart" class="text-xs text-red-500 flex items-center gap-1.5 mt-1 px-4">
            <CircleAlert class="w-3.5 h-3.5" /> {{ errors.reminderMinutesBeforeStart }}
          </p>
        </div>
      </div>

      <!-- ════════════════════════════════════════════════════════════
           STEP 2 — Questionnaire & Scoring
           ════════════════════════════════════════════════════════════ -->
      <div v-else-if="currentStep === 1" class="p-8 space-y-8">

        <div class="space-y-1">
          <h2 class="text-lg font-black text-slate-800">{{ t('campaignBuilder.step2Title') }}</h2>
          <p class="text-sm text-muted-foreground">{{ t('campaignBuilder.step2Desc') }}</p>
        </div>

        <!-- Company context card -->
        <div v-if="company" class="flex items-center gap-3 p-3 rounded-lg bg-muted/40 border border-border">
          <div class="h-9 w-9 rounded-lg bg-background border border-border flex items-center justify-center shrink-0 overflow-hidden">
            <img v-if="company.logo" :src="company.logo" :alt="company.name" class="h-full w-full object-cover" />
            <Building2 v-else class="w-4 h-4 text-muted-foreground" />
          </div>
          <div class="min-w-0">
            <p class="text-xs  text-slate-800 truncate">{{ company.name }}</p>
            <p v-if="company.email" class="text-[10px] text-muted-foreground truncate">{{ company.email }}</p>
          </div>
          <span class="ms-auto text-[10px] text-slate-800 px-2 py-0.5 rounded-full bg-primary/10 text-primary shrink-0">
            {{ t('campaignBuilder.companyContext') }}
          </span>
        </div>

        <!-- Create-questionnaire quick dialog -->
        <Dialog v-model:open="createQDialog">
          <DialogContent class="sm:max-w-md">
            <DialogHeader>
              <DialogTitle>{{ t('campaignBuilder.createQTitle') }}</DialogTitle>
              <DialogDescription>{{ t('campaignBuilder.createQDesc') }}</DialogDescription>
            </DialogHeader>
            <div class="space-y-3 py-2">
              <div class="space-y-1.5">
                <label class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.createQNameLabel') }}</label>
                <Input
                  v-model="newQTitle"
                  :placeholder="t('campaignBuilder.createQNamePlaceholder')"
                  autofocus
                  @keydown.enter="submitCreateQ"
                />
                <p v-if="createQError" class="text-xs text-red-500 flex items-center gap-1.5">
                  <CircleAlert class="w-3.5 h-3.5" /> {{ createQError }}
                </p>
              </div>
              <div class="space-y-1.5">
                <label class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.createQDescLabel') }}</label>
                <Input v-model="newQDescription" :placeholder="t('campaignBuilder.createQDescPlaceholder')" />
              </div>
            </div>
            <DialogFooter>
              <button
                class="px-4 py-2 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors"
                @click="createQDialog = false"
              >
                {{ t('common.cancel') }}
              </button>
              <button
                class="flex items-center gap-2 px-4 py-2 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors disabled:opacity-50"
                :disabled="creatingQ"
                @click="submitCreateQ"
              >
                <Loader2 v-if="creatingQ" class="w-3.5 h-3.5 animate-spin" />
                {{ creatingQ ? t('campaignBuilder.createQSaving') : t('campaignBuilder.createQSave') }}
              </button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        <!-- Multi-questionnaire manager -->
        <div class="space-y-3">
          <div class="flex items-center justify-between">
            <label class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.questionnairesLabel') }}</label>
            <span class="text-xs text-muted-foreground">{{ campaignQuestionnaires.length }} {{ t('campaignBuilder.questionnairesUnit') }}</span>
          </div>

          <!-- Attached questionnaires list -->
          <div v-if="campaignQuestionnaires.length > 0" class="space-y-2">
            <div
              v-for="(cq, index) in campaignQuestionnaires"
              :key="cq.questionnaireId"
              class="flex items-center gap-2 p-3 rounded-xl border border-border bg-background"
            >
              <span class="text-xs font-black text-muted-foreground w-5 text-center shrink-0">{{ index + 1 }}</span>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-semibold text-slate-800 truncate">{{ cq.questionnaireTitle }}</p>
                <p v-if="cq.label" class="text-xs text-muted-foreground truncate">{{ cq.label }}</p>
              </div>
              <span class="text-[10px] px-1.5 py-0.5 rounded-md text-slate-800 shrink-0" :class="cq.isRequired ? 'bg-primary/10 text-primary' : 'bg-muted text-muted-foreground'">
                {{ cq.isRequired ? t('campaignBuilder.qRequired') : t('campaignBuilder.qOptional') }}
              </span>
              <button
                v-if="index > 0"
                class="h-7 w-7 flex items-center justify-center rounded-lg hover:bg-muted/50 transition-colors shrink-0"
                @click="moveQuestionnaire(index, 'up')"
              >
                <ChevronUp class="w-4 h-4 text-muted-foreground" />
              </button>
              <button
                v-if="index < campaignQuestionnaires.length - 1"
                class="h-7 w-7 flex items-center justify-center rounded-lg hover:bg-muted/50 transition-colors shrink-0"
                @click="moveQuestionnaire(index, 'down')"
              >
                <ChevronDown class="w-4 h-4 text-muted-foreground" />
              </button>
              <button
                class="h-7 w-7 flex items-center justify-center rounded-lg hover:bg-red-50 dark:hover:bg-red-950/20 text-muted-foreground hover:text-red-500 transition-colors shrink-0"
                @click="removeAttachedQuestionnaire(index)"
              >
                <X class="w-4 h-4" />
              </button>
            </div>
          </div>

          <!-- Warning if none attached -->
          <div v-if="campaignQuestionnaires.length === 0" class="flex items-center gap-2 px-3 py-2 rounded-lg bg-amber-50 dark:bg-amber-950/30 text-amber-700 dark:text-amber-400 text-xs font-medium border border-amber-200 dark:border-amber-900">
            <CircleAlert class="w-3.5 h-3.5 shrink-0" />
            {{ t('campaignBuilder.noQuestionnairesWarning') }}
          </div>

          <!-- Add questionnaire row -->
          <div class="flex gap-2">
            <Select v-model="addQId" class="flex-1">
              <SelectTrigger class="h-10">
                <SelectValue :placeholder="questionnairesLoading ? t('common.loading') : t('campaignBuilder.questionnairePlaceholder')" />
              </SelectTrigger>
              <SelectContent>
                <div v-if="questionnairesLoading" class="py-3 px-3 text-sm text-muted-foreground flex items-center gap-2">
                  <Loader2 class="w-3.5 h-3.5 animate-spin" /> {{ t('common.loading') }}
                </div>
                <div v-else-if="availableQuestionnaires.length === 0" class="py-3 px-3 text-sm text-muted-foreground">
                  {{ t('campaignBuilder.questionnaireEmpty') }}
                </div>
                <SelectItem v-for="q in availableQuestionnaires" :key="q.id" :value="String(q.id)">
                  {{ q.title }}
                </SelectItem>
              </SelectContent>
            </Select>
            <Input v-model="addQLabel" :placeholder="t('campaignBuilder.qLabelPlaceholder')" class="h-10 w-36" />
            <button
              class="h-10 px-4 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors disabled:opacity-50 shrink-0"
              :disabled="!addQId"
              @click="appendQuestionnaire"
            >
              {{ t('common.add') }}
            </button>
            <button
              class="h-10 w-10 flex items-center justify-center rounded-lg border border-border hover:bg-muted/50 transition-colors shrink-0"
              @click="loadQuestionnaires"
            >
              <RefreshCcw class="w-4 h-4 text-muted-foreground" :class="questionnairesLoading ? 'animate-spin' : ''" />
            </button>
          </div>

          <p class="text-xs text-muted-foreground">
            {{ t('campaignBuilder.noQuestionnaire') }}
            <button class="text-primary font-semibold hover:underline" @click.prevent="createQDialog = true">
              {{ t('campaignBuilder.createQuestionnaire') }}
            </button>
          </p>
        </div>

        <div class="border-t border-border" />

        <!-- Passing score -->
        <div class="space-y-3">
          <div class="flex items-center justify-between">
            <label class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.passingScoreLabel') }}</label>
            <div class="flex items-center gap-2">
              <Input
                v-model.number="form.passingScorePercent"
                type="number"
                min="0"
                max="100"
                class="h-8 w-20 text-center text-slate-800"
              />
              <span class="text-sm text-muted-foreground font-medium">%</span>
            </div>
          </div>
          <input
            v-model.number="form.passingScorePercent"
            type="range"
            min="0"
            max="100"
            step="5"
            class="w-full h-2 rounded-full accent-primary cursor-pointer"
          />
          <div class="flex justify-between text-[10px] text-muted-foreground font-medium">
            <span>0%</span><span>25%</span><span>50%</span><span>75%</span><span>100%</span>
          </div>
          <div
            class="flex items-center gap-2 px-3 py-2 rounded-lg text-xs font-medium border"
            :class="
              form.passingScorePercent >= 70
                ? 'bg-emerald-50 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-400 border-emerald-200 dark:border-emerald-900'
                : form.passingScorePercent >= 50
                  ? 'bg-amber-50 dark:bg-amber-950/30 text-amber-700 dark:text-amber-400 border-amber-200 dark:border-amber-900'
                  : 'bg-red-50 dark:bg-red-950/30 text-red-700 dark:text-red-400 border-red-200 dark:border-red-900'
            "
          >
            {{ t('campaignBuilder.passingScoreReq', { percent: form.passingScorePercent }) }} —
            {{
              form.passingScorePercent >= 70 ? t('campaignBuilder.passingStrict') :
              form.passingScorePercent >= 50 ? t('campaignBuilder.passingModerate') :
              t('campaignBuilder.passingLenient')
            }}
          </div>
        </div>

        <!-- Scoring mode -->
        <div class="space-y-2">
          <label class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.scoringModeLabel') }}</label>
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <button
              v-for="mode in scoringModes"
              :key="mode.value"
              class="text-start p-3.5 rounded-xl border transition-all"
              :class="
                form.scoringMode === mode.value
                  ? 'border-primary bg-primary/5 ring-1 ring-primary'
                  : 'border-border hover:border-primary/30 hover:bg-muted/30'
              "
              @click="form.scoringMode = mode.value"
            >
              <p class="text-sm text-slate-800">{{ mode.label }}</p>
              <p class="text-[11px] text-muted-foreground mt-0.5">{{ mode.desc }}</p>
            </button>
          </div>
        </div>

        <!-- Allow partial score -->
        <div class="flex items-center justify-between p-4 rounded-xl border border-border bg-muted/30">
          <div>
            <p class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.partialScoreLabel') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">
              {{ t('campaignBuilder.partialScoreDesc') }}
            </p>
          </div>
          <ToggleSwitch v-model="form.allowPartialScore" />
        </div>

        <div class="border-t border-border" />

        <!-- Attempt settings -->
        <div class="space-y-4">
          <p class="text-sm  text-slate-800">{{ t('campaignBuilder.attemptSettings') }}</p>

          <!-- Max attempts -->
          <div class="flex items-center justify-between p-4 rounded-xl border border-border bg-muted/30">
            <div class="flex items-start gap-3">
              <div class="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
                <RefreshCcw class="w-4 h-4 text-primary" />
              </div>
              <div>
                <p class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.maxAttemptsLabel') }}</p>
                <p class="text-xs text-muted-foreground mt-0.5">
                  {{ t('campaignBuilder.maxAttemptsDesc') }}
                </p>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <button
                class="w-8 h-8 rounded-lg border border-border hover:bg-muted flex items-center justify-center text-base text-slate-800 transition-colors disabled:opacity-40"
                :disabled="form.maxAttempts <= 1"
                @click="form.maxAttempts = Math.max(1, form.maxAttempts - 1)"
              >−</button>
              <span class="w-8 text-center text-sm font-black text-slate-800 tabular-nums">
                {{ form.maxAttempts }}
              </span>
              <button
                class="w-8 h-8 rounded-lg border border-border hover:bg-muted flex items-center justify-center text-base text-slate-800 transition-colors disabled:opacity-40"
                :disabled="form.maxAttempts >= 10"
                @click="form.maxAttempts = Math.min(10, form.maxAttempts + 1)"
              >+</button>
            </div>
          </div>

          <!-- Cooldown (only if multiple attempts) -->
          <div
            v-if="form.maxAttempts > 1"
            class="flex items-center justify-between p-4 rounded-xl border border-border bg-muted/30"
          >
            <div class="flex items-start gap-3">
              <div class="w-9 h-9 rounded-lg bg-sky-100 dark:bg-sky-900/30 flex items-center justify-center shrink-0">
                <Timer class="w-4 h-4 text-sky-600 dark:text-sky-400" />
              </div>
              <div>
                <p class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.cooldownLabel') }}</p>
                <p class="text-xs text-muted-foreground mt-0.5">
                  {{ t('campaignBuilder.cooldownDesc') }}
                </p>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <Input
                v-model.number="form.cooldownBetweenAttemptsMinutes"
                type="number"
                min="0"
                placeholder="—"
                class="h-8 w-20 text-center"
              />
              <span class="text-xs text-muted-foreground shrink-0">{{ t('campaignBuilder.cooldownUnit') }}</span>
            </div>
          </div>

          <!-- Global duration -->
          <div class="flex items-center justify-between p-4 rounded-xl border border-border bg-muted/30">
            <div class="flex items-start gap-3">
              <div class="w-9 h-9 rounded-lg bg-amber-100 dark:bg-amber-900/30 flex items-center justify-center shrink-0">
                <Hourglass class="w-4 h-4 text-amber-600 dark:text-amber-400" />
              </div>
              <div>
                <p class="text-sm font-semibold text-slate-800">
                  {{ t('campaignBuilder.timeLimitLabel') }}
                  <span class="text-muted-foreground font-normal ms-1">({{ t('campaignBuilder.timeLimitOptional') }})</span>
                </p>
                <p class="text-xs text-muted-foreground mt-0.5">
                  {{ t('campaignBuilder.timeLimitDesc') }}
                </p>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <Input
                v-model.number="form.globalDurationMinutes"
                type="number"
                min="1"
                placeholder="—"
                class="h-8 w-20 text-center"
              />
              <span class="text-xs text-muted-foreground shrink-0">{{ t('campaignBuilder.timeLimitUnit') }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- ════════════════════════════════════════════════════════════
           STEP 3 — Candidate Experience
           ════════════════════════════════════════════════════════════ -->
      <div v-else-if="currentStep === 2" class="p-8 space-y-8">

        <div class="space-y-1">
          <h2 class="text-lg font-black text-slate-800">{{ t('campaignBuilder.step3Title') }}</h2>
          <p class="text-sm text-muted-foreground">
            {{ t('campaignBuilder.step3Desc') }}
          </p>
        </div>

        <!-- Navigation & Randomization -->
        <div class="space-y-3">
          <p class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">
            {{ t('campaignBuilder.navRandSection') }}
          </p>
          <div class="divide-y divide-border rounded-xl border border-border overflow-hidden">
            <ExperienceToggle
              v-model="form.showTimer"
              icon="clock"
              :label="t('campaignBuilder.showTimerLabel')"
              :desc="t('campaignBuilder.showTimerDesc')"
            />
            <ExperienceToggle
              v-model="form.allowNavigationBack"
              icon="arrow-left-right"
              :label="t('campaignBuilder.allowBackLabel')"
              :desc="t('campaignBuilder.allowBackDesc')"
            />
            <ExperienceToggle
              v-model="form.randomizeQuestions"
              icon="shuffle"
              :label="t('campaignBuilder.randomizeQLabel')"
              :desc="t('campaignBuilder.randomizeQDesc')"
            />
            <ExperienceToggle
              v-model="form.randomizeChoices"
              icon="list-ordered"
              :label="t('campaignBuilder.randomizeCLabel')"
              :desc="t('campaignBuilder.randomizeCDesc')"
            />
          </div>
        </div>

        <!-- Results & Feedback -->
        <div class="space-y-3">
          <p class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">
            {{ t('campaignBuilder.resultsFeedbackSection') }}
          </p>
          <div class="divide-y divide-border rounded-xl border border-border overflow-hidden">
            <ExperienceToggle
              v-model="form.showResultsImmediately"
              icon="check-circle"
              :label="t('campaignBuilder.showResultsLabel')"
              :desc="t('campaignBuilder.showResultsDesc')"
            />
            <ExperienceToggle
              v-model="form.showQuestionScore"
              icon="star"
              :label="t('campaignBuilder.showQPointsLabel')"
              :desc="t('campaignBuilder.showQPointsDesc')"
            />
            <ExperienceToggle
              v-model="form.showCorrectAfterEach"
              icon="circle-check-big"
              :label="t('campaignBuilder.revealCorrectLabel')"
              :desc="t('campaignBuilder.revealCorrectDesc')"
            />
          </div>
        </div>

        <!-- Anti-cheat -->
        <div class="space-y-3">
          <p class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('campaignBuilder.antiCheatSection') }}</p>
          <div class="rounded-xl border border-border overflow-hidden">
            <!-- Block tab switch toggle -->
            <div class="flex items-center justify-between p-4 bg-muted/30">
              <div class="flex items-start gap-3">
                <div
                  class="w-9 h-9 rounded-lg flex items-center justify-center shrink-0"
                  :class="blockTabSwitch ? 'bg-red-100 dark:bg-red-900/30' : 'bg-muted'"
                >
                  <ShieldAlert
                    class="w-4 h-4"
                    :class="blockTabSwitch ? 'text-red-600 dark:text-red-400' : 'text-muted-foreground'"
                  />
                </div>
                <div>
                  <p class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.blockTabLabel') }}</p>
                  <p class="text-xs text-muted-foreground mt-0.5">
                    {{ t('campaignBuilder.blockTabDesc') }}
                  </p>
                </div>
              </div>
              <ToggleSwitch v-model="blockTabSwitch" />
            </div>

            <!-- Sub-config: max switches -->
            <div v-if="blockTabSwitch" class="p-4 border-t border-border bg-red-50/40 dark:bg-red-950/10">
              <div class="flex items-center justify-between mb-3">
                <div>
                  <p class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.maxViolationsLabel') }}</p>
                  <p class="text-xs text-muted-foreground mt-0.5">
                    {{ t('campaignBuilder.maxViolationsDesc') }}
                  </p>
                </div>
                <div class="flex items-center gap-2">
                  <button
                    class="w-8 h-8 rounded-lg border border-border hover:bg-muted flex items-center justify-center text-base text-slate-800 transition-colors disabled:opacity-40"
                    :disabled="(form.tabSwitchMaxCount ?? 0) <= 0"
                    @click="form.tabSwitchMaxCount = Math.max(0, (form.tabSwitchMaxCount ?? 0) - 1)"
                  >−</button>
                  <span class="w-8 text-center text-sm font-black text-slate-800 tabular-nums">
                    {{ form.tabSwitchMaxCount ?? 0 }}
                  </span>
                  <button
                    class="w-8 h-8 rounded-lg border border-border hover:bg-muted flex items-center justify-center text-base text-slate-800 transition-colors disabled:opacity-40"
                    :disabled="(form.tabSwitchMaxCount ?? 0) >= 20"
                    @click="form.tabSwitchMaxCount = Math.min(20, (form.tabSwitchMaxCount ?? 0) + 1)"
                  >+</button>
                </div>
              </div>
              <div class="flex items-center gap-2 text-xs text-red-600 dark:text-red-400 font-medium">
                <ShieldAlert class="w-3.5 h-3.5 shrink-0" />
                <span>
                  {{
                    (form.tabSwitchMaxCount ?? 0) === 0
                      ? t('campaignBuilder.zeroTolerance')
                      : t('campaignBuilder.violationsWarning', { count: form.tabSwitchMaxCount ?? 0 })
                  }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ════════════════════════════════════════════════════════════
           STEP 4 — Review & Publish
           ════════════════════════════════════════════════════════════ -->
      <div v-else-if="currentStep === 3" class="p-8 space-y-8">

        <div class="space-y-1">
          <h2 class="text-lg font-black text-slate-800">{{ t('campaignBuilder.step4Title') }}</h2>
          <p class="text-sm text-muted-foreground">
            {{ t('campaignBuilder.step4Desc') }}
          </p>
        </div>

        <div class="space-y-4">

          <!-- Campaign Info -->
          <ReviewSection :title="t('campaignBuilder.reviewInfoSection')" icon="clipboard">
            <ReviewRow :label="t('campaignBuilder.rowName')"        :value="form.name || '—'" />
            <ReviewRow :label="t('campaignBuilder.rowDesc')"        :value="form.description || t('campaignBuilder.rowNone')" />
            <ReviewRow :label="t('campaignBuilder.rowStarts')"      :value="formatDateTime(form.availableFrom)" />
            <ReviewRow :label="t('campaignBuilder.rowEnds')"        :value="formatDateTime(form.availableUntil)" />
            <ReviewRow :label="t('campaignBuilder.rowWindow')"      :value="campaignDuration ?? '—'" />
            <ReviewRow :label="t('campaignBuilder.rowInviteEmail')" :value="form.sendInviteEmail ? t('campaignBuilder.rowYes') : t('campaignBuilder.rowNo')" :highlight="form.sendInviteEmail" />
            <ReviewRow
              v-if="form.sendInviteEmail && form.reminderMinutesBeforeStart"
              :label="t('campaignBuilder.rowReminder')"
              :value="`${form.reminderMinutesBeforeStart} ${t('campaignBuilder.reminderUnit')}`"
            />
          </ReviewSection>

          <!-- Scoring -->
          <ReviewSection :title="t('campaignBuilder.reviewScoringSection')" icon="chart">
            <ReviewRow :label="t('campaignBuilder.rowQuestionnaire')" :value="campaignQuestionnaires.length > 0 ? `${campaignQuestionnaires.length} questionnaire(s)` : '—'" />
            <ReviewRow :label="t('campaignBuilder.rowPassingScore')"   :value="`${form.passingScorePercent}%`" />
            <ReviewRow :label="t('campaignBuilder.rowScoringMode')"    :value="scoringModes.find(m => m.value === form.scoringMode)?.label ?? '—'" />
            <ReviewRow :label="t('campaignBuilder.rowPartialScore')"   :value="form.allowPartialScore ? t('campaignBuilder.rowEnabled') : t('campaignBuilder.rowDisabled')" />
            <ReviewRow :label="t('campaignBuilder.rowMaxAttempts')"    :value="String(form.maxAttempts)" />
            <ReviewRow
              v-if="form.maxAttempts > 1 && form.cooldownBetweenAttemptsMinutes"
              :label="t('campaignBuilder.rowCooldown')"
              :value="`${form.cooldownBetweenAttemptsMinutes} min`"
            />
            <ReviewRow :label="t('campaignBuilder.rowTimeLimit')" :value="form.globalDurationMinutes ? `${form.globalDurationMinutes} min` : t('campaignBuilder.rowNoLimit')" />
          </ReviewSection>

          <!-- Experience -->
          <ReviewSection :title="t('campaignBuilder.reviewExperienceSection')" icon="users">
            <ReviewRow :label="t('campaignBuilder.rowTimer')"            :value="form.showTimer ? t('campaignBuilder.rowVisible') : t('campaignBuilder.rowHidden')" />
            <ReviewRow :label="t('campaignBuilder.rowBackNav')"          :value="form.allowNavigationBack ? t('campaignBuilder.rowAllowed') : t('campaignBuilder.rowDisabled')" />
            <ReviewRow :label="t('campaignBuilder.rowRandQ')"            :value="form.randomizeQuestions ? t('campaignBuilder.rowYes') : t('campaignBuilder.rowNo')" />
            <ReviewRow :label="t('campaignBuilder.rowRandC')"            :value="form.randomizeChoices ? t('campaignBuilder.rowYes') : t('campaignBuilder.rowNo')" />
            <ReviewRow :label="t('campaignBuilder.rowShowResults')"      :value="form.showResultsImmediately ? t('campaignBuilder.rowImmediately') : t('campaignBuilder.rowManualRelease')" />
            <ReviewRow :label="t('campaignBuilder.rowQPoints')"          :value="form.showQuestionScore ? t('campaignBuilder.rowShown') : t('campaignBuilder.rowHidden')" />
            <ReviewRow :label="t('campaignBuilder.rowCorrectAnswers')"   :value="form.showCorrectAfterEach ? t('campaignBuilder.rowAfterEach') : t('campaignBuilder.rowNotShown')" />
            <ReviewRow
              :label="t('campaignBuilder.rowTabSwitch')"
              :value="blockTabSwitch ? `${t('common.active')} (${t('campaignBuilder.violationsWarning', { count: form.tabSwitchMaxCount ?? 0 })})` : t('campaignBuilder.rowAllowed')"
              :danger="blockTabSwitch"
            />
          </ReviewSection>

          <!-- Save as -->
          <div class="space-y-3">
            <p class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('campaignBuilder.saveAsLabel') }}</p>
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
              <button
                v-for="opt in publishOptions"
                :key="opt.value"
                class="text-start p-4 rounded-xl border transition-all"
                :class="
                  selectedStatus === opt.value
                    ? 'border-primary bg-primary/5 ring-1 ring-primary'
                    : 'border-border hover:border-primary/30 hover:bg-muted/30'
                "
                @click="selectedStatus = opt.value"
              >
                <div class="flex items-center gap-2 mb-1.5">
                  <component :is="opt.icon" class="w-4 h-4" :class="opt.iconClass" />
                  <p class="text-sm text-slate-800">{{ opt.label }}</p>
                </div>
                <p class="text-[11px] text-muted-foreground leading-relaxed">{{ opt.desc }}</p>
              </button>
            </div>
          </div>

        </div>
      </div>

      <!-- ════════════════════════════════════════════════════════════
           STEP 5 — Candidates (edit mode only)
           ════════════════════════════════════════════════════════════ -->
      <div v-else-if="currentStep === 4" class="p-8 space-y-6">

        <div class="flex items-center justify-between">
          <div class="space-y-1">
            <h2 class="text-lg font-black text-slate-800">{{ t('campaignBuilder.candidatesStep') }}</h2>
            <p class="text-sm text-muted-foreground">{{ t('campaignBuilder.candidatesStepSub') }}</p>
          </div>
          <button
            class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors"
            @click="isInviteOpen = true"
          >
            <UserPlus class="w-4 h-4" />
            {{ t('campaignBuilder.inviteBtn') }}
          </button>
        </div>

        <!-- Error -->
        <div v-if="candidatesError" class="flex items-center gap-2 px-4 py-3 rounded-lg bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 text-sm text-red-700 dark:text-red-400">
          <CircleAlert class="w-4 h-4 shrink-0" />
          {{ t('common.errorOccurred') }}
          <button class="ms-auto text-xs text-slate-800 underline" @click="loadCandidates">{{ t('common.retry') }}</button>
        </div>

        <!-- Loading skeletons -->
        <div v-if="loadingCandidates" class="space-y-3">
          <div v-for="i in 5" :key="i" class="flex items-center gap-3 p-3 rounded-xl border border-border">
            <div class="w-9 h-9 rounded-full bg-muted animate-pulse shrink-0" />
            <div class="flex-1 space-y-1.5">
              <div class="h-3.5 w-1/3 rounded bg-muted animate-pulse" />
              <div class="h-3 w-1/2 rounded bg-muted animate-pulse" />
            </div>
            <div class="h-5 w-16 rounded-full bg-muted animate-pulse" />
          </div>
        </div>

        <!-- Empty state -->
        <div v-else-if="candidates.length === 0 && !loadingCandidates && !candidatesError" class="flex flex-col items-center justify-center py-12 gap-4 text-center">
          <div class="w-14 h-14 rounded-2xl bg-muted flex items-center justify-center">
            <UserX class="w-7 h-7 text-muted-foreground" />
          </div>
          <p class=" text-slate-800">{{ t('campaignBuilder.candidatesEmpty') }}</p>
          <p class="text-sm text-muted-foreground">{{ t('campaignBuilder.candidatesEmptyHint') }}</p>
        </div>

        <!-- Candidates list -->
        <div v-else-if="!loadingCandidates" class="space-y-2">
          <div
            v-for="c in candidates"
            :key="c.campaignCandidateId"
            class="flex items-center gap-3 px-4 py-3 rounded-xl border border-border bg-secondary/50 hover:border-border/80 transition-colors group"
          >
            <div class="w-9 h-9 rounded-full bg-primary/10 text-primary flex items-center justify-center text-xs text-slate-800 shrink-0 overflow-hidden">
              <img v-if="c.candidateImageUrl" :src="c.candidateImageUrl" :alt="c.candidateName" class="w-full h-full object-cover" />
              <span v-else>{{ candidateInitials(c.candidateName) }}</span>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-slate-800 truncate">{{ c.candidateName }}</p>
              <p class="text-xs text-muted-foreground truncate">{{ c.candidateEmail }}</p>
            </div>
            <span :class="candidateStatusBadge(c.status)" class="text-[10px] text-slate-800 uppercase tracking-wide px-2 py-0.5 rounded-full shrink-0">
              {{ c.status }}
            </span>
            <button
              v-if="c.status === 'Invited' || c.status === 'Expired'"
              class="p-1.5 rounded-lg text-muted-foreground hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-950/30 transition-colors opacity-0 group-hover:opacity-100 focus:opacity-100 shrink-0"
              @click="askRemoveCandidate(c)"
            >
              <X class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>

        <!-- Invite dialog -->
        <InviteCandidatesDialog
          v-if="editCampaignId"
          v-model:open="isInviteOpen"
          :campaign-id="editCampaignId"
          :already-invited="alreadyInvitedIds"
          @invited="loadCandidates"
        />

        <!-- Remove confirm dialog -->
        <AlertDialog v-model:open="isRemoveConfOpen">
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>{{ t('campaignCandidates.removeConfirmTitle') }}</AlertDialogTitle>
              <AlertDialogDescription>{{ t('campaignCandidates.removeConfirmDesc') }}</AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>{{ t('common.cancel') }}</AlertDialogCancel>
              <AlertDialogAction class="bg-red-600 hover:bg-red-700 text-white border-none" @click="confirmRemoveCandidate">
                {{ t('campaignCandidates.removeBtn') }}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>

      </div>

    </div>

    <!-- ── Navigation Buttons ───────────────────────────────────────── -->
    <div class="flex items-center justify-between">
      <button
        v-if="currentStep > 0"
        class="flex items-center gap-2 px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors"
        @click="currentStep--"
      >
        <ChevronLeft class="w-4 h-4" /> {{ t('campaignBuilder.backBtn') }}
      </button>
      <div v-else />

      <div class="flex items-center gap-3">
        <!-- Save draft shortcut (except on last step where it's embedded) -->
        <button
          v-if="currentStep < steps.length - 1"
          class="px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors"
          @click="saveDraft"
        >
          {{ t('campaignBuilder.saveDraftBtn') }}
        </button>

        <!-- Continue -->
        <button
          v-if="currentStep < steps.length - 1"
          class="flex items-center gap-2 px-5 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors"
          @click="nextStep"
        >
          {{ t('campaignBuilder.continueBtn') }} <ChevronRight class="w-4 h-4" />
        </button>

        <!-- Final submit -->
        <button
          v-else
          class="flex items-center gap-2 px-6 py-2.5 rounded-lg text-sm font-semibold transition-colors disabled:opacity-60"
          :class="
            selectedStatus === 'Active'
              ? 'bg-emerald-600 text-white hover:bg-emerald-700'
              : selectedStatus === 'Scheduled'
                ? 'bg-sky-600 text-white hover:bg-sky-700'
                : selectedStatus === 'Closed'
                  ? 'bg-rose-600 text-white hover:bg-rose-700'
                  : 'bg-foreground text-background hover:bg-foreground/85'
          "
          :disabled="isSaving"
          @click="submit"
        >
          <component :is="publishOptions.find(o => o.value === selectedStatus)?.icon" class="w-4 h-4" />
          {{ isSaving ? t('campaignBuilder.savingBtn') : publishOptions.find(o => o.value === selectedStatus)?.label ?? t('common.save') }}
        </button>
      </div>
    </div>

    </template><!-- end v-else loadingCampaign -->
    </template><!-- end v-else isSuccess -->

  <!-- ── Save / Publish Dialog ────────────────────────────────────── -->
  <Dialog v-model:open="saveDialogOpen">
    <DialogContent class="sm:max-w-lg">
      <DialogHeader>
        <DialogTitle class="text-base text-slate-800">{{ t('campaignBuilder.saveDialogTitle') }}</DialogTitle>
        <DialogDescription class="text-xs">{{ t('campaignBuilder.saveDialogDesc') }}</DialogDescription>
      </DialogHeader>

      <div class="space-y-5 py-1">

        <!-- Status options -->
        <div class="space-y-2">
          <p class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('campaignBuilder.saveAsLabel') }}</p>
          <div class="grid grid-cols-2 gap-2">
            <button
              v-for="opt in publishOptions"
              :key="opt.value"
              type="button"
              class="text-start p-3 rounded-xl border transition-all"
              :class="
                selectedStatus === opt.value
                  ? 'border-primary bg-primary/5 ring-1 ring-primary'
                  : 'border-border hover:border-primary/30 hover:bg-muted/30'
              "
              @click="selectedStatus = opt.value"
            >
              <div class="flex items-center gap-2 mb-0.5">
                <component :is="opt.icon" class="w-3.5 h-3.5" :class="opt.iconClass" />
                <p class="text-sm  text-slate-800">{{ opt.label }}</p>
              </div>
              <p class="text-[11px] text-muted-foreground leading-snug">{{ opt.desc }}</p>
            </button>
          </div>
        </div>

        <div class="border-t border-border" />

        <!-- Invite settings -->
        <div class="space-y-3">
          <p class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('campaignBuilder.inviteSettings') }}</p>

          <!-- Send invite email -->
          <div class="flex items-center justify-between p-3.5 rounded-xl border border-border bg-muted/30">
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
                <Mail class="w-4 h-4 text-primary" />
              </div>
              <div>
                <p class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.sendInviteLabel') }}</p>
                <p class="text-xs text-muted-foreground">{{ t('campaignBuilder.sendInviteDesc') }}</p>
              </div>
            </div>
            <ToggleSwitch v-model="form.sendInviteEmail" />
          </div>

          <!-- Reminder hours -->
          <div
            v-if="form.sendInviteEmail"
            class="flex items-center justify-between p-3.5 rounded-xl border border-border bg-muted/30"
          >
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg bg-amber-100 dark:bg-amber-900/30 flex items-center justify-center shrink-0">
                <BellRing class="w-4 h-4 text-amber-600 dark:text-amber-400" />
              </div>
              <div>
                <p class="text-sm font-semibold text-slate-800">{{ t('campaignBuilder.reminderLabel') }}</p>
                <p class="text-xs text-muted-foreground">{{ t('campaignBuilder.reminderDesc') }}</p>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <Input
                v-model.number="form.reminderMinutesBeforeStart"
                type="number"
                min="1"
                placeholder="15"
                class="h-8 w-20 text-center"
                @input="validateStep(0)"
              />
              <span class="text-xs text-muted-foreground font-medium shrink-0">{{ t('campaignBuilder.reminderUnit') }}</span>
            </div>
          </div>
          <p v-if="errors.reminderMinutesBeforeStart" class="text-xs text-red-500 flex items-center gap-1.5 mt-1">
            <CircleAlert class="w-3.5 h-3.5" /> {{ errors.reminderMinutesBeforeStart }}
          </p>
        </div>
      </div>

      <DialogFooter class="gap-2">
        <button
          class="px-4 py-2 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors"
          @click="saveDialogOpen = false"
        >
          {{ t('common.cancel') }}
        </button>
        <button
          class="flex items-center gap-2 px-5 py-2 rounded-lg text-sm font-semibold transition-colors disabled:opacity-60"
          :class="
            selectedStatus === 'Active'
              ? 'bg-emerald-600 text-white hover:bg-emerald-700'
              : selectedStatus === 'Scheduled'
                ? 'bg-sky-600 text-white hover:bg-sky-700'
                : selectedStatus === 'Closed'
                  ? 'bg-rose-600 text-white hover:bg-rose-700'
                  : 'bg-foreground text-background hover:bg-foreground/85'
          "
          :disabled="isSaving"
          @click="saveDialogOpen = false; submit()"
        >
          <Loader2 v-if="isSaving" class="w-3.5 h-3.5 animate-spin" />
          <component v-else :is="publishOptions.find(o => o.value === selectedStatus)?.icon" class="w-3.5 h-3.5" />
          {{ isSaving ? t('campaignBuilder.savingBtn') : publishOptions.find(o => o.value === selectedStatus)?.label ?? t('common.save') }}
        </button>
      </DialogFooter>
    </DialogContent>
  </Dialog>

  <!-- ── Theme Picker Dialog ──────────────────────────────────────── -->
  <Dialog v-model:open="themePickerOpen">
    <DialogContent class="sm:max-w-xs p-0 gap-0 overflow-hidden">
      <DialogHeader class="px-5 pt-5 pb-3 border-b border-border">
        <DialogTitle class="text-base text-slate-800">{{ t('campaignBuilder.themeLabel') }}</DialogTitle>
        <DialogDescription class="text-xs">{{ t('campaignBuilder.themeHint') }}</DialogDescription>
      </DialogHeader>

      <div class="p-3 max-h-80 overflow-y-auto space-y-0.5">
        <!-- None option -->
        <button
          type="button"
          class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm transition-colors"
          :class="!form.themeId ? 'bg-primary/10 text-primary font-semibold' : 'text-muted-foreground hover:bg-muted/50'"
          @click="selectTheme(null)"
        >
          <X class="w-4 h-4 shrink-0" />
          {{ t('campaignBuilder.themeAllPlaceholder') }}
        </button>

        <template v-for="th in topicStore.themes" :key="th.id">
          <!-- Root theme with no children -->
          <button
            v-if="th.children.length === 0"
            type="button"
            class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm transition-colors"
            :class="form.themeId === String(th.id) ? 'bg-primary/10 text-primary font-semibold' : 'text-slate-800 hover:bg-muted/50'"
            @click="selectTheme(th.id)"
          >
            <Folder class="w-4 h-4 shrink-0 text-amber-500" />
            {{ th.name }}
          </button>

          <!-- Root theme with children: selectable + expandable -->
          <div v-else>
            <button
              type="button"
              class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm transition-colors"
              :class="form.themeId === String(th.id) ? 'bg-primary/10 text-primary font-semibold' : 'text-slate-800 hover:bg-muted/50'"
              @click="selectTheme(th.id)"
            >
              <span
                class="p-0.5 rounded hover:bg-muted/80 shrink-0"
                @click.stop="toggleThemeExpand(th.id)"
              >
                <ChevronRight
                  class="w-3.5 h-3.5 transition-transform text-muted-foreground"
                  :class="themePickerExpanded.has(th.id) ? 'rotate-90' : ''"
                />
              </span>
              <FolderOpen v-if="themePickerExpanded.has(th.id)" class="w-4 h-4 shrink-0 text-amber-500" />
              <Folder v-else class="w-4 h-4 shrink-0 text-amber-500" />
              <span class="font-medium flex-1 text-start">{{ th.name }}</span>
              <span class="text-[10px] text-muted-foreground">{{ th.children.length }}</span>
            </button>

            <!-- Subthemes -->
            <div v-if="themePickerExpanded.has(th.id)" class="ms-6 space-y-0.5 mt-0.5">
              <button
                v-for="sub in th.children"
                :key="sub.id"
                type="button"
                class="w-full flex items-center gap-2 px-3 py-1.5 rounded-lg text-sm transition-colors"
                :class="form.themeId === String(sub.id) ? 'bg-primary/10 text-primary font-semibold' : 'text-muted-foreground hover:bg-muted/50 hover:text-slate-800'"
                @click="selectTheme(sub.id)"
              >
                <FileText class="w-3.5 h-3.5 shrink-0" />
                {{ sub.name }}
              </button>
            </div>
          </div>
        </template>
      </div>
    </DialogContent>
  </Dialog>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { Input } from '@/components/ui/input'
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from '@/components/ui/select'
// Note: Select is still used for questionnaire + scoring mode selects
import { useTopicStore } from '@/store/topicStore'
import ToggleSwitch   from '@/components/campaign/builder/ToggleSwitch.vue'
import ExperienceToggle from '@/components/campaign/builder/ExperienceToggle.vue'
import ReviewSection  from '@/components/campaign/builder/ReviewSection.vue'
import ReviewRow      from '@/components/campaign/builder/ReviewRow.vue'
import { useLocale } from '@/composables/useLocale'
import { useAuthStore } from '@/store/authStore'
import {
  getQuestionnairesApi, createCampaignApi, updateCampaignApi, getCampaignByIdApi, createQuestionnaireApi,
  addCampaignQuestionnaireApi, getCampaignQuestionnairesApi, removeCampaignQuestionnaireApi, reorderCampaignQuestionnairesApi,
  getCampaignCandidatesApi, removeCampaignCandidateApi,
} from '@/utils/api'
import {
  ArrowLeft, Check, ChevronLeft, ChevronRight, ChevronDown, ChevronUp, Building2,
  CalendarDays, Clock, Mail, BellRing,
  RefreshCcw, Timer, Hourglass,
  ShieldAlert, CircleAlert,
  PenLine, Send, PlayCircle,
  CheckCircle2, FileText, ClipboardList, Loader2,
  Folder, FolderOpen, X, XCircle,
  UserPlus, UserX, Save,
} from 'lucide-vue-next'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle,
  DialogDescription, DialogFooter,
} from '@/components/ui/dialog'
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel,
  AlertDialogContent, AlertDialogDescription,
  AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import InviteCandidatesDialog from '@/components/campaign/InviteCandidatesDialog.vue'
import type { CampaignCandidateItem, CampaignCandidateStatus } from '@/utils/models'

// ── Props ──────────────────────────────────────────────────────────────────
const props = withDefaults(defineProps<{
  isEdit?: boolean
}>(), { isEdit: false })

const router = useRouter()
const route  = useRoute()
const { t } = useLocale()
const authStore  = useAuthStore()
const topicStore = useTopicStore()
const company = computed(() => authStore.state.company)

// ── Edit mode — ID from route ───────────────────────────────────────────────
const editCampaignId = computed(() => {
  const id = route.params.id
  return id ? Number(id) : null
})

const loadingCampaign = ref(false)

// Helper: convert ISO date string → datetime-local input format (YYYY-MM-DDTHH:mm)
function toDatetimeLocal(iso: string): string {
  if (!iso) return ''
  const d = new Date(iso)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

async function loadCampaignForEdit() {
  if (!editCampaignId.value) return
  loadingCampaign.value = true
  const { data } = await getCampaignByIdApi(editCampaignId.value)
  loadingCampaign.value = false
  if (!data) return
  form.value.name               = data.name
  form.value.description        = data.description ?? ''
  form.value.availableFrom      = toDatetimeLocal(data.availableFrom)
  form.value.availableUntil     = toDatetimeLocal(data.availableUntil)
  form.value.themeId             = data.themeId ? String(data.themeId) : ''
  form.value.sendInviteEmail     = data.sendInviteEmail
  form.value.reminderMinutesBeforeStart = data.reminderHoursBefore ?? undefined // Map backend field
  selectedStatus.value           = (data.status as 'Draft' | 'Scheduled' | 'Active' | 'Closed' | 'Archived') ?? 'Draft'
  // Pre-fill campaign execution settings
  form.value.passingScorePercent          = data.passingScorePercent          ?? 60
  form.value.scoringMode                  = data.scoringMode                  ?? 'SumWeighted'
  form.value.allowPartialScore            = data.allowPartialScore             ?? true
  form.value.maxAttempts                  = data.maxAttempts                  ?? 1
  form.value.cooldownBetweenAttemptsMinutes = data.cooldownBetweenAttemptsMinutes ?? undefined
  form.value.globalDurationMinutes        = data.globalDurationMinutes        ?? undefined
  form.value.showTimer                    = data.showTimer                    ?? true
  form.value.allowNavigationBack          = data.allowNavigationBack          ?? true
  form.value.allowTabSwitch               = data.allowTabSwitch               ?? false
  form.value.tabSwitchMaxCount            = data.tabSwitchMaxCount            ?? undefined
  form.value.randomizeQuestions           = data.randomizeQuestions           ?? false
  form.value.randomizeChoices             = data.randomizeChoices             ?? false
  form.value.showResultsImmediately       = data.showResultsImmediately       ?? true
  form.value.showQuestionScore            = data.showQuestionScore            ?? false
  form.value.showCorrectAfterEach         = data.showCorrectAfterEach         ?? false
  // Load attached questionnaires
  const { data: cqData } = await getCampaignQuestionnairesApi(editCampaignId.value)
  if (cqData) {
    campaignQuestionnaires.value = cqData.map(cq => ({
      id:                 cq.id,
      questionnaireId:    cq.questionnaireId,
      questionnaireTitle: cq.questionnaireTitle,
      label:              cq.label ?? '',
      isRequired:         cq.isRequired,
    }))
  }
}


// ── Candidates state (step 5, edit mode only) ───────────────────────────────
const candidates        = ref<CampaignCandidateItem[]>([])
const loadingCandidates = ref(false)
const candidatesError   = ref(false)
const isInviteOpen      = ref(false)
const isRemoveConfOpen  = ref(false)
const removingCandidate = ref<CampaignCandidateItem | null>(null)

const alreadyInvitedIds = computed(() => candidates.value.map(c => c.candidateId))

async function loadCandidates() {
  if (!editCampaignId.value) return
  loadingCandidates.value = true
  candidatesError.value   = false
  const { data, error } = await getCampaignCandidatesApi(editCampaignId.value)
  if (error || !data) candidatesError.value = true
  else candidates.value = data
  loadingCandidates.value = false
}

function candidateStatusBadge(s: CampaignCandidateStatus) {
  const m: Record<CampaignCandidateStatus, string> = {
    Invited:    'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300',
    InProgress: 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300',
    Completed:  'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300',
    Expired:    'bg-muted text-muted-foreground',
    Withdrawn:  'bg-red-100 text-red-600 dark:bg-red-900/40 dark:text-red-400',
  }
  return m[s] ?? 'bg-muted text-muted-foreground'
}

function candidateInitials(name: string) {
  return name.split(' ').filter(Boolean).map(n => n[0]).join('').toUpperCase().slice(0, 2) || '?'
}

function askRemoveCandidate(c: CampaignCandidateItem) {
  removingCandidate.value = c
  isRemoveConfOpen.value  = true
}

async function confirmRemoveCandidate() {
  if (!removingCandidate.value || !editCampaignId.value) return
  isRemoveConfOpen.value = false
  await removeCampaignCandidateApi(editCampaignId.value, removingCandidate.value.candidateId)
  removingCandidate.value = null
  loadCandidates()
}

// Determine which panel we're in for post-creation navigation
const isFormateurPanel = computed(() => route.path.startsWith('/formateur'))
const testsRoute       = computed(() => isFormateurPanel.value ? '/formateur/tests' : '/company/tests')
const questionnaireRouteName = computed(() =>
  isFormateurPanel.value ? 'FormateurQuestionnaire' : 'CompanyQuestionnaire'
)

// ── Steps config ───────────────────────────────────────────────────────────
const steps = computed(() => {
  const base = [
    { key: 'info',       label: t('campaignBuilder.step1Label'), sublabel: t('campaignBuilder.step1Sub') },
    { key: 'scoring',    label: t('campaignBuilder.step2Label'), sublabel: t('campaignBuilder.step2Sub') },
    { key: 'experience', label: t('campaignBuilder.step3Label'), sublabel: t('campaignBuilder.step3Sub') },
    { key: 'review',     label: t('campaignBuilder.step4Label'), sublabel: t('campaignBuilder.step4Sub') },
  ]
  if (props.isEdit && editCampaignId.value) {
    base.push({ key: 'candidates', label: t('campaignBuilder.candidatesStep'), sublabel: t('campaignBuilder.candidatesStepSub') })
  }
  return base
})
const currentStep = ref(0)

// Load candidates when entering step 5
watch(currentStep, (s) => {
  if (s === 4 && props.isEdit && editCampaignId.value) loadCandidates()
})

// ── Form state ─────────────────────────────────────────────────────────────
const form = ref({
  // Step 1
  name:                  '',
  description:           '',
  availableFrom:         '',
  availableUntil:        '',
  themeId:               '' as string,
  sendInviteEmail:       true,
  reminderMinutesBeforeStart: undefined as number | undefined,
  // Step 2
  passingScorePercent:   60,
  scoringMode:           'SumWeighted' as 'SumWeighted' | 'Average' | 'Percentage',
  allowPartialScore:     true,
  maxAttempts:           1,
  cooldownBetweenAttemptsMinutes: undefined as number | undefined,
  globalDurationMinutes: undefined as number | undefined,
  // Step 3
  showTimer:             true,
  allowNavigationBack:   true,
  randomizeQuestions:    false,
  randomizeChoices:      false,
  showResultsImmediately: true,
  showQuestionScore:     false,
  showCorrectAfterEach:  false,
  allowTabSwitch:        false,
  tabSwitchMaxCount:     undefined as number | undefined,
})

// Invert allowTabSwitch for UX clarity: "block" = !allow
const blockTabSwitch = computed({
  get: () => !form.value.allowTabSwitch,
  set: (v: boolean) => {
    form.value.allowTabSwitch = !v
    if (!v) {
      form.value.tabSwitchMaxCount = undefined
    } else if (form.value.tabSwitchMaxCount === undefined) {
      form.value.tabSwitchMaxCount = 3
    }
  },
})

// Ensure end date remains after start date when start date changes
watch(() => form.value.availableFrom, (newStart) => {
  if (newStart && form.value.availableUntil && newStart >= form.value.availableUntil) {
    form.value.availableUntil = ''
  }
})

// ── Validation ─────────────────────────────────────────────────────────────
const errors = ref({ name: '', availableFrom: '', availableUntil: '', reminderMinutesBeforeStart: '' })

function clearErrors() {
  errors.value = { name: '', availableFrom: '', availableUntil: '', reminderMinutesBeforeStart: '' }
}

const getNowLocal = () => {
  const now = new Date()
  const offset = now.getTimezoneOffset() * 60000
  return new Date(now.getTime() - offset).toISOString().slice(0, 16)
}

function validateStep(step: number): boolean {
  clearErrors()
  const nowTimestamp = new Date().getTime()
  const nowLocalStr = getNowLocal()
  let valid = true
  if (step === 0) {
    if (!form.value.name.trim()) {
      errors.value.name = t('campaignBuilder.nameRequired')
      valid = false
    }
    if (!form.value.availableFrom) {
      errors.value.availableFrom = t('campaignBuilder.startRequired')
      valid = false
    } else if (form.value.availableFrom < nowLocalStr) {
      errors.value.availableFrom = t('campaignBuilder.startInPast')
      valid = false
    }

    if (!form.value.availableUntil) {
      errors.value.availableUntil = t('campaignBuilder.endRequired')
      valid = false
    } else if (form.value.availableUntil < nowLocalStr) {
      errors.value.availableUntil = t('campaignBuilder.endInPast')
      valid = false
    }

    if (form.value.availableFrom && form.value.availableUntil &&
        form.value.availableFrom >= form.value.availableUntil) {
      errors.value.availableUntil = t('campaignBuilder.endAfterStart')
      valid = false
    }

    // Validation du rappel (Minutes avant le début)
    if (form.value.sendInviteEmail && form.value.reminderMinutesBeforeStart && form.value.availableFrom) {
      const startTime = new Date(form.value.availableFrom).getTime()
      const reminderTime = startTime - (form.value.reminderMinutesBeforeStart * 60000)
      if (reminderTime < nowTimestamp) {
        errors.value.reminderMinutesBeforeStart = t('campaignBuilder.reminderInPast')
        valid = false
      }
    }
  }
  return valid
}

// ── Questionnaires (loaded from API) ───────────────────────────────────────
const questionnaires        = ref<{ id: number; title: string }[]>([])
const questionnairesLoading = ref(false)

async function loadQuestionnaires() {
  questionnairesLoading.value = true
  const { data } = await getQuestionnairesApi()
  if (data) {
    questionnaires.value = data.map(q => ({ id: q.id, title: q.title }))
  }
  questionnairesLoading.value = false
}

onMounted(() => {
  loadQuestionnaires()
  topicStore.fetchThemes()
  if (props.isEdit) loadCampaignForEdit()
})

// ── Quick-create questionnaire dialog ─────────────────────────────────────
const createQDialog      = ref(false)
const newQTitle          = ref('')
const newQDescription    = ref('')
const creatingQ          = ref(false)
const createQError       = ref('')

async function submitCreateQ() {
  if (!newQTitle.value.trim()) {
    createQError.value = t('campaignBuilder.createQNameRequired')
    return
  }
  creatingQ.value    = true
  createQError.value = ''
  const { data, error } = await createQuestionnaireApi({
    title:       newQTitle.value.trim(),
    description: newQDescription.value.trim() || undefined,
  })
  creatingQ.value = false
  if (error || !data) {
    createQError.value = error ?? 'Failed to create questionnaire'
    return
  }
  // Add to library list
  questionnaires.value.push({ id: data.id, title: data.title })
  // In edit mode: immediately attach to campaign via API
  if (props.isEdit && editCampaignId.value) {
    await addCampaignQuestionnaireApi(editCampaignId.value, { questionnaireId: data.id, label: addQLabel.value || null, isRequired: true })
  }
  // Add to local attached list
  campaignQuestionnaires.value.push({
    id:                 0,
    questionnaireId:    data.id,
    questionnaireTitle: data.title,
    label:              addQLabel.value,
    isRequired:         true,
  })
  addQId.value    = ''
  addQLabel.value = ''
  // Reset + close
  newQTitle.value       = ''
  newQDescription.value = ''
  createQDialog.value   = false
}

// ── Campaign questionnaires (multi) ────────────────────────────────────────
type LocalCQ = { id: number; questionnaireId: number; questionnaireTitle: string; label: string; isRequired: boolean }
const campaignQuestionnaires = ref<LocalCQ[]>([])
const addQId    = ref('')
const addQLabel = ref('')

// Watch questionnaire selection → auto-load its settings into form steps 2 & 3

const availableQuestionnaires = computed(() => {
  const attachedIds = new Set(campaignQuestionnaires.value.map(cq => cq.questionnaireId))
  return questionnaires.value.filter(q => !attachedIds.has(q.id))
})

async function appendQuestionnaire() {
  const qId = Number(addQId.value)
  if (!qId) return
  const q = questionnaires.value.find(x => x.id === qId)
  if (!q) return
  // In edit mode: call API immediately
  if (props.isEdit && editCampaignId.value) {
    const { data } = await addCampaignQuestionnaireApi(editCampaignId.value, { questionnaireId: qId, label: addQLabel.value || null, isRequired: true })
    if (data) {
      campaignQuestionnaires.value.push({ id: data.id, questionnaireId: qId, questionnaireTitle: q.title, label: addQLabel.value, isRequired: true })
    }
  } else {
    campaignQuestionnaires.value.push({ id: 0, questionnaireId: qId, questionnaireTitle: q.title, label: addQLabel.value, isRequired: true })
  }
  addQId.value    = ''
  addQLabel.value = ''
}

async function removeAttachedQuestionnaire(index: number) {
  const cq = campaignQuestionnaires.value[index]
  if (!cq) return
  if (props.isEdit && editCampaignId.value && cq.id > 0) {
    await removeCampaignQuestionnaireApi(editCampaignId.value, cq.questionnaireId)
  }
  campaignQuestionnaires.value.splice(index, 1)
}

async function moveQuestionnaire(index: number, direction: 'up' | 'down') {
  const list = campaignQuestionnaires.value
  const swapIdx = direction === 'up' ? index - 1 : index + 1
  if (swapIdx < 0 || swapIdx >= list.length) return
  const a = list[index], b = list[swapIdx]
  if (!a || !b) return
  list[index] = b
  list[swapIdx] = a
  campaignQuestionnaires.value = [...list]
  // In edit mode: sync order to API
  if (props.isEdit && editCampaignId.value) {
    await reorderCampaignQuestionnairesApi(editCampaignId.value, {
      items: list.map((cq, i) => ({ questionnaireId: cq.questionnaireId, order: i + 1 }))
    })
  }
}

// ── Theme picker dialog ─────────────────────────────────────────────────────
const themePickerOpen     = ref(false)
const themePickerExpanded = ref(new Set<number>())

function toggleThemeExpand(id: number) {
  if (themePickerExpanded.value.has(id)) {
    themePickerExpanded.value.delete(id)
  } else {
    themePickerExpanded.value.add(id)
  }
  // trigger reactivity
  themePickerExpanded.value = new Set(themePickerExpanded.value)
}

function selectTheme(id: number | null) {
  form.value.themeId = id ? String(id) : ''
  themePickerOpen.value = false
}

const selectedThemeName = computed(() => {
  if (!form.value.themeId) return ''
  const idNum = Number(form.value.themeId)
  for (const th of topicStore.themes) {
    if (th.id === idNum) return th.name
    const sub = th.children.find(c => c.id === idNum)
    if (sub) return `${th.name} › ${sub.name}`
  }
  return ''
})

// ── Config options ─────────────────────────────────────────────────────────
const scoringModes = computed(() => [
  { value: 'SumWeighted' as const, label: t('campaignBuilder.scoringWeighted'), desc: t('campaignBuilder.scoringWeightedDesc') },
  { value: 'Average'     as const, label: t('campaignBuilder.scoringAverage'),  desc: t('campaignBuilder.scoringAverageDesc') },
  { value: 'Percentage'  as const, label: t('campaignBuilder.scoringPercentage'), desc: t('campaignBuilder.scoringPercentageDesc') },
])

const selectedStatus = ref<'Draft' | 'Scheduled' | 'Active' | 'Closed' | 'Archived'>('Draft')
const publishOptions = computed(() => [
  {
    value: 'Draft'     as const,
    label: t('campaignBuilder.draftLabel'),
    desc:  t('campaignBuilder.draftDesc'),
    icon: PenLine,
    iconClass: 'text-muted-foreground',
  },
  {
    value: 'Scheduled' as const,
    label: t('campaignBuilder.scheduleLabel'),
    desc:  t('campaignBuilder.scheduleDesc'),
    icon: Send,
    iconClass: 'text-sky-600 dark:text-sky-400',
  },
  {
    value: 'Active'    as const,
    label: t('campaignBuilder.publishLabel'),
    desc:  t('campaignBuilder.publishDesc'),
    icon: PlayCircle,
    iconClass: 'text-emerald-600 dark:text-emerald-400',
  },
  {
    value: 'Closed'    as const,
    label: t('campaignBuilder.closedLabel'),
    desc:  t('campaignBuilder.closedDesc'),
    icon: XCircle,
    iconClass: 'text-rose-600 dark:text-rose-400',
  },
])

// ── Computed ───────────────────────────────────────────────────────────────
const campaignDuration = computed(() => {
  if (!form.value.availableFrom || !form.value.availableUntil) return null
  const ms = new Date(form.value.availableUntil).getTime() - new Date(form.value.availableFrom).getTime()
  if (ms <= 0) return null
  const days  = Math.floor(ms / 86_400_000)
  const hours = Math.floor((ms % 86_400_000) / 3_600_000)
  const d = t('common.daysShort')
  const h = t('common.hoursShort')
  return days > 0 ? `${days}${d}${hours ? ` ${hours}${h}` : ''}` : `${hours}${h}`
})

function formatDateTime(dt: string) {
  if (!dt) return '—'
  return new Date(dt).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
}

// ── Navigation ─────────────────────────────────────────────────────────────
function nextStep() {
  if (!validateStep(currentStep.value)) return
  currentStep.value++
}

async function saveDraft() {
  if (!validateStep(0)) return
  isSaving.value = true
  try {
    const draftPayload = {
      name:                form.value.name,
      description:         form.value.description || null,
      status:              'Draft' as const,
      availableFrom:       form.value.availableFrom ? new Date(form.value.availableFrom).toISOString() : new Date().toISOString(),
      availableUntil:      form.value.availableUntil ? new Date(form.value.availableUntil).toISOString() : new Date(Date.now() + 86400000).toISOString(),
      themeId:             Number(form.value.themeId) || null,
      sendInviteEmail:     form.value.sendInviteEmail,
      reminderHoursBefore: form.value.reminderMinutesBeforeStart ?? null, // Hijack field as minutes
      // Campaign execution settings
      passingScorePercent:             form.value.passingScorePercent,
      scoringMode:                     form.value.scoringMode,
      allowPartialScore:               form.value.allowPartialScore,
      maxAttempts:                     form.value.maxAttempts,
      cooldownBetweenAttemptsMinutes:  form.value.cooldownBetweenAttemptsMinutes ?? null,
      globalDurationMinutes:           form.value.globalDurationMinutes ?? null,
      showTimer:                       form.value.showTimer,
      allowNavigationBack:             form.value.allowNavigationBack,
      allowTabSwitch:                  form.value.allowTabSwitch,
      tabSwitchMaxCount:               form.value.tabSwitchMaxCount ?? null,
      randomizeQuestions:              form.value.randomizeQuestions,
      randomizeChoices:                form.value.randomizeChoices,
      showResultsImmediately:          form.value.showResultsImmediately,
      showQuestionScore:               form.value.showQuestionScore,
      showCorrectAfterEach:            form.value.showCorrectAfterEach,
    }
    if (props.isEdit && editCampaignId.value) {
      await updateCampaignApi(editCampaignId.value, draftPayload)
    } else {
      await createCampaignApi(draftPayload)
    }
  } finally {
    isSaving.value = false
  }
}

// ── Save dialog ────────────────────────────────────────────────────────────
const saveDialogOpen = ref(false)

// ── Submit ─────────────────────────────────────────────────────────────────
const isSaving          = ref(false)
const isSuccess         = ref(false)
const createdName       = ref('')
const createdStatus     = ref<'Draft' | 'Scheduled' | 'Active'>('Draft')
const createdCampaignId = ref<number>(0)

async function submit() {
  if (!validateStep(0)) {
    currentStep.value = 0
    saveDialogOpen.value = false
    return
  }
  isSaving.value = true
  try {
    const payload = {
      name:                form.value.name,
      description:         form.value.description || null,
      status:              selectedStatus.value,
      availableFrom:       new Date(form.value.availableFrom).toISOString(),
      availableUntil:      new Date(form.value.availableUntil).toISOString(),
      themeId:             Number(form.value.themeId) || null,
      sendInviteEmail:     form.value.sendInviteEmail,
      reminderHoursBefore: form.value.reminderMinutesBeforeStart ?? null, // Hijack field as minutes
      // Campaign execution settings (saved directly on campaign)
      passingScorePercent:             form.value.passingScorePercent,
      scoringMode:                     form.value.scoringMode,
      allowPartialScore:               form.value.allowPartialScore,
      maxAttempts:                     form.value.maxAttempts,
      cooldownBetweenAttemptsMinutes:  form.value.cooldownBetweenAttemptsMinutes ?? null,
      globalDurationMinutes:           form.value.globalDurationMinutes ?? null,
      showTimer:                       form.value.showTimer,
      allowNavigationBack:             form.value.allowNavigationBack,
      allowTabSwitch:                  form.value.allowTabSwitch,
      tabSwitchMaxCount:               form.value.tabSwitchMaxCount ?? null,
      randomizeQuestions:              form.value.randomizeQuestions,
      randomizeChoices:                form.value.randomizeChoices,
      showResultsImmediately:          form.value.showResultsImmediately,
      showQuestionScore:               form.value.showQuestionScore,
      showCorrectAfterEach:            form.value.showCorrectAfterEach,
    }

    const { data, error: apiErr } = props.isEdit && editCampaignId.value
      ? await updateCampaignApi(editCampaignId.value, payload)
      : await createCampaignApi(payload)

    if (apiErr || !data) {
      errors.value.name = apiErr ?? t('campaignBuilder.errorCreate')
      currentStep.value = 0
      return
    }

    if (props.isEdit) {
      router.push(testsRoute.value)
      return
    }

    // Attach questionnaires after creation (create mode only)
    for (const cq of campaignQuestionnaires.value) {
      if (!cq) continue
      await addCampaignQuestionnaireApi(data.id, {
        questionnaireId: cq.questionnaireId,
        label:           cq.label || null,
        isRequired:      cq.isRequired,
      })
    }

    createdCampaignId.value = data.id
    createdName.value       = data.name
    createdStatus.value     = (data.status as 'Draft' | 'Scheduled' | 'Active')
    isSuccess.value         = true
  } finally {
    isSaving.value = false
  }
}

// ── Post-creation navigation ───────────────────────────────────────────────
function goToQuestionnaire() {
  const firstQId = campaignQuestionnaires.value[0]?.questionnaireId
  if (firstQId) {
    router.push({ name: questionnaireRouteName.value, params: { id: firstQId } })
  } else {
    router.push(testsRoute.value)
  }
}

function goToTests() {
  router.push(testsRoute.value)
}
</script>
