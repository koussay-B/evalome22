import { ref, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

export const VALID_SIZES = [5, 10, 20, 50] as const

export function useTableQuery(defaults?: {
  page?: number
  size?: number
  search?: string
  filter?: string
}) {
  const route = useRoute()
  const router = useRouter()

  const page = ref(defaults?.page ?? 1)
  const size = ref<number>(defaults?.size ?? 10)
  const search = ref(defaults?.search ?? '')
  const filter = ref(defaults?.filter ?? 'All')

  // ── Read from URL on mount ─────────────────────────────────────
  onMounted(() => {
    const qPage = parseInt(route.query.page as string)
    const qSize = parseInt(route.query.size as string)
    const qSearch = route.query.search as string | undefined
    const qFilter = route.query.filter as string | undefined

    if (!isNaN(qPage) && qPage > 0) page.value = qPage
    if ((VALID_SIZES as readonly number[]).includes(qSize)) size.value = qSize
    if (qSearch !== undefined) search.value = qSearch
    if (qFilter !== undefined) filter.value = qFilter
  })

  // ── Sync back to URL on every change ──────────────────────────
  watch(
    [page, size, search, filter],
    ([p, s, q, f]) => {
      router.replace({
        query: {
          page: String(p),
          size: String(s),
          ...(q ? { search: q } : {}),
          ...(f && f !== 'All' ? { filter: f } : {}),
        },
      })
    },
    { flush: 'post' },
  )

  function resetPage() {
    page.value = 1
  }

  return { page, size, search, filter, resetPage }
}
