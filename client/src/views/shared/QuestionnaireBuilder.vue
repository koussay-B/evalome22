<template>
  <!-- ── Wrapper: split layout when AI panel is open ─────────────────── -->
  <div class="flex gap-6 pb-10" :class="aiPanelOpen ? 'items-start' : 'flex-col space-y-6'">

    <!-- ══ LEFT COLUMN / MAIN CONTENT ══════════════════════════════════ -->
    <div class="flex-1 min-w-0 space-y-6">

    <!-- ── Page Header ─────────────────────────────────────────────── -->
    <div class="flex items-start justify-between gap-4">
      <div class="flex items-start gap-4">
        <button
          class="p-2 rounded-lg border border-border hover:bg-muted/50 transition-colors shrink-0 mt-0.5"
          @click="router.back()"
        >
          <ArrowLeft class="w-4 h-4" />
        </button>
        <div>
          <p class="text-[11px]  uppercase tracking-widest text-muted-foreground mb-0.5">
            {{ t('questionnaireBuilder.subtitle') }}
          </p>
          <div v-if="loading" class="h-7 w-48 rounded-lg bg-muted animate-pulse" />
          <h1 v-else class="text-2xl  tracking-tight text-slate-800">
            {{ questionnaire?.title ?? t('questionnaireBuilder.loadingTitle') }}
          </h1>
          <div class="flex items-center gap-2 mt-1.5">
            <span
              class="inline-flex items-center gap-1 text-[10px]  uppercase tracking-wide px-2 py-0.5 rounded-md"
              :class="statusStyle.class"
            >
              <span class="w-1.5 h-1.5 rounded-full" :class="statusStyle.dot" />
              {{ questionnaire?.status ? t('questionnaireBuilder.status' + questionnaire.status) : '—' }}
            </span>
            <span class="text-xs text-muted-foreground">·</span>
            <span class="text-xs text-muted-foreground">
              {{ questionnaire?.questionnaireQuestions?.length ?? 0 }} {{ t('questionnaireBuilder.statQuestions') }}
            </span>
          </div>
        </div>
      </div>

      <!-- Actions -->
      <div class="flex items-center gap-2 shrink-0">
        <button
          v-if="questionnaire?.status === 'Draft'"
          class="flex items-center gap-2 px-4 py-2 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors"
          :disabled="isPublishing"
          @click="publishQuestionnaire"
        >
          <Send class="w-4 h-4" />
          {{ isPublishing ? t('questionnaireBuilder.publishing') : t('questionnaireBuilder.publish') }}
        </button>
        <!-- AI Generate button — always visible -->
        <button
          class="flex items-center gap-2 px-4 py-2 rounded-lg border text-sm font-semibold transition-all"
          :class="
            aiPanelOpen
              ? 'bg-primary text-primary-foreground border-primary hover:bg-primary/90'
              : 'border-border bg-secondary text-slate-800 hover:bg-muted'
          "
          @click="toggleAiPanel"
        >
          <Sparkles class="w-4 h-4" />
          {{ aiPanelOpen ? t('questionnaireBuilder.aiClose') : t('questionnaireBuilder.aiGenerate') }}
        </button>
        <button
          class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 transition-colors"
          @click="openAddQuestion"
        >
          <Plus class="w-4 h-4" /> {{ t('questionnaireBuilder.addQuestion') }}
        </button>
      </div>
    </div>

    <!-- ── Stats Bar ───────────────────────────────────────────────── -->
    <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
      <div class="rounded-xl border border-border bg-secondary p-4 flex items-center gap-3">
        <div class="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
          <List class="w-4 h-4 text-primary" />
        </div>
        <div>
          <div class="text-xl text-slate-800 text-slate-800">{{ sortedQuestions.length }}</div>
          <div class="text-xs text-muted-foreground">{{ t('questionnaireBuilder.statQuestions') }}</div>
        </div>
      </div>
      <div class="rounded-xl border border-border bg-secondary p-4 flex items-center gap-3">
        <div class="h-9 w-9 rounded-lg bg-primary/12 flex items-center justify-center shrink-0">
          <Star class="w-4 h-4 text-primary" />
        </div>
        <div>
          <div class="text-xl text-slate-800 ">{{ totalPoints }}</div>
          <div class="text-xs text-muted-foreground">{{ t('questionnaireBuilder.statPoints') }}</div>
        </div>
      </div>
      <div class="rounded-xl border border-border bg-secondary p-4 flex items-center gap-3">
        <div class="h-9 w-9 rounded-lg bg-orange-100 dark:bg-orange-950/30 flex items-center justify-center shrink-0">
          <Clock class="w-4 h-4 text-orange-600 dark:text-orange-400" />
        </div>
        <div>
          <div class="text-xl  text-slate-800">{{ estimatedMinutes }}</div>
          <div class="text-xs text-muted-foreground">{{ t('questionnaireBuilder.statMinutes') }}</div>
        </div>
      </div>
      <div class="rounded-xl border border-border bg-secondary p-4 flex items-center gap-3">
        <div class="h-9 w-9 rounded-lg bg-primary/12 flex items-center justify-center shrink-0">
          <Target class="w-4 h-4 text-primary" />
        </div>
        <div>
          <div class="text-xl  text-slate-800">{{ questionnaire?.passingScorePercent ?? 60 }}%</div>
          <div class="text-xs text-muted-foreground">{{ t('questionnaireBuilder.statPassThreshold') }}</div>
        </div>
      </div>
    </div>

    <!-- ── Type breakdown ──────────────────────────────────────────── -->
    <div v-if="sortedQuestions.length > 0" class="flex flex-wrap items-center gap-2">
      <span
        v-for="stat in typeStats"
        :key="stat.type"
        class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full border text-xs font-semibold"
        :class="stat.style"
      >
        <component :is="stat.icon" class="w-3.5 h-3.5" />
        {{ stat.count }} × {{ stat.label }}
      </span>
      <span class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full border border-border bg-muted/30 text-xs font-semibold text-muted-foreground">
        <Shuffle class="w-3.5 h-3.5" />
        {{ questionnaire?.randomizeQuestions ? t('questionnaireBuilder.randomized') : t('questionnaireBuilder.fixedOrder') }}
      </span>
    </div>

    <!-- ── Error banner ────────────────────────────────────────────── -->
    <div v-if="error" class="flex items-center gap-3 px-4 py-3 rounded-xl bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 text-sm text-red-700 dark:text-red-400">
      <CircleAlert class="w-4 h-4 shrink-0" />
      {{ error }}
    </div>

    <!-- ── Loading skeleton ────────────────────────────────────────── -->
    <div v-if="loading" class="space-y-3">
      <div v-for="i in 4" :key="i" class="h-20 rounded-xl border border-border bg-secondary animate-pulse" />
    </div>

    <!-- ── Empty state ─────────────────────────────────────────────── -->
    <EmptyData
      v-else-if="sortedQuestions.length === 0"
      :icon="FileQuestion"
      :title="t('questionnaireBuilder.emptyTitle')"
      :description="t('questionnaireBuilder.emptyDesc')"
    >
      <button
        class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 transition-colors mt-2"
        @click="openAddQuestion"
      >
        <Plus class="w-4 h-4" /> {{ t('questionnaireBuilder.addFirstQuestion') }}
      </button>
    </EmptyData>

    <!-- ── Question list ───────────────────────────────────────────── -->
    <div v-else class="space-y-2">
      <!-- Publish nudge -->
      <div
        v-if="questionnaire?.status === 'Draft' && sortedQuestions.length > 0"
        class="flex items-center justify-between px-4 py-3 rounded-xl bg-primary/10 border border-primary/20 text-sm"
      >
        <div class="flex items-center gap-2 text-primary">
          <Info class="w-4 h-4 shrink-0" />
          {{ t('questionnaireBuilder.draftNudge') }}
        </div>
        <button
          class="text-xs font-bold text-primary hover:underline shrink-0 ms-3"
          @click="publishQuestionnaire"
        >
          {{ t('questionnaireBuilder.publishNow') }}
        </button>
      </div>

      <QuestionCard
        v-for="qq in sortedQuestions"
        :key="qq.id"
        :question="qq.question!"
        :order="qq.order"
        :weight="qq.weight"
        :is-mandatory="qq.isMandatory"
        :theme-name="getThemeName(qq.question?.themeId)"
        @edit="openEditQuestion"
        @delete="confirmRemove"
      />
    </div>

    </div><!-- end left column -->

    <!-- ══ RIGHT COLUMN — AI Panel ══════════════════════════════════════ -->
    <transition
      enter-active-class="transition-all duration-300 ease-out"
      enter-from-class="opacity-0 translate-x-4"
      enter-to-class="opacity-100 translate-x-0"
      leave-active-class="transition-all duration-200 ease-in"
      leave-from-class="opacity-100 translate-x-0"
      leave-to-class="opacity-0 translate-x-4"
    >
      <div
        v-if="aiPanelOpen"
        class="w-full lg:w-[420px] shrink-0 space-y-4"
      >
        <!-- Panel header -->
        <div class="rounded-xl border border-border bg-card overflow-hidden">
          <!-- Always-visible top bar -->
          <div class="flex items-center justify-between px-5 pt-5" :class="aiFormExpanded ? 'pb-3' : 'pb-5'">
            <div class="flex items-center gap-2">
              <div class="w-8 h-8 rounded-lg bg-primary flex items-center justify-center shrink-0">
                <component :is="aiGenerating ? Loader2 : Sparkles" class="w-4 h-4 text-primary-foreground" :class="aiGenerating ? 'animate-spin' : ''" />
              </div>
              <div>
                <h3 class="text-sm  text-slate-800">{{ t('questionnaireBuilder.aiPanelTitle') }}</h3>
                <p class="text-[10px] text-muted-foreground">
                  {{ aiGenerating ? t('questionnaireBuilder.aiGenerating') : t('questionnaireBuilder.aiPanelSubtitle') }}
                </p>
              </div>
            </div>
            <div class="flex items-center gap-1">
              <!-- Collapse/expand toggle (hidden during generation) -->
              <button
                v-if="!aiGenerating"
                class="p-1.5 rounded-lg text-muted-foreground hover:bg-muted/50 transition-colors"
                :title="aiFormExpanded ? 'Collapse' : 'Expand'"
                @click="aiFormExpanded = !aiFormExpanded"
              >
                <component :is="aiFormExpanded ? ChevronUp : ChevronDown" class="w-4 h-4" />
              </button>
              <button
                class="p-1.5 rounded-lg text-muted-foreground hover:bg-muted/50 transition-colors"
                @click="aiPanelOpen = false"
              >
                <X class="w-4 h-4" />
              </button>
            </div>
          </div>

          <!-- Collapsed summary bar -->
          <div v-if="!aiFormExpanded && !aiGenerating" class="px-5 pb-4 flex flex-wrap items-center gap-2">
            <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-muted border border-border text-[10px] font-semibold text-muted-foreground">
              {{ aiGenerateCount }} questions
            </span>
            <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-muted border border-border text-[10px] font-semibold text-muted-foreground">
              {{ aiDifficulty }}
            </span>
            <span v-for="(t2, ti) in aiTopics.slice(0, 3)" :key="ti" class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-muted border border-border text-[10px] font-semibold text-muted-foreground">
              {{ t2 }}
            </span>
            <button
              class="ms-auto flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-primary text-primary-foreground text-xs font-bold hover:bg-primary/90 transition-colors"
              @click="generateAiQuestions"
            >
              <Sparkles class="w-3 h-3" />
              {{ t('questionnaireBuilder.aiGenerateBtn') }}
            </button>
          </div>

          <!-- Expandable form section -->
          <div v-if="aiFormExpanded && !aiGenerating" class="px-5 pb-5 space-y-4 border-t border-border pt-4">
            <!-- Context info -->
            <div class="flex flex-wrap gap-x-4 gap-y-1">
              <div class="flex items-center gap-1.5 text-xs text-muted-foreground">
                <FileText class="w-3.5 h-3.5 shrink-0" />
                <span class="truncate font-medium max-w-[160px]">{{ questionnaire?.title }}</span>
              </div>
              <div class="flex items-center gap-1.5 text-xs text-muted-foreground">
                <List class="w-3.5 h-3.5 shrink-0" />
                <span>{{ sortedQuestions.length }} {{ t('questionnaireBuilder.aiContextQuestions') }}</span>
              </div>
            </div>

            <!-- Model selector -->
            <div v-if="aiAvailableModels.length > 0">
              <Select v-model="aiSelectedModelId">
                <SelectTrigger class="h-8 text-xs">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem v-for="m in aiAvailableModels" :key="m.id" :value="m.id">
                    {{ m.name }} ({{ m.provider }})
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div v-else class="flex items-center gap-1.5 text-[10px] text-muted-foreground">
              <AlertTriangle class="w-3 h-3 shrink-0" />
              No AI models configured — add one in Admin Settings
            </div>

            <!-- Theme / Category selector -->
            <div class="space-y-1.5">
              <label class="text-xs font-semibold text-slate-800">{{ t('questionnaireBuilder.aiTheme') }}</label>
              <button
                type="button"
                class="w-full flex items-center gap-2 h-8 px-2.5 rounded-lg border border-border bg-background text-xs hover:bg-accent transition-colors text-start"
                @click="aiThemePickerOpen = true"
              >
                <Folder class="w-3.5 h-3.5 text-muted-foreground shrink-0" />
                <span :class="aiSelectedThemeName ? 'text-slate-800' : 'text-muted-foreground'" class="flex-1 truncate">
                  {{ aiSelectedThemeName || t('questionnaireBuilder.aiThemeAll') }}
                </span>
                <X v-if="aiSelectedThemeId" class="w-3 h-3 text-muted-foreground hover:text-slate-800 shrink-0" @click.stop="aiSelectedThemeId = null" />
                <ChevronDown v-else class="w-3.5 h-3.5 text-muted-foreground shrink-0" />
              </button>
            </div>

            <!-- Count selector -->
            <div class="flex items-center gap-3">
              <label class="text-xs font-semibold text-slate-800 shrink-0">{{ t('questionnaireBuilder.aiCount') }}</label>
              <div class="flex items-center gap-2">
                <button
                  class="w-7 h-7 rounded-lg border border-border bg-background flex items-center justify-center text-sm font-bold hover:bg-muted/50 transition-colors disabled:opacity-50"
                  :disabled="aiGenerateCount <= 1"
                  @click="aiGenerateCount = Math.max(1, aiGenerateCount - 1)"
                >−</button>
                <span class="text-sm text-slate-800 w-6 text-center">{{ aiGenerateCount }}</span>
                <button
                  class="w-7 h-7 rounded-lg border border-border bg-background flex items-center justify-center text-sm font-bold hover:bg-muted/50 transition-colors disabled:opacity-50"
                  :disabled="aiGenerateCount >= 10"
                  @click="aiGenerateCount = Math.min(10, aiGenerateCount + 1)"
                >+</button>
              </div>
            </div>

            <!-- Difficulty -->
            <div class="space-y-1.5">
              <label class="text-xs font-semibold text-slate-800">{{ t('questionnaireBuilder.aiDifficulty') }}</label>
              <div class="flex flex-wrap gap-1.5">
                <button
                  v-for="level in difficultyLevels"
                  :key="level.value"
                  class="px-2.5 py-1 rounded-md text-[11px] font-bold border transition-all"
                  :class="
                    aiDifficulty === level.value
                      ? 'bg-primary text-primary-foreground border-primary'
                      : 'border-border bg-background text-muted-foreground hover:border-primary/50 hover:text-slate-800'
                  "
                  @click="aiDifficulty = level.value"
                >{{ level.label }}</button>
              </div>
            </div>

            <!-- Topics -->
            <div class="space-y-1.5">
              <label class="text-xs font-semibold text-slate-800">{{ t('questionnaireBuilder.aiTopics') }}</label>
              <div v-if="aiTopics.length > 0" class="flex flex-wrap gap-1.5">
                <span
                  v-for="(topic, ti) in aiTopics"
                  :key="ti"
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-secondary border border-border text-[11px] font-semibold text-slate-800"
                >
                  {{ topic }}
                  <button class="text-muted-foreground hover:text-slate-800 transition-colors" @click="removeTopic(ti)">
                    <X class="w-2.5 h-2.5" />
                  </button>
                </span>
              </div>
              <div class="flex gap-1.5">
                <input
                  v-model="aiTopicInput"
                  type="text"
                  :placeholder="t('questionnaireBuilder.aiTopicsPlaceholder')"
                  class="flex-1 h-8 rounded-lg border border-border bg-background text-xs text-slate-800 px-2.5 focus:outline-none focus:ring-1 focus:ring-ring placeholder:text-muted-foreground"
                  @keydown.enter.prevent="addTopic(aiTopicInput)"
                  @keydown="(e: KeyboardEvent) => { if (e.key === ',') { e.preventDefault(); addTopic(aiTopicInput) } }"
                />
                <button
                  class="h-8 px-2.5 rounded-lg border border-border bg-background text-muted-foreground hover:bg-muted transition-colors"
                  @click="addTopic(aiTopicInput)"
                >
                  <Plus class="w-3.5 h-3.5" />
                </button>
              </div>
              <!-- Quick theme chips -->
              <div v-if="themeOptions.length > 0" class="flex flex-wrap gap-1">
                <button
                  v-for="th in themeOptions.slice(0, 6)"
                  :key="th.id"
                  class="px-2 py-0.5 rounded-full text-[10px] font-medium border border-dashed border-border text-muted-foreground hover:border-primary/60 hover:text-slate-800 transition-colors"
                  @click="addTopic(th.name)"
                >+ {{ th.name }}</button>
              </div>
            </div>

            <!-- Custom context (collapsible) -->
            <div class="space-y-1.5">
              <button
                type="button"
                class="flex items-center gap-1.5 text-xs text-muted-foreground hover:text-slate-800 transition-colors w-full text-start"
                @click="aiContextExpanded = !aiContextExpanded"
              >
                <ChevronRight class="w-3 h-3 shrink-0 transition-transform" :class="aiContextExpanded ? 'rotate-90' : ''" />
                <span v-if="!aiContextExpanded && aiCustomContext" class="truncate">
                  <span class="font-semibold text-slate-800">{{ t('questionnaireBuilder.aiCustomContext') }}:</span>
                  {{ aiCustomContext.slice(0, 40) }}{{ aiCustomContext.length > 40 ? '…' : '' }}
                </span>
                <span v-else>{{ t('questionnaireBuilder.aiCustomContext') }}</span>
              </button>
              <textarea
                v-if="aiContextExpanded"
                v-model="aiCustomContext"
                :placeholder="t('questionnaireBuilder.aiCustomContextPlaceholder')"
                rows="2"
                class="w-full rounded-lg border border-border bg-background text-xs text-slate-800 px-2.5 py-2 focus:outline-none focus:ring-1 focus:ring-ring placeholder:text-muted-foreground resize-none"
              />
            </div>

            <!-- Generate button -->
            <button
              class="w-full flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-bold hover:bg-primary/90 transition-colors disabled:opacity-60"
              :disabled="aiGenerating"
              @click="generateAiQuestions"
            >
              <Sparkles class="w-4 h-4" />
              {{ t('questionnaireBuilder.aiGenerateBtn') }}
            </button>
          </div>
        </div>

        <!-- Shimmer skeletons while loading -->
        <div v-if="aiGenerating" class="space-y-3">
          <div
            v-for="i in aiGenerateCount"
            :key="i"
            class="rounded-xl border border-border bg-secondary overflow-hidden"
          >
            <div class="p-4 space-y-2.5">
              <div class="h-3.5 rounded-md bg-muted shimmer-line w-4/5" />
              <div class="h-3 rounded-md bg-muted shimmer-line w-3/5" />
              <div class="flex gap-2 pt-1">
                <div class="h-5 w-14 rounded-full bg-muted shimmer-line" />
                <div class="h-5 w-10 rounded-full bg-muted shimmer-line" />
                <div class="h-5 w-12 rounded-full bg-muted shimmer-line" />
              </div>
            </div>
          </div>
          <p class="text-xs text-center text-muted-foreground animate-pulse">{{ t('questionnaireBuilder.aiLoadingHint') }}</p>
        </div>

        <!-- Generation error -->
        <div v-else-if="aiGenerationError" class="rounded-xl border border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-950/20 px-4 py-3 text-xs text-red-600 dark:text-red-400 font-medium flex items-start gap-2">
          <AlertTriangle class="w-4 h-4 shrink-0 mt-0.5" />
          {{ aiGenerationError }}
        </div>

        <!-- Generated question cards -->
        <div v-else-if="aiGeneratedQuestions.length > 0" class="space-y-3">
          <div class="flex items-center justify-between">
            <p class="text-xs font-bold text-slate-800">
              {{ t('questionnaireBuilder.aiResults') }}
              <span class="text-primary">({{ aiSelectedIds.size }}{{ t('questionnaireBuilder.aiSelectedSuffix') }})</span>
            </p>
            <button class="text-xs text-muted-foreground hover:text-slate-800 transition-colors" @click="toggleSelectAll">
              {{ aiSelectedIds.size === aiGeneratedQuestions.length ? t('questionnaireBuilder.aiDeselectAll') : t('questionnaireBuilder.aiSelectAll') }}
            </button>
          </div>

          <div
            v-for="(q, idx) in aiGeneratedQuestions"
            :key="idx"
            class="rounded-xl border transition-all cursor-pointer"
            :class="
              aiSelectedIds.has(idx)
                ? 'border-primary bg-primary/5 ring-1 ring-primary/20'
                : 'border-border bg-secondary hover:border-primary/40'
            "
            @click="toggleAiSelection(idx)"
          >
            <div class="p-4 space-y-2">
              <div class="flex items-start gap-3">
                <Checkbox
                  :checked="aiSelectedIds.has(idx)"
                  class="mt-0.5 shrink-0"
                  @click.stop
                  @update:checked="toggleAiSelection(idx)"
                />
                <p class="text-sm font-semibold text-slate-800 leading-snug flex-1">{{ q.title }}</p>
              </div>
              <div class="flex flex-wrap items-center gap-1.5 ps-8">
                <!-- type badge -->
                <span
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[10px] font-bold uppercase tracking-wide"
                  :class="
                    q.type === 'QCU' ? 'bg-primary/10 text-primary' :
                    q.type === 'QCM' ? 'bg-orange-100 dark:bg-orange-950/30 text-orange-700 dark:text-orange-300 border border-orange-200/70 dark:border-orange-800/50' :
                    'bg-muted text-muted-foreground'
                  "
                >{{ t('questionnaireBuilder.type' + q.type) }}</span>
                <span class="text-[10px] font-bold uppercase tracking-wide text-muted-foreground">
                  {{ t('questionCard.complexity' + q.complexity) }}
                </span>
                <span class="flex items-center gap-1 text-[10px] font-bold text-muted-foreground">
                  <Star class="w-3 h-3" />{{ q.points }}pts
                </span>
              </div>
              <!-- Choices preview -->
              <div class="flex flex-wrap gap-1 ps-8" v-if="q.choices.length > 0">
                <span
                  v-for="(choice, ci) in q.choices.slice(0, 3)"
                  :key="ci"
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-medium border"
                  :class="
                    choice.isCorrect
                      ? 'bg-emerald-50 dark:bg-emerald-950/30 border-emerald-200 dark:border-emerald-800 text-emerald-700 dark:text-emerald-400'
                      : 'bg-muted/50 border-border/60 text-muted-foreground'
                  "
                >
                  <CheckCircle v-if="choice.isCorrect" class="w-2.5 h-2.5" />
                  {{ choice.text }}
                </span>
                <span v-if="q.choices.length > 3" class="text-[10px] text-muted-foreground">+{{ q.choices.length - 3 }}</span>
              </div>
            </div>
          </div>

          <!-- Add selected button -->
          <button
            class="w-full flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-bold hover:bg-primary/90 transition-colors disabled:opacity-60"
            :disabled="aiSelectedIds.size === 0 || isAddingAiQuestions"
            @click="addSelectedAiQuestions"
          >
            <component :is="isAddingAiQuestions ? Loader2 : Plus" class="w-4 h-4" :class="isAddingAiQuestions ? 'animate-spin' : ''" />
            {{ isAddingAiQuestions
              ? t('questionnaireBuilder.aiAdding')
              : `${t('questionnaireBuilder.aiAddSelected')} (${aiSelectedIds.size})` }}
          </button>
        </div>

        <!-- Empty state (before first generation) -->
        <div v-else class="rounded-xl border border-dashed border-border p-8 text-center space-y-2">
          <div class="w-10 h-10 rounded-full bg-muted flex items-center justify-center mx-auto">
            <Sparkles class="w-5 h-5 text-muted-foreground" />
          </div>
          <p class="text-sm font-semibold text-muted-foreground">{{ t('questionnaireBuilder.aiEmptyTitle') }}</p>
          <p class="text-xs text-muted-foreground">{{ t('questionnaireBuilder.aiEmptyDesc') }}</p>
        </div>
      </div>
    </transition>

  </div><!-- end flex wrapper -->

  <!-- ── Question Form Sheet ──────────────────────────────────────── -->
  <QuestionFormSheet
    v-model:open="isFormOpen"
    :editing="editingQuestion"
    :themes="themeOptions"
    @saved="onQuestionSaved"
  />

  <!-- ── AI Theme Picker Dialog ───────────────────────────────────── -->
  <Dialog v-model:open="aiThemePickerOpen">
    <DialogContent class="sm:max-w-xs p-0 gap-0 overflow-hidden">
      <DialogHeader class="px-5 pt-5 pb-3 border-b border-border">
        <DialogTitle class="text-base font-bold">{{ t('questionnaireBuilder.aiTheme') }}</DialogTitle>
      </DialogHeader>
      <div class="p-3 max-h-72 overflow-y-auto space-y-0.5">
        <!-- None -->
        <button
          type="button"
          class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs transition-colors"
          :class="!aiSelectedThemeId ? 'bg-primary/10 text-primary font-semibold' : 'text-muted-foreground hover:bg-muted/50'"
          @click="selectAiTheme(null)"
        >
          <X class="w-3.5 h-3.5 shrink-0" />
          {{ t('questionnaireBuilder.aiThemeAll') }}
        </button>

        <template v-for="th in topicStore.themes" :key="th.id">
          <!-- Root with no children -->
          <button
            v-if="th.children.length === 0"
            type="button"
            class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs transition-colors"
            :class="aiSelectedThemeId === th.id ? 'bg-primary/10 text-primary font-semibold' : 'text-slate-800 hover:bg-muted/50'"
            @click="selectAiTheme(th.id)"
          >
            <Folder class="w-3.5 h-3.5 shrink-0 text-primary" />
            {{ th.name }}
          </button>

          <!-- Root with children: selectable + expandable -->
          <div v-else>
            <button
              type="button"
              class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs transition-colors"
              :class="aiSelectedThemeId === th.id ? 'bg-primary/10 text-primary font-semibold' : 'text-slate-800 hover:bg-muted/50'"
              @click="selectAiTheme(th.id)"
            >
              <span class="p-0.5 rounded hover:bg-muted/80 shrink-0" @click.stop="toggleAiThemeExpand(th.id)">
                <ChevronRight
                  class="w-3 h-3 transition-transform text-muted-foreground"
                  :class="aiThemePickerExpanded.has(th.id) ? 'rotate-90' : ''"
                />
              </span>
              <FolderOpen v-if="aiThemePickerExpanded.has(th.id)" class="w-3.5 h-3.5 shrink-0 text-primary" />
              <Folder v-else class="w-3.5 h-3.5 shrink-0 text-primary" />
              <span class="font-medium flex-1 text-start">{{ th.name }}</span>
              <span class="text-[10px] text-muted-foreground">{{ th.children.length }}</span>
            </button>
            <div v-if="aiThemePickerExpanded.has(th.id)" class="ms-6 space-y-0.5 mt-0.5">
              <button
                v-for="sub in th.children"
                :key="sub.id"
                type="button"
                class="w-full flex items-center gap-2 px-3 py-1.5 rounded-lg text-xs transition-colors"
                :class="aiSelectedThemeId === sub.id ? 'bg-primary/10 text-primary font-semibold' : 'text-muted-foreground hover:bg-muted/50 hover:text-slate-800'"
                @click="selectAiTheme(sub.id)"
              >
                <FileText class="w-3 h-3 shrink-0" />
                {{ sub.name }}
              </button>
            </div>
          </div>
        </template>
      </div>
    </DialogContent>
  </Dialog>

  <!-- ── Delete Confirm Dialog ────────────────────────────────────── -->
  <AlertDialog v-model:open="isDeleteOpen">
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>{{ t('questionnaireBuilder.removeTitle') }}</AlertDialogTitle>
        <AlertDialogDescription>
          {{ t('questionnaireBuilder.removeDesc') }}
        </AlertDialogDescription>
      </AlertDialogHeader>
      <AlertDialogFooter>
        <AlertDialogCancel>{{ t('common.cancel') }}</AlertDialogCancel>
        <AlertDialogAction
          class="bg-red-600 text-white hover:bg-red-700 disabled:opacity-60 flex items-center gap-2"
          :disabled="isRemoving"
          @click.prevent="doRemove"
        >
          <Loader2 v-if="isRemoving" class="w-3.5 h-3.5 animate-spin" />
          {{ t('questionnaireBuilder.remove') }}
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>
</template>

<style scoped>
@keyframes shimmer {
  0%   { transform: translateX(-150%); }
  100% { transform: translateX(150%); }
}

.shimmer-line {
  position: relative;
  overflow: hidden;
}

.shimmer-line::after {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(
    90deg,
    transparent 0%,
    rgba(255, 255, 255, 0.12) 50%,
    transparent 100%
  );
  animation: shimmer 1.4s ease-in-out infinite;
}
</style>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import EmptyData from '@/components/common/EmptyData.vue'
import QuestionCard from '@/components/question/QuestionCard.vue'
import QuestionFormSheet from '@/components/question/QuestionFormSheet.vue'
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel,
  AlertDialogContent, AlertDialogDescription,
  AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle,
} from '@/components/ui/dialog'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { Checkbox } from '@/components/ui/checkbox'
import {
  Tooltip, TooltipContent, TooltipProvider, TooltipTrigger,
} from '@/components/ui/tooltip'
import { useTopicStore } from '@/store/topicStore'
import { useAuthStore } from '@/store/authStore'
import { useLocale } from '@/composables/useLocale'
import {
  getQuestionnaireByIdApi,
  publishQuestionnaireApi,
  createQuestionApi,
  addQuestionToQuestionnaireApi,
  removeQuestionFromQuestionnaireApi,
  generateQuestionsApi,
  getAiModelsApi,
} from '@/utils/api'
import {
  ArrowLeft, Plus, Send, List, Star, Clock, Target,
  CircleDot, CheckSquare, ToggleLeft, Shuffle, CircleAlert,
  Info, FileQuestion, Sparkles, X, FileText,
  Loader2, CheckCircle, AlertTriangle, ChevronUp, ChevronDown, ChevronRight,
  Folder, FolderOpen,
} from 'lucide-vue-next'
import type { QuestionnaireItem, QuestionnaireQuestionItem, QuestionItem, AiModelItem } from '@/utils/models'

const route  = useRoute()
const router = useRouter()
const { t, locale } = useLocale()

// ── Stores ─────────────────────────────────────────────────────────────────
const topicStore = useTopicStore()
const authStore  = useAuthStore()

// ── Theme options for QuestionFormSheet ───────────────────────────────────
const themeOptions = computed(() =>
  topicStore.themes.flatMap(th => [
    { id: th.id, name: th.name },
    ...th.children.map(c => ({ id: c.id, name: `${th.name} › ${c.name}` })),
  ])
)

// ── Theme lookup map: themeId → name ──────────────────────────────────────
const themeNameMap = computed(() => {
  const map = new Map<number, string>()
  topicStore.themes.forEach(th => {
    map.set(th.id, th.name)
    th.children.forEach(c => map.set(c.id, `${th.name} › ${c.name}`))
  })
  return map
})

function getThemeName(themeId?: number): string | undefined {
  if (!themeId) return undefined
  return themeNameMap.value.get(themeId)
}

// ── Data ───────────────────────────────────────────────────────────────────
const loading       = ref(true)
const error         = ref<string | null>(null)
const questionnaire = ref<QuestionnaireItem | null>(null)
const isPublishing  = ref(false)

const questionnaireId = computed(() => Number(route.params.id))

onMounted(async () => {
  await topicStore.fetchThemes()
  await fetchQuestionnaire()
})

async function fetchQuestionnaire() {
  loading.value = true
  error.value   = null
  const { data, error: err } = await getQuestionnaireByIdApi(questionnaireId.value)
  if (err) {
    error.value = err
  } else {
    questionnaire.value = data!
  }
  loading.value = false
}

// ── Sorted questions ───────────────────────────────────────────────────────
const sortedQuestions = computed<QuestionnaireQuestionItem[]>(() =>
  [...(questionnaire.value?.questionnaireQuestions ?? [])].sort((a, b) => a.order - b.order)
)

// ── Stats ──────────────────────────────────────────────────────────────────
const totalPoints = computed(() =>
  sortedQuestions.value.reduce((sum, qq) => sum + (qq.question?.points ?? 0) * qq.weight, 0)
)

const estimatedMinutes = computed(() => {
  const totalSec = sortedQuestions.value.reduce((sum, qq) => {
    const limit = qq.question?.timeLimitSeconds
    return sum + (limit ?? 60)
  }, 0)
  return Math.ceil(totalSec / 60)
})

const typeStats = computed(() => {
  const counts: Record<string, number> = {}
  sortedQuestions.value.forEach(qq => {
    const type = qq.question?.type ?? 'QCU'
    counts[type] = (counts[type] ?? 0) + 1
  })
  return Object.entries(counts).map(([type, count]) => {
    if (type === 'QCU') return { type, count, label: t('questionnaireBuilder.typeQcu'),      icon: CircleDot,   style: 'bg-primary/10 border-primary/20 text-primary' }
    if (type === 'QCM') return { type, count, label: t('questionnaireBuilder.typeQcm'),      icon: CheckSquare, style: 'bg-orange-100 dark:bg-orange-950/30 border-orange-200 dark:border-orange-800 text-orange-700 dark:text-orange-300' }
    return {              type, count, label: t('questionnaireBuilder.typeTrueFalse'), icon: ToggleLeft,  style: 'bg-amber-100 dark:bg-amber-950/30 border-amber-200 dark:border-amber-800 text-amber-700 dark:text-amber-300' }
  })
})

// ── Status style ───────────────────────────────────────────────────────────
const statusStyle = computed(() => {
  switch (questionnaire.value?.status) {
    case 'Published': return { class: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-400', dot: 'bg-emerald-500' }
    case 'Archived':  return { class: 'bg-muted text-muted-foreground',                                               dot: 'bg-muted-foreground' }
    default:          return { class: 'bg-primary/10 text-primary dark:bg-primary/15 dark:text-orange-300',            dot: 'bg-primary' }
  }
})

// ── Add / Edit question ────────────────────────────────────────────────────
const isFormOpen      = ref(false)
const editingQuestion = ref<QuestionItem | null>(null)

function openAddQuestion() {
  editingQuestion.value = null
  isFormOpen.value      = true
}

function openEditQuestion(q: QuestionItem) {
  editingQuestion.value = q
  isFormOpen.value      = true
}

async function onQuestionSaved(q: QuestionItem) {
  if (!questionnaire.value) return

  const existingQQ = questionnaire.value.questionnaireQuestions.find(
    qq => qq.questionId === q.id
  )

  if (existingQQ) {
    // Update in place
    existingQQ.question = q
  } else {
    // New question: add to questionnaire via API
    const nextOrder = (questionnaire.value.questionnaireQuestions[questionnaire.value.questionnaireQuestions.length - 1]?.order ?? 0) + 1
    const { data: updatedQuestionnaire, error: addErr } = await addQuestionToQuestionnaireApi(
      questionnaireId.value,
      { questionId: q.id, order: nextOrder, weight: 1, isMandatory: true },
    )
    if (addErr) {
      error.value = addErr
      return
    }
    if (updatedQuestionnaire) {
      questionnaire.value = updatedQuestionnaire
    } else {
      // Optimistic update fallback
      questionnaire.value.questionnaireQuestions.push({
        id:              Date.now(),
        questionnaireId: questionnaireId.value,
        questionId:      q.id,
        order:           nextOrder,
        weight:          1,
        isMandatory:     true,
        question:        q,
      })
    }
  }
}

// ── Remove question ────────────────────────────────────────────────────────
const isDeleteOpen = ref(false)
const toRemove     = ref<QuestionItem | null>(null)

function confirmRemove(q: QuestionItem) {
  toRemove.value     = q
  isDeleteOpen.value = true
}

const isRemoving = ref(false)

async function doRemove() {
  if (!questionnaire.value || !toRemove.value) return
  isRemoving.value = true
  const { data, error: removeErr } = await removeQuestionFromQuestionnaireApi(
    questionnaireId.value,
    toRemove.value.id,
  )
  isRemoving.value = false
  if (removeErr) {
    error.value = removeErr
  } else if (data) {
    questionnaire.value = data
  } else {
    // optimistic fallback
    questionnaire.value.questionnaireQuestions =
      questionnaire.value.questionnaireQuestions.filter(
        qq => qq.questionId !== toRemove.value!.id
      )
  }
  toRemove.value     = null
  isDeleteOpen.value = false
}

// ── Publish ────────────────────────────────────────────────────────────────
async function publishQuestionnaire() {
  if (!questionnaire.value) return
  if (sortedQuestions.value.length === 0) {
    error.value = t('questionnaireBuilder.noQuestionsError')
    return
  }
  isPublishing.value = true
  try {
    const { data, error: err } = await publishQuestionnaireApi(questionnaireId.value)
    if (err) {
      error.value = err
      return
    }
    if (data) questionnaire.value = data
    else if (questionnaire.value) questionnaire.value.status = 'Published'
  } finally {
    isPublishing.value = false
  }
}

// ── AI Question Generator ──────────────────────────────────────────────────
const aiPanelOpen          = ref(false)
const aiFormExpanded       = ref(true)
const aiGenerating         = ref(false)
const aiGenerateCount      = ref(3)
const aiGeneratedQuestions = ref<QuestionItem[]>([])
const aiSelectedIds        = ref(new Set<number>())
const isAddingAiQuestions  = ref(false)
const aiGenerationError    = ref<string | null>(null)

// Generation options
const aiDifficulty      = ref('Mixed')
const aiTopics          = ref<string[]>([])
const aiTopicInput      = ref('')
const aiCustomContext   = ref('')
const aiContextExpanded = ref(false)


const difficultyLevels = [
  { value: 'Mixed',        label: 'Mixed' },
  { value: 'Beginner',     label: 'Beginner' },
  { value: 'Intermediate', label: 'Intermediate' },
  { value: 'Advanced',     label: 'Advanced' },
  { value: 'Expert',       label: 'Expert' },
]

function addTopic(tag: string) {
  const clean = tag.trim().replace(/,$/, '')
  if (clean && !aiTopics.value.includes(clean)) aiTopics.value.push(clean)
  aiTopicInput.value = ''
}

function removeTopic(idx: number) {
  aiTopics.value.splice(idx, 1)
}

// Available AI models
const aiAvailableModels    = ref<AiModelItem[]>([])
const aiSelectedModelId    = ref<number | null>(null)

// Theme selector for AI generation
const aiSelectedThemeId = ref<number | null>(null)
const aiSelectedTheme = computed(() => {
  if (!aiSelectedThemeId.value) return null
  for (const th of topicStore.themes) {
    if (th.id === aiSelectedThemeId.value) return { id: th.id, name: th.name }
    const child = th.children.find(c => c.id === aiSelectedThemeId.value)
    if (child) return { id: child.id, name: `${th.name} › ${child.name}` }
  }
  return null
})

// AI theme picker dialog
const aiThemePickerOpen     = ref(false)
const aiThemePickerExpanded = ref(new Set<number>())

function toggleAiThemeExpand(id: number) {
  if (aiThemePickerExpanded.value.has(id)) {
    aiThemePickerExpanded.value.delete(id)
  } else {
    aiThemePickerExpanded.value.add(id)
  }
  aiThemePickerExpanded.value = new Set(aiThemePickerExpanded.value)
}

function selectAiTheme(id: number | null) {
  aiSelectedThemeId.value = id
  aiThemePickerOpen.value = false
}

const aiSelectedThemeName = computed(() => {
  if (!aiSelectedThemeId.value) return ''
  for (const th of topicStore.themes) {
    if (th.id === aiSelectedThemeId.value) return th.name
    const sub = th.children.find(c => c.id === aiSelectedThemeId.value)
    if (sub) return `${th.name} › ${sub.name}`
  }
  return ''
})

async function loadAiModels() {
  const { data } = await getAiModelsApi()
  if (data) {
    aiAvailableModels.value = data.filter(m => m.isEnabled)
    const def = data.find(m => m.isDefault && m.isEnabled)
    aiSelectedModelId.value = def?.id ?? data[0]?.id ?? null
  }
}

function toggleAiPanel() {
  aiPanelOpen.value = !aiPanelOpen.value
  if (aiPanelOpen.value && aiAvailableModels.value.length === 0) loadAiModels()
}

function toggleAiSelection(idx: number) {
  const s = new Set(aiSelectedIds.value)
  if (s.has(idx)) s.delete(idx)
  else s.add(idx)
  aiSelectedIds.value = s
}

function toggleSelectAll() {
  if (aiSelectedIds.value.size === aiGeneratedQuestions.value.length) {
    aiSelectedIds.value = new Set()
  } else {
    aiSelectedIds.value = new Set(aiGeneratedQuestions.value.map((_, i) => i))
  }
}

async function generateAiQuestions() {
  aiGenerating.value    = true
  aiFormExpanded.value  = false
  aiGenerationError.value = null
  aiGeneratedQuestions.value = []
  aiSelectedIds.value = new Set()

  // Build locale → language name mapping
  const localeToLang: Record<string, string> = { en: 'English', fr: 'French', ar: 'Arabic' }
  const language = localeToLang[locale.value] ?? 'English'

  const existingTitles = sortedQuestions.value
    .map(qq => qq.question?.title)
    .filter(Boolean) as string[]

  const selectedTheme = aiSelectedTheme.value

  const { data, error: genError } = await generateQuestionsApi({
    modelId: aiSelectedModelId.value,
    count:   aiGenerateCount.value,
    context: {
      questionnaireTitle:       questionnaire.value?.title ?? '',
      questionnaireDescription: questionnaire.value?.description ?? null,
      themeId:                  selectedTheme?.id ?? null,
      themeName:                selectedTheme?.name ?? null,
      language,
      existingQuestions:        existingTitles,
      difficulty:               aiDifficulty.value !== 'Mixed' ? aiDifficulty.value : null,
      topics:                   aiTopics.value.length > 0 ? [...aiTopics.value] : null,
      customContext:             aiCustomContext.value.trim() || null,
    },
  })

  if (genError || !data) {
    aiGenerationError.value = genError ?? 'Generation failed'
    aiGenerating.value  = false
    aiFormExpanded.value = true
    return
  }

  // Map generated DTOs to QuestionItem shape (negative ids = not persisted yet)
  const generated: QuestionItem[] = data.map((dto, i) => ({
    id:               -(i + 1),
    title:            dto.title,
    type:             dto.type,
    complexity:       dto.complexity,
    points:           dto.points,
    timeLimitSeconds: dto.timeLimitSeconds ?? null,
    explanation:      dto.explanation ?? null,
    themeId:          selectedTheme?.id ?? themeOptions.value[0]?.id ?? 1,
    companyId:        authStore.state.company?.id ?? 0,
    createdById:      0,
    createdAt:        new Date().toISOString(),
    enable:           true,
    choices:          dto.choices.map((c, ci) => ({ id: ci + 1, text: c.text, isCorrect: c.isCorrect, order: c.order })),
  }))

  aiGeneratedQuestions.value = generated
  aiSelectedIds.value = new Set(generated.map((_, i) => i))
  aiGenerating.value = false
}

async function addSelectedAiQuestions() {
  if (!questionnaire.value || aiSelectedIds.value.size === 0) return
  isAddingAiQuestions.value = true

  const selected = [...aiSelectedIds.value].map(i => aiGeneratedQuestions.value[i])
  let nextOrder = (questionnaire.value.questionnaireQuestions[questionnaire.value.questionnaireQuestions.length - 1]?.order ?? 0) + 1

  for (const q of selected) {
    // 1. Create the question via API
    const { data: createdQ, error: createErr } = await createQuestionApi({
      title:            q!.title,
      type:             q!.type,
      complexity:       q!.complexity,
      points:           q!.points,
      timeLimitSeconds: q!.timeLimitSeconds ?? undefined,
      themeId:          q!.themeId,
      choices:          q!.choices.map(c => ({ text: c.text, isCorrect: c.isCorrect, order: c.order })),
    })

    if (createErr || !createdQ) {
      error.value = createErr ?? 'Failed to create AI question'
      continue
    }

    // 2. Add to questionnaire
    const { data: updatedQ } = await addQuestionToQuestionnaireApi(
      questionnaireId.value,
      { questionId: createdQ.id, order: nextOrder, weight: 1, isMandatory: true },
    )

    if (updatedQ) {
      questionnaire.value = updatedQ
    } else {
      // Optimistic
      questionnaire.value.questionnaireQuestions.push({
        id:              Date.now() + nextOrder,
        questionnaireId: questionnaireId.value,
        questionId:      createdQ.id,
        order:           nextOrder,
        weight:          1,
        isMandatory:     true,
        question:        createdQ,
      })
    }
    nextOrder++
  }

  // Remove added questions from AI panel
  aiGeneratedQuestions.value = aiGeneratedQuestions.value.filter((_, i) => !aiSelectedIds.value.has(i))
  aiSelectedIds.value = new Set()
  isAddingAiQuestions.value = false
}
</script>
