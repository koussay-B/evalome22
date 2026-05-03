import http from '@utils/http'
import type { CertificateItem } from '@utils/models'
export type { CertificateItem }

function extractError(err: unknown, fallback: string): string {
  const e = err as { response?: { data?: unknown } }
  const raw = e.response?.data
  if (typeof raw === 'string') return raw
  if (Array.isArray(raw)) return raw.join(' ')
  return (raw as { message?: string })?.message ?? fallback
}

export async function getMyCertificatesApi() {
  try {
    const res = await http.get<CertificateItem[]>('/certificate/my')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to load certificates') }
  }
}

export async function getCertificateByCodeApi(code: string) {
  try {
    const res = await http.get<CertificateItem>(`/certificate/verify/${code}`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Certificate not found') }
  }
}
