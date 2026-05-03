<template>
  <EditUserSheet
    v-model:open="isEditOpen"
    :user="localUser"
    @saved="handleUserSaved"
  />

  <AlertDialog v-model:open="isResetOpen">
    <AlertDialogContent
      overlay-class="z-[70] bg-black/70"
      class="z-[71]"
    >
      <AlertDialogHeader>
        <AlertDialogTitle>{{ t('users.resetConfirmTitle') }}</AlertDialogTitle>
        <AlertDialogDescription>
          {{ t('users.resetConfirmDesc', { name: localUser?.userName ?? '' }) }}
        </AlertDialogDescription>
      </AlertDialogHeader>
      <AlertDialogFooter>
        <AlertDialogCancel>{{ t('common.cancel') }}</AlertDialogCancel>
        <AlertDialogAction
          @click="handleResetPassword"
          class="bg-foreground text-background hover:bg-foreground/85 border-none"
        >
          <Loader2 v-if="isResetting" class="w-4 h-4 animate-spin" />
          <span v-else>{{ t('users.resetPassword') }}</span>
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>

  <AlertDialog v-model:open="isDeleteOpen">
    <AlertDialogContent
      overlay-class="z-[70] bg-black/70"
      class="z-[71]"
    >
      <AlertDialogHeader>
        <AlertDialogTitle>{{ t('users.deleteConfirmTitle') }}</AlertDialogTitle>
        <AlertDialogDescription>
          {{ t('users.deleteConfirmDesc', { name: localUser?.userName ?? '' }) }}
        </AlertDialogDescription>
      </AlertDialogHeader>
      <AlertDialogFooter>
        <AlertDialogCancel>{{ t('common.cancel') }}</AlertDialogCancel>
        <AlertDialogAction
          @click="handleDeleteUser"
          class="bg-red-600 hover:bg-red-700 text-white border-none"
        >
          <Loader2 v-if="isDeleting" class="w-4 h-4 animate-spin" />
          <span v-else>{{ t('common.delete') }}</span>
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>

  <Sheet v-model:open="isOpen">
    <SheetContent :side="sheetSide" class="sm:max-w-sm overflow-y-auto p-0">
      <template v-if="localUser">
        <div class="px-6 pt-8 pb-6 border-b border-border">
          <div class="flex justify-center mb-4">
            <div
              class="w-20 h-20 rounded-2xl flex items-center justify-center text-2xl text-slate-800 shadow-sm overflow-hidden"
              :class="getRoleAvatarStyles(localUser.role)"
            >
              <img
                v-if="localUser.imageUrl"
                :src="localUser.imageUrl"
                :alt="localUser.userName"
                class="w-full h-full object-cover"
              />
              <span v-else>{{ initials }}</span>
            </div>
          </div>
          <div class="text-center space-y-2">
            <h2 class="text-xl text-slate-800 text-slate-800">{{ localUser.userName }}</h2>
            <div class="flex items-center justify-center gap-2 flex-wrap">
              <span
                :class="getRoleChipStyles(localUser.role)"
                class="text-[10px] font-bold uppercase tracking-wide px-2.5 py-1 rounded-md"
              >
                {{ roleLabel }}
              </span>
              <span
                class="flex items-center gap-1.5 text-xs font-semibold"
                :class="localUser.isActive ? 'text-emerald-600' : 'text-muted-foreground'"
              >
                <span
                  class="w-1.5 h-1.5 rounded-full"
                  :class="localUser.isActive ? 'bg-emerald-500' : 'bg-muted-foreground'"
                />
                {{ localUser.isActive ? t('common.active') : t('common.inactive') }}
              </span>
            </div>
          </div>
        </div>

        <div class="px-6 py-5 space-y-6">
          <div v-if="actionNotice" class="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">
            {{ actionNotice }}
          </div>

          <div v-if="actionError" class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
            {{ actionError }}
          </div>

          <div>
            <p class="text-[10px] font-bold uppercase tracking-widest text-muted-foreground mb-3">
              {{ t('users.contact') }}
            </p>
            <div class="space-y-2.5">
              <div class="flex items-center gap-3 text-sm">
                <Mail class="w-4 h-4 text-muted-foreground shrink-0" />
                <span class="text-slate-800 font-medium truncate">{{ localUser.email }}</span>
              </div>
              <div v-if="showCompany" class="flex items-center gap-3 text-sm">
                <Building2 class="w-4 h-4 text-muted-foreground shrink-0" />
                <span class="text-slate-800 font-medium">
                  {{ localUser.companyName || t('common.noCompany') }}
                </span>
              </div>
            </div>
          </div>

          <UserStatsGrid
            v-if="localUser.role !== 'Admin'"
            :role="localUser.role"
            :stats="liveStats"
            :loading="statsLoading"
          />

          <div v-if="allowActions" class="space-y-2 pt-2 border-t border-border">
            <button
              @click="isEditOpen = true"
              class="w-full flex items-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 transition-colors"
            >
              <Pencil class="w-4 h-4" /> {{ t('users.editProfile') }}
            </button>
            <button
              @click="isResetOpen = true"
              class="w-full flex items-center gap-2 px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-slate-800 hover:bg-muted/50 transition-colors"
            >
              <RefreshCw class="w-4 h-4" /> {{ t('users.resetPassword') }}
            </button>
            <button
              @click="isDeleteOpen = true"
              class="w-full flex items-center gap-2 px-4 py-2.5 rounded-lg border border-red-200 dark:border-red-900 text-red-600 text-sm font-semibold hover:bg-red-50 dark:hover:bg-red-950/30 transition-colors"
            >
              <Trash2 class="w-4 h-4" /> {{ t('users.deleteUser') }}
            </button>
          </div>
        </div>
      </template>
    </SheetContent>
  </Sheet>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Sheet, SheetContent } from '@/components/ui/sheet'
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
import { useLocale } from '@/composables/useLocale'
import { deleteManagedUserApi, getUserStatsApi, resetManagedUserPasswordApi } from '@/utils/api'
import { getRoleAvatarStyles, getRoleChipStyles } from '@/utils/roles'
import type { ManagedUserStats, UserListItem } from '@utils/models'
import { Building2, Loader2, Mail, Pencil, RefreshCw, Trash2 } from 'lucide-vue-next'
import EditUserSheet from './EditUserSheet.vue'
import UserStatsGrid from './UserStatsGrid.vue'

const props = withDefaults(defineProps<{
  user: UserListItem | null
  showCompany?: boolean
  allowActions?: boolean
  initialAction?: 'edit' | 'reset' | 'delete' | null
}>(), {
  showCompany: true,
  allowActions: true,
  initialAction: null,
})

const emit = defineEmits<{
  updated: [user: UserListItem]
  deleted: [id: number]
  passwordReset: [{ id: number; message: string }]
}>()

const isOpen = defineModel<boolean>('open', { default: false })

const { t, isRtl } = useLocale()

const sheetSide = computed<'right' | 'left'>(() => (isRtl.value ? 'left' : 'right'))
const localUser = ref<UserListItem | null>(props.user)
const liveStats = ref<ManagedUserStats | null>(null)
const statsLoading = ref(false)
const isEditOpen = ref(false)
const isResetOpen = ref(false)
const isDeleteOpen = ref(false)
const isResetting = ref(false)
const isDeleting = ref(false)
const actionNotice = ref<string | null>(null)
const actionError = ref<string | null>(null)

const initials = computed(() =>
  (localUser.value?.userName ?? '')
    .split(' ')
    .filter(Boolean)
    .map(part => part[0])
    .join('')
    .toUpperCase()
    .slice(0, 2)
)

const roleLabel = computed(() => {
  const role = localUser.value?.role ?? ''
  return role
    ? t(`users.roles.${role.charAt(0).toLowerCase() + role.slice(1)}`)
    : ''
})

watch(() => props.user, (user) => {
  localUser.value = user
  actionNotice.value = null
  actionError.value = null
}, { immediate: true })

watch([isOpen, localUser], async ([open, user]) => {
  if (!open || !user) return
  if (user.role === 'Admin') {
    liveStats.value = null
    statsLoading.value = false
    return
  }
  await loadStats(user.id)
}, { immediate: true })

watch([isOpen, localUser, () => props.initialAction], ([open, user, action]) => {
  if (!open || !user || !action) return
  if (action === 'edit') isEditOpen.value = true
  if (action === 'reset') isResetOpen.value = true
  if (action === 'delete') isDeleteOpen.value = true
})

async function loadStats(userId: number) {
  statsLoading.value = true
  const { data, error } = await getUserStatsApi(userId)
  liveStats.value = data ?? null
  statsLoading.value = false

  if (error) {
    actionError.value = error
  }
}

function handleUserSaved(user: UserListItem) {
  localUser.value = user
  actionError.value = null
  actionNotice.value = t('users.updateSuccess')
  emit('updated', user)
  void loadStats(user.id)
}

async function handleResetPassword() {
  if (!localUser.value) return

  isResetting.value = true
  actionError.value = null

  const { data, error } = await resetManagedUserPasswordApi(localUser.value.id)

  isResetting.value = false

  if (error || !data) {
    actionError.value = error ?? t('common.unknownError')
    return
  }

  isResetOpen.value = false
  actionNotice.value = t('users.resetSuccess')
  emit('passwordReset', { id: localUser.value.id, message: data.message })
}

async function handleDeleteUser() {
  if (!localUser.value) return

  isDeleting.value = true
  actionError.value = null

  const { error } = await deleteManagedUserApi(localUser.value.id)

  isDeleting.value = false

  if (error) {
    actionError.value = error
    return
  }

  const deletedId = localUser.value.id
  isDeleteOpen.value = false
  isOpen.value = false
  emit('deleted', deletedId)
}
</script>
