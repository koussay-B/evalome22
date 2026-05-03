import http from '@utils/http'
import type {
  CampaignItem,
  CampaignCandidateItem,
  CampaignQuestionnaireItem,
  AvailableCandidateItem,
  CreateCampaignPayload,
  UpdateCampaignPayload,
  AddCampaignQuestionnairePayload,
  ReorderCampaignQuestionnairesPayload,
} from '@utils/models'

function extractError(err: unknown, fallback: string): string {
  const e   = err as { response?: { data?: unknown } }
  const raw = e.response?.data
  return typeof raw === 'string'
    ? raw
    : (raw as { message?: string })?.message ?? fallback
}

/** GET /api/campaign — all campaigns for current user's company */
export async function getCampaignsApi() {
  try {
    const res = await http.get<CampaignItem[]>('/campaign')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch campaigns') }
  }
}

/** GET /api/campaign/:id — single campaign with candidates */
export async function getCampaignByIdApi(id: number) {
  try {
    const res = await http.get<CampaignItem>(`/campaign/${id}`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch campaign') }
  }
}

/** POST /api/campaign — create a new campaign */
export async function createCampaignApi(payload: CreateCampaignPayload) {
  try {
    const res = await http.post<CampaignItem>('/campaign', payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to create campaign') }
  }
}

/** PUT /api/campaign/:id — update a campaign */
export async function updateCampaignApi(id: number, payload: UpdateCampaignPayload) {
  try {
    const res = await http.put<CampaignItem>(`/campaign/${id}`, payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to update campaign') }
  }
}

/** DELETE /api/campaign/:id — delete a draft campaign */
export async function deleteCampaignApi(id: number) {
  try {
    await http.delete(`/campaign/${id}`)
    return { error: undefined }
  } catch (err) {
    return { error: extractError(err, 'Failed to delete campaign') }
  }
}

/** GET /api/campaign/:id/candidates — list all invited candidates */
export async function getCampaignCandidatesApi(campaignId: number) {
  try {
    const res = await http.get<CampaignCandidateItem[]>(`/campaign/${campaignId}/candidates`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch candidates') }
  }
}

/** POST /api/campaign/:id/invite — invite candidates by user IDs */
export async function inviteCandidatesApi(campaignId: number, userIds: number[]) {
  try {
    await http.post(`/campaign/${campaignId}/invite`, { userIds })
    return { error: undefined }
  } catch (err) {
    return { error: extractError(err, 'Failed to invite candidates') }
  }
}

/** DELETE /api/campaign/:id/candidates/:candidateId — remove a candidate */
export async function removeCampaignCandidateApi(campaignId: number, candidateId: number) {
  try {
    await http.delete(`/campaign/${campaignId}/candidates/${candidateId}`)
    return { error: undefined }
  } catch (err) {
    return { error: extractError(err, 'Failed to remove candidate') }
  }
}

/** GET /api/campaign/:id/questionnaires — ordered questionnaire list for a campaign */
export async function getCampaignQuestionnairesApi(campaignId: number) {
  try {
    const res = await http.get<CampaignQuestionnaireItem[]>(`/campaign/${campaignId}/questionnaires`)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch questionnaires') }
  }
}

/** POST /api/campaign/:id/questionnaires — attach a questionnaire to a campaign */
export async function addCampaignQuestionnaireApi(campaignId: number, payload: AddCampaignQuestionnairePayload) {
  try {
    const res = await http.post<CampaignQuestionnaireItem>(`/campaign/${campaignId}/questionnaires`, payload)
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to add questionnaire') }
  }
}

/** DELETE /api/campaign/:id/questionnaires/:questionnaireId — remove a questionnaire from a campaign */
export async function removeCampaignQuestionnaireApi(campaignId: number, questionnaireId: number) {
  try {
    await http.delete(`/campaign/${campaignId}/questionnaires/${questionnaireId}`)
    return { error: undefined }
  } catch (err) {
    return { error: extractError(err, 'Failed to remove questionnaire') }
  }
}

/** PUT /api/campaign/:id/questionnaires/reorder — reorder questionnaires in a campaign */
export async function reorderCampaignQuestionnairesApi(campaignId: number, payload: ReorderCampaignQuestionnairesPayload) {
  try {
    await http.put(`/campaign/${campaignId}/questionnaires/reorder`, payload)
    return { error: undefined }
  } catch (err) {
    return { error: extractError(err, 'Failed to reorder questionnaires') }
  }
}

/** GET /api/campaign/candidates/available — same-company candidates only */
export async function getAvailableCandidatesApi() {
  try {
    const res = await http.get<AvailableCandidateItem[]>('/campaign/candidates/available')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to fetch candidates') }
  }
}
