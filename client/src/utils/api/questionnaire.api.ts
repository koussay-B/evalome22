import http from '@utils/http'
import type {
  QuestionnaireItem,
  CreateQuestionnairePayload,
  UpdateQuestionnairePayload,
  AddQuestionToQuestionnairePayload,
} from '@utils/models'

function extractError(err: unknown, fallback: string): string {
  const e   = err as { response?: { data?: unknown } }
  const raw = e.response?.data
  return typeof raw === 'string'
    ? raw
    : (raw as { message?: string })?.message ?? fallback
}

/** GET /api/questionnaire — all questionnaires for company */
export async function getQuestionnairesApi() {
  try {
    const res = await http.get<QuestionnaireItem[]>('/questionnaire')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch questionnaires') }
  }
}

/** GET /api/questionnaire/:id — single questionnaire with full question hierarchy */
export async function getQuestionnaireByIdApi(id: number) {
  try {
    const res = await http.get<QuestionnaireItem>(`/questionnaire/${id}`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch questionnaire') }
  }
}

/** POST /api/questionnaire — create a new questionnaire */
export async function createQuestionnaireApi(payload: CreateQuestionnairePayload) {
  try {
    const res = await http.post<QuestionnaireItem>('/questionnaire', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to create questionnaire') }
  }
}

/** PUT /api/questionnaire/:id — update questionnaire settings */
export async function updateQuestionnaireApi(id: number, payload: UpdateQuestionnairePayload) {
  try {
    const res = await http.put<QuestionnaireItem>(`/questionnaire/${id}`, payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to update questionnaire') }
  }
}

/** PUT /api/questionnaire/:id/publish — publish the questionnaire */
export async function publishQuestionnaireApi(id: number) {
  try {
    const res = await http.put<QuestionnaireItem>(`/questionnaire/${id}/publish`, {})
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to publish questionnaire') }
  }
}

/** POST /api/questionnaire/:id/questions — add an existing question to a questionnaire */
export async function addQuestionToQuestionnaireApi(
  questionnaireId: number,
  payload: AddQuestionToQuestionnairePayload | AddQuestionToQuestionnairePayload[],
) {
  try {
    const body = Array.isArray(payload) ? payload : [payload]
    const res = await http.post<QuestionnaireItem>(
      `/questionnaire/${questionnaireId}/questions`,
      body,
    )
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to add question') }
  }
}

/** DELETE /api/questionnaire/:id/questions/:questionId — remove a question from a questionnaire */
export async function removeQuestionFromQuestionnaireApi(questionnaireId: number, questionId: number) {
  try {
    const res = await http.delete<QuestionnaireItem>(`/questionnaire/${questionnaireId}/questions/${questionId}`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to remove question') }
  }
}

/** DELETE /api/questionnaire/:id — delete questionnaire (only if no active campaigns) */
export async function deleteQuestionnaireApi(id: number) {
  try {
    await http.delete(`/questionnaire/${id}`)
    return { error: undefined }
  } catch (err) {
    return { error: extractError(err, 'Failed to delete questionnaire') }
  }
}
