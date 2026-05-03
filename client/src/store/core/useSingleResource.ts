import { shallowRef, computed } from 'vue'
import type { AsyncState } from './asyncState'

// ─── Types ────────────────────────────────────────────────────────────────────

export interface SingleFetchResult<T> {
    data?: T
    error?: string
}

// ─── Factory ──────────────────────────────────────────────────────────────────

/**
 * Creates a self-contained single-entity resource with an in-memory ID cache.
 *
 * @param fetcher - async function that receives an ID and returns the entity
 *
 * @example
 * const company = useSingleResource<Company, string>(
 *   (id) => getCompanyByIdApi(id),
 * )
 */
export function useSingleResource<T, Id = string | number>(
    fetcher: (id: Id) => Promise<SingleFetchResult<T>>,
) {
    // ── Internal state ──────────────────────────────────────────────────────
    const state = shallowRef<AsyncState<T>>({ status: 'idle' })
    const currentId = shallowRef<Id | null>(null)

    const _cache = new Map<string, T>()

    // ── Computed ────────────────────────────────────────────────────────────
    const isLoading = computed(() => state.value.status === 'loading')
    const isError = computed(() => state.value.status === 'error')
    const hasData = computed(() => state.value.status === 'data')

    const item = computed<T | null>(() =>
        state.value.status === 'data' ? state.value.data : null,
    )

    const errorMessage = computed<string | null>(() =>
        state.value.status === 'error' ? state.value.message : null,
    )

    // ── Actions ─────────────────────────────────────────────────────────────

    async function fetchById(id: Id) {
        const cacheKey = String(id)

        // Cache hit
        if (_cache.has(cacheKey)) {
            currentId.value = id
            state.value = { status: 'data', data: _cache.get(cacheKey)! }
            return
        }

        // Cache miss
        currentId.value = id
        state.value = { status: 'loading' }

        const { data, error } = await fetcher(id)

        if (error || !data) {
            state.value = { status: 'error', message: error ?? 'Failed to load item' }
            return
        }

        _cache.set(cacheKey, data)
        state.value = { status: 'data', data }
    }

    /** Invalidate a specific ID from cache (forces refetch next time) */
    function invalidate(id: Id) {
        _cache.delete(String(id))
    }

    /** Force-refetch the current ID */
    async function refresh() {
        if (currentId.value === null) return
        invalidate(currentId.value)
        await fetchById(currentId.value)
    }

    function reset() {
        _cache.clear()
        currentId.value = null
        state.value = { status: 'idle' }
    }

    return {
        state,
        currentId,
        isLoading,
        isError,
        hasData,
        item,
        errorMessage,
        fetchById,
        invalidate,
        refresh,
        reset,
    }
}