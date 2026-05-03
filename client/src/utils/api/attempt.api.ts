import http from '@utils/http'
import type {
  StartAttemptResponse,
  SubmitAttemptResult,
  AttemptResultItem,
  MyCampaignItem,
  MyCampaignDetail,
  AttemptQuestion,
} from '@utils/models'

function extractError(err: unknown, fallback: string): string {
  const e   = err as { response?: { data?: unknown } }
  const raw = e.response?.data
  if (typeof raw === 'string' && raw.length < 300) return raw
  return fallback
}

// ── GET /api/campaign/my ─────────────────────────────────────────────────────

export async function getMyCampaignsApi(): Promise<{ data: MyCampaignItem[] | null; error: string | null }> {
  try {
    const res = await http.get<MyCampaignItem[]>('/campaign/my')
    return { data: res.data, error: null }
  } catch (err) {
    return { data: null, error: extractError(err, 'Failed to load campaigns') }
  }
}

// ── GET /api/campaign/{id}/my-detail ─────────────────────────────────────────

export async function getCampaignDetailApi(
  campaignId: number
): Promise<{ data: MyCampaignDetail | null; error: string | null }> {
  try {
    const res = await http.get<MyCampaignDetail>(`/campaign/${campaignId}/my-detail`)
    return { data: res.data, error: null }
  } catch (err) {
    return { data: null, error: extractError(err, 'Failed to load campaign detail') }
  }
}

// ── POST /api/attempt/start ──────────────────────────────────────────────────

export async function startAttemptApi(
  campaignId: number,
  questionnaireId?: number
): Promise<{ data: StartAttemptResponse | null; error: string | null }> {
  try {
    const res = await http.post<{
      attemptId:            number
      questions:            string
      questionnaireName:    string
      showTimer:            boolean
      durationMinutes:      number | null
      allowNavigationBack:  boolean
      tabSwitchMaxCount:    number
      maxAttempts:          number
      campaignCandidateId:  number
    }>('/attempt/start', {
      campaignId,
      questionnaireId: questionnaireId ?? null,
    })

    const questions: AttemptQuestion[] = typeof res.data.questions === 'string'
      ? JSON.parse(res.data.questions)
      : res.data.questions as unknown as AttemptQuestion[]

    return {
      data: {
        attemptId:            res.data.attemptId,
        questions,
        questionnaireName:    res.data.questionnaireName,
        durationMinutes:      res.data.durationMinutes,
        allowNavigationBack:  res.data.allowNavigationBack,
        showTimer:            res.data.showTimer,
        tabSwitchMaxCount:    res.data.tabSwitchMaxCount,
        maxAttempts:          res.data.maxAttempts,
        campaignCandidateId:  res.data.campaignCandidateId,
      },
      error: null,
    }
  } catch (err) {
    return { data: null, error: extractError(err, 'Failed to start attempt') }
  }
}

// ── POST /api/attempt/:id/answer ─────────────────────────────────────────────

export async function submitAnswerApi(
  attemptId:        number,
  questionId:       number,
  selectedChoiceIds: number[],
  timeSpentSeconds: number
): Promise<{ error: string | null }> {
  try {
    await http.post(`/attempt/${attemptId}/answer`, { questionId, selectedChoiceIds, timeSpentSeconds })
    return { error: null }
  } catch (err) {
    return { error: extractError(err, 'Failed to save answer') }
  }
}

// ── POST /api/attempt/:id/submit ─────────────────────────────────────────────

export async function submitAttemptApi(
  attemptId: number
): Promise<{ data: SubmitAttemptResult | null; error: string | null }> {
  try {
    const res = await http.post<SubmitAttemptResult>(`/attempt/${attemptId}/submit`)
    return { data: res.data, error: null }
  } catch (err) {
    return { data: null, error: extractError(err, 'Failed to submit attempt') }
  }
}

// ── POST /api/attempt/:id/tab-switch ─────────────────────────────────────────

export async function reportTabSwitchApi(attemptId: number): Promise<void> {
  try {
    await http.post(`/attempt/${attemptId}/tab-switch`)
  } catch {
    // best-effort
  }
}

// ── GET /api/attempt/:id/result ──────────────────────────────────────────────

export async function getAttemptResultApi(
  attemptId: number
): Promise<{ data: AttemptResultItem | null; error: string | null }> {
  try {
    const res = await http.get<AttemptResultItem>(`/attempt/${attemptId}/result`)
    return { data: res.data, error: null }
  } catch (err) {
    return { data: null, error: extractError(err, 'Failed to load results') }
  }
}

// ── POST /api/attempt/:id/generate-report ─────────────────────────────────────

export async function generateReportApi(
  attemptId: number
): Promise<{ data: { aiStrengths: string | null; aiRecommendations: string | null } | null; error: string | null }> {
  try {
    const res = await http.post<{ aiStrengths: string | null; aiRecommendations: string | null }>(`/attempt/${attemptId}/generate-report`)
    return { data: res.data, error: null }
  } catch (err) {
    return { data: null, error: extractError(err, 'Failed to generate report') }
  }
}

