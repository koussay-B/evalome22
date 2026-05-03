<template>
  <div class="overflow-x-hidden bg-[#FDFCFB]"> <section class="relative isolate overflow-hidden border-b border-stone-200/60">
      <div class="absolute inset-0 -z-10">
        <div class="absolute -top-24 left-0 h-80 w-80 rounded-full bg-orange-200/20 blur-3xl" />
        <div class="absolute right-0 top-24 h-96 w-96 rounded-full bg-amber-100/15 blur-3xl" />
        <div class="absolute inset-0 [background-image:radial-gradient(circle,oklch(0.75_0.15_55/0.05)_1px,transparent_1px)] [background-size:24px_24px]" />
      </div>

      <div class="mx-auto max-w-7xl px-4 py-20 sm:px-6 lg:px-8">
        <div class="grid gap-10 lg:grid-cols-[1.05fr_0.95fr] lg:items-start">
          <div class="max-w-2xl">
            <span class="inline-flex items-center rounded-full border border-orange-200 bg-orange-50 px-4 py-1.5 text-xs font-bold uppercase tracking-[0.22em] text-orange-700">
              {{ t('public.companyRequest.badge') }}
            </span>
            
            <h1 class="mt-6 text-5xl text-slate-800 tracking-tight text-slate-800 sm:text-6xl">
              {{ t('public.companyRequest.title') }}
            </h1>
            <p class="mt-6 max-w-xl text-lg leading-8 text-slate-500">
              {{ t('public.companyRequest.subtitle') }}
            </p>

            <div class="mt-10 grid gap-4 sm:grid-cols-3">
              <div class="rounded-2xl border border-stone-200/60 bg-[#F1EFEC]/80 p-5 backdrop-blur-sm">
                <p class="text-sm font-bold text-slate-800">{{ t('public.companyRequest.feature1Title') }}</p>
                <p class="mt-2 text-xs leading-6 text-slate-500">{{ t('public.companyRequest.feature1Desc') }}</p>
              </div>
              <div class="rounded-2xl border border-stone-200/60 bg-[#F1EFEC]/80 p-5 backdrop-blur-sm">
                <p class="text-sm font-bold text-slate-800">{{ t('public.companyRequest.feature2Title') }}</p>
                <p class="mt-2 text-xs leading-6 text-slate-500">{{ t('public.companyRequest.feature2Desc') }}</p>
              </div>
              <div class="rounded-2xl border border-stone-200/60 bg-[#F1EFEC]/80 p-5 backdrop-blur-sm">
                <p class="text-sm font-bold text-slate-800">{{ t('public.companyRequest.feature3Title') }}</p>
                <p class="mt-2 text-xs leading-6 text-slate-500">{{ t('public.companyRequest.feature3Desc') }}</p>
              </div>
            </div>
          </div>

          <Card class="border-stone-200/70 bg-white/95 shadow-2xl shadow-orange-950/5 backdrop-blur">
            <CardHeader class="space-y-2">
              <CardTitle class="text-2xl text-slate-800 text-slate-800 tracking-tight">{{ t('public.companyRequest.cardTitle') }}</CardTitle>
              <CardDescription class="text-slate-500">
                {{ t('public.companyRequest.cardDesc') }}
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div v-if="submitted" class="rounded-2xl border border-orange-200 bg-orange-50 p-5 text-orange-800">
                <p class="text-sm font-bold">{{ t('public.companyRequest.successTitle') }}</p>
                <p class="mt-2 text-sm leading-6">
                  {{ t('public.companyRequest.successDesc', { email: form.requesterEmail }) }}
                </p>
              </div>

              <form v-else class="space-y-4" @submit.prevent="submitRequest">
                <div v-if="errorMessage" class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                  {{ errorMessage }}
                </div>

                <div class="grid gap-4 sm:grid-cols-2">
                  <div class="space-y-2 sm:col-span-2">
                    <label class="text-[10px] text-slate-800 uppercase tracking-widest text-slate-400">{{ t('public.companyRequest.companyName') }}</label>
                    <Input v-model="form.name" required :placeholder="t('public.companyRequest.companyNamePlaceholder')" class="border-stone-200 focus-visible:ring-orange-500" />
                  </div>

                  <div class="space-y-2">
                    <label class="text-[10px] text-slate-800 uppercase tracking-widest text-slate-400">{{ t('public.companyRequest.requesterName') }}</label>
                    <Input v-model="form.requesterName" required :placeholder="t('public.companyRequest.requesterNamePlaceholder')" class="border-stone-200 focus-visible:ring-orange-500" />
                  </div>

                  <div class="space-y-2">
                    <label class="text-[10px] text-slate-800 uppercase tracking-widest text-slate-400">{{ t('public.companyRequest.requesterEmail') }}</label>
                    <Input v-model="form.requesterEmail" type="email" required :placeholder="t('public.companyRequest.requesterEmailPlaceholder')" class="border-stone-200 focus-visible:ring-orange-500" />
                  </div>

                  <div class="space-y-2">
                    <label class="text-[10px] text-slate-800 uppercase tracking-widest text-slate-400">{{ t('public.companyRequest.phone') }}</label>
                    <Input v-model="form.phone" :placeholder="t('public.companyRequest.phonePlaceholder')" class="border-stone-200 focus-visible:ring-orange-500" />
                  </div>

                  <div class="space-y-2">
                    <label class="text-[10px] text-slate-800 uppercase tracking-widest text-slate-400">{{ t('public.companyRequest.website') }}</label>
                    <Input v-model="form.website" :placeholder="t('public.companyRequest.websitePlaceholder')" class="border-stone-200 focus-visible:ring-orange-500" />
                  </div>

                  <div class="space-y-2 sm:col-span-2">
                    <label class="text-[10px] text-slate-800 uppercase tracking-widest text-slate-400">{{ t('public.companyRequest.address') }}</label>
                    <Input v-model="form.address" :placeholder="t('public.companyRequest.addressPlaceholder')" class="border-stone-200 focus-visible:ring-orange-500" />
                  </div>

                  <div class="space-y-2 sm:col-span-2">
                    <label class="text-[10px] text-slate-800 uppercase tracking-widest text-slate-400">{{ t('public.companyRequest.description') }}</label>
                    <textarea
                      v-model="form.description"
                      rows="4"
                      :placeholder="t('public.companyRequest.descriptionPlaceholder')"
                      class="w-full rounded-md border border-stone-200 bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-orange-500 focus-visible:ring-offset-2 resize-none"
                    />
                  </div>
                </div>

                <Button type="submit" class="w-full bg-orange-600 text-white hover:bg-orange-700 shadow-lg shadow-orange-600/20 font-bold py-6 rounded-xl transition-all" :disabled="submitting">
                  {{ submitting ? t('public.companyRequest.submitting') : t('public.companyRequest.submit') }}
                </Button>
              </form>
            </CardContent>
          </Card>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { useCompanyStore } from '@/store/companyStore'
import { useLocale } from '@/composables/useLocale'

const companyStore = useCompanyStore()
const { t } = useLocale()

const submitting = ref(false)
const submitted = ref(false)
const errorMessage = ref('')

const form = reactive({
  name: '',
  requesterName: '',
  requesterEmail: '',
  phone: '',
  website: '',
  address: '',
  description: '',
})

async function submitRequest() {
  submitting.value = true
  errorMessage.value = ''

  const { error } = await companyStore.submitCompanyRequest({
    name: form.name.trim(),
    requesterName: form.requesterName.trim(),
    requesterEmail: form.requesterEmail.trim(),
    phone: form.phone.trim() || undefined,
    website: form.website.trim() || undefined,
    address: form.address.trim() || undefined,
    description: form.description.trim() || undefined,
  })

  submitting.value = false
  if (error) {
    errorMessage.value = error
    return
  }

  submitted.value = true
}
</script>
