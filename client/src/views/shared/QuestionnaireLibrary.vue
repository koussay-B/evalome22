<template>
  <div class="space-y-5">
    <div class="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
      <div>
        <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">{{ t('questionnaireLibrary.eyebrow') }}</p>
        <h1 class="text-3xl  tracking-tight text-slate-800">{{ t('questionnaireLibrary.title') }}</h1>
        <p class="mt-2 max-w-2xl text-sm text-muted-foreground">
          {{ t('questionnaireLibrary.subtitle') }}
        </p>
      </div>

      <Card class="w-full max-w-xl border-slate-200/60 bg-gray-50 shadow-sm backdrop-blur-sm">            <CardContent class="p-4">
          <form class="grid gap-5 sm:grid-cols-[1fr_1fr_auto]" @submit.prevent="createQuestionnaire">
            <Input v-model="newTitle" :placeholder="t('questionnaireLibrary.newTitlePlaceholder')" class="w-41 bg-background border-border focus-visible:ring-primary/30" />
            <Input v-model="newDescription" :placeholder="t('questionnaireLibrary.newDescriptionPlaceholder')" class="bg-background border-border focus-visible:ring-primary/30" />
            <Button type="submit" :disabled="creating || !newTitle.trim()" class="gap-2 shadow-[0_4px_16px_0_oklch(0.52_0.16_42/0.22)] hover:shadow-[0_8px_24px_0_oklch(0.52_0.16_42/0.35)] transition-shadow">
              <Plus class="w-4 h-4" />
              {{ creating ? t('questionnaireLibrary.creating') : t('questionnaireLibrary.create') }}
            </Button>
          </form>
          <p v-if="prefillThemeName" class="mt-3 text-xs text-muted-foreground ">
            {{ t('questionnaireLibrary.startedFromTheme') }} <span class="font-semibold text-slate-800">{{ prefillThemeName }}</span>
          </p>
          <p v-if="createError" class="mt-3 text-sm text-red-600">{{ createError }}</p>
        </CardContent>
      </Card>
    </div>

    <div class="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
  <Card
    v-for="questionnaire in questionnaires"
    :key="questionnaire.id"
    class="h-fit max-h-[180px] border-slate-100 bg-white shadow-sm transition-all duration-300 hover:border-orange-200"  >
    <CardHeader class="p-4 py-3 space-y-1">
      <div class="flex items-start justify-between gap-3">
        <div class="min-w-0">
          <CardTitle class="text-base  tracking-tight text-slate-800 truncate leading-none">
            {{ questionnaire.title }}
          </CardTitle>
          <CardDescription class="mt-0.5 text-[10px] line-clamp-1 leading-tight text-slate-500 font-medium">
            {{ questionnaire.description || t('questionnaireLibrary.noDescription') }}
          </CardDescription>
        </div>
        
        <span
          class="inline-flex shrink-0 rounded-full px-2 py-0.5 text-[8px]  uppercase tracking-widest border"
          :class="questionnaire.status === 'Published'
            ? 'bg-emerald-50 text-emerald-600 border-emerald-100'
            : 'bg-orange-50 text-orange-600 border-orange-100'"
        >
          {{ questionnaire.status }}
        </span>
      </div>
    </CardHeader>

    <CardContent class="p-4 pt-0 pb-3 space-y-1.5">
      <div class="flex items-center justify-between text-[10px] font-bold text-slate-400 uppercase tracking-tight">
        <span>{{ t('questionnaireLibrary.questionsCount', { count: questionnaire.questionnaireQuestions.length }) }}</span>
        <span>{{ formatDate(questionnaire.updatedAt ?? questionnaire.createdAt) }}</span>
      </div>
      
      <Button 
        variant="outline" 
        class="w-full h-8.5 gap-2 rounded-xl border-orange-100 bg-orange-50/30 text-orange-600 text-[11px] font-bold hover:bg-orange-50 transition-all" 
        @click="openBuilder(questionnaire.id)"
      >
        <ArrowRight class="w-3.5 h-3.5" />
        {{ t('questionnaireLibrary.openBuilder') }}
      </Button>
    </CardContent>
  </Card>

  <Card v-if="!loading && !questionnaires.length" class="border-dashed border-slate-200 md:col-span-2 xl:col-span-3 bg-slate-50/50">
    <CardContent class="flex flex-col items-center justify-center gap-3 py-10 text-center">
      <div class="size-12 rounded-2xl bg-white border border-slate-100 flex items-center justify-center mb-1 shadow-sm">
        <FileText class="w-6 h-6 text-slate-300" />
      </div>
      <div>
        <p class="text-sm font-bold text-slate-800">{{ t('questionnaireLibrary.emptyTitle') }}</p>
        <p class="mt-1 text-xs text-slate-400">{{ t('questionnaireLibrary.emptyDesc') }}</p>
      </div>
    </CardContent>
  </Card>
</div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowRight, FileText, Plus } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { createQuestionnaireApi, getQuestionnairesApi } from '@/utils/api'
import type { QuestionnaireItem } from '@/utils/models'
import { useTopicStore } from '@/store/topicStore'
import { useLocale } from '@/composables/useLocale'

const route = useRoute()
const router = useRouter()
const topicStore = useTopicStore()
const { t } = useLocale()

const loading = ref(true)
const creating = ref(false)
const createError = ref('')
const questionnaires = ref<QuestionnaireItem[]>([])
const newTitle = ref('')
const newDescription = ref('')

const prefix = computed(() => route.path.startsWith('/company') ? '/company' : '/formateur')
const prefillThemeId = computed(() => {
  const raw = route.query.themeId
  const id = Number(raw)
  return Number.isFinite(id) && id > 0 ? id : null
})
const prefillThemeName = computed(() => {
  if (!prefillThemeId.value) return ''
  for (const theme of topicStore.themes) {
    if (theme.id === prefillThemeId.value) return theme.name
    const child = theme.children.find(item => item.id === prefillThemeId.value)
    if (child) return `${theme.name} › ${child.name}`
  }
  return ''
})

onMounted(async () => {
  await Promise.all([loadQuestionnaires(), topicStore.fetchThemes()])
})

async function loadQuestionnaires() {
  loading.value = true
  const { data } = await getQuestionnairesApi()
  questionnaires.value = data ?? []
  loading.value = false
}

async function createQuestionnaire() {
  if (!newTitle.value.trim()) return

  creating.value = true
  createError.value = ''
  const { data, error } = await createQuestionnaireApi({
    title: newTitle.value.trim(),
    description: newDescription.value.trim() || undefined,
  })
  creating.value = false

  if (error || !data) {
    createError.value = error ?? 'Failed to create questionnaire'
    return
  }

  router.push(`${prefix.value}/questionnaire/${data.id}`)
}

function openBuilder(id: number) {
  router.push(`${prefix.value}/questionnaire/${id}`)
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}
</script>