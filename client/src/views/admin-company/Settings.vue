<template>
  <div class="space-y-8 max-w-5xl">

    <!-- ── Company Logo Dialog ──────────────────────────────────────────── -->
    <DialogRoot v-model:open="isLogoDialogOpen" @update:open="v => { if (!v) cancelDialog(logoDropzoneRef) }">
      <DialogPortal>
        <DialogOverlay class="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <DialogContent class="fixed left-1/2 top-1/2 z-50 w-full max-w-md -translate-x-1/2 -translate-y-1/2 rounded-xl border border-border bg-background shadow-xl focus:outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95 duration-200">
          <div class="flex items-start justify-between px-6 pt-6 pb-5 border-b border-border">
            <div>
              <DialogTitle class="text-base text-slate-800 text-slate-800">{{ t('companyPanel.settings.logoDialog.title') }}</DialogTitle>
              <DialogDescription class="text-xs text-muted-foreground mt-1">{{ t('companyPanel.settings.logoDialog.desc') }}</DialogDescription>
            </div>
            <DialogClose class="rounded-lg p-1.5 text-muted-foreground hover:bg-muted hover:text-slate-800 transition-colors focus:outline-none">
              <X class="w-4 h-4" />
            </DialogClose>
          </div>
          <div class="p-6 space-y-5">
            <FileDropzone
              ref="logoDropzoneRef"
              v-model="pendingLogoFiles"
              accept="image/png,image/jpeg,image/svg+xml,image/webp"
              :max-size-mb="2"
              height="md"
              :drop-label="t('companyPanel.settings.logoDialog.dropOrClick')"
              :drop-now-label="t('companyPanel.settings.logoDialog.dropNow')"
            />
            <div class="flex items-center gap-3">
              <div class="flex-1 h-px bg-border" />
              <span class="text-xs text-muted-foreground font-medium">{{ t('companyPanel.settings.logoDialog.or') }}</span>
              <div class="flex-1 h-px bg-border" />
            </div>
            <Button variant="outline" class="w-full gap-2" @click="logoDropzoneRef?.open()">
              <FolderOpen class="w-4 h-4" /> {{ t('companyPanel.settings.logoDialog.browse') }}
            </Button>
          </div>
          <div class="flex items-center justify-end gap-3 px-6 pb-6 pt-2 border-t border-border">
            <DialogClose as-child>
              <Button variant="outline" @click="cancelDialog(logoDropzoneRef)">{{ t('common.cancel') }}</Button>
            </DialogClose>
            <Button :disabled="pendingLogoFiles.length === 0" @click="applyLogo" class="gap-2">
              <CheckCircle class="w-4 h-4" /> {{ t('companyPanel.settings.logoDialog.apply') }}
            </Button>
          </div>
        </DialogContent>
      </DialogPortal>
    </DialogRoot>

    <!-- Header -->
    <div>
      <p class="text-[10px] font-bold uppercase tracking-[0.25em] text-primary/60 mb-1">{{ t('companyPanel.settings.subtitle') }}</p>
      <h1 class="text-3xl text-slate-800 tracking-tight text-slate-800">{{ t('companyPanel.settings.title') }}</h1>
    </div>

    <MyAccountSection @saved="toast" />


    <!-- ── Company Profile ───────────────────────────────────────────────── -->
    <div class="rounded-xl border border-border bg-gray-50">
      <div class="px-6 py-5 border-b border-border">
        <h2 class="text-sm text-slate-800 uppercase tracking-wider text-slate-800">{{ t('companyPanel.settings.companyProfile') }}</h2>
        <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.companyProfileDesc') }}</p>
      </div>
      <div class="p-6 space-y-6">

        <!-- Logo Row -->
        <div class="flex items-center gap-5">
          <div class="relative shrink-0 group">
            <div class="w-20 h-20 rounded-2xl border-2 border-border bg-muted flex items-center justify-center overflow-hidden shadow-sm">
              <img v-if="logoUrl" :src="logoUrl" alt="Company logo" class="w-full h-full object-contain" />
              <span v-else class="text-3xl text-slate-800 text-slate-800 select-none">{{ form.name.charAt(0) }}</span>
            </div>
            <button @click="isLogoDialogOpen = true"
              class="absolute inset-0 rounded-2xl flex items-center justify-center bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity">
              <Camera class="w-5 h-5 text-white" />
            </button>
          </div>
          <div class="space-y-2">
            <p class="text-sm font-semibold text-slate-800">{{ t('companyPanel.settings.logo') }}</p>
            <p class="text-xs text-muted-foreground">{{ t('companyPanel.settings.logoHint') }}</p>
            <div class="flex items-center gap-2 pt-1">
              <Button size="sm" variant="outline" class="gap-1.5 h-8 text-xs" @click="isLogoDialogOpen = true">
                <Pencil class="w-3.5 h-3.5" />
                {{ logoUrl ? t('companyPanel.settings.changeLogo') : t('companyPanel.settings.uploadLogo') }}
              </Button>
              <Button v-if="logoUrl" size="sm" variant="ghost" class="gap-1.5 h-8 text-xs text-red-600 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-950/30" @click="logoUrl = null">
                <Trash2 class="w-3.5 h-3.5" /> {{ t('companyPanel.settings.removeLogo') }}
              </Button>
            </div>
          </div>
        </div>

        <div class="h-px bg-border" />

        <!-- Fields Grid -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
          <div class="space-y-2">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('companyPanel.settings.companyName') }}</label>
            <div class="relative">
              <Building2 class="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <Input v-model="form.name" class="ps-9 h-10" />
            </div>
          </div>
          <div class="space-y-2">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('companyPanel.settings.phone') }}</label>
            <div class="relative">
              <Phone class="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <Input v-model="form.phone" class="ps-9 h-10" placeholder="+1 555 000 0000" />
            </div>
          </div>
          <div class="space-y-2">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('companyPanel.settings.website') }}</label>
            <div class="relative">
              <Globe class="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <Input v-model="form.website" class="ps-9 h-10" placeholder="techcorp.com" />
            </div>
          </div>
          <div class="space-y-2">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('companyPanel.settings.contactEmail') }}</label>
            <div class="relative">
              <Mail class="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <Input v-model="form.contactEmail" type="email" class="ps-9 h-10" />
            </div>
          </div>
          <div class="space-y-2 md:col-span-2">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('companyPanel.settings.address') }}</label>
            <div class="relative">
              <MapPin class="absolute start-3 top-3 w-4 h-4 text-muted-foreground" />
              <Input v-model="form.address" class="ps-9 h-10" placeholder="123 Main St, City, Country" />
            </div>
          </div>
          <div class="space-y-2 md:col-span-2">
            <label class="text-xs font-bold uppercase tracking-wider text-muted-foreground">{{ t('companyPanel.settings.description') }}</label>
            <textarea
              v-model="form.description"
              rows="3"
              class="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring resize-none"
              :placeholder="t('companyPanel.settings.descriptionPlaceholder')"
            />
          </div>
        </div>

        <Transition
          enter-active-class="transition-all duration-300 ease-out"
          enter-from-class="opacity-0 translate-y-3"
          leave-active-class="transition-all duration-200 ease-in"
          leave-to-class="opacity-0 translate-y-3"
        >
          <div v-if="isDirty || saveError" class="flex items-center justify-between pt-3 border-t border-border">
            <p v-if="saveError" class="text-xs text-destructive font-medium">{{ saveError }}</p>
            <p v-else class="text-xs text-muted-foreground">{{ t('companyPanel.settings.unsavedChanges') }}</p>
            <Button @click="saveProfile" class="gap-2">
              <Save class="w-4 h-4" /> {{ t('companyPanel.settings.saveChanges') }}
            </Button>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ── Notifications ─────────────────────────────────────────────────── -->
    <div class="rounded-xl border border-border bg-gray-50">
      <div class="px-6 py-5 border-b border-border">
        <h2 class="text-sm text-slate-800 uppercase tracking-wider text-slate-800">{{ t('companyPanel.settings.notifications') }}</h2>
        <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.notificationsDesc') }}</p>
      </div>
      <div class="divide-y divide-border">
        <div class="flex items-center justify-between px-6 py-4">
          <div>
            <p class="text-sm font-semibold text-slate-800">{{ t('companyPanel.settings.inviteEmails') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.inviteEmailsDesc') }}</p>
          </div>
          <ToggleSwitch v-model="notif.invite" />
        </div>
        <div class="flex items-center justify-between px-6 py-4">
          <div>
            <p class="text-sm font-semibold text-slate-800">{{ t('companyPanel.settings.resultEmails') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.resultEmailsDesc') }}</p>
          </div>
          <ToggleSwitch v-model="notif.results" />
        </div>
        <div class="flex items-center justify-between px-6 py-4">
          <div>
            <p class="text-sm font-semibold text-slate-800">{{ t('companyPanel.settings.reminderEmails') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.reminderEmailsDesc') }}</p>
          </div>
          <ToggleSwitch v-model="notif.reminders" />
        </div>
      </div>
    </div>

    <!-- ── Security / Test Settings ──────────────────────────────────────── -->
    <div class="rounded-xl border border-border bg-gray-50">
      <div class="px-6 py-5 border-b border-border">
        <h2 class="text-sm text-slate-800 uppercase tracking-wider text-slate-800">{{ t('companyPanel.settings.security') }}</h2>
        <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.securityDesc') }}</p>
      </div>
      <div class="divide-y divide-border">
        <div class="flex items-center justify-between px-6 py-4">
          <div>
            <p class="text-sm font-semibold text-slate-800">{{ t('companyPanel.settings.allowTabSwitch') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.allowTabSwitchDesc') }}</p>
          </div>
          <ToggleSwitch v-model="security.allowTabSwitch" />
        </div>
        <div class="flex items-center justify-between px-6 py-4">
          <div>
            <p class="text-sm font-semibold text-slate-800">{{ t('companyPanel.settings.maxAttempts') }}</p>
            <p class="text-xs text-muted-foreground mt-0.5">{{ t('companyPanel.settings.maxAttemptsDesc') }}</p>
          </div>
          <Select v-model="security.maxAttempts">
            <SelectTrigger class="h-8 w-20 text-xs"><SelectValue /></SelectTrigger>
            <SelectContent>
              <SelectItem value="1">1</SelectItem>
              <SelectItem value="2">2</SelectItem>
              <SelectItem value="3">3</SelectItem>
              <SelectItem value="5">5</SelectItem>
              <SelectItem value="10">10</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>
    </div>

    <!-- Saved Toast -->
    <Transition
      enter-from-class="opacity-0 translate-y-2"
      enter-active-class="transition-all duration-200"
      leave-to-class="opacity-0 translate-y-2"
      leave-active-class="transition-all duration-200"
    >
      <div v-if="showSaved" class="fixed bottom-6 end-6 z-50 flex items-center gap-2 px-4 py-3 rounded-xl bg-foreground text-background text-sm font-semibold shadow-lg">
        <CheckCircle class="w-4 h-4" /> {{ t('companyPanel.settings.savedSuccessfully') }}
      </div>
    </Transition>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { DialogRoot, DialogPortal, DialogOverlay, DialogContent, DialogTitle, DialogDescription, DialogClose } from 'reka-ui'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from '@/components/ui/select'
import FileDropzone from '@/components/common/FileDropzone.vue'
import { useLocale } from '@/composables/useLocale'
import MyAccountSection from '@/components/common/MyAccountSection.vue'
import ToggleSwitch from '@/components/campaign/builder/ToggleSwitch.vue'
import { getMyCompanyApi, updateCompanyApi, updateCompanyLogoApi } from '@/utils/api/company.api'
import {
  Building2, Globe, Mail, Save, CheckCircle, X,
  FolderOpen, Camera, Pencil, Trash2, Phone, MapPin,
} from 'lucide-vue-next'

const { t } = useLocale()

// ── Dropzone refs + shared helpers ─────────────────────────────────────────
interface DropzoneRef { clear: () => void; open: () => void }
const logoDropzoneRef  = ref<DropzoneRef | null>(null)

function cancelDialog(dz: DropzoneRef | null) { dz?.clear() }

// ── Company Logo ───────────────────────────────────────────────────────────
const isLogoDialogOpen = ref(false)
const logoUrl          = ref<string | null>(null)
const pendingLogoFiles = ref<File[]>([])
const companyId        = ref<number | null>(null)

async function applyLogo() {
  const file = pendingLogoFiles.value[0]
  if (file && companyId.value) {
    const { logoUrl: url } = await updateCompanyLogoApi(companyId.value, file)
    if (url) logoUrl.value = url
  }
  pendingLogoFiles.value = []
  logoDropzoneRef.value?.clear()
  isLogoDialogOpen.value = false
  toast()
}

// ── Company Form ───────────────────────────────────────────────────────────
const showSaved  = ref(false)
const saveError  = ref<string | null>(null)

const EMPTY = { name: '', description: '', website: '', contactEmail: '', phone: '', address: '' }
const form     = reactive({ ...EMPTY })
const original = reactive({ ...EMPTY })

const isDirty = computed(() =>
  form.name !== original.name || form.description !== original.description ||
  form.website !== original.website || form.contactEmail !== original.contactEmail ||
  form.phone !== original.phone || form.address !== original.address
)

const notif    = reactive({ invite: true, results: true, reminders: false })
const security = reactive({ allowTabSwitch: false, maxAttempts: '3' })

// ── Load real company data ──────────────────────────────────────────────────
onMounted(async () => {
  const { data } = await getMyCompanyApi()
  if (data) {
    companyId.value = data.id
    const patch = {
      name:         data.name         ?? '',
      description:  data.description  ?? '',
      website:      data.website      ?? '',
      contactEmail: data.email        ?? '',
      phone:        data.phone        ?? '',
      address:      data.address      ?? '',
    }
    Object.assign(form,     patch)
    Object.assign(original, patch)
    if (data.logo) logoUrl.value = data.logo
  }
})

// ── Actions ────────────────────────────────────────────────────────────────
function toast() {
  showSaved.value = true
  saveError.value = null
  setTimeout(() => { showSaved.value = false }, 2500)
}

async function saveProfile() {
  if (!companyId.value) return
  const { error } = await updateCompanyApi(companyId.value, {
    name:        form.name,
    description: form.description,
    website:     form.website,
    email:       form.contactEmail,
    phone:       form.phone,
    address:     form.address,
    logo:        logoUrl.value ?? undefined,
  })
  if (error) { saveError.value = error; return }
  Object.assign(original, { ...form })
  toast()
}
</script>
