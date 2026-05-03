<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { FlaskConical, User, ChevronRight } from 'lucide-vue-next'
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetDescription,
} from '@/components/ui/sheet'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { ScrollArea } from '@/components/ui/scroll-area'
import { getDevUsersApi, type DevUser } from '@utils/api'
import { useLocale } from '@/composables/useLocale'

const { t } = useLocale()

const DEV_PASSWORD = 'Password'

const emit = defineEmits<{
  (e: 'fill', payload: { username: string; password: string }): void
}>()

const open = ref(false)
const users = ref<DevUser[]>([])
const loading = ref(false)

const roleOrder = ['Admin', 'CompanyAdmin', 'Formateur', 'Condidat']

const roleColors: Record<string, string> = {
  Admin:        'bg-red-500/15 text-red-600 dark:text-red-400',
  CompanyAdmin: 'bg-violet-500/15 text-violet-600 dark:text-violet-400',
  Formateur:    'bg-blue-500/15 text-blue-600 dark:text-blue-400',
  Condidat:     'bg-green-500/15 text-green-600 dark:text-green-400',
}

const groupedUsers = computed(() => {
  const groups: Record<string, DevUser[]> = {}
  for (const user of users.value) {
    const role = user.roles[0] ?? 'Unknown'
    if (!groups[role]) groups[role] = []
    groups[role].push(user)
  }
  // Sort by roleOrder
  return Object.fromEntries(
    Object.entries(groups).sort(
      ([a], [b]) => (roleOrder.indexOf(a) ?? 99) - (roleOrder.indexOf(b) ?? 99)
    )
  )
})

function initials(name?: string) {
  if (!name) return '?'
  return name
    .split(/[\s._-]/)
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase() ?? '')
    .join('')
}

function select(user: DevUser) {
  emit('fill', { username: user.userName!, password: DEV_PASSWORD })
  open.value = false
}

async function fetchUsers() {
  loading.value = true
  const { data } = await getDevUsersApi()
  users.value = data ?? []
  loading.value = false
}

onMounted(fetchUsers)
</script>

<template>
  <!-- FAB -->
  <button
    @click="open = true"
    :title="t('auth.login.devPanel.switchUser')"
    class="fixed bottom-6 right-6 z-50 flex h-12 w-12 items-center justify-center rounded-full bg-orange-500 text-white shadow-lg transition-transform hover:scale-110 hover:bg-orange-600 active:scale-95"
  >
    <FlaskConical class="h-5 w-5" />
  </button>

  <!-- Sheet -->
  <Sheet v-model:open="open">
    <SheetContent side="right" class="flex w-80 flex-col p-0">
      <SheetHeader class="border-b px-5 py-4">
        <div class="flex items-center gap-2">
          <div class="flex h-7 w-7 items-center justify-center rounded-md bg-orange-500 text-white">
            <FlaskConical class="h-4 w-4" />
          </div>
          <div>
            <SheetTitle class="text-sm">{{ t('auth.login.devPanel.title') }}</SheetTitle>
            <SheetDescription class="text-xs">{{ t('auth.login.devPanel.description') }}</SheetDescription>
          </div>
        </div>
      </SheetHeader>

      <ScrollArea class="flex-1 overflow-y-auto">
        <div v-if="loading" class="flex flex-col gap-2 p-4">
          <div v-for="i in 5" :key="i" class="h-12 animate-pulse rounded-md bg-muted" />
        </div>

        <div v-else class="p-3 space-y-4">
          <div v-for="(group, role) in groupedUsers" :key="role">
            <!-- Role header -->
            <div class="flex items-center gap-2 px-1 pb-1">
              <span
                class="inline-flex items-center rounded-full px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wider"
                :class="roleColors[role as string] ?? 'bg-muted text-muted-foreground'"
              >
                {{ t(`users.roles.${(role as string).charAt(0).toLowerCase() + (role as string).slice(1)}`) }}
              </span>
              <span class="text-[10px] text-muted-foreground">{{ group.length }}</span>
            </div>

            <!-- Users -->
            <div class="space-y-0.5">
              <button
                v-for="user in group"
                :key="user.id"
                @click="select(user)"
                class="group flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-left transition-colors hover:bg-muted"
              >
                <Avatar class="h-8 w-8 shrink-0">
                  <AvatarImage :src="user.imageUrl ?? ''" />
                  <AvatarFallback class="text-xs">{{ initials(user.userName) }}</AvatarFallback>
                </Avatar>
                <div class="min-w-0 flex-1">
                  <p class="truncate text-sm font-medium leading-tight">{{ user.userName }}</p>
                  <p class="truncate text-xs text-muted-foreground">{{ user.email }}</p>
                </div>
                <ChevronRight class="h-4 w-4 shrink-0 text-muted-foreground opacity-0 transition-opacity group-hover:opacity-100" />
              </button>
            </div>
          </div>

          <div v-if="!loading && Object.keys(groupedUsers).length === 0" class="py-8 text-center text-sm text-muted-foreground">
            <User class="mx-auto mb-2 h-8 w-8 opacity-30" />
            {{ t('auth.login.devPanel.noUsers') }}
          </div>
        </div>
      </ScrollArea>

      <div class="border-t px-5 py-3 text-center text-[11px] text-muted-foreground">
        {{ t('auth.login.devPanel.defaultPassword') }}: <code class="rounded bg-muted px-1 font-mono">Password</code>
      </div>
    </SheetContent>
  </Sheet>
</template>
