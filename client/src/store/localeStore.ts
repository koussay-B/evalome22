import { ref } from 'vue'
import { defineStore } from 'pinia'
import en from '@/i18n/locales/en'
import fr from '@/i18n/locales/fr'
import ar from '@/i18n/locales/ar'

export type SupportedLocale = 'en' | 'fr' | 'ar'

const STORAGE_KEY = 'app-lang'
const SUPPORTED: SupportedLocale[] = ['en', 'fr', 'ar']

const allMessages = { en, fr, ar }

function detectInitialLocale(): SupportedLocale {
  const saved = localStorage.getItem(STORAGE_KEY) as SupportedLocale | null
  if (saved && SUPPORTED.includes(saved)) return saved
  const browser = navigator.language.slice(0, 2) as SupportedLocale
  return SUPPORTED.includes(browser) ? browser : 'en'
}

export const useLocaleStore = defineStore('locale', () => {
  const locale = ref<SupportedLocale>(detectInitialLocale())
  const messages = ref<typeof en>(allMessages[locale.value])

  // Apply initial attributes
  document.documentElement.lang = locale.value
  document.documentElement.dir = locale.value === 'ar' ? 'rtl' : 'ltr'

  function setLocale(lang: SupportedLocale) {
    locale.value = lang
    messages.value = allMessages[lang]
    localStorage.setItem(STORAGE_KEY, lang)
    document.documentElement.lang = lang
    document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr'
  }

  function t(key: string, params?: Record<string, string | number>): string {
    const parts = key.split('.')
    let val: unknown = messages.value
    for (const p of parts) val = (val as Record<string, unknown>)?.[p]
    if (typeof val !== 'string') return key
    if (params) return val.replace(/\{(\w+)\}/g, (_, k) => String(params[k] ?? `{${k}}`))
    return val
  }

  return { locale, setLocale, t }
})
