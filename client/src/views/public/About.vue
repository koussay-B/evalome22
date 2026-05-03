<template>
  <div class="overflow-x-hidden">

    <!-- ═══════════════════════════ HERO ════════════════════════════ -->
    <section class="relative py-28 md:py-36 text-center">
      <div class="absolute inset-0 -z-10 overflow-hidden">
        <div class="animate-orb absolute -top-16 left-1/4 h-[600px] w-[600px] rounded-full opacity-20 blur-[110px] brand-gradient" />
        <div class="animate-orb-delay absolute top-20 right-1/5 h-[500px] w-[500px] rounded-full opacity-15 blur-[100px] brand-gradient-warm" />
        <div class="absolute inset-0 [background-image:radial-gradient(circle,oklch(0.6_0.22_270/0.07)_1px,transparent_1px)] [background-size:28px_28px]" />
      </div>
      <div class="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
        <span class="text-xs font-bold uppercase tracking-[0.2em] brand-text-gradient">{{ t('public.about.badge') }}</span>
        <h1 class="mt-3 text-5xl md:text-6xl text-slate-800 tracking-tight leading-[1.05] animate-fade-in-up">
          {{ t('public.about.heroTitle') }}
          <span class="brand-text-gradient"> {{ t('public.about.heroTitleHighlight') }}</span>
        </h1>
        <p class="mt-6 text-lg text-muted-foreground max-w-xl mx-auto leading-relaxed animate-fade-in-up" style="animation-delay:100ms">
          {{ t('public.about.heroSubtitle') }}
        </p>
      </div>
    </section>

    <!-- ═══════════════════════ STATS COUNTERS ════════════════════════ -->
    <section class="pb-20">
      <div class="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8">
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div
            v-for="stat in aboutStats"
            :key="stat.label"
            class="group relative rounded-2xl border border-border/60 bg-card p-7 text-center hover:border-brand/40 hover:shadow-xl hover:shadow-brand/8 transition-all duration-300 overflow-hidden"
          >
            <div class="absolute inset-0 opacity-0 group-hover:opacity-100 mesh-bg transition-opacity duration-500" />
            <div class="relative">
              <div class="h-10 w-10 rounded-xl brand-gradient flex items-center justify-center shadow-md mx-auto mb-4">
                <component :is="stat.icon" class="h-5 w-5 text-white" />
              </div>
              <div class="text-3xl md:text-4xl text-slate-800 brand-text-gradient">{{ stat.value }}</div>
              <div class="text-sm text-muted-foreground mt-2 leading-snug">{{ stat.label }}</div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ═══════════════════════════ STORY ════════════════════════════ -->
    <section class="pb-24">
      <div class="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8">
        <div class="relative rounded-3xl border border-border/60 bg-card overflow-hidden p-10 md:p-16">
          <!-- Accent line at top -->
          <div class="absolute top-0 left-0 h-1 w-full brand-gradient" />
          <!-- Decorative orb -->
          <div class="absolute -top-20 -right-20 h-48 w-48 brand-gradient opacity-8 rounded-full blur-3xl pointer-events-none" />
          <!-- Corner accent number -->
          <div class="absolute -bottom-6 -right-4 text-[140px] text-slate-800 brand-text-gradient opacity-5 leading-none select-none pointer-events-none">
            01
          </div>
          <div class="relative">
            <div class="flex items-center gap-3 mb-8">
              <div class="h-10 w-10 rounded-xl brand-gradient flex items-center justify-center shadow-md">
                <BookOpen class="h-5 w-5 text-white" />
              </div>
              <h2 class="text-3xl md:text-4xl text-slate-800">{{ t('public.about.storyTitle') }}</h2>
            </div>
            <div class="space-y-5 text-muted-foreground text-lg leading-relaxed">
              <p>{{ t('public.about.storyP1') }}</p>
              <p>{{ t('public.about.storyP2') }}</p>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ═══════════════════════════ VALUES ═══════════════════════════ -->
    <section class="py-24 relative overflow-hidden">
      <div class="absolute inset-0 -z-10 mesh-bg" />
      <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div class="text-center mb-14">
          <span class="text-xs font-bold uppercase tracking-[0.2em] brand-text-gradient">{{ t('public.about.valuesBadge') }}</span>
          <h2 class="mt-3 text-4xl md:text-5xl text-slate-800">{{ t('public.about.valuesTitle') }}</h2>
        </div>

        <div class="grid sm:grid-cols-2 lg:grid-cols-4 gap-5">
          <div
            v-for="(value, i) in values"
            :key="value.titleKey"
            class="group relative rounded-2xl border border-border/60 bg-card p-8 hover:border-brand/40 hover:shadow-2xl hover:shadow-brand/8 hover:-translate-y-1.5 transition-all duration-400 overflow-hidden"
          >
            <div class="absolute inset-0 opacity-0 group-hover:opacity-100 mesh-bg transition-opacity duration-500" />
            <!-- Large accent number -->
            <div class="absolute -top-3 -right-2 text-[80px] text-slate-800 brand-text-gradient opacity-6 leading-none pointer-events-none select-none">
              {{ i + 1 }}
            </div>
            <div class="relative h-12 w-12 rounded-xl brand-gradient flex items-center justify-center shadow-lg mb-6 group-hover:scale-110 transition-transform duration-300">
              <component :is="value.icon" class="h-6 w-6 text-white" />
            </div>
            <h3 class="relative text-lg text-slate-800 mb-3">{{ t(value.titleKey) }}</h3>
            <p class="relative text-sm text-muted-foreground leading-relaxed">{{ t(value.descKey) }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- ═══════════════════════════ TEAM ═════════════════════════════ -->
    <section class="py-24">
      <div class="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8">
        <div class="text-center mb-14">
          <span class="text-xs font-bold uppercase tracking-[0.2em] brand-text-gradient">{{ t('public.about.teamBadge') }}</span>
          <h2 class="mt-3 text-4xl md:text-5xl text-slate-800">{{ t('public.about.teamTitle') }}</h2>
          <p class="mt-4 text-muted-foreground text-lg max-w-xl mx-auto">{{ t('public.about.teamSubtitle') }}</p>
        </div>

        <div class="grid sm:grid-cols-2 lg:grid-cols-4 gap-5">
          <div
            v-for="member in team"
            :key="member.name"
            class="group relative rounded-2xl border border-border/60 bg-card p-7 text-center hover:border-brand/40 hover:shadow-xl hover:shadow-brand/8 hover:-translate-y-1 transition-all duration-300 overflow-hidden"
          >
            <!-- Glow blob -->
            <div class="absolute -top-10 -right-10 h-24 w-24 brand-gradient opacity-10 rounded-full blur-2xl pointer-events-none" />
            <!-- Avatar -->
            <div class="relative mx-auto mb-5 h-20 w-20">
              <div class="absolute inset-0 brand-gradient rounded-full opacity-25 blur-md" />
              <div class="relative h-20 w-20 rounded-full brand-gradient flex items-center justify-center text-white text-2xl text-slate-800 shadow-lg">
                {{ member.initials }}
              </div>
            </div>
            <h4 class="text-slate-800 text-lg">{{ member.name }}</h4>
            <!-- Role badge -->
            <div class="mt-2 inline-flex items-center gap-1.5 rounded-full border border-border/60 bg-muted/40 px-3 py-1">
              <div class="h-1.5 w-1.5 rounded-full brand-gradient" />
              <span class="text-xs font-medium text-muted-foreground">{{ member.role }}</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ═══════════════════════════ CTA ══════════════════════════════ -->
    <section class="py-24">
      <div class="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
        <div class="relative rounded-3xl overflow-hidden">
          <div class="absolute inset-0 brand-gradient" />
          <div class="absolute inset-0 [background:radial-gradient(ellipse_60%_80%_at_0%_100%,oklch(1_0_0/0.10),transparent),radial-gradient(ellipse_50%_70%_at_100%_0%,oklch(1_0_0/0.08),transparent)]" />
          <div class="absolute -top-16 -right-16 h-48 w-48 bg-white/10 rounded-full blur-3xl pointer-events-none" />
          <div class="absolute -bottom-16 -left-16 h-40 w-40 bg-white/10 rounded-full blur-3xl pointer-events-none" />

          <div class="relative px-10 py-16 md:px-16 md:py-20 text-center">
            <h2 class="text-3xl md:text-4xl text-slate-800 text-white">{{ t('public.about.ctaTitle') }}</h2>
            <p class="mt-4 text-white/70 text-lg max-w-sm mx-auto leading-relaxed">{{ t('public.about.ctaSubtitle') }}</p>
            <div class="mt-8 flex flex-col sm:flex-row items-center justify-center gap-4">
              <RouterLink to="/company-request">
                <Button size="lg" class="bg-white text-slate-800 font-bold px-10 shadow-xl hover:bg-white/95 border-0" style="height:3.25rem">
                  {{ t('public.about.ctaButton') }}
                  <ArrowRight class="ml-2 h-4 w-4" />
                </Button>
              </RouterLink>
              <RouterLink to="/features">
                <Button size="lg" variant="ghost" class="text-white hover:bg-white/15 border border-white/30 font-semibold px-8" style="height:3.25rem">
                  {{ t('public.about.ctaSecondary') }}
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
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowRight, Target, BarChart3, Heart, Sparkles, Building2, UserCheck, Layers, BookOpen } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { useLocale } from '@/composables/useLocale'

const { t } = useLocale()

const aboutStats = [
  { value: '200+', label: 'Companies onboarded', icon: Building2 },
  { value: '15K+', label: 'Candidates assessed', icon: UserCheck },
  { value: '1,200+', label: 'Tests published', icon: Layers },
  { value: '92%', label: 'Average pass rate', icon: BarChart3 },
]

const values = computed(() => [
  { icon: Target, titleKey: 'public.about.value1Title', descKey: 'public.about.value1Desc' },
  { icon: BarChart3, titleKey: 'public.about.value2Title', descKey: 'public.about.value2Desc' },
  { icon: Heart, titleKey: 'public.about.value3Title', descKey: 'public.about.value3Desc' },
  { icon: Sparkles, titleKey: 'public.about.value4Title', descKey: 'public.about.value4Desc' },
])

const team = [
  { name: 'Yassine A.', initials: 'YA', role: 'Founder & Full-Stack' },
  { name: 'Sarah M.', initials: 'SM', role: 'Product & UX Design' },
  { name: 'Karim D.', initials: 'KD', role: 'Backend Lead' },
  { name: 'Amina L.', initials: 'AL', role: 'People & Talent' },
]
</script>
