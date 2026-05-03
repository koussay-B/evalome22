<template>
  <Sheet v-model:open="isOpen">
    <SheetContent
      :side="sheetSide"
      overlay-class="z-[70] bg-black/70"
      class="z-[71] sm:max-w-md overflow-y-auto flex flex-col p-0"
    >
      <SheetHeader class="px-6 pt-6 pb-5 border-b border-border shrink-0">
        <SheetTitle class="text-xl text-slate-800">{{ t('users.editProfile') }}</SheetTitle>
        <SheetDescription class="mt-1">{{ t('users.editProfileDesc') }}</SheetDescription>
      </SheetHeader>

      <form @submit.prevent="handleSubmit" class="flex flex-col gap-5 p-6 flex-1">
        <div class="space-y-2">
          <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
            {{ t('users.fullName') }} *
          </label>
          <div class="relative">
            <UserIcon class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <Input
              v-model="form.userName"
              :placeholder="t('employees.usernamePlaceholder')"
              class="ps-9 h-10"
              required
              autocomplete="off"
            />
          </div>
        </div>

        <div class="space-y-2">
          <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
            {{ t('users.email') }} *
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

        <div class="space-y-2">
          <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">
            {{ t('users.role') }} *
          </label>
          <Select v-model="form.role" required>
            <SelectTrigger class="h-10">
              <SelectValue :placeholder="t('employees.selectRole')" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem v-for="role in availableRoles" :key="role.value" :value="role.value">
                <div class="flex items-center gap-2">
                  <span class="w-2 h-2 rounded-full" :class="getRoleDotClass(role.value)" />
                  {{ role.label }}
                </div>
              </SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div class="space-y-3 rounded-xl border border-border bg-muted/20 px-4 py-3">
          <div class="flex items-center justify-between gap-3">
            <div>
              <p class="text-sm font-semibold text-slate-800">{{ t('users.statusLabel') }}</p>
              <p class="text-xs text-muted-foreground">{{ t('users.statusHelp') }}</p>
            </div>
            <Switch v-model="form.isActive" />
          </div>
          <p class="text-xs font-medium" :class="form.isActive ? 'text-emerald-600' : 'text-muted-foreground'">
            {{ form.isActive ? t('common.active') : t('common.inactive') }}
          </p>
        </div>

        <p v-if="formError" class="text-sm text-red-600 dark:text-red-400 -mt-1">
          {{ formError }}
        </p>

        <div class="flex gap-3 pt-4 border-t border-border mt-auto">
          <button
            type="submit"
            :disabled="isSubmitting || !user"
            class="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-foreground text-background text-sm font-semibold hover:bg-foreground/85 transition-colors disabled:opacity-60"
          >
            <Loader2 v-if="isSubmitting" class="w-4 h-4 animate-spin" />
            <Save v-else class="w-4 h-4" />
            {{ isSubmitting ? t('common.saving') : t('common.save') }}
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
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { Sheet, SheetContent, SheetDescription, SheetHeader, SheetTitle } from '@/components/ui/sheet'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { Switch } from '@/components/ui/switch'
import { useAuthStore } from '@/store/authStore'
import { useLocale } from '@/composables/useLocale'
import { getRoleDotClass, UserRole } from '@/utils/roles'
import { updateManagedUserApi } from '@/utils/api'
import type { UserListItem } from '@utils/models'
import { Loader2, Mail, Save, User as UserIcon } from 'lucide-vue-next'

const props = defineProps<{
  user: UserListItem | null
}>()

const emit = defineEmits<{
  saved: [user: UserListItem]
}>()

const isOpen = defineModel<boolean>('open', { default: false })

const { t, isRtl } = useLocale()
const authStore = useAuthStore()

const sheetSide = computed<'right' | 'left'>(() => (isRtl.value ? 'left' : 'right'))

const form = reactive({
  userName: '',
  email: '',
  role: '',
  isActive: true,
})

const isSubmitting = ref(false)
const formError = ref<string | null>(null)

const user = computed(() => props.user)

const availableRoles = computed(() => {
  if (authStore.isAdmin) {
    return [
      { value: UserRole.Admin, label: t('users.roles.admin') },
      { value: UserRole.CompanyAdmin, label: t('users.roles.companyAdmin') },
      { value: UserRole.Formateur, label: t('users.roles.formateur') },
      { value: UserRole.Condidat, label: t('users.roles.condidat') },
    ]
  }

  return [
    { value: UserRole.Formateur, label: t('users.roles.formateur') },
    { value: UserRole.Condidat, label: t('users.roles.condidat') },
  ]
})

watch([isOpen, user], ([open, currentUser]) => {
  if (!open || !currentUser) return

  form.userName = currentUser.userName
  form.email = currentUser.email
  form.role = currentUser.role
  form.isActive = currentUser.isActive
  formError.value = null
}, { immediate: true })

async function handleSubmit() {
  if (!user.value) return

  isSubmitting.value = true
  formError.value = null

  const { data, error } = await updateManagedUserApi(user.value.id, {
    userName: form.userName,
    email: form.email,
    role: form.role,
    isActive: form.isActive,
  })

  isSubmitting.value = false

  if (error || !data) {
    formError.value = error ?? t('common.unknownError')
    return
  }

  emit('saved', data)
  isOpen.value = false
}
</script>
