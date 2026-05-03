import http from '@utils/http'
import type { User, Company } from '@utils/models'

export interface LoginPayload {
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  redirectTo: string
  user: User
  currentCompany: Company | null
}

export interface DevUser {
  id: number
  userName: string
  email: string
  imageUrl?: string | null
  roles: string[]
}

export async function loginApi(payload: LoginPayload) {
  try {
    const res = await http.post<LoginResponse>('/auth/login', payload)
    return { data: res.data, error: null }
  } catch (err: unknown) {
    const e = err as { response?: { data?: unknown } }
    const raw = e.response?.data
    const error = typeof raw === 'string' ? raw : (raw as { message?: string })?.message ?? 'Login failed'
    return { data: null, error }
  }
}

export async function getDevUsersApi() {
  try {
    const res = await http.get<DevUser[]>('/auth/dev/users')
    return { data: res.data, error: null }
  } catch (err: unknown) {
    const e = err as { response?: { data?: unknown } }
    const raw = e.response?.data
    const error = typeof raw === 'string' ? raw : (raw as { message?: string })?.message ?? 'Failed to fetch users'
    return { data: null, error }
  }
}
