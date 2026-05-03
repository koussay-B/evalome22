import http from '@utils/http'
import type { CampaignReportData } from '@utils/models'

function extractError(err: unknown, fallback: string): string {
  const e = err as { response?: { data?: unknown } }
  const raw = e.response?.data
  return typeof raw === 'string'
    ? raw
    : (raw as { message?: string })?.message ?? fallback
}

export async function getCampaignReportApi(campaignId: number) {
  try {
    const res = await http.get<CampaignReportData>(`/report/campaign/${campaignId}`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to load campaign report') }
  }
}
