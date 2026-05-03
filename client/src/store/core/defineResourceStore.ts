import { defineStore } from 'pinia'
import { usePaginatedResource } from './usePaginatedResource'
import { useSingleResource }    from './useSingleResource'
import type { PagedParams, FetchResult } from './usePaginatedResource'
import type { SingleFetchResult }        from './useSingleResource'

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ResourceStoreOptions<T, P extends object, Id = string | number> {
  /** Pinia store id, e.g. 'company' */
  id: string
  /** Fetches a paginated list */
  fetchList: (params: PagedParams<P>) => Promise<FetchResult<T>>
  /** Fetches a single entity by id */
  fetchOne:  (id: Id) => Promise<SingleFetchResult<T>>
  /** Optional: how to serialise filter params (excluding pageNumber) into a cache key */
  toFilterKey?: (params: P) => string
}

// ─── Factory ──────────────────────────────────────────────────────────────────

/**
 * Creates a fully typed Pinia store that owns both a paginated list state
 * and a single-entity state for the same resource type.
 *
 * @example
 * export const useCompanyStore = defineResourceStore<Company, CompanySearchParams, string>({
 *   id: 'company',
 *   fetchList: getCompaniesApi,
 *   fetchOne:  getCompanyByIdApi,
 * })
 */
export function defineResourceStore<T, P extends object, Id = string | number>({
  id,
  fetchList,
  fetchOne,
  toFilterKey,
}: ResourceStoreOptions<T, P, Id>) {
  return defineStore(id, () => {
    // ── List (paginated) ──────────────────────────────────────────────────
    const list = usePaginatedResource<T, P>(fetchList, toFilterKey)

    // ── Single (by id) ────────────────────────────────────────────────────
    const single = useSingleResource<T, Id>(fetchOne)

    // ── Combined reset ────────────────────────────────────────────────────
    function resetAll() {
      list.reset()
      single.reset()
    }

    return {
      // ── list namespace ──────────────────────────────────────────────────
      list: {
        state:        list.state,
        isLoading:    list.isLoading,
        isError:      list.isError,
        hasData:      list.hasData,
        currentItems: list.currentItems,
        meta:         list.meta,
        canGoNext:    list.canGoNext,
        canGoPrev:    list.canGoPrev,
        fetchPage:    list.fetchPage,
        nextPage:     list.nextPage,
        prevPage:     list.prevPage,
        goToPage:     list.goToPage,
        search:       list.search,
        reset:        list.reset,
      },

      // ── single namespace ─────────────────────────────────────────────────
      single: {
        state:        single.state,
        currentId:    single.currentId,
        isLoading:    single.isLoading,
        isError:      single.isError,
        hasData:      single.hasData,
        item:         single.item,
        errorMessage: single.errorMessage,
        fetchById:    single.fetchById,
        invalidate:   single.invalidate,
        refresh:      single.refresh,
        reset:        single.reset,
      },

      resetAll,
    }
  })
}