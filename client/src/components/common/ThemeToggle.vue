<template>
  <TooltipProvider>
    <Tooltip>
      <TooltipTrigger as-child>
        <Button variant="secondary" size="icon" @click="toggleTheme" class="rounded-full shadow-sm">
          <Sun v-if="theme === 'light'" class="h-[1.2rem] w-[1.2rem] transition-all" />
          <Moon v-else-if="theme === 'dark'" class="h-[1.2rem] w-[1.2rem] transition-all" />
          <Monitor v-else class="h-[1.2rem] w-[1.2rem] transition-all" />
          <span class="sr-only">{{ t('themeToggle.toggle') }}</span>
        </Button>
      </TooltipTrigger>
      <TooltipContent side="bottom" class="flex items-center gap-1.5">
        <Moon v-if="theme === 'light'" class="w-3.5 h-3.5" />
        <Monitor v-else-if="theme === 'dark'" class="w-3.5 h-3.5" />
        <Sun v-else class="w-3.5 h-3.5" />
        <span>{{ nextThemeLabel }}</span>
      </TooltipContent>
    </Tooltip>
  </TooltipProvider>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useTheme } from '@/composables/useTheme'
import { useLocale } from '@/composables/useLocale'
import { Button } from '@/components/ui/button'
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip'
import { Sun, Moon, Monitor } from 'lucide-vue-next'

const { theme, toggleTheme } = useTheme()
const { t } = useLocale()

const nextThemeLabel = computed(() => {
  if (theme.value === 'light') return t('themeToggle.toDark')
  if (theme.value === 'dark') return t('themeToggle.toSystem')
  return t('themeToggle.toLight')
})
</script>
