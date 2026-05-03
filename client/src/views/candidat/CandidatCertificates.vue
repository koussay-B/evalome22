<template>
  <div class="space-y-6">

    <!-- Header -->
    <div>
      <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">
        {{ t('certificates.subtitle') }}
      </p>
      <h1 class="text-3xl text-slate-800 tracking-tight text-foreground">{{ t('certificates.title') }}</h1>
    </div>

    <!-- Error -->
    <div
      v-if="loadError"
      class="flex items-center gap-3 px-4 py-3 rounded-xl bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 text-sm text-red-700 dark:text-red-400"
    >
      <CircleAlert class="w-4 h-4 shrink-0" /> {{ loadError }}
      <button class="ms-auto text-xs font-bold underline" @click="load">{{ t('common.retry') }}</button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <div v-for="i in 3" :key="i" class="rounded-2xl border border-border bg-secondary p-6 space-y-4">
        <div class="flex items-center gap-3">
          <div class="w-12 h-12 rounded-2xl bg-muted animate-pulse shrink-0" />
          <div class="flex-1 space-y-2">
            <div class="h-4 rounded bg-muted animate-pulse w-3/4" />
            <div class="h-3 rounded bg-muted animate-pulse w-1/2" />
          </div>
        </div>
        <div class="h-8 rounded-lg bg-muted animate-pulse" />
      </div>
    </div>

    <!-- Empty -->
    <EmptyData
      v-else-if="certificates.length === 0 && !loadError"
      :icon="Award"
      :title="t('certificates.emptyTitle')"
      :description="t('certificates.emptyDesc')"
    >
      <button
        class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors mt-2"
        @click="router.push({ name: 'CandidatTests' })"
      >
        <ClipboardList class="w-4 h-4" /> {{ t('candidatTests.title') }}
      </button>
    </EmptyData>

    <!-- Certificate cards -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
  <div
    v-for="cert in certificates"
    :key="cert.id"
    class="relative rounded-2xl border border-[oklch(65%_0.18_51.057_/_0.2)] bg-white p-6 flex flex-col gap-4 overflow-hidden shadow-sm hover:shadow-md transition-all duration-300"
  >
    <div class="absolute -top-6 -end-6 w-24 h-24 rounded-full bg-[oklch(97.5%_0.02_51.057_/_0.8)]" />

    <div class="flex items-start gap-4 relative z-10">
      <div class="w-12 h-12 rounded-xl bg-[oklch(97.5%_0.02_51.057)] border border-[oklch(65%_0.18_51.057_/_0.1)] flex items-center justify-center shrink-0">
        <Award class="w-6 h-6 text-[oklch(65%_0.18_51.057)]" />
      </div>
      <div class="min-w-0 flex-1">
        <h3 class="text-slate-900 font-semibold truncate">{{ cert.campaignName }}</h3>
        <p class="text-[11px] text-slate-400 font-medium mt-0.5">
          {{ t('certificates.issuedOn') }} {{ formatDate(cert.issuedAt) }}
        </p>
      </div>
    </div>

    <div class="flex items-center gap-3 relative z-10">
      <span class="text-3xl font-medium text-slate-900 tracking-tight">{{ Math.round(cert.percentage) }}%</span>
      <span class="text-[10px] font-bold uppercase tracking-wider text-[oklch(65%_0.18_51.057)] bg-[oklch(97.5%_0.02_51.057)] px-2.5 py-0.5 rounded-full border border-[oklch(65%_0.18_51.057_/_0.1)]">
        {{ t('certificates.passed') }}
      </span>
    </div>

    <div class="bg-slate-50/80 rounded-xl px-3 py-2.5 flex items-center justify-between gap-2 relative z-10">
      <span class="text-[10px] font-mono text-slate-500 truncate">{{ cert.certificateCode }}</span>
      <button
        class="shrink-0 text-[11px] font-semibold text-[oklch(65%_0.18_51.057)] hover:underline"
        @click="copyCode(cert.certificateCode)"
      >
        {{ copied === cert.certificateCode ? '✓' : t('certificates.copy') }}
      </button>
    </div>

    <div class="flex gap-2 mt-2 relative z-10">
      <button
        class="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl bg-[oklch(65%_0.18_51.057)] text-white text-xs font-bold uppercase tracking-wide hover:opacity-90 transition-all"
        @click="router.push({ name: 'CertificatePage', params: { code: cert.certificateCode } })"
      >
        <Award class="w-3.5 h-3.5" /> View Certificate
      </button>
      
      <button
        class="flex items-center justify-center p-2.5 rounded-xl border border-[oklch(65%_0.18_51.057_/_0.2)] bg-[oklch(97.5%_0.02_51.057_/_0.5)] text-[oklch(65%_0.18_51.057)] hover:bg-[oklch(97.5%_0.02_51.057)] transition-all disabled:opacity-50"
        :disabled="downloading !== null"
        @click="downloadCertificate(cert)"
      >
        <Loader2 v-if="downloading === cert.id" class="w-4 h-4 animate-spin" />
        <Download v-else class="w-4 h-4" />
      </button>

      <a
        :href="`/certificate/${cert.certificateCode}`"
        target="_blank"
        rel="noopener"
        class="flex items-center justify-center p-2.5 rounded-xl border border-[oklch(65%_0.18_51.057_/_0.2)] bg-[oklch(97.5%_0.02_51.057_/_0.5)] text-[oklch(65%_0.18_51.057)] hover:bg-[oklch(97.5%_0.02_51.057)] transition-all"
      >
        <ExternalLink class="w-4 h-4" />
      </a>
    </div>
  </div>
</div>

  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import EmptyData from '@/components/common/EmptyData.vue'
import { useLocale } from '@/composables/useLocale'
import { getMyCertificatesApi } from '@/utils/api'
import type { CertificateItem } from '@/utils/models'
import { useCertificateDownload } from '@/composables/useCertificateDownload'
import { Award, ClipboardList, CircleAlert, ExternalLink, Download, Loader2 } from 'lucide-vue-next'

const { t }  = useLocale()
const router = useRouter()
const { downloading, downloadCertificate } = useCertificateDownload()

const loading      = ref(true)
const loadError    = ref<string | null>(null)
const certificates = ref<CertificateItem[]>([])
const copied       = ref<string | null>(null)

async function load() {
  loading.value   = true
  loadError.value = null
  const { data, error } = await getMyCertificatesApi()
  if (error || !data) {
    loadError.value = error ?? 'Failed to load certificates'
  } else {
    certificates.value = data
  }
  loading.value = false
}

async function copyCode(code: string) {
  try {
    await navigator.clipboard.writeText(code)
    copied.value = code
    setTimeout(() => { copied.value = null }, 2000)
  } catch { /* ignore */ }
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}

onMounted(load)
</script>
