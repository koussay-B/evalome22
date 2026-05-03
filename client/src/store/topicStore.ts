import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { ThemeItem } from '@utils/models'
import {
  getRootThemesApi,
  createThemeApi,
  updateThemeApi,
  deleteThemeApi,
  type CreateThemePayload,
  type UpdateThemePayload,
} from '@utils/api'

/**
 * Pinia store for domain-level themes (categories for questions).
 * Named "topic" to avoid collision with the UI theme (dark/light) store.
 */
export const useTopicStore = defineStore('topic', () => {
  // ── State ──────────────────────────────────────────────────────────────────
  const themes = ref<ThemeItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // ── Computed ───────────────────────────────────────────────────────────────
  const totalSubthemes = computed(() =>
    themes.value.reduce((acc, t) => acc + (t.children?.length ?? 0), 0),
  )

  const totalQuestions = computed(() =>
    themes.value.reduce((acc, t) => acc + (t.questions?.length ?? 0), 0),
  )

  // ── Actions ────────────────────────────────────────────────────────────────

  /** Load all root themes with their children from the API */
  async function fetchThemes() {
    loading.value = true
    error.value = null
    const { data, error: err } = await getRootThemesApi()
    if (err || !data) {
      error.value = err ?? 'Failed to load themes'
    } else {
      themes.value = data
    }
    loading.value = false
  }

  /** Create a root theme. Returns the created theme or an error. */
  async function createTheme(payload: CreateThemePayload) {
    const { data, error: err } = await createThemeApi({ ...payload, parentId: null })
    if (err || !data) return { data: undefined, error: err ?? 'Failed to create theme' }
    themes.value.push({ ...data, children: data.children ?? [] })
    return { data, error: undefined }
  }

  /** Update a root theme's name / description. */
  async function updateTheme(id: number, payload: UpdateThemePayload) {
    const { data, error: err } = await updateThemeApi(id, payload)
    if (err || !data) return { data: undefined, error: err ?? 'Failed to update theme' }
    const idx = themes.value.findIndex(t => t.id === id)
    if (idx !== -1) {
      themes.value[idx] = { ...themes.value[idx]!, ...data, children: themes.value[idx]!.children }
    }
    return { data, error: undefined }
  }

  /** Soft-delete a root theme (CompanyAdmin only). */
  async function deleteTheme(id: number) {
    const { error: err } = await deleteThemeApi(id)
    if (err) return { error: err }
    themes.value = themes.value.filter(t => t.id !== id)
    return { error: undefined }
  }

  // ── Subtheme helpers ───────────────────────────────────────────────────────

  /** Create a subtheme under a parent theme. */
  async function createSubTheme(parentId: number, payload: CreateThemePayload) {
    const { data, error: err } = await createThemeApi({ ...payload, parentId })
    if (err || !data) return { data: undefined, error: err ?? 'Failed to create subtheme' }
    const parent = themes.value.find(t => t.id === parentId)
    if (parent) {
      parent.children = [...(parent.children ?? []), { ...data, children: [] }]
    }
    return { data, error: undefined }
  }

  /** Update a subtheme. */
  async function updateSubTheme(id: number, parentId: number, payload: UpdateThemePayload) {
    const { data, error: err } = await updateThemeApi(id, { ...payload, parentId })
    if (err || !data) return { data: undefined, error: err ?? 'Failed to update subtheme' }
    const parent = themes.value.find(t => t.id === parentId)
    if (parent) {
      const ci = parent.children.findIndex(c => c.id === id)
      if (ci !== -1) parent.children[ci] = { ...data, children: [] }
    }
    return { data, error: undefined }
  }

  /** Delete a subtheme (CompanyAdmin only). */
  async function deleteSubTheme(id: number, parentId: number) {
    const { error: err } = await deleteThemeApi(id)
    if (err) return { error: err }
    const parent = themes.value.find(t => t.id === parentId)
    if (parent) {
      parent.children = parent.children.filter(c => c.id !== id)
    }
    return { error: undefined }
  }

  function reset() {
    themes.value = []
    error.value = null
    loading.value = false
  }

  return {
    themes,
    loading,
    error,
    totalSubthemes,
    totalQuestions,
    fetchThemes,
    createTheme,
    updateTheme,
    deleteTheme,
    createSubTheme,
    updateSubTheme,
    deleteSubTheme,
    reset,
  }
})
