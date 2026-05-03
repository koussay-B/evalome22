import http from '@utils/http'
import type { GenerateQuestionsPayload, GeneratedQuestionDto } from '@utils/models'

function extractError(err: unknown, fallback: string): string {
  const e   = err as { response?: { data?: unknown } }
  const raw = e.response?.data
  return typeof raw === 'string'
    ? raw
    : (raw as { message?: string })?.message ?? fallback
}

export async function generateQuestionsApi(payload: GenerateQuestionsPayload) {
  try {
    const res = await http.post<GeneratedQuestionDto[]>('/ai/generate-questions', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'AI generation failed') }
  }
}
