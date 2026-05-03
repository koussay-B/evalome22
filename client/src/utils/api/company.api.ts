import http from '@utils/http'
import type { Company } from '@utils/models'
import type { PaginationMeta } from '@utils/models'

// ─── Search params ────────────────────────────────────────────────────────────

export interface CompanySearchParams {
  pageNumber?: number
  pageSize?: number
  search?: string
  orderBy?: string
  isActive?: boolean
  status?: string
}

// ─── Payloads ─────────────────────────────────────────────────────────────────

export interface CreateCompanyPayload {
  name: string
  description?: string
  email?: string
  phone?: string
  website?: string
  address?: string
  logo?: string
  requesterName?: string
  requesterEmail?: string
}

export type UpdateCompanyPayload = CreateCompanyPayload

export interface CompanyRequestPayload {
  name: string
  requesterName: string
  requesterEmail: string
  description?: string
  phone?: string
  website?: string
  address?: string
}

// ─── Error helper ─────────────────────────────────────────────────────────────

function extractError(err: unknown, fallback: string): string {
  const e = err as { response?: { data?: unknown } }
  const raw = e.response?.data
  return typeof raw === 'string'
    ? raw
    : (raw as { message?: string })?.message ?? fallback
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function getCompaniesApi(params: CompanySearchParams) {
  try {
    const res = await http.get<Company[]>('/company', { params })
    const meta: PaginationMeta = {
      currentPage: Number(res.headers['x-pagination-page']),
      totalPages:  Number(res.headers['x-pagination-total-pages']),
      pageSize:    Number(res.headers['x-pagination-page-size']),
      totalCount:  Number(res.headers['x-pagination-total-count']),
    }
    return { data: res.data, meta, error: undefined }
  } catch (err) {
    return { data: undefined, meta: undefined, error: extractError(err, 'Failed to fetch companies') }
  }
}

export async function getCompanyByIdApi(id: string) {
  try {
    const res = await http.get<Company>(`/company/${id}`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch company') }
  }
}

export async function createCompanyApi(payload: CreateCompanyPayload) {
  try {
    const res = await http.post<Company>('/company', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to create company') }
  }
}

export async function submitCompanyRequestApi(payload: CompanyRequestPayload) {
  try {
    const res = await http.post<Company>('/company/request', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to submit company request') }
  }
}

export async function getCompanyRequestsApi(status?: string) {
  try {
    const res = await http.get<Company[]>('/company/requests', {
      params: status ? { status } : undefined,
    })
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch company requests') }
  }
}

export async function approveCompanyRequestApi(id: number) {
  try {
    const res = await http.patch<Company>(`/company/${id}/approve`, {})
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to approve company request') }
  }
}

export async function rejectCompanyRequestApi(id: number, reason: string) {
  try {
    const res = await http.patch<Company>(`/company/${id}/reject`, { reason })
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to reject company request') }
  }
}

export async function updateCompanyApi(id: number, payload: UpdateCompanyPayload) {
  try {
    const res = await http.put<Company>(`/company/${id}`, payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to update company') }
  }
}

export async function deleteCompanyApi(id: number) {
  try {
    await http.delete(`/company/${id}`)
    return { error: undefined }
  } catch (err) {
    return { error: extractError(err, 'Failed to delete company') }
  }
}

// ─── Upload helpers ───────────────────────────────────────────────────────────

/**
 * Upload any image file to Cloudinary via the generic upload endpoint.
 * Returns the secure Cloudinary URL.
 */
export async function uploadImageApi(file: File) {
  const formData = new FormData()
  formData.append('file', file)
  try {
    const res = await http.post<{ url: string }>('/upload/image', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return { url: res.data.url, error: undefined }
  } catch (err) {
    return { url: undefined, error: extractError(err, 'Failed to upload image') }
  }
}

/**
 * Update only the logo of a company — uploads directly to the company logo endpoint.
 * Returns the new secure Cloudinary URL.
 */
export async function updateCompanyLogoApi(id: number, file: File) {
  const formData = new FormData()
  formData.append('logo', file)
  try {
    const res = await http.patch<{ logoUrl: string }>(`/company/${id}/logo`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return { logoUrl: res.data.logoUrl, error: undefined }
  } catch (err) {
    return { logoUrl: undefined, error: extractError(err, 'Failed to update logo') }
  }
}

export async function getMyCompanyApi() {
  try {
    const res = await http.get<Company>('/company/my')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to load company') }
  }
}
