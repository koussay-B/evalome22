import http from '@utils/http'
import type { PaginationMeta, UserListItem, UpdateManagedUserPayload, ManagedUserStats } from '@utils/models'

export interface UserSearchParams {
  pageNumber?: number
  pageSize?: number
  search?: string
  role?: string
  includeCompany?: boolean
  orderBy?: string
}

export interface InviteUserPayload {
  userName: string
  email: string
  role: string
  companyId?: number
}

// ─── Error helper ─────────────────────────────────────────────────────────────

function extractError(err: unknown, fallback: string): string {
  const e = err as { response?: { data?: unknown } }
  const raw = e.response?.data
  if (typeof raw === 'string') return raw
  if (Array.isArray(raw)) return raw.join(' ')
  return (raw as { message?: string })?.message ?? fallback
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function getUsersApi(params: UserSearchParams) {
  try {
    const res = await http.get<UserListItem[]>('/users', { params })
    const meta: PaginationMeta = {
      currentPage: Number(res.headers['x-pagination-page']),
      totalPages:  Number(res.headers['x-pagination-total-pages']),
      pageSize:    Number(res.headers['x-pagination-page-size']),
      totalCount:  Number(res.headers['x-pagination-total-count']),
    }
    return { data: res.data, meta, error: undefined }
  } catch (err) {
    return { data: undefined, meta: undefined, error: extractError(err, 'Failed to fetch users') }
  }
}

export async function getUserByIdApi(id: string) {
  try {
    const res = await http.get<UserListItem>(`/users/${id}`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch user') }
  }
}

export async function inviteUserApi(payload: InviteUserPayload) {
  try {
    const res = await http.post<UserListItem>('/users/invite', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to invite user') }
  }
}

// ── My-account mutations ───────────────────────────────────────────────────

export async function updateMyProfileApi(payload: { userName: string; email: string }) {
  try {
    const res = await http.patch<UserListItem>('/users/me/profile', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to update profile') }
  }
}

export async function updateMyAvatarApi(imageUrl: string) {
  try {
    const res = await http.patch<{ imageUrl: string }>('/users/me/avatar', { imageUrl })
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to update avatar') }
  }
}

export async function changeMyPasswordApi(payload: { currentPassword: string; newPassword: string }) {
  try {
    const res = await http.patch<{ message: string }>('/users/me/password', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to change password') }
  }
}

export async function getUserStatsApi(id: number) {
  try {
    const res = await http.get<ManagedUserStats>(`/users/${id}/stats`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch user stats') }
  }
}

export async function updateManagedUserApi(id: number, payload: UpdateManagedUserPayload) {
  try {
    const res = await http.patch<UserListItem>(`/users/${id}`, payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to update user') }
  }
}

export async function resetManagedUserPasswordApi(id: number) {
  try {
    const res = await http.post<{ message: string }>(`/users/${id}/reset-password`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to reset password') }
  }
}

export async function deleteManagedUserApi(id: number) {
  try {
    await http.delete(`/users/${id}`)
    return { error: undefined }
  } catch (err) {
    return { error: extractError(err, 'Failed to delete user') }
  }
}
