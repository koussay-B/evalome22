import { shallowRef, computed, ref } from 'vue'
import type { PaginatedState, PaginationMeta } from './asyncState'

// ─── Types ────────────────────────────────────────────────────────────────────

/** Any params object that carries an optional pageNumber */
export type PagedParams<P> = P & { pageNumber?: number }

/** What every API fetcher must return */
export interface FetchResult<T> {
    data?: T[]
    meta?: PaginationMeta
    error?: string
}

/** The shape returned by usePaginatedResource */
export interface PaginatedResource<T, P> {
    // State
    state: ReturnType<typeof ref<PaginatedState<T, P>>>
    isLoading: ReturnType<typeof computed<boolean>>
    isError: ReturnType<typeof computed<boolean>>
    hasData: ReturnType<typeof computed<boolean>>
    currentItems: ReturnType<typeof computed<T[]>>
    meta: ReturnType<typeof computed<PaginationMeta | null>>
    canGoNext: ReturnType<typeof computed<boolean>>
    canGoPrev: ReturnType<typeof computed<boolean>>
    // Actions
    fetchPage: (params: PagedParams<P>) => Promise<void>
    nextPage: () => Promise<void>
    prevPage: () => Promise<void>
    goToPage: (page: number) => Promise<void>
    search: (filters: P) => Promise<void>
    reset: () => void
}

// ─── Factory ──────────────────────────────────────────────────────────────────

/**
 * Creates a self-contained paginated resource with an in-memory page cache.
 *
 * @param fetcher  - async function that calls the API; receives full params
 * @param filterKey - function that serialises everything except pageNumber into
 *                    a cache-busting key (defaults to JSON.stringify of non-page fields)
 *
 * @example
 * const companies = usePaginatedResource<Company, CompanySearchParams>(
 *   (params) => getCompaniesApi(params),
 * )
 */
export function usePaginatedResource<T, P extends object>(
    fetcher: (params: PagedParams<P>) => Promise<FetchResult<T>>,
    toFilterKey: (params: P) => string = defaultFilterKey,
) {
    // ── Internal state ──────────────────────────────────────────────────────
    const state = shallowRef<PaginatedState<T, P>>({ status: 'idle' })

    let _cache: Record<number, T[]> = {}
    let _filterKey: string = ''

    // ── Helpers ─────────────────────────────────────────────────────────────
    function _getDataState() {
        return state.value.status === 'data'
            ? (state.value as Extract<PaginatedState<T, P>, { status: 'data' }>)
            : null
    }

    // ── Computed ────────────────────────────────────────────────────────────
    const isLoading = computed(() => state.value.status === 'loading')
    const isError = computed(() => state.value.status === 'error')
    const hasData = computed(() => state.value.status === 'data')

    const currentItems = computed<T[]>(() => {
        const s = _getDataState()
        if (!s) return []
        return s.data.pages[s.data.meta.currentPage] ?? []
    })

    const meta = computed<PaginationMeta | null>(() => {
        const s = _getDataState()
        return s ? s.data.meta : null
    })

    const canGoNext = computed(() => {
        const s = _getDataState()
        return s ? s.data.meta.currentPage < s.data.meta.totalPages : false
    })

    const canGoPrev = computed(() => {
        const s = _getDataState()
        return s ? s.data.meta.currentPage > 1 : false
    })

    // ── Actions ─────────────────────────────────────────────────────────────

    async function fetchPage(params: PagedParams<P>) {
        const page = params.pageNumber ?? 1
        const key = toFilterKey(params as P)

        // Filter changed → bust the cache
        if (key !== _filterKey) {
            _cache = {}
            _filterKey = key
        }

        // Cache hit → update page pointer, skip API
        if (_cache[page]) {
            const s = _getDataState()
            state.value = {
                status: 'data',
                data: {
                    pages: { ..._cache },
                    meta: s ? { ...s.data.meta, currentPage: page } : buildFallbackMeta(page, _cache),
                },
                params,
            } as PaginatedState<T, P>
            return
        }

        // Cache miss → hit the API
        state.value = { status: 'loading' }

        const { data, meta: fetchedMeta, error } = await fetcher(params)

        if (error || !data || !fetchedMeta) {
            state.value = { status: 'error', message: error ?? 'Failed to load data' }
            return
        }

        _cache[page] = data

        state.value = {
            status: 'data',
            data: { pages: { ..._cache }, meta: fetchedMeta },
            params,
        } as PaginatedState<T, P>
    }

    async function nextPage() {
        const s = _getDataState()
        if (!s) return
        const currentData = s.data
        if (currentData.meta.currentPage >= currentData.meta.totalPages) return
        await fetchPage({ ...(s as any).params, pageNumber: currentData.meta.currentPage + 1 })
    }

    async function prevPage() {
        const s = _getDataState()
        if (!s) return
        const currentData = s.data
        if (currentData.meta.currentPage <= 1) return
        await fetchPage({ ...(s as any).params, pageNumber: currentData.meta.currentPage - 1 })
    }

    async function goToPage(page: number) {
        const s = _getDataState()
        if (!s) return
        await fetchPage({ ...(s as any).params, pageNumber: page })
    }

    /** Apply new filter params — always resets to page 1 */
    async function search(filters: P) {
        await fetchPage({ ...filters, pageNumber: 1 })
    }

    function reset() {
        _cache = {}
        _filterKey = ''
        state.value = { status: 'idle' }
    }

    return {
        state,
        isLoading,
        isError,
        hasData,
        currentItems,
        meta,
        canGoNext,
        canGoPrev,
        fetchPage,
        nextPage,
        prevPage,
        goToPage,
        search,
        reset,
    }
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

function defaultFilterKey<P extends object>(params: P): string {
    const { pageNumber: _, ...rest } = params as P & { pageNumber?: number }
    return JSON.stringify(rest)
}

function buildFallbackMeta(currentPage: number, cache: Record<number, unknown[]>): PaginationMeta {
    const knownPages = Object.keys(cache).map(Number)
    return {
        currentPage,
        totalPages: Math.max(...knownPages, currentPage),
        totalCount: 0,
        pageSize: 0,
    }
}