import http from '@utils/http'
import type { AiModelItem, AiModelPayload, TestAiModelPayload, TestAiModelResult } from '@utils/models'

function extractError(err: unknown, fallback: string): string {
  const e   = err as { response?: { data?: unknown } }
  const raw = e.response?.data
  return typeof raw === 'string'
    ? raw
    : (raw as { message?: string })?.message ?? fallback
}

export async function getAiModelsApi() {
  try {
    const res = await http.get<AiModelItem[]>('/ai-model')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch AI models') }
  }
}

export async function createAiModelApi(payload: AiModelPayload) {
  try {
    const res = await http.post<AiModelItem>('/ai-model', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to create AI model') }
  }
}

export async function updateAiModelApi(id: number, payload: AiModelPayload) {
  try {
    const res = await http.put<AiModelItem>(`/ai-model/${id}`, payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to update AI model') }
  }
}

export async function deleteAiModelApi(id: number) {
  try {
    await http.delete(`/ai-model/${id}`)
    return { error: undefined }
  } catch (err) {
    return { error: extractError(err, 'Failed to delete AI model') }
  }
}

export async function setDefaultAiModelApi(id: number) {
  try {
    const res = await http.put<AiModelItem>(`/ai-model/${id}/set-default`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to set default AI model') }
  }
}

export async function testAiModelApi(payload: TestAiModelPayload) {
  try {
    const res = await http.post<TestAiModelResult>('/ai-model/test-inline', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Test request failed') }
  }
}
