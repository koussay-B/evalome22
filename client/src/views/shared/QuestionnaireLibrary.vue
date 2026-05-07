<template>
  <div class="space-y-5">
    <AlertDialog v-model:open="isDeleteOpen">
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{{ t('questionnaireLibrary.deleteConfirmTitle') }}</AlertDialogTitle>
          <AlertDialogDescription>
            {{ t('questionnaireLibrary.deleteConfirmDesc', { title: selectedForDelete?.title ?? '' }) }}
          </AlertDialogDescription>
        </AlertDialogHeader>

        <p v-if="deleteError" class="text-sm font-medium text-red-600">
          {{ deleteError }}
        </p>

        <AlertDialogFooter>
          <AlertDialogCancel :disabled="deletingId !== null" @click="deleteError = ''">
            {{ t('questionnaireLibrary.cancel') }}
          </AlertDialogCancel>
          <AlertDialogAction
            class="bg-red-600 hover:bg-red-700 text-white border-none"
            :disabled="deletingId !== null"
            @click.prevent="confirmDelete"
          >
            {{ deletingId !== null ? t('questionnaireLibrary.deleting') : t('questionnaireLibrary.delete') }}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>

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
    class="h-fit border-slate-100 bg-white shadow-sm transition-all duration-300 hover:border-orange-200"  >
    <CardHeader class="p-4 py-3 space-y-1">
      <div class="flex items-start justify-between gap-3">
        <div v-if="editingId === questionnaire.id" class="min-w-0 flex-1 space-y-2">
          <Input
            v-model="editTitle"
            :placeholder="t('questionnaireLibrary.newTitlePlaceholder')"
            class="h-8 bg-background border-border focus-visible:ring-primary/30"
          />
          <Input
            v-model="editDescription"
            :placeholder="t('questionnaireLibrary.newDescriptionPlaceholder')"
            class="h-8 bg-background border-border focus-visible:ring-primary/30"
          />
        </div>

        <div v-else class="min-w-0">
          <CardTitle class="text-base  tracking-tight text-slate-800 truncate leading-none">
            {{ questionnaire.title }}
          </CardTitle>
          <CardDescription class="mt-0.5 text-[10px] line-clamp-1 leading-tight text-slate-500 font-medium">
            {{ questionnaire.description || t('questionnaireLibrary.noDescription') }}
          </CardDescription>
        </div>

        <div class="flex shrink-0 items-center gap-2">
          <Button
            v-if="editingId !== questionnaire.id"
            type="button"
            variant="outline"
            class="size-8 p-0 border-slate-100 text-slate-500 hover:text-orange-600 hover:bg-orange-50"
            :title="t('questionnaireLibrary.edit')"
            @click="startEdit(questionnaire)"
          >
            <Pencil class="w-3.5 h-3.5" />
          </Button>

          <Button
            v-if="editingId !== questionnaire.id"
            type="button"
            variant="outline"
            class="size-8 p-0 border-red-100 text-red-500 hover:text-red-600 hover:bg-red-50"
            :title="t('questionnaireLibrary.delete')"
            :disabled="deletingId === questionnaire.id"
            @click="openDelete(questionnaire)"
          >
            <Trash2 class="w-3.5 h-3.5" />
          </Button>

          <span
            class="inline-flex rounded-full px-2 py-0.5 text-[8px]  uppercase tracking-widest border"
            :class="questionnaire.status === 'Published'
              ? 'bg-emerald-50 text-emerald-600 border-emerald-100'
              : 'bg-orange-50 text-orange-600 border-orange-100'"
          >
            {{ questionnaire.status }}
          </span>
        </div>
      </div>
    </CardHeader>

    <CardContent class="p-4 pt-0 pb-3 space-y-1.5">
      <p v-if="editingId === questionnaire.id && editError" class="text-xs font-medium text-red-600">
        {{ editError }}
      </p>

      <div class="flex items-center justify-between text-[10px] font-bold text-slate-400 uppercase tracking-tight">
        <span>{{ t('questionnaireLibrary.questionsCount', { count: questionnaire.questionnaireQuestions.length }) }}</span>
        <span>{{ formatDate(questionnaire.updatedAt ?? questionnaire.createdAt) }}</span>
      </div>

      <div v-if="editingId === questionnaire.id" class="grid grid-cols-2 gap-2">
        <Button
          type="button"
          variant="outline"
          class="h-8.5 gap-2 rounded-xl border-slate-100 bg-slate-50/60 text-slate-600 text-[11px] font-bold hover:bg-slate-100 transition-all"
          :disabled="updatingId === questionnaire.id"
          @click="cancelEdit"
        >
          <X class="w-3.5 h-3.5" />
          {{ t('questionnaireLibrary.cancel') }}
        </Button>
        <Button
          type="button"
          class="h-8.5 gap-2 rounded-xl text-[11px] font-bold transition-all"
          :disabled="updatingId === questionnaire.id || !editTitle.trim()"
          @click="saveQuestionnaire(questionnaire.id)"
        >
          <Check class="w-3.5 h-3.5" />
          {{ updatingId === questionnaire.id ? t('questionnaireLibrary.saving') : t('questionnaireLibrary.save') }}
        </Button>
      </div>
      
      <Button 
        v-else
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
import { ArrowRight, Check, FileText, Pencil, Plus, Trash2, X } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { createQuestionnaireApi, deleteQuestionnaireApi, getQuestionnairesApi, updateQuestionnaireApi } from '@/utils/api'
import type { QuestionnaireItem } from '@/utils/models'
import { useTopicStore } from '@/store/topicStore'
import { useLocale } from '@/composables/useLocale'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'

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
const editingId = ref<number | null>(null)
const updatingId = ref<number | null>(null)
const editTitle = ref('')
const editDescription = ref('')
const editError = ref('')
const isDeleteOpen = ref(false)
const selectedForDelete = ref<QuestionnaireItem | null>(null)
const deletingId = ref<number | null>(null)
const deleteError = ref('')

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

function startEdit(questionnaire: QuestionnaireItem) {
  editingId.value = questionnaire.id
  editTitle.value = questionnaire.title
  editDescription.value = questionnaire.description ?? ''
  editError.value = ''
}

function cancelEdit() {
  editingId.value = null
  editTitle.value = ''
  editDescription.value = ''
  editError.value = ''
}

async function saveQuestionnaire(id: number) {
  if (!editTitle.value.trim()) return

  updatingId.value = id
  editError.value = ''

  const { data, error } = await updateQuestionnaireApi(id, {
    title: editTitle.value.trim(),
    description: editDescription.value.trim() || null,
  })

  updatingId.value = null

  if (error || !data) {
    editError.value = error ?? 'Failed to update questionnaire'
    return
  }

  questionnaires.value = questionnaires.value.map(item => item.id === id ? data : item)
  cancelEdit()
}

function openDelete(questionnaire: QuestionnaireItem) {
  selectedForDelete.value = questionnaire
  deleteError.value = ''
  isDeleteOpen.value = true
}

async function confirmDelete() {
  if (!selectedForDelete.value) return

  const id = selectedForDelete.value.id
  deletingId.value = id
  deleteError.value = ''

  const { error } = await deleteQuestionnaireApi(id)

  deletingId.value = null

  if (error) {
    deleteError.value = error
    return
  }

  questionnaires.value = questionnaires.value.filter(item => item.id !== id)
  if (editingId.value === id) cancelEdit()
  selectedForDelete.value = null
  isDeleteOpen.value = false
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}
</script>
