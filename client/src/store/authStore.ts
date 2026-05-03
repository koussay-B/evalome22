import { defineStore } from 'pinia'
import { reactive, computed } from 'vue'
import type { User, Company } from '@utils/models'
import http from '@utils/http'
import { loginApi } from '@utils/api'

interface AuthState {
  user: User | null
  company: Company | null
  token: string | null
  loading: boolean
  error: string | null
}

function loadFromStorage(): Pick<AuthState, 'user' | 'company' | 'token'> {
  return {
    user: JSON.parse(localStorage.getItem('user') || 'null'),
    company: JSON.parse(localStorage.getItem('company') || 'null'),
    token: localStorage.getItem('token'),
  }
}

export const useAuthStore = defineStore('auth', () => {
  const state = reactive<AuthState>({
    ...loadFromStorage(),
    loading: false,
    error: null,
  })

  const isAuthenticated = computed(() => !!state.token)
  const isAdmin        = computed(() => state.user?.role === 'Admin')
  const isCompanyAdmin = computed(() => state.user?.role === 'CompanyAdmin')
  const isFormateur    = computed(() => state.user?.role === 'Formateur')
  const isCondidat     = computed(() => state.user?.role === 'Condidat')

  async function login(credentials: { username: string; password: string }) {
    state.loading = true
    state.error = null

    const { data, error } = await loginApi(credentials)

    if (error || !data) {
      state.error = error
      state.loading = false
      return { success: false }
    }

    state.token   = data.token
    state.user    = data.user
    state.company = data.currentCompany

    localStorage.setItem('token',   data.token)
    localStorage.setItem('user',    JSON.stringify(data.user))
    localStorage.setItem('company', JSON.stringify(data.currentCompany))

    state.loading = false
    return { success: true, redirectUrl: data.redirectTo }
  }

  function logout() {
    state.user    = null
    state.company = null
    state.token   = null
    state.error   = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    localStorage.removeItem('company')
  }

  async function getUser() {
    if (!state.token) return null
    try {
      const res = await http.get('/auth/user')
      state.user = res.data
      localStorage.setItem('user', JSON.stringify(res.data))
      return state.user
    } catch {
      logout()
      return null
    }
  }

  function clearError() {
    state.error = null
  }

  function patchUser(updates: Partial<User>) {
    if (!state.user) return
    Object.assign(state.user, updates)
    localStorage.setItem('user', JSON.stringify(state.user))
  }

  return {
    state,
    isAuthenticated,
    isAdmin,
    isCompanyAdmin,
    isFormateur,
    isCondidat,
    login,
    logout,
    getUser,
    clearError,
    patchUser,
  }
})
