<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getCertificateByCodeApi, type CertificateItem } from '@/utils/api/certificate.api'
import { useCertificateDownload } from '@/composables/useCertificateDownload'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Download, ArrowLeft, Share2, CheckCircle2, Loader2 } from 'lucide-vue-next'

const route  = useRoute()
const router = useRouter()
const code   = route.params.code as string

const { downloading, downloadCertificate } = useCertificateDownload()

const cert    = ref<CertificateItem | null>(null)
const loading = ref(true)
const error   = ref<string | null>(null)
const copied  = ref(false)

onMounted(async () => {
  const { data, error: err } = await getCertificateByCodeApi(code)
  if (err || !data) { error.value = err ?? 'Certificate not found'; loading.value = false; return }
  cert.value = data
  loading.value = false
})

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })
}

const scoreLabel = computed(() => {
  if (!cert.value) return ''
  const p = cert.value.percentage
  if (p >= 90) return 'with Distinction'
  if (p >= 75) return 'with Merit'
  return 'with Completion'
})

async function copyLink() {
  await navigator.clipboard.writeText(window.location.href)
  copied.value = true
  setTimeout(() => copied.value = false, 2000)
}
</script>

<template>
  <!-- Google Fonts loaded once -->
  <link rel="preconnect" href="https://fonts.googleapis.com" />
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
  <link href="https://fonts.googleapis.com/css2?family=Cormorant+Garamond:ital,wght@0,400;0,600;1,400&family=Great+Vibes&display=swap" rel="stylesheet" />

  <div class="min-h-screen bg-gradient-to-br from-zinc-900 via-slate-900 to-zinc-900 flex flex-col items-center justify-center gap-6 p-6 no-print">

    <!-- Loading -->
    <div v-if="loading" class="flex flex-col items-center gap-4">
      <Skeleton class="w-[900px] h-[640px] rounded-2xl" />
    </div>

    <!-- Error -->
    <div v-else-if="error" class="text-center text-red-400 space-y-3">
      <p class="text-lg font-semibold">{{ error }}</p>
      <Button variant="outline" @click="router.back()">Go Back</Button>
    </div>

    <template v-else-if="cert">
      <!-- Toolbar -->
      <div class="no-print flex items-center gap-3 w-full max-w-5xl">
        <Button variant="ghost" class="text-white/70 hover:text-white hover:bg-white/10" @click="router.back()">
          <ArrowLeft class="w-4 h-4 mr-2" /> Back
        </Button>
        <div class="flex-1" />
        <Button variant="outline" class="border-white/20 text-white/70 hover:text-white hover:bg-white/10 bg-transparent" @click="copyLink">
          <Share2 class="w-4 h-4 mr-2" />
          {{ copied ? 'Copied!' : 'Share Link' }}
        </Button>
        <Button
          class="bg-indigo-600 hover:bg-indigo-700 text-white font-semibold px-5"
          :disabled="downloading !== null"
          @click="cert && downloadCertificate(cert)"
        >
          <Loader2 v-if="downloading !== null" class="w-4 h-4 mr-2 animate-spin" />
          <Download v-else class="w-4 h-4 mr-2" />
          {{ downloading !== null ? 'Generating PDF…' : 'Download PDF' }}
        </Button>
      </div>

      <!-- Certificate Paper -->
      <div
        id="certificate-print-area"
        class="relative bg-white shadow-[0_32px_80px_rgba(0,0,0,0.6)] overflow-hidden"
        style="width:1120px;height:793px;border:14px solid #1a2e4a;font-family:'Cormorant Garamond',Georgia,serif;"
      >
        <!-- Inner border inset -->
        <div class="absolute inset-[18px] border-[1.5px] border-[#1a2e4a]/25 pointer-events-none" />
        <!-- Subtle background pattern -->
        <div class="absolute inset-0 opacity-[0.03]"
          style="background-image:radial-gradient(circle,#1a2e4a 1px,transparent 1px);background-size:28px 28px;" />

        <!-- ── Corner ornaments ─────────────────────────────────────────── -->
        <!-- Top-left -->
        <svg class="absolute top-3 left-3 w-20 h-20" viewBox="0 0 80 80" fill="none">
          <path d="M8 8 L40 8 M8 8 L8 40" stroke="#1a2e4a" stroke-width="2"/>
          <path d="M14 14 L36 14 M14 14 L14 36" stroke="#1a2e4a" stroke-width="1" stroke-dasharray="3 3"/>
          <circle cx="8" cy="8" r="3" fill="#1a2e4a"/>
          <path d="M20 8 Q14 14 8 20" stroke="#b8860b" stroke-width="1.5" fill="none"/>
          <path d="M28 8 Q18 18 8 28" stroke="#1a2e4a" stroke-width="0.5" fill="none" opacity="0.4"/>
        </svg>
        <!-- Top-right -->
        <svg class="absolute top-3 right-3 w-20 h-20" viewBox="0 0 80 80" fill="none">
          <path d="M72 8 L40 8 M72 8 L72 40" stroke="#1a2e4a" stroke-width="2"/>
          <path d="M66 14 L44 14 M66 14 L66 36" stroke="#1a2e4a" stroke-width="1" stroke-dasharray="3 3"/>
          <circle cx="72" cy="8" r="3" fill="#1a2e4a"/>
          <path d="M60 8 Q66 14 72 20" stroke="#b8860b" stroke-width="1.5" fill="none"/>
        </svg>
        <!-- Bottom-left -->
        <svg class="absolute bottom-3 left-3 w-20 h-20" viewBox="0 0 80 80" fill="none">
          <path d="M8 72 L40 72 M8 72 L8 40" stroke="#1a2e4a" stroke-width="2"/>
          <path d="M14 66 L36 66 M14 66 L14 44" stroke="#1a2e4a" stroke-width="1" stroke-dasharray="3 3"/>
          <circle cx="8" cy="72" r="3" fill="#1a2e4a"/>
          <path d="M20 72 Q14 66 8 60" stroke="#b8860b" stroke-width="1.5" fill="none"/>
        </svg>
        <!-- Bottom-right -->
        <svg class="absolute bottom-3 right-3 w-20 h-20" viewBox="0 0 80 80" fill="none">
          <path d="M72 72 L40 72 M72 72 L72 40" stroke="#1a2e4a" stroke-width="2"/>
          <path d="M66 66 L44 66 M66 66 L66 44" stroke="#1a2e4a" stroke-width="1" stroke-dasharray="3 3"/>
          <circle cx="72" cy="72" r="3" fill="#1a2e4a"/>
          <path d="M60 72 Q66 66 72 60" stroke="#b8860b" stroke-width="1.5" fill="none"/>
        </svg>

        <!-- ── Left accent bar ─────────────────────────────────────────── -->
        <div class="absolute left-[40px] top-[40px] bottom-[40px] w-[3px]"
          style="background:linear-gradient(to bottom,transparent,#1a2e4a 20%,#b8860b 50%,#1a2e4a 80%,transparent)" />

        <!-- ── Main content ───────────────────────────────────────────── -->
        <div class="flex flex-col items-center justify-center h-full gap-[10px] px-28 text-[#1a2e4a]">

          <!-- Top: Issuer -->
          <p class="text-[11px] tracking-[0.5em] uppercase font-semibold text-[#1a2e4a]/50 leading-none">
            {{ cert.companyName || 'EVALO.me' }}
          </p>

          <!-- Decorative line -->
          <div class="flex items-center gap-3 w-64">
            <div class="flex-1 h-px bg-gradient-to-r from-transparent to-[#b8860b]" />
            <svg width="12" height="12" viewBox="0 0 12 12"><polygon points="6,0 12,6 6,12 0,6" fill="#b8860b"/></svg>
            <div class="flex-1 h-px bg-gradient-to-l from-transparent to-[#b8860b]" />
          </div>

          <!-- Title block -->
          <h1 class="text-[28px] font-semibold tracking-[0.22em] uppercase text-[#1a2e4a] leading-tight">
            Certificate of Completion
          </h1>

          <p class="text-[13px] italic text-[#1a2e4a]/60 leading-none tracking-wide">
            This is to certify that
          </p>

          <!-- Recipient name -->
          <h2
            class="text-[58px] text-[#1a2e4a] leading-none mt-[-4px]"
            style="font-family:'Great Vibes',cursive;"
          >
            {{ cert.candidateName }}
          </h2>

          <!-- Body text -->
          <p class="text-[13px] italic text-[#1a2e4a]/60 leading-none tracking-wide">
            has successfully completed
          </p>

          <h3 class="text-[20px] font-semibold text-[#1a2e4a] text-center max-w-xl leading-snug tracking-wide">
            {{ cert.campaignName }}
          </h3>

          <!-- Score + honour -->
          <div class="flex items-center gap-2 text-[13px] text-[#1a2e4a]/70">
            <span>{{ scoreLabel }} · Score:</span>
            <span class="font-bold text-[#1a2e4a] text-[15px]">{{ cert.percentage.toFixed(1) }}%</span>
            <span>· Issued on</span>
            <span class="font-semibold">{{ formatDate(cert.issuedAt) }}</span>
          </div>

          <!-- Spacer line -->
          <div class="flex items-center gap-3 w-64 mt-1">
            <div class="flex-1 h-px bg-gradient-to-r from-transparent to-[#b8860b]" />
            <svg width="12" height="12" viewBox="0 0 12 12"><polygon points="6,0 12,6 6,12 0,6" fill="#b8860b"/></svg>
            <div class="flex-1 h-px bg-gradient-to-l from-transparent to-[#b8860b]" />
          </div>

          <!-- Signature row -->
          <div class="flex items-end justify-between w-full max-w-lg mt-1">
            <!-- Sig left -->
            <div class="flex flex-col items-center gap-1">
              <div class="h-[1px] w-36 bg-[#1a2e4a]" />
              <p class="text-[10px] tracking-widest uppercase text-[#1a2e4a]/50">Team Leader</p>
            </div>
            <!-- Seal -->
            <div class="flex flex-col items-center justify-center w-[68px] h-[68px] rounded-full border-[3px] border-[#1a2e4a] relative">
              <div class="absolute inset-[5px] rounded-full border border-[#1a2e4a]/30" />
              <CheckCircle2 class="w-5 h-5 text-[#1a2e4a]" />
              <p class="text-[7px] font-bold uppercase tracking-wide text-[#1a2e4a] leading-tight text-center">Certified</p>
            </div>
            <!-- Sig right -->
            <div class="flex flex-col items-center gap-1">
              <div class="h-[1px] w-36 bg-[#1a2e4a]" />
              <p class="text-[10px] tracking-widest uppercase text-[#1a2e4a]/50">Manager</p>
            </div>
          </div>
        </div>

        <!-- ── Footer: certificate code ───────────────────────────────── -->
        <p class="absolute bottom-[10px] left-1/2 -translate-x-1/2 text-[9px] tracking-[0.35em] uppercase text-[#1a2e4a]/30 whitespace-nowrap">
          Certificate ID: {{ cert.certificateCode }}
        </p>
      </div>
    </template>
  </div>
</template>

<style>
@media print {
  .no-print { display: none !important; }
  body { margin: 0; padding: 0; background: white; }
  #certificate-print-area { box-shadow: none !important; }
}
</style>
