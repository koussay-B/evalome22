<template>
  <div class="overflow-x-hidden">

    <!-- ════════════════════════ HERO ═══════════════════════════════ -->
    <section class="relative py-28 md:py-36">
      <div class="absolute inset-0 -z-10 overflow-hidden">
        <div class="animate-orb absolute -top-20 left-1/2 -translate-x-1/2 h-[600px] w-[600px] rounded-full opacity-20 blur-[110px] brand-gradient" />
        <div class="absolute inset-0 [background-image:radial-gradient(circle,oklch(0.6_0.22_270/0.08)_1px,transparent_1px)] [background-size:28px_28px]" />
      </div>
      <div class="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8 text-center">
        <span class="text-xs font-bold uppercase tracking-[0.2em] brand-text-gradient">Transparent pricing</span>
        <h1 class="mt-3 text-5xl md:text-6xl text-slate-800 tracking-tight leading-[1.05] animate-fade-in-up">
          {{ t('public.pricing.heroTitle') }}
          <span class="brand-text-gradient"> {{ t('public.pricing.heroTitleHighlight') }}</span>
        </h1>
        <p class="mt-6 text-lg text-muted-foreground animate-fade-in-up" style="animation-delay:100ms">
          {{ t('public.pricing.heroSubtitle') }}
        </p>

        <!-- Billing toggle -->
        <div class="mt-10 flex items-center justify-center gap-4 animate-fade-in-up" style="animation-delay:200ms">
          <span :class="['text-sm font-semibold transition-colors', !annual ? 'text-foreground' : 'text-muted-foreground']">
            Monthly
          </span>
          <button
            class="relative h-7 w-14 rounded-full transition-all duration-300 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand"
            :class="annual ? 'brand-gradient' : 'bg-muted border border-border'"
            @click="annual = !annual"
            aria-label="Toggle billing period"
          >
            <span
              class="absolute top-0.5 h-6 w-6 rounded-full bg-white shadow-md transition-transform duration-300"
              :class="annual ? 'translate-x-7' : 'translate-x-0.5'"
            />
          </button>
          <div class="flex items-center gap-2">
            <span :class="['text-sm font-semibold transition-colors', annual ? 'text-foreground' : 'text-muted-foreground']">
              Annual
            </span>
            <Transition name="badge">
              <span
                v-if="annual"
                class="inline-flex items-center gap-1 text-xs font-bold text-green-600 dark:text-green-400 bg-green-500/10 border border-green-500/20 px-2 py-0.5 rounded-full"
              >
                <TrendingDown class="h-3 w-3" />
                Save 20%
              </span>
            </Transition>
          </div>
        </div>
      </div>
    </section>

    <!-- ═══════════════════════ PRICING CARDS ════════════════════════ -->
    <section class="pb-28">
      <div class="mx-auto max-w-6xl px-4 sm:px-6 lg:px-8">
        <div class="grid md:grid-cols-3 gap-5 items-start">

          <!-- ── Starter ── -->
          <div class="relative rounded-2xl border border-border/60 bg-card p-8 flex flex-col hover:border-brand/30 hover:shadow-xl transition-all duration-300">
            <div class="mb-6">
              <div class="inline-flex items-center gap-2 mb-4">
                <div class="h-8 w-8 rounded-lg bg-muted border border-border flex items-center justify-center">
                  <Zap class="h-4 w-4 text-muted-foreground" />
                </div>
                <p class="text-sm font-bold uppercase tracking-wider text-muted-foreground">{{ t('public.pricing.freeName') }}</p>
              </div>
              <div class="flex items-baseline gap-1">
                <span class="text-5xl text-slate-800">{{ t('public.pricing.freePrice') }}</span>
              </div>
              <p class="mt-3 text-sm text-muted-foreground leading-relaxed">{{ t('public.pricing.freeDesc') }}</p>
            </div>
            <div class="h-px bg-border/60 mb-6" />
            <ul class="space-y-3 flex-1 mb-8">
              <li v-for="fKey in freeFeatures" :key="fKey" class="flex items-start gap-3 text-sm">
                <div class="h-5 w-5 rounded-full bg-muted border border-border flex items-center justify-center shrink-0 mt-0.5">
                  <Check class="h-3 w-3 text-muted-foreground" />
                </div>
                <span class="text-muted-foreground">{{ t(fKey) }}</span>
              </li>
            </ul>
            <RouterLink to="/company-request">
              <Button variant="outline" class="w-full font-semibold h-11 hover:bg-muted/60">
                {{ t('public.pricing.freeCta') }}
              </Button>
            </RouterLink>
          </div>

          <!-- ── Professional (elevated) ── -->
          <div class="relative md:-mt-6 md:mb-0">
            <!-- Gradient border wrapper -->
            <div class="relative rounded-2xl overflow-hidden p-[1.5px]">
              <div class="absolute inset-0 brand-gradient animate-pulse-glow rounded-2xl" />
              <div class="relative rounded-[14px] bg-card p-8 flex flex-col">
                <!-- Most Popular badge -->
                <div class="absolute -top-4 left-1/2 -translate-x-1/2 whitespace-nowrap">
                  <span class="brand-gradient text-white text-xs font-bold px-5 py-1.5 rounded-full shadow-lg inline-flex items-center gap-1.5">
                    <Sparkles class="h-3 w-3" />
                    {{ t('public.pricing.proBadge') }}
                  </span>
                </div>

                <div class="mb-6 pt-4">
                  <div class="inline-flex items-center gap-2 mb-4">
                    <div class="h-8 w-8 rounded-lg brand-gradient flex items-center justify-center shadow-sm">
                      <Rocket class="h-4 w-4 text-white" />
                    </div>
                    <p class="text-sm font-bold uppercase tracking-wider brand-text-gradient">{{ t('public.pricing.proName') }}</p>
                  </div>
                  <div class="flex items-baseline gap-1">
                    <span class="text-5xl text-slate-800">
                      {{ annual ? annualProPrice : t('public.pricing.proPrice') }}
                    </span>
                    <span class="text-muted-foreground font-medium">{{ t('public.pricing.proPriceUnit') }}</span>
                  </div>
                  <Transition name="fade">
                    <p v-if="annual" class="mt-1 text-xs text-green-600 dark:text-green-400 font-semibold">
                      Billed annually · Save 20%
                    </p>
                  </Transition>
                  <p class="mt-3 text-sm text-muted-foreground leading-relaxed">{{ t('public.pricing.proDesc') }}</p>
                </div>

                <div class="h-px bg-border/60 mb-6" />
                <ul class="space-y-3 flex-1 mb-8">
                  <li v-for="fKey in proFeatures" :key="fKey" class="flex items-start gap-3 text-sm">
                    <div class="h-5 w-5 rounded-full brand-gradient flex items-center justify-center shrink-0 mt-0.5 shadow-sm">
                      <Check class="h-3 w-3 text-white" />
                    </div>
                    <span>{{ t(fKey) }}</span>
                  </li>
                </ul>
                <RouterLink to="/company-request">
                  <Button class="w-full brand-gradient text-white border-0 font-bold h-11 shadow-lg hover:opacity-90 transition-all">
                    {{ t('public.pricing.proCta') }}
                    <ArrowRight class="ml-2 h-4 w-4" />
                  </Button>
                </RouterLink>
              </div>
            </div>
          </div>

          <!-- ── Enterprise ── -->
          <div class="relative rounded-2xl border border-border/60 bg-card p-8 flex flex-col hover:border-brand/30 hover:shadow-xl transition-all duration-300">
            <div class="mb-6">
              <div class="inline-flex items-center gap-2 mb-4">
                <div class="h-8 w-8 rounded-lg bg-muted border border-border flex items-center justify-center">
                  <Building2 class="h-4 w-4 text-muted-foreground" />
                </div>
                <p class="text-sm font-bold uppercase tracking-wider text-muted-foreground">{{ t('public.pricing.enterpriseName') }}</p>
              </div>
              <div class="flex items-baseline gap-1">
                <span class="text-5xl text-slate-800">{{ t('public.pricing.enterprisePrice') }}</span>
              </div>
              <p class="mt-3 text-sm text-muted-foreground leading-relaxed">{{ t('public.pricing.enterpriseDesc') }}</p>
            </div>
            <div class="h-px bg-border/60 mb-6" />
            <ul class="space-y-3 flex-1 mb-8">
              <li v-for="fKey in enterpriseFeatures" :key="fKey" class="flex items-start gap-3 text-sm">
                <div class="h-5 w-5 rounded-full bg-muted border border-border flex items-center justify-center shrink-0 mt-0.5">
                  <Check class="h-3 w-3 text-muted-foreground" />
                </div>
                <span class="text-muted-foreground">{{ t(fKey) }}</span>
              </li>
            </ul>
            <Button variant="outline" class="w-full font-semibold h-11">
              {{ t('public.pricing.enterpriseCta') }}
            </Button>
          </div>

        </div>

        <!-- Guarantee note -->
        <div class="mt-12 flex flex-col sm:flex-row items-center justify-center gap-6 text-sm text-muted-foreground">
          <div class="flex items-center gap-2">
            <Shield class="h-4 w-4 text-brand" />
            <span>14-day free trial, no credit card</span>
          </div>
          <div class="hidden sm:block h-1 w-1 rounded-full bg-border" />
          <div class="flex items-center gap-2">
            <Lock class="h-4 w-4 text-brand" />
            <span>Cancel anytime</span>
          </div>
          <div class="hidden sm:block h-1 w-1 rounded-full bg-border" />
          <div class="flex items-center gap-2">
            <HeadphonesIcon class="h-4 w-4 text-brand" />
            <span>Priority support on Pro+</span>
          </div>
        </div>
      </div>
    </section>

    <!-- ═══════════════════════════ FAQ ══════════════════════════════ -->
    <section class="py-24 relative overflow-hidden">
      <div class="absolute inset-0 -z-10 mesh-bg" />
      <div class="mx-auto max-w-2xl px-4 sm:px-6 lg:px-8">
        <div class="text-center mb-14">
          <span class="text-xs font-bold uppercase tracking-[0.2em] brand-text-gradient">FAQ</span>
          <h2 class="mt-3 text-3xl md:text-4xl text-slate-800">{{ t('public.pricing.faqTitle') }}</h2>
          <p class="mt-3 text-muted-foreground">Everything you need to know before getting started.</p>
        </div>

        <div class="space-y-3">
          <div
            v-for="faq in faqs"
            :key="faq.q"
            class="rounded-2xl border border-border/60 bg-card/80 overflow-hidden transition-all duration-300"
            :class="faq.open && 'border-brand/40 shadow-lg shadow-brand/5'"
          >
            <button
              class="w-full flex items-center justify-between p-6 text-left font-semibold hover:bg-muted/30 transition-colors"
              @click="faq.open = !faq.open"
            >
              <span>{{ faq.q }}</span>
              <div
                :class="[
                  'h-7 w-7 rounded-full border flex items-center justify-center shrink-0 transition-all duration-200',
                  faq.open ? 'brand-gradient border-transparent' : 'border-border',
                ]"
              >
                <ChevronDown
                  :class="['h-4 w-4 transition-transform duration-200', faq.open ? 'rotate-180 text-white' : 'text-muted-foreground']"
                />
              </div>
            </button>
            <Transition name="accordion">
              <div v-if="faq.open" class="px-6 pb-6">
                <p class="text-muted-foreground text-sm leading-relaxed">{{ faq.a }}</p>
              </div>
            </Transition>
          </div>
        </div>
      </div>
    </section>

    <!-- Bottom CTA -->
    <section class="py-24">
      <div class="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8 text-center">
        <h2 class="text-3xl text-slate-800">Still have questions?</h2>
        <p class="mt-3 text-muted-foreground">Our team is happy to help you find the right plan.</p>
        <div class="mt-6 flex items-center justify-center gap-4">
          <RouterLink to="/about">
            <Button variant="outline" class="font-semibold px-8 h-11">Contact Us</Button>
          </RouterLink>
          <RouterLink to="/company-request">
            <Button class="brand-gradient text-white border-0 font-bold px-8 h-11 shadow-lg hover:opacity-90">
              Start Free Trial
            </Button>
          </RouterLink>
        </div>
      </div>
    </section>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { RouterLink } from 'vue-router'
import {
  Check, ChevronDown, ArrowRight, Zap, Rocket, Building2,
  Sparkles, Shield, Lock, TrendingDown,
} from 'lucide-vue-next'
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { HeadphonesIcon } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { useLocale } from '@/composables/useLocale'

const { t } = useLocale()

const annual = ref(false)
const annualProPrice = '$39'   // 20% off the monthly price (placeholder)

const freeFeatures = [
  'public.pricing.freeFeature1',
  'public.pricing.freeFeature2',
  'public.pricing.freeFeature3',
  'public.pricing.freeFeature4',
  'public.pricing.freeFeature5',
]
const proFeatures = [
  'public.pricing.proFeature1',
  'public.pricing.proFeature2',
  'public.pricing.proFeature3',
  'public.pricing.proFeature4',
  'public.pricing.proFeature5',
  'public.pricing.proFeature6',
  'public.pricing.proFeature7',
]
const enterpriseFeatures = [
  'public.pricing.enterpriseFeature1',
  'public.pricing.enterpriseFeature2',
  'public.pricing.enterpriseFeature3',
  'public.pricing.enterpriseFeature4',
  'public.pricing.enterpriseFeature5',
  'public.pricing.enterpriseFeature6',
]

const faqs = reactive([
  { q: t('public.pricing.faq1Q'), a: t('public.pricing.faq1A'), open: false },
  { q: t('public.pricing.faq2Q'), a: t('public.pricing.faq2A'), open: false },
  { q: t('public.pricing.faq3Q'), a: t('public.pricing.faq3A'), open: false },
  { q: t('public.pricing.faq4Q'), a: t('public.pricing.faq4A'), open: false },
])
</script>

<style scoped>
.accordion-enter-active,
.accordion-leave-active {
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
  overflow: hidden;
}
.accordion-enter-from,
.accordion-leave-to {
  opacity: 0;
  max-height: 0;
  padding-bottom: 0;
}
.accordion-enter-to,
.accordion-leave-from {
  opacity: 1;
  max-height: 200px;
}

.badge-enter-active,
.badge-leave-active {
  transition: all 0.2s ease;
}
.badge-enter-from,
.badge-leave-to {
  opacity: 0;
  transform: scale(0.85);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
