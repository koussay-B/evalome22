import { defineStore } from 'pinia'
import type { UserListItem } from '@utils/models'
import {
  getUsersApi,
  getUserByIdApi,
  inviteUserApi,
  type UserSearchParams,
  type InviteUserPayload,
} from '@utils/api'
import { usePaginatedResource } from './core/usePaginatedResource'
import { useSingleResource }    from './core/useSingleResource'

export const useUserStore = defineStore('users', () => {
  // ── Paginated list ───────────────────────────────────────────────────────
  const list = usePaginatedResource<UserListItem, UserSearchParams>(getUsersApi)

  // ── Single entity ────────────────────────────────────────────────────────
  const single = useSingleResource<UserListItem, string>(getUserByIdApi)

  // ── Mutations ────────────────────────────────────────────────────────────

  /**
   * Invite a new user. Busts the list cache so the next fetch is fresh.
   */
  async function inviteUser(payload: InviteUserPayload) {
    const result = await inviteUserApi(payload)
    if (!result.error) {
      list.reset()
    }
    return result
  }

  function removeFromCurrentPage(id: number) {
    const current = list.state.value
    if (current.status !== 'data') return

    const page = current.data.meta.currentPage
    const currentPageItems = current.data.pages[page] ?? []
    const nextPageItems = currentPageItems.filter(user => user.id !== id)

    if (nextPageItems.length === currentPageItems.length) return

    list.state.value = {
      ...current,
      data: {
        pages: {
          ...current.data.pages,
          [page]: nextPageItems,
        },
        meta: {
          ...current.data.meta,
          totalCount: Math.max(0, current.data.meta.totalCount - 1),
        },
      },
    }
  }

  function resetAll() {
    list.reset()
    single.reset()
  }

  return {
    // ── list namespace ───────────────────────────────────────────────────
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
      removeFromCurrentPage,
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

    // ── mutations ────────────────────────────────────────────────────────
    inviteUser,
    resetAll,
  }
})
