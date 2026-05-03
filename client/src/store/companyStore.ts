import { defineStore } from 'pinia'
import type { Company } from '@utils/models'
import {
  getCompaniesApi,
  getCompanyByIdApi,
  createCompanyApi,
  updateCompanyApi,
  deleteCompanyApi,
  updateCompanyLogoApi,
  submitCompanyRequestApi,
  getCompanyRequestsApi,
  approveCompanyRequestApi,
  rejectCompanyRequestApi,
  type CompanySearchParams,
  type CreateCompanyPayload,
  type UpdateCompanyPayload,
  type CompanyRequestPayload,
} from '@utils/api'
import { ref } from 'vue'
import { usePaginatedResource } from './core/usePaginatedResource'
import { useSingleResource }    from './core/useSingleResource'

export const useCompanyStore = defineStore('company', () => {
  // ── Paginated list ───────────────────────────────────────────────────────
  const list = usePaginatedResource<Company, CompanySearchParams>(getCompaniesApi)

  // ── Single entity ────────────────────────────────────────────────────────
  const single = useSingleResource<Company, string>(getCompanyByIdApi)

  // ── Mutations ────────────────────────────────────────────────────────────
  const requests = ref<Company[]>([])

  /**
   * Create a new company. Busts the list cache so the next fetch is fresh.
   */
  async function createCompany(payload: CreateCompanyPayload) {
    const result = await createCompanyApi(payload)
    if (!result.error) {
      list.reset()   // force list to reload
    }
    return result
  }

  async function submitCompanyRequest(payload: CompanyRequestPayload) {
    return await submitCompanyRequestApi(payload)
  }

  async function fetchRequests(status?: string) {
    const result = await getCompanyRequestsApi(status)
    if (!result.error) {
      requests.value = result.data ?? []
    }
    return result
  }

  async function approveRequest(id: number) {
    const result = await approveCompanyRequestApi(id)
    if (!result.error) {
      list.reset()
      requests.value = requests.value.map(item => item.id === id ? result.data! : item)
    }
    return result
  }

  async function rejectRequest(id: number, reason: string) {
    const result = await rejectCompanyRequestApi(id, reason)
    if (!result.error) {
      list.reset()
      requests.value = requests.value.map(item => item.id === id ? result.data! : item)
    }
    return result
  }

  /**
   * Update an existing company.
   * - Invalidates the single-entity cache for this id
   * - Busts the list cache
   * - Returns the updated Company DTO from the server
   */
  async function updateCompany(id: number, payload: UpdateCompanyPayload) {
    const result = await updateCompanyApi(id, payload)
    if (!result.error) {
      single.invalidate(String(id))
      list.reset()
    }
    return result
  }

  /**
   * Upload a new logo for a company.
   * - Sends file directly to the company logo endpoint (Cloudinary via backend)
   * - Invalidates both single-entity and list cache
   */
  async function updateCompanyLogo(id: number, file: File) {
    const result = await updateCompanyLogoApi(id, file)
    if (!result.error) {
      single.invalidate(String(id))
      list.reset()
    }
    return result
  }

  /**
   * Soft-delete a company.
   * - Invalidates the single-entity cache
   * - Busts the list cache
   */
  async function deleteCompany(id: number) {
    const result = await deleteCompanyApi(id)
    if (!result.error) {
      single.invalidate(String(id))
      list.reset()
    }
    return result
  }

  // ── Combined reset ───────────────────────────────────────────────────────
  function resetAll() {
    list.reset()
    single.reset()
    requests.value = []
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
    createCompany,
    updateCompany,
    updateCompanyLogo,
    deleteCompany,
    submitCompanyRequest,
    requests,
    fetchRequests,
    approveRequest,
    rejectRequest,
    resetAll,
  }
})
