import http from '@utils/http'
import type { ThemeItem } from '@utils/models'

// ─── Payloads ─────────────────────────────────────────────────────────────────

export interface CreateThemePayload {
  name: string
  description?: string | null
  icon?: string | null
  order?: number
  parentId?: number | null
}

export interface UpdateThemePayload {
  name: string
  description?: string | null
  icon?: string | null
  order?: number
  parentId?: number | null
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

/** GET /api/theme/roots — root themes with their children for the current user's company */
export async function getRootThemesApi() {
  try {
    const res = await http.get<ThemeItem[]>('/theme/roots')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch themes') }
  }
}

/** GET /api/theme/:id — single theme */
export async function getThemeByIdApi(id: number) {
  try {
    const res = await http.get<ThemeItem>(`/theme/${id}`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch theme') }
  }
}

/** GET /api/theme/:id/children — direct children of a theme */
export async function getThemeChildrenApi(id: number) {
  try {
    const res = await http.get<ThemeItem[]>(`/theme/${id}/children`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch subthemes') }
  }
}

/** POST /api/theme — create a new theme or subtheme */
export async function createThemeApi(payload: CreateThemePayload) {
  try {
    const res = await http.post<ThemeItem>('/theme', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to create theme') }
  }
}

/** PUT /api/theme/:id — update an existing theme */
export async function updateThemeApi(id: number, payload: UpdateThemePayload) {
  try {
    const res = await http.put<ThemeItem>(`/theme/${id}`, payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to update theme') }
  }
}

/** DELETE /api/theme/:id — soft-delete a theme (CompanyAdmin only) */
export async function deleteThemeApi(id: number) {
  try {
    await http.delete(`/theme/${id}`)
    return { error: undefined }
  } catch (err) {
    return { error: extractError(err, 'Failed to delete theme') }
  }
}

// ─── Admin-only API (cross-company, Admin role) ───────────────────────────────

export interface AdminCreateThemePayload {
  name: string
  description?: string | null
  icon?: string | null
  order?: number
  companyId: number
  parentId?: number | null
}

export type AdminUpdateThemePayload = Omit<AdminCreateThemePayload, 'companyId'>

/** GET /api/theme/admin/all — all root themes across every company */
export async function getAdminThemesApi() {
  try {
    const res = await http.get<ThemeItem[]>('/theme/admin/all')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch themes') }
  }
}

/** POST /api/theme/admin — admin creates a theme for any company */
export async function adminCreateThemeApi(payload: AdminCreateThemePayload) {
  try {
    const res = await http.post<ThemeItem>('/theme/admin', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to create theme') }
  }
}

/** PUT /api/theme/admin/:id — admin updates any theme */
export async function adminUpdateThemeApi(id: number, payload: AdminUpdateThemePayload) {
  try {
    const res = await http.put<ThemeItem>(`/theme/admin/${id}`, payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to update theme') }
  }
}

/** DELETE /api/theme/admin/:id — admin soft-deletes any theme */
export async function adminDeleteThemeApi(id: number) {
  try {
    await http.delete(`/theme/admin/${id}`)
    return { error: undefined }
  } catch (err) {
    return { error: extractError(err, 'Failed to delete theme') }
  }
}
