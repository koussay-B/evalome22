import http from '@utils/http'
import type { QuestionItem, CreateQuestionPayload, UpdateQuestionPayload } from '@utils/models'

function extractError(err: unknown, fallback: string): string {
  const e   = err as { response?: { data?: unknown } }
  const raw = e.response?.data
  return typeof raw === 'string'
    ? raw
    : (raw as { message?: string })?.message ?? fallback
}

/** GET /api/question — all questions for the current user's company */
export async function getQuestionsApi() {
  try {
    const res = await http.get<QuestionItem[]>('/question')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch questions') }
  }
}

/** GET /api/question/:id — single question with choices & prerequisites */
export async function getQuestionByIdApi(id: number) {
  try {
    const res = await http.get<QuestionItem>(`/question/${id}`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch question') }
  }
}

/** GET /api/question/theme/:themeId — questions filtered by theme */
export async function getQuestionsByThemeApi(themeId: number) {
  try {
    const res = await http.get<QuestionItem[]>(`/question/theme/${themeId}`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch questions') }
  }
}

/** POST /api/question — create a new question */
export async function createQuestionApi(payload: CreateQuestionPayload) {
  try {
    const res = await http.post<QuestionItem>('/question', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to create question') }
  }
}

/** PUT /api/question/:id — update an existing question */
export async function updateQuestionApi(id: number, payload: UpdateQuestionPayload) {
  try {
    const res = await http.put<QuestionItem>(`/question/${id}`, payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to update question') }
  }
}

/** DELETE /api/question/:id — soft-delete a question */
export async function deleteQuestionApi(id: number) {
  try {
    await http.delete(`/question/${id}`)
    return { error: undefined }
  } catch (err) {
    return { error: extractError(err, 'Failed to delete question') }
  }
}
