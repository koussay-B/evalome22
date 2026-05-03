import { computed } from 'vue'
import { useLocaleStore } from '@/store/localeStore'

export function useLocale() {
  const store = useLocaleStore()
  return {
    t: store.t,
    locale: computed(() => store.locale),
    isRtl: computed(() => store.locale === 'ar'),
    setLocale: store.setLocale,
  }
}
