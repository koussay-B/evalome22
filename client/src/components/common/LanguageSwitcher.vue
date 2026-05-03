<template>
  <DropdownMenu>
    <DropdownMenuTrigger as-child>
      <button
        class="flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg border border-border text-xs font-bold text-slate-800 hover:bg-muted/60 transition-colors outline-none focus-visible:ring-2 focus-visible:ring-ring"
        :aria-label="t('lang.select')"
      >
        <!-- Flag + current language name always visible -->
        <span class="text-sm leading-none">{{ currentLang.flag }}</span>
        <span class="tracking-wide">{{ t(`lang.${locale}`) }}</span>
        <ChevronDown class="w-3 h-3 text-muted-foreground" />
      </button>
    </DropdownMenuTrigger>

    <DropdownMenuContent align="end" class="w-40">
      <DropdownMenuLabel class="text-xs text-muted-foreground font-semibold">
        {{ t('lang.select') }}
      </DropdownMenuLabel>
      <DropdownMenuSeparator />
      <DropdownMenuItem
        v-for="lang in languages"
        :key="lang.code"
        class="gap-2.5 cursor-pointer"
        @click="setLocale(lang.code)"
      >
        <span class="text-base leading-none">{{ lang.flag }}</span>
        <span class="text-sm flex-1">{{ t(`lang.${lang.code}`) }}</span>
        <span
          v-if="locale === lang.code"
          class="w-1.5 h-1.5 rounded-full bg-foreground shrink-0"
        />
      </DropdownMenuItem>
    </DropdownMenuContent>
  </DropdownMenu>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { ChevronDown } from 'lucide-vue-next'
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
} from '@/components/ui/dropdown-menu'
import { useLocale } from '@/composables/useLocale'
import type { SupportedLocale } from '@/store/localeStore'

const { t, locale, setLocale } = useLocale()

const languages: { code: SupportedLocale; flag: string }[] = [
  { code: 'en', flag: '🇬🇧' },
  { code: 'fr', flag: '🇫🇷' },
  { code: 'ar', flag: '🇸🇦' },
]

// Resolved entry for the currently-selected locale
const currentLang = computed(
  () => languages.find(l => l.code === locale.value) || languages[0]!
)
</script>
