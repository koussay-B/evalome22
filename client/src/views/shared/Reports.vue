<template>
  <div class="space-y-6">
    <div class="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
      <div>
        <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">{{ t('reports.eyebrow') }}</p>
        <h1 class="text-3xl  tracking-tight text-slate-800">{{ t('reports.title') }}</h1>
        <p class="mt-2 max-w-3xl text-sm text-muted-foreground">
          {{ authStore.isFormateur ? t('reports.subtitleFormateur') : t('reports.subtitleCompany') }}
        </p>
      </div>
      <div class="flex flex-wrap gap-2">
        <Button :disabled="downloading === 'company'" @click="downloadCompanyReport">
          {{ downloading === 'company' ? t('reports.downloading') : t('reports.downloadCompany') }}
        </Button>
      </div>
    </div>

    <div class="space-y-6">
      <div class="grid grid-cols-2 gap-4 xl:grid-cols-7">
        <Card v-for="item in summaryCards" :key="item.label" class="border-border/70">
          <CardContent class="p-5">
            <p class="text-[11px] text-slate-800 uppercase tracking-widest ">{{ item.label }}</p>
            <p class="mt-3 text-3xl  text-slate-800">{{ item.value }}</p>
          </CardContent>
        </Card>
      </div>

      <Card class="overflow-hidden border-border/70">
        <CardHeader class="border-b border-border/60 bg-muted/20">
          <CardTitle class="text-xl text-slate-800">{{ t('reports.campaignsTitle') }}</CardTitle>
          <CardDescription>{{ t('reports.campaignsDesc') }}</CardDescription>
        </CardHeader>
        <CardContent class="p-0">
          <div class="overflow-x-auto">
            <table class="w-full text-left">
              <thead class="bg-muted/40 text-slate-800 text-[11px]  uppercase tracking-widest ">
                <tr>
                  <th class="px-5 py-4">{{ t('reports.tableCampaign') }}</th>
                  <th class="px-5 py-4">{{ t('reports.tableStatus') }}</th>
                  <th class="px-5 py-4">{{ t('reports.tableInvited') }}</th>
                  <th class="px-5 py-4">{{ t('reports.tableCompleted') }}</th>
                  <th class="px-5 py-4">{{ t('reports.tablePassed') }}</th>
                  <th class="px-5 py-4">{{ t('reports.tableFailed') }}</th>
                  <th class="px-5 py-4">{{ t('reports.tablePassRate') }}</th>
                  <th class="px-5 py-4 text-right">{{ t('common.actions') }}</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-border text-sm">
                <tr v-for="i in 6" v-if="loading" :key="i">
                  <td class="px-5 py-4" colspan="8">
                    <Skeleton class="h-10 w-full rounded-xl" />
                  </td>
                </tr>
                <tr
                  v-for="campaign in visibleCampaigns"
                  v-else
                  :key="campaign.id"
                  class="transition-colors hover:bg-muted/20"
                >
                  <td class="px-5 py-4">
                    <div class="font-semibold text-slate-800">{{ campaign.name }}</div>
                    <div class="mt-1 text-xs text-muted-foreground">
                      {{ formatDate(campaign.availableFrom) }} - {{ formatDate(campaign.availableUntil) }}
                    </div>
                  </td>
                  <td class="px-5 py-4 text-muted-foreground">{{ campaign.status }}</td>
                  <td class="px-5 py-4">{{ campaign.invitedCount }}</td>
                  <td class="px-5 py-4">{{ campaign.completedCount }}</td>
                  <td class="px-5 py-4 text-emerald-600">{{ campaign.passedCount }}</td>
                  <td class="px-5 py-4 text-red-600">{{ getFailedCount(campaign) }}</td>
                  <td class="px-5 py-4 font-semibold">{{ passRate(campaign) }}%</td>
                  <td class="px-5 py-4">
                    <div class="flex justify-end gap-2">
                      <Button size="sm" variant="outline" :disabled="campaignLoadingId === campaign.id" @click="openCampaignReport(campaign)">
                        {{ t('reports.selectCampaign') }}
                      </Button>
                      <Button size="sm" :disabled="downloading === `campaign-${campaign.id}`" @click="downloadCampaignReport(campaign)">
                        {{ downloading === `campaign-${campaign.id}` ? t('reports.downloading') : t('reports.downloadPdf') }}
                      </Button>
                    </div>
                  </td>
                </tr>
                <tr v-if="!loading && !visibleCampaigns.length">
                  <td class="px-5 py-8 text-center text-muted-foreground" colspan="8">{{ t('common.noData') }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>

    <Dialog v-model:open="isCampaignDialogOpen">
      <DialogScrollContent class="max-w-6xl border-border/70 p-0">
        <div class="max-h-[85vh] overflow-y-auto">
          <div class="sticky top-0 z-10 border-b border-border/70 bg-background/95 px-6 py-5 backdrop-blur">
            <DialogHeader class="space-y-2 text-left">
              <DialogTitle class="text-2xl font-black text-slate-800">
                {{ activeCampaign?.name || t('reports.currentCampaign') }}
              </DialogTitle>
              <DialogDescription class="flex flex-wrap items-center gap-3 text-sm text-muted-foreground">
                <span>{{ activeCampaign?.status || '-' }}</span>
                <span v-if="activeCampaign">{{ formatDate(activeCampaign.availableFrom) }} - {{ formatDate(activeCampaign.availableUntil) }}</span>
              </DialogDescription>
            </DialogHeader>
            <div v-if="activeCampaign" class="mt-4 flex flex-wrap gap-2">
              <Button :disabled="downloading === `campaign-${activeCampaign.id}`" @click="downloadCampaignReport(activeCampaign)">
                {{ downloading === `campaign-${activeCampaign.id}` ? t('reports.downloading') : t('reports.downloadPdf') }}
              </Button>
            </div>
          </div>

          <div class="space-y-6 px-6 py-6">
            <div v-if="activeCampaign" class="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
              <Card
                v-for="item in activeCampaignSummaryCards"
                :key="item.label"
                class="border-border/70 bg-gradient-to-br from-background to-muted/20"
              >
                <CardContent class="p-5">
                  <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ item.label }}</p>
                  <p class="mt-3 text-3xl font-black text-slate-800">{{ item.value }}</p>
                </CardContent>
              </Card>
            </div>

            <div v-if="campaignLoadingId === activeCampaign?.id" class="space-y-4">
              <Skeleton class="h-28 w-full rounded-2xl" />
              <Skeleton class="h-80 w-full rounded-2xl" />
            </div>

            <template v-else-if="activeCampaign">
              <div class="grid gap-4 lg:grid-cols-2">
                <Card class="border-border/70">
                  <CardHeader class="border-b border-border/60 bg-muted/15">
                    <CardTitle class="text-base font-black">{{ t('reports.companyInfo') }}</CardTitle>
                  </CardHeader>
                  <CardContent class="grid gap-3 p-5 sm:grid-cols-2">
                    <div v-for="item in companyInfoItems" :key="item.label" class="rounded-xl border border-border/60 bg-background px-4 py-3">
                      <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ item.label }}</p>
                      <p class="mt-2 text-sm font-medium text-slate-800 break-words">{{ item.value }}</p>
                    </div>
                  </CardContent>
                </Card>

                <Card class="border-border/70">
                  <CardHeader class="border-b border-border/60 bg-muted/15">
                    <CardTitle class="text-base font-black">{{ t('reports.campaignInfo') }}</CardTitle>
                  </CardHeader>
                  <CardContent class="grid gap-3 p-5 sm:grid-cols-2">
                    <div v-for="item in campaignInfoItems" :key="item.label" class="rounded-xl border border-border/60 bg-background px-4 py-3">
                      <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ item.label }}</p>
                      <p class="mt-2 text-sm font-medium text-slate-800 break-words">{{ item.value }}</p>
                    </div>
                  </CardContent>
                </Card>
              </div>

              <Card class="border-border/70">
                <CardHeader class="border-b border-border/60 bg-muted/15">
                  <CardTitle class="text-lg font-black">{{ t('reports.currentCampaign') }}</CardTitle>
                  <CardDescription>{{ t('reports.campaignsDesc') }}</CardDescription>
                </CardHeader>
                <CardContent class="space-y-6 p-6">
                  <div class="grid gap-4 lg:grid-cols-[1.2fr_0.8fr]">
                    <div class="rounded-2xl border border-border/70 bg-background p-5">
                      <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('reports.attemptCandidate') }}</p>
                      <div class="mt-4 space-y-3">
                        <div
                          v-for="leader in campaignLeaders"
                          :key="leader.id"
                          class="flex items-center justify-between rounded-xl border border-border/60 px-4 py-3"
                        >
                          <div>
                            <p class="font-semibold text-slate-800">{{ leader.candidateName }}</p>
                            <p class="text-xs text-muted-foreground">{{ leader.candidateEmail || t('common.noData') }}</p>
                          </div>
                          <div class="text-right">
                            <p class="text-lg font-black text-slate-800">{{ formatPercent(leader.percentage) }}</p>
                            <p class="text-xs text-muted-foreground">{{ leader.passed ? t('reports.resultPassed') : t('reports.resultFailed') }}</p>
                          </div>
                        </div>
                        <div v-if="!campaignLeaders.length" class="rounded-xl border border-dashed border-border p-5 text-sm text-muted-foreground">
                          {{ t('reports.noAttempts') }}
                        </div>
                      </div>
                    </div>

                    <div class="rounded-2xl border border-border/70 bg-muted/15 p-5">
                      <p class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ t('reports.campaignPassRate') }}</p>
                      <div class="mt-4 h-4 overflow-hidden rounded-full bg-muted">
                        <div
                          class="h-full rounded-full bg-primary transition-all"
                          :style="{ width: `${Number(activeCampaignReport?.passRate ?? 0).toFixed(1)}%` }"
                        />
                      </div>
                      <div class="mt-5 space-y-3 text-sm">
                        <div class="flex items-center justify-between rounded-xl bg-background px-4 py-3">
                          <span class="text-muted-foreground">{{ t('reports.totalPassed') }}</span>
                          <span class="font-bold text-slate-800">{{ activeCampaign.passedCount }}</span>
                        </div>
                        <div class="flex items-center justify-between rounded-xl bg-background px-4 py-3">
                          <span class="text-muted-foreground">{{ t('reports.totalFailed') }}</span>
                          <span class="font-bold text-slate-800">{{ getFailedCount(activeCampaign) }}</span>
                        </div>
                        <div class="flex items-center justify-between rounded-xl bg-background px-4 py-3">
                          <span class="text-muted-foreground">{{ t('reports.totalInvited') }}</span>
                          <span class="font-bold text-slate-800">{{ activeCampaign.invitedCount }}</span>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div class="rounded-2xl border border-border/70 overflow-hidden">
                    <div class="flex items-center justify-between border-b border-border/60 bg-muted/20 px-5 py-4">
                      <div>
                        <h3 class="text-lg font-black text-slate-800">{{ t('reports.currentCampaign') }}</h3>
                        <p class="text-sm text-muted-foreground">{{ attemptsCountLabel(activeCampaignReport?.attempts.length ?? 0) }}</p>
                      </div>
                    </div>

                    <div v-if="activeCampaignReport?.attempts.length" class="overflow-x-auto">
                      <table class="w-full min-w-[820px] text-left text-sm">
                        <thead class="bg-muted/40 text-[11px] font-bold uppercase tracking-widest text-muted-foreground">
                          <tr>
                            <th class="px-5 py-4">{{ t('reports.attemptCandidate') }}</th>
                            <th class="px-5 py-4">{{ t('reports.attemptQuestionnaire') }}</th>
                            <th class="px-5 py-4">{{ t('reports.attemptScore') }}</th>
                            <th class="px-5 py-4">{{ t('reports.attemptResult') }}</th>
                            <th class="px-5 py-4">{{ t('reports.attemptSubmitted') }}</th>
                          </tr>
                        </thead>
                        <tbody class="divide-y divide-border">
                          <tr v-for="attempt in activeCampaignReport.attempts" :key="attempt.id" class="align-top">
                            <td class="px-5 py-4">
                              <div class="font-semibold text-slate-800">{{ attempt.candidateName }}</div>
                              <div class="mt-1 text-xs text-muted-foreground">{{ attempt.candidateEmail || t('common.noData') }}</div>
                            </td>
                            <td class="px-5 py-4 text-muted-foreground">{{ attempt.questionnaireTitle || t('reports.allQuestionnaires') }}</td>
                            <td class="px-5 py-4">
                              <span class="inline-flex rounded-full bg-primary/10 px-3 py-1 text-xs font-bold text-primary">
                                {{ formatPercent(attempt.percentage) }}
                              </span>
                            </td>
                            <td class="px-5 py-4">
                              <span
                                :class="attempt.passed ? 'bg-emerald-500/10 text-emerald-700' : 'bg-red-500/10 text-red-700'"
                                class="inline-flex rounded-full px-3 py-1 text-xs font-bold"
                              >
                                {{ attempt.passed ? t('reports.resultPassed') : t('reports.resultFailed') }}
                              </span>
                            </td>
                            <td class="px-5 py-4 text-muted-foreground">{{ formatDateTime(attempt.submittedAt || attempt.startedAt) }}</td>
                          </tr>
                        </tbody>
                      </table>
                    </div>

                    <div v-else class="p-8 text-center text-sm text-muted-foreground">
                      {{ t('reports.noAttempts') }}
                    </div>
                  </div>
                </CardContent>
              </Card>
            </template>
          </div>
        </div>
      </DialogScrollContent>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Dialog, DialogDescription, DialogHeader, DialogScrollContent, DialogTitle } from '@/components/ui/dialog'
import { Skeleton } from '@/components/ui/skeleton'
import { useLocale } from '@/composables/useLocale'
import { useAuthStore } from '@/store/authStore'
import { getCampaignReportApi, getCampaignsApi, getCompanyStatsApi, getFormateurStatsApi, getMyCompanyApi } from '@/utils/api'
import type { CompanyStats, FormateurStats } from '@/utils/api'
import type { CampaignItem, CampaignReportAttempt, CampaignReportData, Company } from '@/utils/models'

type DownloadTarget = 'company' | `campaign-${number}` | null

type CompanyPrintableSection = {
  campaign: CampaignItem
  report: CampaignReportData
}

const authStore = useAuthStore()
const { t } = useLocale()

const loading = ref(true)
const campaignLoadingId = ref<number | null>(null)
const downloading = ref<DownloadTarget>(null)
const isCampaignDialogOpen = ref(false)
const campaigns = ref<CampaignItem[]>([])
const stats = ref<CompanyStats | FormateurStats | null>(null)
const company = ref<Company | null>(null)
const activeCampaign = ref<CampaignItem | null>(null)
const campaignReports = ref<Record<number, CampaignReportData>>({})

const visibleCampaigns = computed(() => {
  if (authStore.isFormateur) {
    const currentUserId = authStore.state.user?.id
    return campaigns.value.filter(campaign => campaign.createdById === currentUserId)
  }

  return campaigns.value
})

const summaryCards = computed(() => {
  const invited = visibleCampaigns.value.reduce((sum, campaign) => sum + campaign.invitedCount, 0)
  const completed = visibleCampaigns.value.reduce((sum, campaign) => sum + campaign.completedCount, 0)
  const passed = visibleCampaigns.value.reduce((sum, campaign) => sum + campaign.passedCount, 0)
  const failed = Math.max(completed - passed, 0)

  return [
    { label: t('reports.totalCampaigns'), value: visibleCampaigns.value.length },
    { label: t('reports.totalInvited'), value: invited },
    { label: t('reports.totalCompleted'), value: completed },
    { label: t('reports.totalPassed'), value: passed },
    { label: t('reports.totalFailed'), value: failed },
    { label: t('reports.averagePassRate'), value: `${Number(stats.value?.passRate ?? 0).toFixed(1)}%` },
    { label: t('reports.averageScore'), value: `${Number(stats.value?.avgScore ?? 0).toFixed(1)}%` },
  ]
})

const activeCampaignReport = computed(() => {
  const campaignId = activeCampaign.value?.id
  return campaignId ? campaignReports.value[campaignId] ?? null : null
})

const activeCampaignSummaryCards = computed(() => {
  const campaign = activeCampaign.value
  if (!campaign) return []

  return [
    { label: t('reports.campaignAverage'), value: `${Number(activeCampaignReport.value?.groupAverageScore ?? 0).toFixed(1)}%` },
    { label: t('reports.campaignPassRate'), value: `${Number(activeCampaignReport.value?.passRate ?? 0).toFixed(1)}%` },
    { label: t('reports.totalCompleted'), value: campaign.completedCount },
    { label: t('reports.totalFailed'), value: getFailedCount(campaign) },
  ]
})

const campaignLeaders = computed(() => {
  return [...(activeCampaignReport.value?.attempts ?? [])]
    .sort((left, right) => right.percentage - left.percentage)
    .slice(0, 3)
})

const companyInfoItems = computed(() => [
  { label: t('reports.companyName'), value: company.value?.name || t('common.noData') },
  { label: t('reports.companyEmail'), value: company.value?.email || t('common.noData') },
  { label: t('reports.companyPhone'), value: company.value?.phone || t('common.noData') },
  { label: t('reports.companyWebsite'), value: company.value?.website || t('common.noData') },
  { label: t('reports.companyAddress'), value: company.value?.address || t('common.noData') },
  { label: t('reports.generatedOn'), value: formatDateTime(new Date().toISOString()) },
])

const campaignInfoItems = computed(() => {
  if (!activeCampaign.value) return []

  return [
    { label: t('reports.tableCampaign'), value: activeCampaign.value.name },
    { label: t('reports.tableStatus'), value: activeCampaign.value.status },
    { label: t('reports.campaignWindow'), value: `${formatDate(activeCampaign.value.availableFrom)} - ${formatDate(activeCampaign.value.availableUntil)}` },
    { label: t('reports.totalInvited'), value: String(activeCampaign.value.invitedCount) },
    { label: t('reports.totalCompleted'), value: String(activeCampaign.value.completedCount) },
    { label: t('reports.attempts'), value: String(activeCampaignReport.value?.attempts.length ?? 0) },
  ]
})

onMounted(loadData)

async function loadData() {
  loading.value = true

  const [campaignRes, statsRes, companyRes] = await Promise.all([
    getCampaignsApi(),
    authStore.isFormateur ? getFormateurStatsApi() : getCompanyStatsApi(),
    getMyCompanyApi(),
  ])

  campaigns.value = campaignRes.data ?? []
  stats.value = statsRes.data ?? null
  company.value = companyRes.data ?? null
  loading.value = false
}

async function ensureCampaignReport(campaign: CampaignItem) {
  const existing = campaignReports.value[campaign.id]
  if (existing) return existing

  campaignLoadingId.value = campaign.id
  try {
    const { data } = await getCampaignReportApi(campaign.id)
    const report = data ?? { attempts: [], groupAverageScore: 0, passRate: 0 }
    campaignReports.value = { ...campaignReports.value, [campaign.id]: report }
    return report
  } finally {
    campaignLoadingId.value = null
  }
}

async function openCampaignReport(campaign: CampaignItem) {
  activeCampaign.value = campaign
  isCampaignDialogOpen.value = true
  await ensureCampaignReport(campaign)
}

function getFailedCount(campaign: CampaignItem) {
  return Math.max(campaign.completedCount - campaign.passedCount, 0)
}

function passRate(campaign: CampaignItem) {
  return campaign.completedCount > 0
    ? Math.round((campaign.passedCount / campaign.completedCount) * 100)
    : 0
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString()
}

function formatDateTime(value: string) {
  return new Date(value).toLocaleString()
}

function formatPercent(value: number) {
  return `${Number(value ?? 0).toFixed(1)}%`
}

function attemptsCountLabel(count: number) {
  return t(count === 1 ? 'reports.attemptSingular' : 'reports.attemptPlural', { count })
}

async function getCompanyPrintableSections() {
  const reports = await Promise.all(
    visibleCampaigns.value.map(async (campaign) => ({
      campaign,
      report: await ensureCampaignReport(campaign),
    }))
  )

  return reports as CompanyPrintableSection[]
}

async function downloadCompanyReport() {
  downloading.value = 'company'

  try {
    const sections = await getCompanyPrintableSections()
    await downloadHtmlPdf(buildCompanyReportHtml(sections), 'company-progress-report.pdf')
  } finally {
    downloading.value = null
  }
}

async function downloadCampaignReport(campaign: CampaignItem) {
  downloading.value = `campaign-${campaign.id}`

  try {
    const report = await ensureCampaignReport(campaign)
    await downloadHtmlPdf(buildCampaignReportHtml(campaign, report), `campaign-report-${campaign.id}.pdf`)
  } finally {
    downloading.value = null
  }
}

async function downloadHtmlPdf(html: string, filename: string) {
  const wrapper = document.createElement('div')
  wrapper.style.cssText = [
    'position:fixed',
    'top:0',
    'left:0',
    'width:960px',
    'background:#ffffff',
    'opacity:0.01',
    'pointer-events:none',
    'z-index:1',
  ].join(';')
  wrapper.innerHTML = html
  document.body.appendChild(wrapper)
  const content = wrapper.querySelector('.report-shell') as HTMLElement | null

  type Stashed = { parent: ParentNode; node: Element; before: ChildNode | null }
  const stashed: Stashed[] = []
  document.querySelectorAll<Element>('link[rel="stylesheet"], style').forEach((node) => {
    if (!node.parentNode || wrapper.contains(node)) return
    stashed.push({ parent: node.parentNode, node, before: node.nextSibling })
    node.remove()
  })

  try {
    if (!content) throw new Error('Printable report content was not created')

    const html2pdf = (await import('html2pdf.js')).default
    await new Promise(resolve => setTimeout(resolve, 80))

    await html2pdf().set({
      margin: [10, 10, 10, 10],
      filename,
      image: { type: 'jpeg', quality: 0.98 },
      html2canvas: { scale: 1.2, useCORS: true, logging: false, backgroundColor: '#ffffff' },
      jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' },
      pagebreak: { mode: ['css', 'legacy'] },
    }).from(content).save()
  } finally {
    stashed.forEach(({ parent, node, before }) => parent.insertBefore(node, before))
    document.body.removeChild(wrapper)
  }
}

function buildCompanyReportHtml(sections: CompanyPrintableSection[]) {
  const summaryMarkup = summaryCards.value
    .map(item => `
      <div class="metric-card">
        <div class="metric-label">${escapeHtml(String(item.label))}</div>
        <div class="metric-value">${escapeHtml(String(item.value))}</div>
      </div>
    `)
    .join('')

  const campaignsTableRows = visibleCampaigns.value
    .map(campaign => `
      <tr>
        <td>${escapeHtml(campaign.name)}</td>
        <td>${escapeHtml(campaign.status)}</td>
        <td>${campaign.invitedCount}</td>
        <td>${campaign.completedCount}</td>
        <td>${campaign.passedCount}</td>
        <td>${getFailedCount(campaign)}</td>
        <td>${passRate(campaign)}%</td>
      </tr>
    `)
    .join('')

  const detailSections = sections.map(({ campaign, report }) => buildCampaignSectionHtml(campaign, report)).join('')

  return `
    ${reportDocumentStyle()}
    <div class="report-shell">
      <div class="report-header">
        <div>
          <div class="eyebrow">${escapeHtml(t('reports.eyebrow'))}</div>
          <h1>${escapeHtml(t('reports.title'))}</h1>
          <p>${escapeHtml(authStore.isFormateur ? t('reports.subtitleFormateur') : t('reports.subtitleCompany'))}</p>
        </div>
        <div class="stamp">${escapeHtml(t('reports.generatedOn'))}: ${escapeHtml(formatDateTime(new Date().toISOString()))}</div>
      </div>

      <section class="report-section">
        <div class="section-title">${escapeHtml(t('reports.title'))}</div>
        <div class="metrics-grid">${summaryMarkup}</div>
      </section>

      <section class="report-section">
        <div class="section-title">${escapeHtml(t('reports.companyInfo'))}</div>
        <div class="info-grid">${buildInfoGridHtml(companyInfoItems.value)}</div>
      </section>

      <section class="report-section">
        <div class="section-title">${escapeHtml(t('reports.campaignsTitle'))}</div>
        <p class="section-copy">${escapeHtml(t('reports.campaignsDesc'))}</p>
        <table class="report-table">
          <thead>
            <tr>
              <th>${escapeHtml(t('reports.tableCampaign'))}</th>
              <th>${escapeHtml(t('reports.tableStatus'))}</th>
              <th>${escapeHtml(t('reports.tableInvited'))}</th>
              <th>${escapeHtml(t('reports.tableCompleted'))}</th>
              <th>${escapeHtml(t('reports.tablePassed'))}</th>
              <th>${escapeHtml(t('reports.tableFailed'))}</th>
              <th>${escapeHtml(t('reports.tablePassRate'))}</th>
            </tr>
          </thead>
          <tbody>${campaignsTableRows}</tbody>
        </table>
      </section>

      ${detailSections}
    </div>
  `
}

function buildCampaignReportHtml(campaign: CampaignItem, report: CampaignReportData) {
  return `
    ${reportDocumentStyle()}
    <div class="report-shell">
      <div class="report-header">
        <div>
          <div class="eyebrow">${escapeHtml(t('reports.currentCampaign'))}</div>
          <h1>${escapeHtml(campaign.name)}</h1>
          <p>${escapeHtml(campaign.status)} · ${escapeHtml(formatDate(campaign.availableFrom))} - ${escapeHtml(formatDate(campaign.availableUntil))}</p>
        </div>
        <div class="stamp">${escapeHtml(t('reports.generatedOn'))}: ${escapeHtml(formatDateTime(new Date().toISOString()))}</div>
      </div>
      ${buildCampaignSectionHtml(campaign, report)}
    </div>
  `
}

function buildCampaignSectionHtml(campaign: CampaignItem, report: CampaignReportData) {
  const summary = [
    { label: t('reports.campaignAverage'), value: formatPercent(report.groupAverageScore) },
    { label: t('reports.campaignPassRate'), value: formatPercent(report.passRate) },
    { label: t('reports.totalCompleted'), value: String(campaign.completedCount) },
    { label: t('reports.totalFailed'), value: String(getFailedCount(campaign)) },
  ]

  const leaders = [...report.attempts].sort((left, right) => right.percentage - left.percentage).slice(0, 3)

  const summaryMarkup = summary
    .map(item => `
      <div class="metric-card compact">
        <div class="metric-label">${escapeHtml(item.label)}</div>
        <div class="metric-value">${escapeHtml(item.value)}</div>
      </div>
    `)
    .join('')

  const leadersMarkup = leaders.length
    ? leaders.map(leader => `
      <div class="leader-row">
        <div>
          <div class="leader-name">${escapeHtml(leader.candidateName)}</div>
          <div class="leader-email">${escapeHtml(leader.candidateEmail || t('common.noData'))}</div>
        </div>
        <div class="leader-score">${escapeHtml(formatPercent(leader.percentage))}</div>
      </div>
    `).join('')
    : `<div class="empty-block">${escapeHtml(t('reports.noAttempts'))}</div>`

  const attemptsMarkup = report.attempts.length
    ? report.attempts.map(attempt => buildAttemptRowHtml(attempt)).join('')
    : `<tr><td colspan="5" class="empty-cell">${escapeHtml(t('reports.noAttempts'))}</td></tr>`

  return `
    <section class="report-section page-break">
      <div class="section-title">${escapeHtml(campaign.name)}</div>
      <p class="section-copy">${escapeHtml(campaign.status)} · ${escapeHtml(formatDate(campaign.availableFrom))} - ${escapeHtml(formatDate(campaign.availableUntil))}</p>

      <div class="metrics-grid compact">${summaryMarkup}</div>

      <div class="split-grid">
        <div class="panel">
          <div class="panel-title">${escapeHtml(t('reports.companyInfo'))}</div>
          <div class="info-grid">${buildInfoGridHtml(companyInfoItems.value)}</div>
        </div>
        <div class="panel muted">
          <div class="panel-title">${escapeHtml(t('reports.campaignInfo'))}</div>
          <div class="info-grid">${buildInfoGridHtml([
            { label: t('reports.tableCampaign'), value: campaign.name },
            { label: t('reports.tableStatus'), value: campaign.status },
            { label: t('reports.campaignWindow'), value: `${formatDate(campaign.availableFrom)} - ${formatDate(campaign.availableUntil)}` },
            { label: t('reports.totalInvited'), value: String(campaign.invitedCount) },
            { label: t('reports.totalCompleted'), value: String(campaign.completedCount) },
            { label: t('reports.attempts'), value: String(report.attempts.length) },
          ])}</div>
        </div>
      </div>

      <div class="split-grid">
        <div class="panel">
          <div class="panel-title">${escapeHtml(t('reports.topCandidates'))}</div>
          <div class="panel-copy">${escapeHtml(attemptsCountLabel(report.attempts.length))}</div>
          <div class="leaders-list">${leadersMarkup}</div>
        </div>
        <div class="panel muted">
          <div class="panel-title">${escapeHtml(t('reports.campaignPassRate'))}</div>
          <div class="progress-track">
            <div class="progress-fill" style="width:${Math.min(Math.max(report.passRate, 0), 100)}%"></div>
          </div>
          <div class="stat-list">
            <div class="stat-row"><span>${escapeHtml(t('reports.totalInvited'))}</span><strong>${campaign.invitedCount}</strong></div>
            <div class="stat-row"><span>${escapeHtml(t('reports.totalPassed'))}</span><strong>${campaign.passedCount}</strong></div>
            <div class="stat-row"><span>${escapeHtml(t('reports.totalFailed'))}</span><strong>${getFailedCount(campaign)}</strong></div>
          </div>
        </div>
      </div>

      <table class="report-table">
        <thead>
          <tr>
            <th>${escapeHtml(t('reports.attemptCandidate'))}</th>
            <th>${escapeHtml(t('reports.attemptQuestionnaire'))}</th>
            <th>${escapeHtml(t('reports.attemptScore'))}</th>
            <th>${escapeHtml(t('reports.attemptResult'))}</th>
            <th>${escapeHtml(t('reports.attemptSubmitted'))}</th>
          </tr>
        </thead>
        <tbody>${attemptsMarkup}</tbody>
      </table>
    </section>
  `
}

function buildInfoGridHtml(items: { label: string; value: string }[]) {
  return items.map(item => `
    <div class="info-card">
      <div class="info-label">${escapeHtml(item.label)}</div>
      <div class="info-value">${escapeHtml(item.value)}</div>
    </div>
  `).join('')
}

function buildAttemptRowHtml(attempt: CampaignReportAttempt) {
  return `
    <tr>
      <td>
        <div class="candidate-name">${escapeHtml(attempt.candidateName)}</div>
        <div class="candidate-email">${escapeHtml(attempt.candidateEmail || t('common.noData'))}</div>
      </td>
      <td>${escapeHtml(attempt.questionnaireTitle || t('reports.allQuestionnaires'))}</td>
      <td>${escapeHtml(formatPercent(attempt.percentage))}</td>
      <td>${escapeHtml(attempt.passed ? t('reports.resultPassed') : t('reports.resultFailed'))}</td>
      <td>${escapeHtml(formatDateTime(attempt.submittedAt || attempt.startedAt))}</td>
    </tr>
  `
}

function reportDocumentStyle() {
  return `
    <style>
      * { box-sizing: border-box; }
      body { margin: 0; font-family: Arial, Helvetica, sans-serif; color: #0f172a; background: #ffffff; }
      .report-shell { width: 100%; padding: 24px; background: #ffffff; }
      .report-header {
        display: flex;
        justify-content: space-between;
        gap: 24px;
        align-items: flex-start;
        border-bottom: 2px solid #e2e8f0;
        padding-bottom: 18px;
        margin-bottom: 24px;
      }
      .report-header h1 {
        margin: 6px 0 8px;
        font-size: 28px;
        line-height: 1.15;
      }
      .report-header p {
        margin: 0;
        color: #475569;
        font-size: 13px;
        line-height: 1.6;
        max-width: 620px;
      }
      .eyebrow {
        font-size: 11px;
        font-weight: 700;
        letter-spacing: 0.18em;
        text-transform: uppercase;
        color: #64748b;
      }
      .stamp {
        min-width: 180px;
        padding: 12px 14px;
        border: 1px solid #dbe4ee;
        border-radius: 14px;
        background: #f8fafc;
        font-size: 12px;
        color: #475569;
        text-align: right;
      }
      .report-section {
        margin-bottom: 24px;
        page-break-inside: avoid;
      }
      .page-break {
        page-break-before: always;
      }
      .section-title {
        font-size: 18px;
        font-weight: 700;
        margin-bottom: 8px;
      }
      .section-copy {
        margin: 0 0 16px;
        color: #475569;
        font-size: 13px;
        line-height: 1.6;
      }
      .metrics-grid {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 12px;
      }
      .metrics-grid.compact {
        grid-template-columns: repeat(4, minmax(0, 1fr));
        margin-bottom: 18px;
      }
      .info-grid {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 12px;
      }
      .info-card {
        border: 1px solid #dbe4ee;
        border-radius: 14px;
        padding: 12px 14px;
        background: #ffffff;
      }
      .info-label {
        font-size: 11px;
        font-weight: 700;
        letter-spacing: 0.12em;
        text-transform: uppercase;
        color: #64748b;
      }
      .info-value {
        margin-top: 8px;
        font-size: 13px;
        line-height: 1.5;
        color: #0f172a;
        word-break: break-word;
      }
      .metric-card {
        border: 1px solid #dbe4ee;
        border-radius: 16px;
        padding: 14px 16px;
        background: #ffffff;
      }
      .metric-card.compact {
        border-radius: 14px;
      }
      .metric-label {
        font-size: 11px;
        font-weight: 700;
        letter-spacing: 0.14em;
        text-transform: uppercase;
        color: #64748b;
      }
      .metric-value {
        margin-top: 12px;
        font-size: 26px;
        font-weight: 700;
        color: #0f172a;
      }
      .split-grid {
        display: grid;
        grid-template-columns: minmax(0, 1.2fr) minmax(0, 0.8fr);
        gap: 16px;
        margin-bottom: 18px;
      }
      .panel {
        border: 1px solid #dbe4ee;
        border-radius: 18px;
        padding: 18px;
        background: #ffffff;
      }
      .panel.muted {
        background: #f8fafc;
      }
      .panel-title {
        font-size: 14px;
        font-weight: 700;
        color: #0f172a;
      }
      .panel-copy {
        margin-top: 4px;
        margin-bottom: 14px;
        color: #64748b;
        font-size: 12px;
      }
      .leaders-list {
        display: grid;
        gap: 10px;
      }
      .leader-row {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 12px;
        border: 1px solid #e2e8f0;
        border-radius: 12px;
        padding: 12px 14px;
      }
      .leader-name,
      .candidate-name {
        font-size: 13px;
        font-weight: 700;
        color: #0f172a;
      }
      .leader-email,
      .candidate-email {
        margin-top: 4px;
        font-size: 11px;
        color: #64748b;
      }
      .leader-score {
        font-size: 18px;
        font-weight: 700;
      }
      .progress-track {
        margin-top: 16px;
        height: 12px;
        border-radius: 999px;
        background: #e2e8f0;
        overflow: hidden;
      }
      .progress-fill {
        height: 100%;
        border-radius: 999px;
        background: #1d4ed8;
      }
      .stat-list {
        margin-top: 16px;
        display: grid;
        gap: 10px;
      }
      .stat-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
        padding: 10px 12px;
        border-radius: 12px;
        background: #ffffff;
        border: 1px solid #e2e8f0;
        font-size: 12px;
        color: #475569;
      }
      .empty-block {
        border: 1px dashed #cbd5e1;
        border-radius: 12px;
        padding: 18px;
        color: #64748b;
        font-size: 12px;
      }
      .report-table {
        width: 100%;
        border-collapse: collapse;
        border: 1px solid #dbe4ee;
        border-radius: 16px;
        overflow: hidden;
      }
      .report-table thead {
        background: #f8fafc;
      }
      .report-table th,
      .report-table td {
        padding: 12px 14px;
        border-bottom: 1px solid #e2e8f0;
        text-align: left;
        vertical-align: top;
        font-size: 12px;
      }
      .report-table th {
        font-size: 11px;
        font-weight: 700;
        letter-spacing: 0.12em;
        text-transform: uppercase;
        color: #64748b;
      }
      .report-table tr:last-child td {
        border-bottom: none;
      }
      .empty-cell {
        text-align: center;
        color: #64748b;
        padding: 20px 14px;
      }
    </style>
  `
}

function escapeHtml(value: string) {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;')
}
</script>
