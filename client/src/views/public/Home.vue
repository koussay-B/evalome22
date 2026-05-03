<template>
 <div class="overflow-x-hidden">

    <!-- ═══════════════════════════════════════════════════════ -->
    <!--  HERO                                                   -->
    <!-- ═══════════════════════════════════════════════════════ -->
    
    <!-- 1. justify-start: bch yabda mel fouq -->
    <!-- 2. pt-20: padding sghir bch may-idokch fil navbar -->
    <section class="relative min-h-screen flex flex-col items-center justify-start pt-20 pb-20">
      
      <!-- Background (ma baddelna chay) -->
      <div class="absolute inset-0 -z-10 overflow-hidden">
        <div class="animate-orb absolute -top-40 -left-40 h-[900px] w-[900px] rounded-full opacity-15 blur-[160px] bg-[oklch(65%_0.18_51.057)]" />
        <div class="animate-orb-delay absolute -bottom-40 right-0 h-[700px] w-[700px] rounded-full opacity-10 blur-[130px] bg-[oklch(65%_0.18_51.057)]" />
        <div class="absolute inset-0 [background-image:radial-gradient(circle,oklch(65%_0.18_51.057_/_0.07)_1px,transparent_1px)] [background-size:32px_32px]" />
        <div class="absolute inset-0 [background:radial-gradient(ellipse_80%_50%_at_50%_0%,transparent_60%,oklch(0.99_0_0/0.95)_100%)] dark:[background:radial-gradient(ellipse_80%_50%_at_50%_0%,transparent_60%,oklch(0.13_0_0/0.95)_100%)]" />
      </div>

      <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 text-center w-full">
        
        <!-- Badge -->
        <div class="inline-flex items-center gap-2.5 rounded-full border border-[oklch(65%_0.18_51.057_/_0.2)] bg-[oklch(97.5%_0.02_51.057_/_0.5)] px-5 py-2 text-sm font-medium -mt-0.1 mb-2 animate-fade-in backdrop-blur-sm">
          <span class="relative flex h-2 w-2">
            <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-[oklch(65%_0.18_51.057)] opacity-60" />
            <span class="relative inline-flex rounded-full h-2 w-2 bg-[oklch(65%_0.18_51.057)]" />
          </span>
          <span class="text-slate-500 font-semibold tracking-tight">{{ t('public.home.badge.label') }}</span>
          <span class="text-[oklch(65%_0.18_51.057)] font-bold">{{ t('public.home.badge.status') }}</span>
        </div>
        <!-- Headline -->
        <h1 class="animate-fade-in-up  sm:text-9xl lg:text-[5.5rem] leading-[1.5] text-slate-800 tracking-tight leading-[0.88]">
          <span class="block text-7xl ">{{ t('public.home.heroTitle') }}</span>
          <span class="block brand-text-gradient ">{{ t('public.home.heroTitleHighlight') }}</span>
        </h1>

        <p class="mt-3 mx-auto max-w-lg text-lg text-muted-foreground leading-relaxed animate-fade-in-up" style="animation-delay:120ms">
          {{ t('public.home.heroSubtitle') }}
        </p>

        <!-- CTAs -->
        <div class="mt-5 flex flex-col sm:flex-row items-center justify-center gap-3 animate-fade-in-up" style="animation-delay:220ms">
          <RouterLink to="/register">
            <Button size="lg" class="relative overflow-hidden brand-gradient text-white border-0   text-base shadow-2xl shadow-brand/30 group" style="height:2.25rem">
              <span class="relative z-10 flex items-center gap-2">
                {{ t('public.home.ctaPrimary') }}
                <ArrowRight class="h-4 w-4 group-hover:translate-x-1 transition-transform" />
              </span>
              <span class="absolute inset-0 animate-shimmer bg-linear-to-r from-transparent via-white/20 to-transparent" />
            </Button>
          </RouterLink>
          <RouterLink to="/features">
            <Button variant="outline" size="lg" class="text-slate-800 px-8 text-base hover:bg-muted/60 backdrop-blur-sm" style="height:2.25rem">
              {{ t('public.home.ctaSecondary') }}
            </Button>
          </RouterLink>
        </div>

        <!-- Stats -->
        <div class="mt-8 flex flex-wrap items-center justify-center gap-2 sm:gap-3 animate-fade-in-up" style="animation-delay:320ms">
          <div
            v-for="(stat, i) in heroStats"
            :key="stat.label"
            class="flex items-center gap-3 rounded-2xl border border-border/50 bg-card/60 backdrop-blur-sm px-5 py-3 hover:border-brand/30 hover:bg-card/80 transition-all duration-300"
          >
            <div class="h-8 w-8 rounded-lg border border-brand/20 bg-brand/8 flex items-center justify-center shrink-0">
              <component :is="stat.icon" class="h-4 w-4 text-brand" />
            </div>
            <div class="text-left">
              <div class="text-lg text-slate-800 brand-text-gradient leading-tight">{{ stat.value }}</div>
              <div class="text-[11px] text-muted-foreground leading-none mt-0.5">{{ stat.label }}</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Product Preview -->
      <div class="relative mt-20 w-full max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 animate-fade-in-up" style="animation-delay:480ms">
        <div class="absolute inset-x-20 -bottom-6 h-20 brand-gradient opacity-20 blur-3xl rounded-full pointer-events-none" />
        <div class="relative rounded-3xl border border-border/50 bg-card shadow-2xl overflow-hidden ring-1 ring-white/5">
          <!-- Chrome -->
          <div class="flex items-center gap-3 px-5 h-11 bg-muted/40 border-b border-border/50">
            <div class="flex gap-1.5 shrink-0">
              <div class="h-3 w-3 rounded-full bg-red-400/60" />
              <div class="h-3 w-3 rounded-full bg-yellow-400/60" />
              <div class="h-3 w-3 rounded-full bg-green-400/60" />
            </div>
            <div class="flex-1 mx-3 h-6 rounded-full bg-background/60 border border-border/40 flex items-center px-3 gap-2">
              <div class="h-2 w-2 rounded-full bg-green-500/60 shrink-0" />
              <span class="text-[10px] text-muted-foreground font-mono">app.candidatapp.com/dashboard</span>
            </div>
          </div>
          <!-- App UI mockup -->
          <div class="flex h-72 bg-background/95">
            <div class="w-48 border-r border-border/30 bg-card/80 flex flex-col shrink-0">
              <div class="p-4 border-b border-border/30">
                <div class="flex items-center gap-2.5">
                  <div class="h-6 w-6 rounded-lg brand-gradient" />
                  <div class="h-2.5 w-20 rounded-full bg-muted" />
                </div>
              </div>
              <div class="p-2.5 flex-1 space-y-1">
                <div
                  v-for="(active, si) in [true, false, false, false, false]"
                  :key="si"
                  :class="['flex items-center gap-2.5 px-3 py-2 rounded-xl', active && 'bg-brand/10']"
                >
                  <div :class="['h-3 w-3 rounded shrink-0', active ? 'brand-gradient' : 'bg-muted/70']" />
                  <div :class="['h-2 rounded-full flex-1', active ? 'bg-brand/30' : 'bg-muted/50']" />
                </div>
              </div>
            </div>
            <div class="flex-1 p-5 overflow-hidden">
              <div class="grid grid-cols-4 gap-3 mb-4">
                <div
                  v-for="(sc, si) in previewStatCards"
                  :key="si"
                  class="rounded-2xl border border-border/40 bg-card p-3.5"
                >
                  <div class="h-1.5 w-12 rounded-full bg-muted mb-2.5" />
                  <div :class="['text-sm text-slate-800', si === 0 && 'brand-text-gradient']">{{ sc }}</div>
                  <div class="mt-2 h-1.5 rounded-full bg-muted overflow-hidden">
                    <div class="h-full brand-gradient rounded-full" :style="{ width: (20 + si * 22) + '%' }" />
                  </div>
                </div>
              </div>
              <div class="grid grid-cols-3 gap-3 h-36">
                <div class="col-span-2 rounded-2xl border border-border/40 bg-card p-3.5 flex flex-col">
                  <div class="h-1.5 w-16 rounded-full bg-muted mb-3" />
                  <div class="flex-1 flex items-end gap-1.5">
                    <div
                      v-for="(bh, bi) in [45, 62, 35, 80, 55, 92, 68, 85, 62, 95]"
                      :key="bi"
                      class="flex-1 rounded-t-sm brand-gradient opacity-50"
                      :style="{ height: bh + '%' }"
                    />
                  </div>
                </div>
                <div class="rounded-2xl border border-border/40 bg-card p-3.5">
                  <div class="h-1.5 w-12 rounded-full bg-muted mb-3" />
                  <div class="space-y-2.5">
                    <div v-for="ri in 3" :key="ri" class="flex items-center gap-2">
                      <div class="h-5 w-5 rounded-full brand-gradient opacity-60 shrink-0" />
                      <div class="h-1.5 rounded-full flex-1 bg-muted/60" />
                      <span class="text-[9px] text-slate-800 brand-text-gradient">{{ 68 + ri * 8 }}%</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ═══════════════════════════════════════════════════════ -->
    <!--  MARQUEE                                                 -->
    <!-- ═══════════════════════════════════════════════════════ -->
    <div class="border-y border-border/30 bg-muted/5 py-6 overflow-hidden">
      <p class="text-center text-[10px] font-bold uppercase tracking-[0.3em] text-muted-foreground/30 mb-5">
        {{ t('public.home.marqueeLabel') }}
      </p>
      <div class="flex gap-16 animate-marquee whitespace-nowrap">
        <span
          v-for="(brand, i) in [...trustedBy, ...trustedBy]"
          :key="i"
          class="text-[11px] font-bold text-muted-foreground/35 uppercase tracking-widest"
        >
          {{ brand }}
        </span>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════ -->
    <!--  BENTO FEATURES                                          -->
    <!-- ═══════════════════════════════════════════════════════ -->
    <section class="py-32">
      <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div class="text-center mb-20">
          <span class="inline-flex items-center gap-2 text-xs font-bold uppercase tracking-[0.2em] brand-text-gradient mb-3">
            <span class="h-px w-8 bg-gradient-to-r from-transparent to-brand/60" />
            {{ t('public.home.featuresBadge') }}
            <span class="h-px w-8 bg-gradient-to-l from-transparent to-brand/60" />
          </span>
          <h2 class="mt-2 text-4xl md:text-5xl text-slate-800 tracking-tight">
            {{ t('public.home.featuresTitle') }}
            <span class="brand-text-gradient"> {{ t('public.home.featuresTitleHighlight') }}</span>
          </h2>
          <p class="mt-4 text-muted-foreground text-lg max-w-xl mx-auto">{{ t('public.home.featuresSubtitle') }}</p>
        </div>

        <!-- Bento grid -->
        <div class="grid grid-cols-1 md:grid-cols-6 gap-4">

          <!-- Card 1: Assessments — 4 cols × 2 rows -->
          <div class="md:col-span-4 group relative rounded-3xl border border-border/50 bg-card overflow-hidden hover:border-brand/35 hover:shadow-2xl hover:shadow-brand/8 transition-all duration-500 p-8">
            <div class="absolute inset-0 opacity-0 group-hover:opacity-100 mesh-bg transition-opacity duration-500" />
            <!-- Icon + Title inline -->
            <div class="relative flex items-center gap-4 mb-4">
              <div class="inline-flex h-11 w-11 rounded-2xl brand-gradient items-center justify-center shadow-lg shrink-0 group-hover:scale-105 transition-transform duration-300">
                <ClipboardCheck class="h-5 w-5 text-white" />
              </div>
              <div>
                <h3 class="text-xl text-slate-800">{{ t('public.home.feature1Title') }}</h3>
                <p class="text-sm text-muted-foreground mt-0.5">{{ t('public.home.feature1Desc') }}</p>
              </div>
            </div>
            <!-- Tags -->
            <div class="relative flex flex-wrap gap-2 mb-6">
              <span
                v-for="key in ['qcu', 'antiCheat', 'scoring', 'timed']"
                :key="key"
                class="px-3 py-1 rounded-full text-xs font-semibold border border-border/60 text-muted-foreground bg-muted/30 hover:border-brand/30 hover:text-foreground transition-colors cursor-default"
              >
                {{ t(`public.home.bento.assessments.tags.${key}`) }}
              </span>
            </div>
            <!-- Question mock list -->
            <div class="relative grid grid-cols-2 gap-2.5">
              <div
                v-for="q in assessmentMocks"
                :key="q.text"
                :class="['rounded-2xl border p-3.5 flex items-start gap-3 text-xs', q.ok ? 'border-green-500/20 bg-green-500/5' : 'border-red-400/20 bg-red-400/5']"
              >
                <div :class="['mt-0.5 h-5 w-5 rounded-full shrink-0 flex items-center justify-center text-[10px] text-slate-800', q.ok ? 'bg-green-500/15 text-green-600' : 'bg-red-400/15 text-red-500']">
                  {{ q.ok ? '✓' : '✗' }}
                </div>
                <span class="text-muted-foreground leading-relaxed">{{ q.text }}</span>
              </div>
            </div>
          </div>

          <!-- Card 2: Analytics — 2 cols (brand filled) -->
          <div class="md:col-span-2 group relative rounded-3xl brand-gradient overflow-hidden p-7 flex flex-col justify-between shadow-xl min-h-[220px]">
            <div>
              <!-- Icon + Title inline -->
              <div class="flex items-center gap-3 mb-2">
                <div class="h-10 w-10 rounded-2xl bg-white/20 flex items-center justify-center shrink-0">
                  <BarChart3 class="h-5 w-5 text-white" />
                </div>
                <h3 class="text-lg text-slate-800 text-white leading-tight">{{ t('public.home.feature3Title') }}</h3>
              </div>
              <p class="text-sm text-white/65 leading-relaxed pl-[52px]">{{ t('public.home.feature3Desc') }}</p>
            </div>
            <div class="flex items-end gap-1.5 h-12 mt-6">
              <div
                v-for="(bh, bi) in [55, 80, 40, 95, 70, 60, 85]"
                :key="bi"
                class="flex-1 rounded-t bg-white/30 group-hover:bg-white/45 transition-colors duration-300"
                :style="{ height: bh + '%' }"
              />
            </div>
          </div>

          <!-- Card 3: Campaigns — 2 cols -->
          <div class="md:col-span-2 group relative rounded-3xl border border-border/50 bg-card overflow-hidden hover:border-brand/35 hover:shadow-xl hover:shadow-brand/8 transition-all duration-500 p-7 flex flex-col min-h-[200px]">
            <div class="absolute inset-0 opacity-0 group-hover:opacity-100 mesh-bg transition-opacity duration-500" />
            <!-- Icon + Title inline -->
            <div class="relative flex items-center gap-3 mb-5">
              <div class="h-10 w-10 rounded-2xl brand-gradient flex items-center justify-center shadow-md shrink-0 group-hover:scale-105 transition-transform duration-300">
                <Megaphone class="h-4.5 w-4.5 text-white" />
              </div>
              <h3 class="text-base text-slate-800">{{ t('public.home.feature2Title') }}</h3>
            </div>
            <div class="relative space-y-2.5 flex-1">
              <div v-for="c in mockCampaigns" :key="c.name" class="rounded-2xl bg-muted/30 border border-border/30 p-3">
                <div class="flex justify-between mb-1.5 text-xs">
                  <span class="font-semibold">{{ c.name }}</span>
                  <span class="brand-text-gradient font-bold">{{ c.pct }}%</span>
                </div>
                <div class="h-1.5 rounded-full bg-muted overflow-hidden">
                  <div class="h-full rounded-full brand-gradient" :style="{ width: c.pct + '%' }" />
                </div>
              </div>
            </div>
          </div>

          <!-- Card 4: Multi-role — 2 cols -->
          <div class="md:col-span-2 group relative rounded-3xl border border-border/50 bg-card overflow-hidden hover:border-brand/35 hover:shadow-xl transition-all duration-500 p-7 flex flex-col justify-between min-h-[200px]">
            <div class="absolute inset-0 opacity-0 group-hover:opacity-100 mesh-bg transition-opacity duration-500" />
            <!-- Icon + Title inline -->
            <div class="relative flex items-center gap-3">
              <div class="h-10 w-10 rounded-2xl brand-gradient flex items-center justify-center shadow-md shrink-0 group-hover:scale-105 transition-transform duration-300">
                <Users class="h-4.5 w-4.5 text-white" />
              </div>
              <div>
                <h3 class="text-base text-slate-800">{{ t('public.home.bento.roles.title') }}</h3>
                <p class="text-xs text-muted-foreground mt-0.5">{{ t('public.home.bento.roles.subtitle') }}</p>
              </div>
            </div>
            <div class="relative grid grid-cols-2 gap-2 text-xs font-bold mt-5">
              <span class="rounded-xl brand-gradient text-white px-3 py-2.5 text-center shadow-sm">{{ t('users.roles.admin') }}</span>
              <span class="rounded-xl bg-muted/50 border border-border/60 px-3 py-2.5 text-center">{{ t('users.roles.formateur') }}</span>
              <span class="rounded-xl bg-muted/50 border border-border/60 px-3 py-2.5 text-center">{{ t('companies.company') }}</span>
              <span class="rounded-xl brand-gradient text-white px-3 py-2.5 text-center shadow-sm">{{ t('users.roles.condidat') }}</span>
            </div>
          </div>

          <!-- Card 5: i18n — 1 col -->
          <div class="md:col-span-1 group relative rounded-3xl border border-border/50 bg-card overflow-hidden hover:border-brand/35 transition-all duration-500 p-6 flex flex-col justify-between min-h-[200px]">
            <!-- Icon + Title inline -->
            <div class="flex items-center gap-2.5">
              <div class="h-9 w-9 rounded-xl brand-gradient flex items-center justify-center shadow-md shrink-0">
                <Globe class="h-4 w-4 text-white" />
              </div>
              <div>
                <h3 class="text-sm text-slate-800">{{ t('public.home.bento.i18n.title') }}</h3>
                <p class="text-[11px] text-muted-foreground mt-0.5">{{ t('public.home.bento.i18n.subtitle') }}</p>
              </div>
            </div>
            <div class="flex flex-col gap-2 mt-4">
              <span class="px-3 py-1.5 rounded-xl brand-gradient text-white text-xs font-bold shadow-sm text-center">{{ t('lang.en') }}</span>
              <span class="px-3 py-1.5 rounded-xl bg-muted/50 border border-border/50 text-xs font-bold text-center">{{ t('lang.fr') }}</span>
              <span class="px-3 py-1.5 rounded-xl bg-muted/50 border border-border/50 text-xs font-bold text-center dir-rtl">{{ t('lang.ar') }}</span>
            </div>
          </div>

          <!-- Card 6: Dark/Light — 1 col -->
          <div class="md:col-span-1 group relative rounded-3xl border border-border/50 bg-card overflow-hidden hover:border-brand/35 transition-all duration-500 p-6 flex flex-col justify-between min-h-[200px]">
            <!-- Icon + Title inline -->
            <div class="flex items-center gap-2.5">
              <div class="h-9 w-9 rounded-xl bg-foreground/10 border border-border/60 flex items-center justify-center shrink-0">
                <Sun class="h-4 w-4 text-foreground/70" />
              </div>
              <div>
                <h3 class="text-sm text-slate-800">{{ t('public.home.bento.theme.title') }}</h3>
                <p class="text-[11px] text-muted-foreground mt-0.5">{{ t('public.home.bento.theme.subtitle') }}</p>
              </div>
            </div>
            <div class="flex gap-2 mt-4">
              <div class="h-10 w-10 rounded-2xl bg-foreground/90 shadow-sm flex-1" />
              <div class="h-10 w-10 rounded-2xl bg-muted border border-border flex-1" />
            </div>
          </div>

        </div>
      </div>
    </section>

    <!-- ═══════════════════════════════════════════════════════ -->
    <!--  HOW IT WORKS                                            -->
    <!-- ═══════════════════════════════════════════════════════ -->
    <section class="py-32 relative overflow-hidden">
      <div class="absolute inset-0 -z-10 mesh-bg" />
      <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div class="text-center mb-20">
          <span class="inline-flex items-center gap-2 text-xs font-bold uppercase tracking-[0.2em] brand-text-gradient mb-3">
            <span class="h-px w-8 bg-gradient-to-r from-transparent to-brand/60" />
            {{ t('public.home.howBadge') }}
            <span class="h-px w-8 bg-gradient-to-l from-transparent to-brand/60" />
          </span>
          <h2 class="mt-2 text-4xl md:text-5xl text-slate-800 tracking-tight">{{ t('public.home.howTitle') }}</h2>
          <p class="mt-4 text-muted-foreground text-lg max-w-lg mx-auto">{{ t('public.home.howSubtitle') }}</p>
        </div>

        <div class="grid md:grid-cols-3 gap-5 relative">
          <!-- Connector -->
          <div
            class="hidden md:block absolute top-10 left-[23%] right-[23%] h-px"
            style="background: linear-gradient(90deg, transparent, oklch(0.6 0.22 270 / 35%) 20%, oklch(0.6 0.22 270 / 35%) 80%, transparent)"
          />

          <div
            v-for="(step, i) in howSteps"
            :key="step.title"
            class="glass-card rounded-3xl p-8 hover:scale-[1.02] transition-transform duration-300 group"
          >
            <!-- Step number badge -->
            <div class="flex items-center gap-4 mb-5">
              <div class="relative h-12 w-12 rounded-2xl brand-gradient flex items-center justify-center shadow-xl shrink-0">
                <span class="text-lg text-slate-800 text-white">{{ i + 1 }}</span>
              </div>
              <!-- Icon next to number -->
              <div class="h-10 w-10 rounded-2xl bg-muted/60 border border-border/60 flex items-center justify-center group-hover:bg-brand/10 group-hover:border-brand/30 transition-colors">
                <component :is="step.icon" class="h-4.5 w-4.5 text-muted-foreground group-hover:text-brand transition-colors" />
              </div>
            </div>
            <h3 class="text-lg text-slate-800 mb-2">{{ step.title }}</h3>
            <p class="text-muted-foreground text-sm leading-relaxed">{{ step.desc }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- ═══════════════════════════════════════════════════════ -->
    <!--  TESTIMONIALS                                            -->
    <!-- ═══════════════════════════════════════════════════════ -->
    <section class="py-32">
      <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div class="text-center mb-20">
          <span class="inline-flex items-center gap-2 text-xs font-bold uppercase tracking-[0.2em] brand-text-gradient mb-3">
            <span class="h-px w-8 bg-gradient-to-r from-transparent to-brand/60" />
            {{ t('public.home.testimonialsBadge') }}
            <span class="h-px w-8 bg-gradient-to-l from-transparent to-brand/60" />
          </span>
          <h2 class="mt-2 text-4xl md:text-5xl text-slate-800 tracking-tight">
            {{ t('public.home.testimonialsTitle') }}
          </h2>
        </div>

        <div class="grid md:grid-cols-3 gap-5">
          <div
            v-for="(testimonial, i) in testimonials"
            :key="testimonial.author"
            :class="[
              'group relative rounded-3xl border bg-card p-8 flex flex-col transition-all duration-500 hover:-translate-y-1.5',
              i === 1
                ? 'md:-translate-y-5 border-brand/25 shadow-xl shadow-brand/10 hover:shadow-2xl hover:shadow-brand/15'
                : 'border-border/50 hover:border-brand/30 hover:shadow-2xl hover:shadow-brand/6',
            ]"
          >
            <div v-if="i === 1" class="absolute top-0 inset-x-0 h-0.5 brand-gradient rounded-t-3xl" />
            <!-- Stars -->
            <div class="flex gap-1 mb-5">
              <Star v-for="s in 5" :key="s" class="h-4 w-4 fill-yellow-400 text-yellow-400" />
            </div>
            <!-- Quote -->
            <blockquote class="text-foreground/80 leading-relaxed flex-1 text-sm mb-6">
              "{{ testimonial.quote }}"
            </blockquote>
            <!-- Author -->
            <div class="flex items-center gap-3 pt-5 border-t border-border/40">
              <div class="h-10 w-10 rounded-full brand-gradient flex items-center justify-center text-white text-slate-800 text-sm shadow-md shrink-0">
                {{ testimonial.author.charAt(0) }}
              </div>
              <div>
                <div class="font-bold text-sm">{{ testimonial.author }}</div>
                <div class="text-xs text-muted-foreground">{{ testimonial.role }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ═══════════════════════════════════════════════════════ -->
    <!--  FINAL CTA                                               -->
    <!-- ═══════════════════════════════════════════════════════ -->
    <section class="py-32">
      <div class="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8">
        <div class="relative rounded-[2.5rem] overflow-hidden">
          <div class="absolute inset-0 brand-gradient" />
          <div class="absolute inset-0 [background:radial-gradient(ellipse_60%_80%_at_0%_100%,oklch(1_0_0/0.1),transparent),radial-gradient(ellipse_50%_70%_at_100%_0%,oklch(1_0_0/0.12),transparent)]" />
          <div class="absolute -top-28 -right-28 h-72 w-72 bg-white/8 rounded-full blur-3xl pointer-events-none" />
          <div class="absolute -bottom-28 -left-28 h-64 w-64 bg-white/8 rounded-full blur-3xl pointer-events-none" />

          <div class="relative px-10 py-24 md:px-28 md:py-28 text-center">
            <h2 class="text-4xl md:text-5xl text-slate-800 tracking-tight text-white leading-tight">
              {{ t('public.home.ctaTitle') }}
            </h2>
            <p class="mt-5 text-white/65 text-lg max-w-md mx-auto leading-relaxed">
              {{ t('public.home.ctaSubtitle') }}
            </p>
            <div class="mt-10 flex flex-col sm:flex-row items-center justify-center gap-3">
              <RouterLink to="/register">
                <Button
                  size="lg"
                  class="bg-white text-foreground font-bold px-10 text-base shadow-2xl hover:bg-white/95 transition-all border-0"
                  style="height:3.25rem"
                >
                  {{ t('public.home.ctaButton') }}
                  <ArrowRight class="ml-2 h-4 w-4" />
                </Button>
              </RouterLink>
              <RouterLink to="/pricing">
                <Button
                  size="lg"
                  variant="ghost"
                  class="text-white hover:bg-white/12 font-semibold px-8 border border-white/25 hover:border-white/40 backdrop-blur-sm"
                  style="height:3.25rem"
                >
                  {{ t('public.home.ctaSecondaryPricing') }}
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
import {
  ArrowRight, ClipboardCheck, Megaphone, BarChart3, Users, Globe, Star,
  Building2, UserCheck, Layers, Sun,
} from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { useLocale } from '@/composables/useLocale'

const { t } = useLocale()

const heroStats = computed(() => [
  { value: '200+', label: t('public.home.statsCompanies'), icon: Building2 },
  { value: '15K+', label: t('public.home.statsCandidates'), icon: UserCheck },
  { value: '1,200+', label: t('public.home.statsTests'), icon: Layers },
  { value: '92%', label: t('public.home.statsPassRate'), icon: BarChart3 },
])

const previewStatCards = ['247', '1.2K', '94%', '12']

const trustedBy = [
  'TechVision  •', 'InnoGroup  •', 'GlobalHR  •', 'NexaTech  •', 'BrightHire  •',
  'TalentFirst  •', 'PeakRecruit  •', 'FutureTalent  •',
]

const assessmentMocks = [
  { text: t('public.home.bento.assessments.mocks.q1'), ok: true },
  { text: t('public.home.bento.assessments.mocks.q2'), ok: true },
  { text: t('public.home.bento.assessments.mocks.q3'), ok: false },
  { text: t('public.home.bento.assessments.mocks.q4'), ok: true },
]

const mockCampaigns = [
  { name: t('public.home.bento.campaigns.mocks.frontend'), pct: 82 },
  { name: t('public.home.bento.campaigns.mocks.ux'), pct: 64 },
  { name: t('public.home.bento.campaigns.mocks.backend'), pct: 91 },
]

const howSteps = computed(() => [
  { title: t('public.home.how1Title'), desc: t('public.home.how1Desc'), icon: ClipboardCheck },
  { title: t('public.home.how2Title'), desc: t('public.home.how2Desc'), icon: Megaphone },
  { title: t('public.home.how3Title'), desc: t('public.home.how3Desc'), icon: BarChart3 },
])

const testimonials = computed(() => [
  {
    quote: t('public.home.testimonial1'),
    author: t('public.home.testimonial1Author'),
    role: t('public.home.testimonial1Role'),
  },
  {
    quote: t('public.home.testimonial2'),
    author: t('public.home.testimonial2Author'),
    role: t('public.home.testimonial2Role'),
  },
  {
    quote: t('public.home.testimonial3'),
    author: t('public.home.testimonial3Author'),
    role: t('public.home.testimonial3Role'),
  },
])
</script>