<script setup lang="ts">
import { reactive, ref, nextTick } from 'vue'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from 'reka-ui'
import { Eye, EyeOff } from 'lucide-vue-next'
import { useLocale } from '@/composables/useLocale'

const { t } = useLocale()

const props = defineProps<{
  className?: string
  loading?: boolean
  serverError?: string | null
}>()

const emit = defineEmits<{
  (e: 'submit', form: { username: string; password: string }): void
}>()

const form = reactive({ username: '' })
const errors = reactive({ username: '', password: '' })
const showPassword = ref(false)
const passwordEl = ref<HTMLInputElement | null>(null)

// Quand l'utilisateur focus le champ password :
// supprimer readonly via DOM direct (pas via Vue binding) + vider le champ
function onPasswordFocus() {
  const el = passwordEl.value
  if (!el) return
  if (el.hasAttribute('readonly')) {
    el.removeAttribute('readonly')
    el.value = ''
  }
}

function toggleShow() {
  showPassword.value = !showPassword.value
  nextTick(() => {
    if (passwordEl.value) {
      passwordEl.value.type = showPassword.value ? 'text' : 'password'
    }
  })
}

function validate(): boolean {
  errors.username = ''
  errors.password = ''
  let valid = true

  if (!form.username.trim()) {
    errors.username = t('auth.login.errors.emailRequired')
    valid = false
  }

  // Lire directement la valeur DOM — bypasse tout autofill Chrome
  const pwd = passwordEl.value?.value ?? ''

  if (!pwd) {
    errors.password = t('auth.login.errors.passwordRequired')
    valid = false
  } else if (pwd.length < 6) {
    errors.password = t('auth.login.errors.passwordMinLength')
    valid = false
  }

  return valid
}

function handleSubmit() {
  if (!validate()) return
  const pwd = passwordEl.value?.value ?? ''
  emit('submit', { username: form.username, password: pwd })
}

defineExpose({
  fill(username: string, password: string) {
    form.username = username
    errors.username = ''
    errors.password = ''
    // Remplir le champ password via DOM direct
    nextTick(() => {
      const el = passwordEl.value
      if (!el) return
      el.removeAttribute('readonly')
      el.value = password
      el.type = 'password'
      showPassword.value = false
    })
  },
})
</script>

<template>
  <div :class="cn('flex flex-col gap-6', props.className)">
    <Card>
      <CardHeader>
        <CardTitle>{{ t('auth.login.title') }}</CardTitle>
        <CardDescription>{{ t('auth.login.description') }}</CardDescription>
      </CardHeader>
      <CardContent>
        <form @submit.prevent="handleSubmit" autocomplete="off">
          <div class="flex flex-col gap-5">

            <!-- Email / Username -->
            <div class="grid gap-1.5">
              <Label for="username">{{ t('auth.login.emailLabel') }}</Label>
              <Input
                id="username"
                v-model="form.username"
                type="text"
                autocomplete="off"
                :placeholder="t('auth.login.usernamePlaceholder')"
                :class="errors.username ? 'border-destructive focus-visible:ring-destructive' : ''"
                @input="errors.username = ''"
              />
              <p v-if="errors.username" class="text-xs text-destructive">{{ errors.username }}</p>
            </div>

            <!-- Password — input natif direct, PAS de v-model, PAS de composant Input -->
            <div class="grid gap-1.5">
              <div class="flex items-center">
                <Label for="password">{{ t('auth.login.passwordLabel') }}</Label>
              </div>
              <div class="relative">
                <input
                  id="password"
                  ref="passwordEl"
                  type="password"
                  readonly
                  autocomplete="new-password"
                  :class="cn(
                    'border-input h-9 w-full min-w-0 rounded-md border bg-transparent px-3 py-1 pr-10 text-base shadow-xs transition-[color,box-shadow] outline-none',
                    'focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:ring-[3px]',
                    'placeholder:text-muted-foreground dark:bg-input/30',
                    errors.password ? 'border-destructive focus-visible:ring-destructive' : ''
                  )"
                  :placeholder="t('auth.login.passwordLabel')"
                  @focus="onPasswordFocus"
                  @input="errors.password = ''"
                />
                <button
                  type="button"
                  tabindex="-1"
                  class="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
                  @click="toggleShow"
                >
                  <EyeOff v-if="showPassword" class="h-4 w-4" />
                  <Eye v-else class="h-4 w-4" />
                </button>
              </div>
              <p v-if="errors.password" class="text-xs text-destructive">{{ errors.password }}</p>
            </div>

            <!-- Backend / server error -->
            <p
              v-if="props.serverError"
              class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive"
            >
              {{ props.serverError }}
            </p>

            <!-- Actions -->
            <div class="flex flex-col gap-3">
              <Button type="submit" :disabled="props.loading">
                {{ props.loading ? t('auth.login.loggingIn') : t('auth.login.loginButton') }}
              </Button>
            </div>
          </div>
        </form>
      </CardContent>
    </Card>
  </div>
</template>
