<template>
  <div class="space-y-4">
    <!-- ── Photo Dialog ─────────────────────────────────────────────────────── -->
    <DialogRoot v-model:open="isPhotoOpen" @update:open="v => { if (!v) photoDropzoneRef?.clear() }">
      <DialogPortal>
        <DialogOverlay class="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <DialogContent class="fixed left-1/2 top-1/2 z-50 w-full max-w-md -translate-x-1/2 -translate-y-1/2 rounded-xl border border-border bg-background shadow-xl focus:outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95 duration-200">
          <div class="flex items-start justify-between px-6 pt-6 pb-5 border-b border-border">
            <div>
              <DialogTitle class="text-base  text-slate-800">{{ t('companyPanel.settings.photoDialog.title') }}</DialogTitle>
              <DialogDescription class="text-xs text-muted-foreground mt-1">{{ t('companyPanel.settings.photoDialog.desc') }}</DialogDescription>
            </div>
            <DialogClose class="rounded-lg p-1.5 text-muted-foreground hover:bg-muted hover:text-slate-800 transition-colors focus:outline-none">
              <X class="w-4 h-4" />
            </DialogClose>
          </div>
          <div class="p-6 space-y-5">
            <FileDropzone
              ref="photoDropzoneRef"
              v-model="pendingPhotoFiles"
              accept="image/png,image/jpeg,image/webp"
              :max-size-mb="3"
              height="md"
              :drop-label="t('companyPanel.settings.logoDialog.dropOrClick')"
              :drop-now-label="t('companyPanel.settings.logoDialog.dropNow')"
            />
            <div class="flex items-center gap-3">
              <div class="flex-1 h-px bg-border" />
              <span class="text-xs text-muted-foreground font-medium">{{ t('companyPanel.settings.logoDialog.or') }}</span>
              <div class="flex-1 h-px bg-border" />
            </div>
            <Button variant="outline" class="w-full gap-2" @click="photoDropzoneRef?.open()">
              <FolderOpen class="w-4 h-4" /> {{ t('companyPanel.settings.logoDialog.browse') }}
            </Button>
          </div>
          <div class="flex items-center justify-end gap-3 px-6 pb-6 pt-2 border-t border-border">
            <DialogClose as-child>
              <Button variant="outline" @click="photoDropzoneRef?.clear()">{{ t('common.cancel') }}</Button>
            </DialogClose>
            <Button :disabled="pendingPhotoFiles.length === 0 || avatarLoading" @click="applyPhoto" class="gap-2">
              <Loader2 v-if="avatarLoading" class="w-4 h-4 animate-spin" />
              <CheckCircle v-else class="w-4 h-4" />
              {{ t('companyPanel.settings.logoDialog.apply') }}
            </Button>
          </div>
        </DialogContent>
      </DialogPortal>
    </DialogRoot>

    <!-- ── Profile Section ─────────────────────────────────────────────────── -->
    <div class="rounded-xl border border-border bg-gray-50">
      <div class="px-6 py-5 border-b border-border">
        <h2 class="text-sm font-semibold uppercase tracking-wider text-slate-800">{{ t('companyPanel.settings.myAccount') }}</h2>
        <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.myAccountDesc') }}</p>
      </div>
      <div class="p-6 space-y-6">

        <!-- Avatar row -->
        <div class="flex items-center gap-4">
          <div class="relative shrink-0 group">
            <div class="w-16 h-16 rounded-xl border-2 border-border bg-muted flex items-center justify-center overflow-hidden shadow-sm select-none">
              <img v-if="currentAvatarUrl" :src="currentAvatarUrl" alt="Profile photo" class="w-full h-full object-cover" />
              <span v-else class="text-xl  text-slate-800">{{ userInitials }}</span>
            </div>
            <button
              @click="isPhotoOpen = true"
              class="absolute inset-0 rounded-xl flex items-center justify-center bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity"
            >
              <Camera class="w-4 h-4 text-white" />
            </button>
          </div>
          <div>
            <p class="text-lg font-semibold text-slate-800">{{ authStore.state.user?.userName || '—' }}</p>
            <p class="text-xs text-slate-800 text-muted-foreground mt-0.5">{{ authStore.state.user?.email }}</p>
            <button
              @click="isPhotoOpen = true"
              class="text-xs text-muted-foreground hover:text-slate-800 transition-colors mt-1.5 underline-offset-2 hover:underline"
            >
              {{ currentAvatarUrl ? t('companyPanel.settings.changePhoto') : t('companyPanel.settings.uploadPhoto') }}
            </button>
          </div>
        </div>

        <!-- Avatar error -->
        <div v-if="avatarError" class="flex items-center gap-2 px-3 py-2.5 rounded-lg bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-800 text-red-600 dark:text-red-400 text-xs font-medium">
          <AlertCircle class="w-4 h-4 shrink-0" /> {{ avatarError }}
        </div>

        <div class="h-px bg-border" />

        <!-- Info fields -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
          <div class="space-y-2">
            <label class="text-xs  uppercase tracking-wider text-muted-foreground">{{ t('companyPanel.settings.fullName') }}</label>
            <div class="relative">
              <UserIcon class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <Input v-model="user.userName" class="ps-9 h-10" />
            </div>
          </div>
          <div class="space-y-2">
            <label class="text-xs  uppercase tracking-wider text-muted-foreground">{{ t('companyPanel.settings.email') }}</label>
            <div class="relative">
              <Mail class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <Input v-model="user.email" type="email" class="ps-9 h-10" />
            </div>
          </div>
        </div>

        <!-- Profile error -->
        <div v-if="profileError" class="flex items-center gap-2 px-3 py-2.5 rounded-lg bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-800 text-red-600 dark:text-red-400 text-xs font-medium">
          <AlertCircle class="w-4 h-4 shrink-0" /> {{ profileError }}
        </div>

        <Transition
          enter-active-class="transition-all duration-300 ease-out"
          enter-from-class="opacity-0 translate-y-3"
          leave-active-class="transition-all duration-200 ease-in"
          leave-to-class="opacity-0 translate-y-3"
        >
          <div v-if="isProfileDirty" class="flex items-center justify-between pt-3 border-t border-border">
            <p class="text-xs text-muted-foreground">{{ t('companyPanel.settings.unsavedChanges') }}</p>
            <Button :disabled="profileLoading" @click="saveProfile" class="gap-2">
              <Loader2 v-if="profileLoading" class="w-4 h-4 animate-spin" />
              <Save v-else class="w-4 h-4" />
              {{ t('companyPanel.settings.saveChanges') }}
            </Button>
          </div>
        </Transition>

        <div class="h-px bg-border" />

        <!-- Change password (collapsible) -->
        <div>
          <button
            type="button"
            @click="showPasswordForm = !showPasswordForm"
            class="flex items-center gap-2 text-sm font-semibold text-slate-800 hover:text-muted-foreground transition-colors"
          >
            <Lock class="w-4 h-4" />
            {{ t('companyPanel.settings.changePassword') }}
            <ChevronDown class="w-4 h-4 text-muted-foreground transition-transform duration-200" :class="showPasswordForm ? 'rotate-180' : ''" />
          </button>

          <Transition
            enter-active-class="transition-all duration-300 ease-out"
            enter-from-class="opacity-0 -translate-y-2"
            leave-active-class="transition-all duration-200 ease-in"
            leave-to-class="opacity-0 -translate-y-2"
          >
            <div v-if="showPasswordForm" class="mt-5 space-y-5">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div class="space-y-2 md:col-span-2">
                  <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('companyPanel.settings.currentPassword') }}</label>
                  <div class="relative">
                    <Lock class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                    <Input v-model="pwd.current" :type="showPwd.current ? 'text' : 'password'" class="ps-9 pe-10 h-10" autocomplete="current-password" />
                    <button type="button" @click="showPwd.current = !showPwd.current" class="absolute inset-e-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-slate-800 transition-colors">
                      <EyeOff v-if="showPwd.current" class="w-4 h-4" /><Eye v-else class="w-4 h-4" />
                    </button>
                  </div>
                </div>
                <div class="space-y-2">
                  <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('companyPanel.settings.newPassword') }}</label>
                  <div class="relative">
                    <Lock class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                    <Input v-model="pwd.next" :type="showPwd.next ? 'text' : 'password'" class="ps-9 pe-10 h-10" autocomplete="new-password" />
                    <button type="button" @click="showPwd.next = !showPwd.next" class="absolute inset-e-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-slate-800 transition-colors">
                      <EyeOff v-if="showPwd.next" class="w-4 h-4" /><Eye v-else class="w-4 h-4" />
                    </button>
                  </div>
                </div>
                <div class="space-y-2">
                  <label class="text-xs text-slate-800 uppercase tracking-wider ">{{ t('companyPanel.settings.confirmPassword') }}</label>
                  <div class="relative">
                    <Lock class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                    <Input v-model="pwd.confirm" :type="showPwd.confirm ? 'text' : 'password'" class="ps-9 pe-10 h-10" autocomplete="new-password"
                      :class="pwd.confirm && pwd.next !== pwd.confirm ? 'border-red-500 focus-visible:ring-red-500' : ''" />
                    <button type="button" @click="showPwd.confirm = !showPwd.confirm" class="absolute inset-e-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-slate-800 transition-colors">
                      <EyeOff v-if="showPwd.confirm" class="w-4 h-4" /><Eye v-else class="w-4 h-4" />
                    </button>
                  </div>
                  <p v-if="pwd.confirm && pwd.next !== pwd.confirm" class="text-xs text-red-500 font-medium">
                    {{ t('companyPanel.settings.passwordMismatch') }}
                  </p>
                </div>
              </div>

              <!-- Password error -->
              <div v-if="passwordError" class="flex items-center gap-2 px-3 py-2.5 rounded-lg bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-800 text-red-600 dark:text-red-400 text-xs font-medium">
                <AlertCircle class="w-4 h-4 shrink-0" /> {{ passwordError }}
              </div>

              <div class="flex justify-end">
                <Button
                  :disabled="!pwd.current || !pwd.next || pwd.next !== pwd.confirm || passwordLoading"
                  @click="savePassword"
                  class="gap-2"
                >
                  <Loader2 v-if="passwordLoading" class="w-4 h-4 animate-spin" />
                  <Lock v-else class="w-4 h-4" />
                  {{ t('companyPanel.settings.updatePassword') }}
                </Button>
              </div>
            </div>
          </Transition>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue'
import { DialogRoot, DialogPortal, DialogOverlay, DialogContent, DialogTitle, DialogDescription, DialogClose } from 'reka-ui'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import FileDropzone from '@/components/common/FileDropzone.vue'
import { useLocale } from '@/composables/useLocale'
import { useAuthStore } from '@/store/authStore'
import { uploadImageApi } from '@/utils/api/company.api'
import { updateMyProfileApi, updateMyAvatarApi, changeMyPasswordApi } from '@/utils/api/user.api'
import {
  Mail, Save, CheckCircle, X, AlertCircle, Loader2,
  FolderOpen, Camera, User as UserIcon, Lock, Eye, EyeOff, ChevronDown,
} from 'lucide-vue-next'

const { t }     = useLocale()
const authStore = useAuthStore()

const emit = defineEmits<{ (e: 'saved'): void }>()

// ── Photo dialog ───────────────────────────────────────────────────────────
interface DropzoneRef { clear: () => void; open: () => void }
const photoDropzoneRef  = ref<DropzoneRef | null>(null)
const isPhotoOpen       = ref(false)
const pendingPhotoFiles = ref<File[]>([])

// ── Avatar ─────────────────────────────────────────────────────────────────
const currentAvatarUrl = ref<string | null>(authStore.state.user?.imageUrl ?? null)
const avatarLoading    = ref(false)
const avatarError      = ref<string | null>(null)

watch(() => authStore.state.user?.imageUrl, v => { currentAvatarUrl.value = v ?? null })

async function applyPhoto() {
  const file = pendingPhotoFiles.value[0]
  if (!file) return
  avatarLoading.value = true
  avatarError.value   = null

  // 1) Upload to Cloudinary
  const { url, error: uploadErr } = await uploadImageApi(file)
  if (uploadErr || !url) {
    avatarError.value   = uploadErr ?? 'Upload failed'
    avatarLoading.value = false
    return
  }

  // 2) Persist URL to user record
  const { error: saveErr } = await updateMyAvatarApi(url)
  if (saveErr) {
    avatarError.value   = saveErr
    avatarLoading.value = false
    return
  }

  authStore.patchUser({ imageUrl: url })
  currentAvatarUrl.value  = url
  pendingPhotoFiles.value = []
  photoDropzoneRef.value?.clear()
  isPhotoOpen.value   = false
  avatarLoading.value = false
  emit('saved')
}

// ── Profile form ───────────────────────────────────────────────────────────
const user = reactive({
  userName: authStore.state.user?.userName ?? '',
  email:    authStore.state.user?.email    ?? '',
})
const userOriginal = reactive({ ...user })

watch(() => authStore.state.user, u => {
  if (!u) return
  user.userName = u.userName;         userOriginal.userName = u.userName
  user.email    = u.email;            userOriginal.email    = u.email
}, { immediate: false })

const userInitials   = computed(() => (user.userName || '?').slice(0, 2).toUpperCase())
const isProfileDirty = computed(() => user.userName !== userOriginal.userName || user.email !== userOriginal.email)
const profileLoading = ref(false)
const profileError   = ref<string | null>(null)

async function saveProfile() {
  profileLoading.value = true
  profileError.value   = null

  const { data, error } = await updateMyProfileApi({ userName: user.userName, email: user.email })
  if (error || !data) {
    profileError.value   = error ?? 'Failed to save'
    profileLoading.value = false
    return
  }

  authStore.patchUser({ userName: data.userName, email: data.email ?? user.email })
  userOriginal.userName = user.userName
  userOriginal.email    = user.email
  profileLoading.value  = false
  emit('saved')
}

// ── Password ───────────────────────────────────────────────────────────────
const showPasswordForm = ref(false)
const pwd     = reactive({ current: '', next: '', confirm: '' })
const showPwd = reactive({ current: false, next: false, confirm: false })
const passwordLoading = ref(false)
const passwordError   = ref<string | null>(null)

async function savePassword() {
  passwordLoading.value = true
  passwordError.value   = null

  const { error } = await changeMyPasswordApi({ currentPassword: pwd.current, newPassword: pwd.next })
  if (error) {
    passwordError.value   = error
    passwordLoading.value = false
    return
  }

  Object.assign(pwd, { current: '', next: '', confirm: '' })
  showPasswordForm.value = false
  passwordLoading.value  = false
  emit('saved')
}
</script>
