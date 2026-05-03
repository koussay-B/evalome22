<template>
  <div class="overflow-x-hidden">

    <!-- ═══════════════════════════ HERO ════════════════════════════ -->
    <section class="relative py-28 md:py-36">
      <div class="absolute inset-0 -z-10 overflow-hidden">
        <div class="animate-orb absolute -top-20 right-0 h-[600px] w-[600px] rounded-full opacity-20 blur-[110px] brand-gradient" />
        <div class="absolute inset-0 [background-image:radial-gradient(circle,oklch(0.6_0.22_270/0.08)_1px,transparent_1px)] [background-size:28px_28px]" />
      </div>
      <div class="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8 text-center">
        <span class="text-xs font-bold uppercase tracking-[0.2em] brand-text-gradient">What you get</span>
        <h1 class="mt-3 text-5xl md:text-6xl text-slate-800 tracking-tight leading-[1.05] animate-fade-in-up">
          {{ t('public.features.heroTitle') }}
          <span class="brand-text-gradient"> {{ t('public.features.heroTitleHighlight') }}</span>
        </h1>
        <p class="mt-6 text-lg text-muted-foreground max-w-xl mx-auto animate-fade-in-up" style="animation-delay:100ms">
          {{ t('public.features.heroSubtitle') }}
        </p>

        <!-- Feature count indicator -->
        <div class="mt-8 inline-flex items-center gap-2 rounded-full border border-border/60 bg-muted/40 px-4 py-2 text-sm text-muted-foreground animate-fade-in-up" style="animation-delay:200ms">
          <div class="flex gap-1">
            <div v-for="n in 6" :key="n" class="h-1.5 w-4 rounded-full brand-gradient opacity-60" :style="{ opacity: 0.2 + n * 0.13 }" />
          </div>
          <span>6 core features</span>
        </div>
      </div>
    </section>

    <!-- ═══════════════════════ FEATURES ════════════════════════════ -->
    <section class="pb-28">
      <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 space-y-5">

        <div
          v-for="(feature, i) in featureList"
          :key="feature.titleKey"
          :class="[
            'group grid grid-cols-1 md:grid-cols-2 rounded-3xl border border-border/60 bg-card overflow-hidden',
            'hover:border-brand/40 hover:shadow-2xl hover:shadow-brand/8 transition-all duration-500',
          ]"
        >
          <!-- Content side -->
          <div :class="['p-10 md:p-14 flex flex-col justify-center', i % 2 === 1 && 'md:order-2']">
            <!-- Feature number -->
            <div class="text-[80px] text-slate-800 leading-none brand-text-gradient opacity-8 pointer-events-none select-none mb-2 -mt-4" aria-hidden="true">
              {{ String(i + 1).padStart(2, '0') }}
            </div>
            <div class="inline-flex h-12 w-12 rounded-xl brand-gradient items-center justify-center shadow-lg mb-5 group-hover:scale-110 transition-transform duration-300 -mt-2">
              <component :is="feature.icon" class="h-6 w-6 text-white" />
            </div>
            <h2 class="text-2xl md:text-3xl text-slate-800 leading-tight">{{ t(feature.titleKey) }}</h2>
            <p class="mt-4 text-muted-foreground leading-relaxed">{{ t(feature.descKey) }}</p>
            <!-- Tags -->
            <div class="mt-6 flex flex-wrap gap-2">
              <span
                v-for="tag in feature.tags"
                :key="tag"
                class="rounded-full border border-border/80 px-3 py-1 text-xs font-medium text-muted-foreground hover:border-brand/40 hover:text-foreground transition-colors cursor-default"
              >
                {{ tag }}
              </span>
            </div>
          </div>

          <!-- Visual side -->
          <div
            :class="[
              'relative overflow-hidden bg-muted/20 min-h-[280px] flex items-center justify-center p-10',
              i % 2 === 1 && 'md:order-1',
            ]"
          >
            <div class="absolute inset-0 opacity-50 group-hover:opacity-75 transition-opacity duration-500 mesh-bg" />

            <div class="relative w-full max-w-xs">
              <!-- Visual: Assessment questions -->
              <template v-if="i === 0">
                <div class="space-y-2.5">
                  <div
                    v-for="q in assessmentVisual"
                    :key="q.text"
                    :class="['rounded-xl border p-3.5 flex items-center gap-3', q.ok ? 'border-green-500/25 bg-green-500/5' : 'border-red-400/25 bg-red-400/5']"
                  >
                    <div :class="['h-5 w-5 rounded-full shrink-0 flex items-center justify-center text-xs text-slate-800', q.ok ? 'bg-green-500/15 text-green-600' : 'bg-red-400/15 text-red-500']">
                      {{ q.ok ? '✓' : '✗' }}
                    </div>
                    <div class="flex-1">
                      <div class="h-2 rounded bg-current opacity-30 mb-1.5" :style="{ width: q.w }" />
                      <div class="h-1.5 rounded bg-muted/60" style="width: 60%" />
                    </div>
                  </div>
                </div>
              </template>

              <!-- Visual: Campaign progress -->
              <template v-else-if="i === 1">
                <div class="space-y-3">
                  <div
                    v-for="c in [{ n: 'Sprint 2025', p: 78, count: '14/18' }, { n: 'Tech Lead Q2', p: 54, count: '7/13' }, { n: 'Designer Batch', p: 91, count: '11/12' }]"
                    :key="c.n"
                    class="rounded-xl border border-border/60 bg-card/80 p-4"
                  >
                    <div class="flex justify-between mb-2">
                      <span class="text-sm font-bold">{{ c.n }}</span>
                      <span class="text-xs text-muted-foreground">{{ c.count }}</span>
                    </div>
                    <div class="h-2 rounded-full bg-muted overflow-hidden">
                      <div class="h-full rounded-full brand-gradient transition-all duration-700" :style="{ width: c.p + '%' }" />
                    </div>
                    <div class="flex justify-end mt-1">
                      <span class="text-xs brand-text-gradient font-bold">{{ c.p }}%</span>
                    </div>
                  </div>
                </div>
              </template>

              <!-- Visual: Analytics chart -->
              <template v-else-if="i === 2">
                <div class="space-y-3">
                  <div class="rounded-xl border border-border/60 bg-card/80 p-4">
                    <div class="text-xs font-semibold text-muted-foreground mb-3">Score Distribution</div>
                    <div class="flex items-end gap-1.5 h-24">
                      <div
                        v-for="(h, hi) in [20, 45, 75, 95, 85, 60, 40, 25]"
                        :key="hi"
                        class="flex-1 rounded-t-lg brand-gradient opacity-70"
                        :style="{ height: h + '%' }"
                      />
                    </div>
                    <div class="flex justify-between mt-2 text-[10px] text-muted-foreground">
                      <span>0%</span><span>50%</span><span>100%</span>
                    </div>
                  </div>
                  <div class="grid grid-cols-2 gap-2">
                    <div class="rounded-xl border border-border/60 bg-card/80 p-3 text-center">
                      <div class="text-xl text-slate-800 brand-text-gradient">94%</div>
                      <div class="text-[10px] text-muted-foreground mt-1">Pass rate</div>
                    </div>
                    <div class="rounded-xl border border-border/60 bg-card/80 p-3 text-center">
                      <div class="text-xl text-slate-800 brand-text-gradient">4.2</div>
                      <div class="text-[10px] text-muted-foreground mt-1">Avg score</div>
                    </div>
                  </div>
                </div>
              </template>

              <!-- Visual: Roles -->
              <template v-else-if="i === 3">
                <div class="grid grid-cols-2 gap-3">
                  <div
                    v-for="role in roles"
                    :key="role.name"
                    :class="['rounded-2xl border p-4 text-center transition-all', role.highlight ? 'border-brand/30 bg-brand/5' : 'border-border/60 bg-card/80']"
                  >
                    <div class="h-10 w-10 rounded-full brand-gradient mx-auto mb-2.5 flex items-center justify-center shadow-md">
                      <span class="text-white text-sm text-slate-800">{{ role.name[0] }}</span>
                    </div>
                    <div class="text-sm font-bold">{{ role.name }}</div>
                    <div class="text-[10px] text-muted-foreground mt-1">{{ role.desc }}</div>
                  </div>
                </div>
              </template>

              <!-- Visual: i18n -->
              <template v-else-if="i === 4">
                <div class="space-y-4 text-center">
                  <div class="text-4xl text-slate-800 brand-text-gradient tracking-wide">EN · FR · عر</div>
                  <div class="h-px bg-border/60" />
                  <div class="space-y-2">
                    <div class="flex items-center justify-center gap-2 text-sm">
                      <div class="h-4 w-4 rounded brand-gradient opacity-80" />
                      <span class="text-muted-foreground">RTL & LTR layouts</span>
                    </div>
                    <div class="flex items-center justify-center gap-2 text-sm">
                      <div class="h-4 w-4 rounded brand-gradient opacity-80" />
                      <span class="text-muted-foreground">Language switcher</span>
                    </div>
                    <div class="flex items-center justify-center gap-2 text-sm">
                      <div class="h-4 w-4 rounded brand-gradient opacity-80" />
                      <span class="text-muted-foreground">Auto-detection</span>
                    </div>
                  </div>
                </div>
              </template>

              <!-- Visual: Theme -->
              <template v-else>
                <div class="flex gap-4 justify-center">
                  <div class="flex-1 rounded-2xl border border-border/60 bg-card/90 p-5 text-center space-y-2">
                    <div class="h-10 w-10 bg-foreground/90 rounded-full mx-auto" />
                    <div class="h-1.5 w-12 rounded bg-muted mx-auto" />
                    <div class="h-1 w-8 rounded bg-muted/60 mx-auto" />
                    <span class="text-xs font-bold text-muted-foreground">Dark</span>
                  </div>
                  <div class="flex-1 rounded-2xl border border-border/60 bg-white/70 dark:bg-muted/40 p-5 text-center space-y-2">
                    <div class="h-10 w-10 bg-gray-200 rounded-full mx-auto" />
                    <div class="h-1.5 w-12 rounded bg-gray-200 mx-auto" />
                    <div class="h-1 w-8 rounded bg-gray-100 mx-auto" />
                    <span class="text-xs font-bold text-muted-foreground">Light</span>
                  </div>
                </div>
              </template>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ═══════════════════════════ CTA ═════════════════════════════ -->
    <section class="py-28">
      <div class="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
        <div class="relative rounded-3xl overflow-hidden">
          <div class="absolute inset-0 brand-gradient" />
          <div class="absolute inset-0 [background:radial-gradient(ellipse_60%_80%_at_100%_0%,oklch(1_0_0/0.12),transparent)]" />
          <div class="absolute -top-16 -right-16 h-48 w-48 bg-white/10 rounded-full blur-3xl" />
          <div class="relative p-14 text-center">
            <h2 class="text-3xl md:text-4xl text-slate-800 text-white">{{ t('public.features.ctaTitle') }}</h2>
            <p class="mt-3 text-white/70 text-base max-w-sm mx-auto">Start with a free account. No credit card needed.</p>
            <div class="mt-8 flex flex-col sm:flex-row items-center justify-center gap-4">
              <RouterLink to="/company-request">
                <Button size="lg" class="bg-white text-foreground font-bold px-10 shadow-xl hover:bg-white/95 border-0" style="height:3.25rem">
                  {{ t('public.features.ctaButton') }}
                  <ArrowRight class="ml-2 h-4 w-4" />
                </Button>
              </RouterLink>
              <RouterLink to="/pricing">
                <Button size="lg" variant="ghost" class="text-white hover:bg-white/15 border border-white/30 font-semibold px-8" style="height:3.25rem">
                  See Pricing
                </Button>
              </RouterLink>
            </div>
          </div>
        </div>
      </div>
    </section>

  </div>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { ArrowRight, ClipboardCheck, Megaphone, BarChart3, Users, Globe, Moon } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { useLocale } from '@/composables/useLocale'

const { t } = useLocale()

const featureList = [
  {
    icon: ClipboardCheck,
    titleKey: 'public.features.assessmentsTitle',
    descKey: 'public.features.assessmentsDesc',
    tags: ['Multiple choice', 'True/False', 'Open questions', 'Themed libraries', 'Anti-cheat'],
  },
  {
    icon: Megaphone,
    titleKey: 'public.features.campaignsTitle',
    descKey: 'public.features.campaignsDesc',
    tags: ['Email invites', 'Deadlines', 'Attempt limits', 'Real-time tracking'],
  },
  {
    icon: BarChart3,
    titleKey: 'public.features.analyticsTitle',
    descKey: 'public.features.analyticsDesc',
    tags: ['Pass rates', 'Score distribution', 'Rankings', 'Trends', 'Export'],
  },
  {
    icon: Users,
    titleKey: 'public.features.rolesTitle',
    descKey: 'public.features.rolesDesc',
    tags: ['Admin', 'Company Admin', 'Formateur', 'Candidat'],
  },
  {
    icon: Globe,
    titleKey: 'public.features.i18nTitle',
    descKey: 'public.features.i18nDesc',
    tags: ['English', 'Français', 'العربية', 'RTL layout', 'Auto-detect'],
  },
  {
    icon: Moon,
    titleKey: 'public.features.themeTitle',
    descKey: 'public.features.themeDesc',
    tags: ['Dark mode', 'Light mode', 'System-aware', 'WCAG compliant'],
  },
]

const assessmentVisual = [
  { text: 'What is the time complexity of quicksort?', ok: true, w: '80%' },
  { text: 'Select all SOLID principles', ok: true, w: '70%' },
  { text: 'True or false: REST is stateful', ok: false, w: '65%' },
  { text: 'Which HTTP method for partial updates?', ok: true, w: '75%' },
]

const roles = [
  { name: 'Admin', desc: 'Full control', highlight: true },
  { name: 'Company', desc: 'Scoped access', highlight: false },
  { name: 'Formateur', desc: 'Create tests', highlight: false },
  { name: 'Candidat', desc: 'Take tests', highlight: true },
]
</script>
