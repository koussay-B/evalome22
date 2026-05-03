<template>
  <Sheet v-model:open="isOpen">
    <SheetContent :side="sheetSide" class="bg-white sm:max-w-md overflow-y-auto flex flex-col p-0">

      <SheetHeader class="px-6 pt-6 pb-5  border-b border-border shrink-0">
        <SheetTitle class="text-xl text-slate-800">{{ t('employees.addEmployee') }}</SheetTitle>
        <SheetDescription class="mt-1">{{ t('employees.inviteDesc') }}</SheetDescription>
      </SheetHeader>

      <form @submit.prevent="handleSubmit" class="flex flex-col gap-5 p-6 flex-1">

        <!-- Username -->
        <div class="space-y-2">
          <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">
            {{ t('employees.username') }} *
          </label>
          <div class="relative">
            <User class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <Input
              v-model="form.userName"
              :placeholder="t('employees.usernamePlaceholder')"
              class="ps-9 h-10"
              required
              autocomplete="off"
            />
          </div>
        </div>

        <!-- Email -->
        <div class="space-y-2">
          <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">
            {{ t('employees.email') }} *
          </label>
          <div class="relative">
            <Mail class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <Input
              v-model="form.email"
              type="email"
              :placeholder="t('employees.emailPlaceholder')"
              class="ps-9 h-10"
              required
              autocomplete="off"
            />
          </div>
        </div>

        <!-- Role -->
        <div class="space-y-2">
          <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">
            {{ t('employees.role') }} *
          </label>
          <Select v-model="form.role" required>
            <SelectTrigger class="h-10">
              <SelectValue :placeholder="t('employees.selectRole')" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem
                v-for="r in availableRoles"
                :key="r.value"
                :value="r.value"
              >
                <div class="flex items-center gap-2">
                  <span
                    class="w-2 h-2 rounded-full"
                    :class="getRoleDotClass(r.value)"
                  />
                  {{ r.label }}
                </div>
              </SelectItem>
            </SelectContent>
          </Select>
        </div>

        <!-- ── Company ─────────────────────────────────────────────────────── -->

        <!-- Admin, no pre-selected company → show picker button / selected card -->
        <div v-if="showCompanyPicker" class="space-y-2">
          <label class="bg-white text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">
            {{ t('employees.company') }}
          </label>

          <!-- Nothing picked yet -->
          <button
            v-if="!pickerCompany"
            type="button"
            @click="pickerOpen = true"
            class="w-full flex items-center gap-2 px-3 py-2.5 rounded-lg border border-dashed border-border hover:border-primary/50 hover:bg-muted/30 transition-colors text-muted-foreground text-sm"
          >
            <Building2 class="w-4 h-4 shrink-0" />
            <span class="flex-1 text-start">{{ t('employees.selectCompany') }}</span>
            <ChevronDown class="w-4 h-4 shrink-0" />
          </button>

          <!-- Company picked → card + change link -->
          <div v-else class="flex items-center gap-3 px-3 py-2.5 rounded-lg bg-muted/40 border border-border">
            <div
              class="w-8 h-8 rounded-md bg-background flex items-center justify-center overflow-hidden border border-border shrink-0"
            >
              <img
                v-if="pickerCompany.logo"
                :src="pickerCompany.logo"
                class="w-full h-full object-cover"
                alt=""
              />
              <Building2 v-else class="w-4 h-4 text-muted-foreground" />
            </div>
            <span class="text-sm font-semibold text-slate-800 flex-1 truncate">
              {{ pickerCompany.name }}
            </span>
            <button
              type="button"
              @click="pickerOpen = true"
              class="text-xs text-muted-foreground hover:text-slate-800 transition-colors font-medium shrink-0"
            >
              {{ t('employees.changeCompany') }}
            </button>
          </div>
        </div>

        <!-- Read-only company badge (pre-selected prop OR CompanyAdmin/Formateur auto-resolved) -->
        <div v-else-if="resolvedCompany" class="space-y-2">
          <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">
            {{ t('employees.company') }}
          </label>
          <div class="flex items-center gap-3 px-3 py-2.5 rounded-lg bg-muted/40 border border-border">
            <div
              class="w-8 h-8 rounded-md bg-background flex items-center justify-center overflow-hidden border border-border shrink-0"
            >
              <img
                v-if="resolvedCompany.logo"
                :src="resolvedCompany.logo"
                class="w-full h-full object-cover"
                alt=""
              />
              <Building2 v-else class="w-4 h-4 text-muted-foreground" />
            </div>
            <span class="text-sm font-semibold text-slate-800 flex-1 truncate">
              {{ resolvedCompany.name }}
            </span>
            <Lock class="w-3.5 h-3.5 text-muted-foreground/60 shrink-0" />
          </div>
        </div>

        <!-- Error -->
        <p v-if="formError" class="text-sm text-red-600 dark:text-red-400 -mt-1">
          {{ formError }}
        </p>

        <!-- Success -->
        <div
          v-if="successName"
          class="flex items-start gap-3 p-4 rounded-xl bg-emerald-50 dark:bg-emerald-950/20 border border-emerald-200 dark:border-emerald-900"
        >
          <CheckCircle2 class="w-5 h-5 text-emerald-600 dark:text-emerald-400 shrink-0 mt-0.5" />
          <div>
            <p class="text-sm font-semibold text-emerald-800 dark:text-emerald-300">
              {{ t('employees.inviteSuccess') }}
            </p>
            <p class="text-xs text-emerald-700/80 dark:text-emerald-400/80 mt-0.5">
              {{ t('employees.inviteSuccessDesc', { name: successName }) }}
            </p>
          </div>
        </div>

        <!-- Actions -->
        <div class="flex gap-3 pt-4 border-t border-border mt-auto">
          <button
            type="submit"
            :disabled="isSubmitting"
            class="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-[oklch(65%_0.18_51.057)] text-white text-sm font-semibold hover:opacity-90 transition-all disabled:opacity-60">
            <Loader2 v-if="isSubmitting" class="w-4 h-4 animate-spin" />
            <Send v-else class="w-4 h-4" />
            {{ isSubmitting ? t('common.saving') : t('employees.sendInvite') }}
          </button>
          <button
            type="button"
            @click="isOpen = false"
            class="px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors"
          >
            {{ t('common.cancel') }}
          </button>
        </div>

      </form>
    </SheetContent>
  </Sheet>

  <!-- Company Picker Dialog (Admin only, no pre-selected company) -->
  <CompanyPickerDialog
    v-if="showCompanyPicker"
    v-model:open="pickerOpen"
    :model-value="pickerCompany"
    @update:model-value="pickerCompany = $event"
  />
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet'
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import CompanyPickerDialog from './CompanyPickerDialog.vue'
import { useLocale }     from '@/composables/useLocale'
import { useAuthStore }  from '@/store/authStore'
import { useUserStore }  from '@/store/userStore'
import { UserRole, getRoleDotClass } from '@/utils/roles'
import type { Company } from '@utils/models'
import {
  User, Mail, Building2, Loader2, Send, CheckCircle2, ChevronDown, Lock,
} from 'lucide-vue-next'

// ── Props / Emits ─────────────────────────────────────────────────────────────

const props = defineProps<{
  companyId?:   number
  companyName?: string
}>()

const emit = defineEmits<{
  invited: []
}>()

// ── Composables ───────────────────────────────────────────────────────────────

const { t, locale } = useLocale()
const authStore     = useAuthStore()
const userStore     = useUserStore()

// ── Sheet open state ──────────────────────────────────────────────────────────

const isOpen = defineModel<boolean>('open', { default: false })

const isRtl     = computed(() => locale.value === 'ar')
const sheetSide = computed<'right' | 'left'>(() => (isRtl.value ? 'left' : 'right'))

// Reset form state when sheet closes
watch(isOpen, (open) => {
  if (!open) {
    Object.assign(form, defaultForm())
    formError.value   = null
    successName.value = null
    pickerCompany.value = null
  }
})

// ── Role options by caller role ───────────────────────────────────────────────

const availableRoles = computed(() => {
  const allRoles = [
    { value: UserRole.Admin,        label: 'Admin' },
    { value: UserRole.CompanyAdmin, label: 'Company Admin' },
    { value: UserRole.Formateur,    label: 'Formateur' },
    { value: UserRole.Condidat,     label: 'Candidat' },
  ]

  if (authStore.isAdmin) return allRoles

  // CompanyAdmin + Formateur can only assign Formateur / Condidat
  return allRoles.filter(r =>
    r.value === UserRole.Formateur || r.value === UserRole.Condidat
  )
})

// ── Company resolution ────────────────────────────────────────────────────────

/** Admin without a pre-selected company → show picker dialog */
const showCompanyPicker = computed(() => authStore.isAdmin && !props.companyId)

/** Company selected via the picker dialog (Admin-only) */
const pickerCompany = ref<Company | null>(null)
const pickerOpen    = ref(false)

/**
 * The final resolved company used for the invite payload.
 * Priority:
 *  1. Prop `companyId` (passed from a company detail sheet)
 *  2. Picker selection (Admin with no prop)
 *  3. Auth store company (CompanyAdmin / Formateur)
 */
const resolvedCompany = computed<{ id: number; name: string; logo?: string | null } | null>(() => {
  if (props.companyId) {
    return { id: props.companyId, name: props.companyName ?? '', logo: undefined }
  }
  if (authStore.isAdmin) {
    return pickerCompany.value
  }
  // CompanyAdmin / Formateur — always use their own company from auth
  return authStore.state.company
})

// ── Form state ────────────────────────────────────────────────────────────────

const defaultForm = () => ({ userName: '', email: '', role: '' })
const form        = reactive(defaultForm())

const isSubmitting = ref(false)
const formError    = ref<string | null>(null)
const successName  = ref<string | null>(null)

async function handleSubmit() {
  isSubmitting.value = true
  formError.value    = null
  successName.value  = null

  const { data, error } = await userStore.inviteUser({
    userName:  form.userName,
    email:     form.email,
    role:      form.role,
    companyId: resolvedCompany.value?.id,
  })

  isSubmitting.value = false

  if (error || !data) {
    formError.value = error ?? t('common.unknownError')
    return
  }

  successName.value = data.userName
  Object.assign(form, defaultForm())
  emit('invited')
}
</script>
