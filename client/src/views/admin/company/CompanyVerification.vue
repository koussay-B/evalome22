<template>
  <div class="space-y-6">
    <!-- ── Header ─────────────────────────────────────────────────────────── -->
    <div class="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
      <div>
        <p class="text-[10px] font-semibold uppercase tracking-[0.25em] text-primary/60 mb-1">{{ t('companyVerification.eyebrow') }}</p>
        <h1 class="text-3xl text-slate-800 tracking-tight font-semibold text-foreground">{{ t('companyVerification.title') }}</h1>
        <p class="mt-2 text-sm text-muted-foreground tracking-tight">
          {{ t('companyVerification.subtitle') }}
        </p>
      </div>
      <div class="flex items-center gap-2">
        <Button variant="outline" @click="loadRequests" :disabled="loading" class="font-semibold bg-[oklch(65%_0.18_51.057)] text-white">{{ t('companyVerification.refresh') }}</Button>
        <span class="rounded-full border border-[oklch(65%_0.18_51.057_/_0.2)] bg-[oklch(97.5%_0.02_51.057)] px-3 py-1 text-xs font-semibold text-[oklch(65%_0.18_51.057)]">
          {{ t('companyVerification.pendingCount', { count: pendingCount }) }}
        </span>
      </div>
    </div>

    <!-- ── Content: Request Table ─────────────────────────────────────────── -->
    <Card class="border-border/60 shadow-sm overflow-hidden rounded-2xl bg-gray-50">
      <CardContent class="p-0">
        <div v-if="errorMessage" class="border-b border-red-200 bg-red-50 px-6 py-4 text-sm text-red-700 ">
          {{ errorMessage }}
        </div>

        <div class="overflow-x-auto">
          <table class="w-full text-left">
            <thead class="bg-gray-50 text-[11px] font-semibold uppercase tracking-widest text-muted-foreground border-b border-border/40">
              <tr>
                <th class="px-6 py-4">{{ t('companyVerification.company') }}</th>
                <th class="px-6 py-4">{{ t('companyVerification.requester') }}</th>
                <th class="px-6 py-4">{{ t('companyVerification.submitted') }}</th>
                <th class="px-6 py-4">{{ t('companyVerification.status') }}</th>
                <th class="px-6 py-4 text-right">{{ t('companyVerification.actions') }}</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-border text-sm">
              <tr v-if="loading" v-for="i in 5" :key="i">
                <td class="px-6 py-5" colspan="5">
                  <Skeleton class="h-10 w-full rounded-2xl" />
                </td>
              </tr>

              <tr v-else-if="!requests.length">
                <td class="px-6 py-12 text-center text-muted-foreground" colspan="5">
                  {{ t('companyVerification.empty') }}
                </td>
              </tr>

              <tr v-else v-for="request in requests" :key="request.id" class="hover:bg-muted/20 transition-colors">
                <td class="px-6 py-5">
                  <div class="font-semibold text-foreground">{{ request.name }}</div>
                  <div class="mt-1 text-xs text-muted-foreground tracking-tight">{{ request.description || t('companyVerification.noDescription') }}</div>
                </td>
                <td class="px-6 py-5">
                  <div class="font-medium text-foreground">{{ request.requesterName || t('companyVerification.noRequester') }}</div>
                  <div class="mt-1 text-xs text-muted-foreground tracking-tight">{{ request.requesterEmail || t('companyVerification.noRequesterEmail') }}</div>
                </td>
                <td class="px-6 py-5 text-muted-foreground tracking-tight">{{ formatDate(request.createdAt) }}</td>
                <td class="px-6 py-5">
                  <span class="inline-flex rounded-full px-2.5 py-1 text-[10px] font-semibold uppercase tracking-widest" :class="statusClass(request.status)">
                    {{ request.status }}
                  </span>
                  <p v-if="request.status === 'Rejected' && request.rejectionReason" class="mt-2 max-w-xs text-xs text-muted-foreground">
                    {{ request.rejectionReason }}
                  </p>
                </td>
                <td class="px-6 py-5"> <!-- ── Actions: Row Buttons ── -->
                  <div class="flex justify-end gap-2">
                    <Button
                      size="sm"
                      class="bg-[oklch(65%_0.18_51.057)] text-white hover:opacity-90 transition-all font-semibold"
                      :disabled="request.status === 'Approved' || approvingId === request.id || rejectingId === request.id"
                      @click="approveRequest(request.id)"
                    >
                      <CheckCircle class="mr-1 h-3.5 w-3.5" />
                      {{ approvingId === request.id ? t('companyVerification.approving') : t('companyVerification.approve') }}
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      class="border-[oklch(65%_0.18_51.057_/_0.2)] text-red-600 hover:bg-red-50 font-semibold"
                      :disabled="request.status === 'Approved' || approvingId === request.id || rejectingId === request.id"
                      @click="openRejectDialog(request)"
                    >
                      <XCircle class="mr-1 h-3.5 w-3.5" />
                      {{ t('companyVerification.reject') }}
                    </Button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </CardContent>
    </Card>

    <!-- ── Actions: Rejection Dialog ──────────────────────────────────────── -->
    <Dialog v-model:open="rejectDialogOpen">
      <DialogContent class="sm:max-w-lg rounded-2xl border-border/60 shadow-lg">
        <DialogHeader>
          <DialogTitle class="text-xl font-semibold tracking-tight">{{ t('companyVerification.rejectTitle') }}</DialogTitle>
          <DialogDescription class="text-sm">
            {{ t('companyVerification.rejectDesc', { email: selectedRequest?.requesterEmail || t('companyVerification.noRequesterEmail') }) }}
          </DialogDescription>
        </DialogHeader>

        <div class="space-y-4 pt-2">
          <p v-if="selectedRequest" class="text-sm text-muted-foreground tracking-tight">
            {{ t('companyVerification.rejectHelper', {
              company: selectedRequest.name,
              requester: selectedRequest.requesterName || t('companyVerification.noRequester')
            }) }}
          </p>

          <textarea
            v-model="rejectReason"
            rows="5"
            :placeholder="t('companyVerification.rejectPlaceholder')"
            class="w-full rounded-xl border border-input bg-white px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 resize-none transition-all"
          />
          <p v-if="rejectError" class="text-sm text-red-600">{{ rejectError }}</p>
        </div>

        <DialogFooter class="gap-2 sm:justify-end mt-4">
          <Button variant="outline" @click="rejectDialogOpen = false" class="font-semibold">{{ t('common.cancel') }}</Button>
          <Button
            class="bg-red-600 text-white hover:bg-red-700 font-semibold shadow-sm"
            :disabled="!selectedRequest || rejectingId === selectedRequest.id"
            @click="rejectRequest"
          >
            {{ rejectingId === selectedRequest?.id ? t('companyVerification.rejecting') : t('companyVerification.sendRejection') }}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { CheckCircle, XCircle } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { useCompanyStore } from '@/store/companyStore'
import type { Company } from '@/utils/models'
import { useLocale } from '@/composables/useLocale'

const companyStore = useCompanyStore()
const { t } = useLocale()

const loading = ref(true)
const errorMessage = ref('')
const approvingId = ref<number | null>(null)
const rejectingId = ref<number | null>(null)
const rejectDialogOpen = ref(false)
const selectedRequest = ref<Company | null>(null)
const rejectReason = ref('')
const rejectError = ref('')

const requests = computed(() => companyStore.requests)
const pendingCount = computed(() => companyStore.requests.filter(item => item.status === 'Pending').length)

onMounted(loadRequests)

async function loadRequests() {
  loading.value = true
  errorMessage.value = ''
  const { error } = await companyStore.fetchRequests()
  loading.value = false
  if (error) errorMessage.value = error
}

async function approveRequest(id: number) {
  approvingId.value = id
  errorMessage.value = ''
  const { error } = await companyStore.approveRequest(id)
  approvingId.value = null
  if (error) {
    errorMessage.value = error
    return
  }
  await loadRequests()
}

function openRejectDialog(request: Company) {
  selectedRequest.value = request
  rejectReason.value = ''
  rejectError.value = ''
  rejectDialogOpen.value = true
}

async function rejectRequest() {
  if (!selectedRequest.value) return
  if (!rejectReason.value.trim()) {
    rejectError.value = t('companyVerification.rejectRequired')
    return
  }

  rejectingId.value = selectedRequest.value.id
  rejectError.value = ''
  const { error } = await companyStore.rejectRequest(selectedRequest.value.id, rejectReason.value.trim())
  rejectingId.value = null
  if (error) {
    rejectError.value = error
    return
  }

  rejectDialogOpen.value = false
  await loadRequests()
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

function statusClass(status: Company['status']) {
  if (status === 'Approved') return 'bg-emerald-100 text-emerald-700'
  if (status === 'Rejected') return 'bg-red-100 text-red-700'
  return 'bg-[oklch(97.5%_0.02_51.057)] text-[oklch(65%_0.18_51.057)] border border-[oklch(65%_0.18_51.057_/_0.1)]'
}
</script>
