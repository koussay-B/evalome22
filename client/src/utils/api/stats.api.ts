import http from '@utils/http'

export interface DailyCount { date: string; count: number }

export interface AdminStats {
  totalUsers: number
  totalCompanies: number
  totalCampaigns: number
  totalQuestionnaires: number
  totalAttempts: number
  avgPassRate: number
  avgScore: number
  attemptStatusCounts: { submitted: number; inProgress: number; timedOut: number; abandoned: number }
  roleDistribution: Record<string, number>
  passRates: { companyName: string; passRate: number }[]
  companyMembers: { companyName: string; condidat: number; formateur: number; companyAdmin: number; total: number }[]
  recentActivity: { type: string; description: string; at: string }[]
  attemptTrend: DailyCount[]
  memberTrend: DailyCount[]
  campaignsPerCompany: { companyName: string; count: number }[] //******* */
  onlineUsersCount: number
}

export interface CandidatStats {
  totalCampaigns: number
  completedAttempts: number
  certificatesEarned: number
  passRate: number
  avgScore: number
  attemptTrend: DailyCount[]
  scoresByCampaign: { campaignName: string; score: number; passed: boolean; submittedAt: string }[]
  passFailRatio: { passed: number; failed: number }
}

export interface FormateurStats {
  totalCampaigns: number
  activeCampaigns: number
  totalQuestionnaires: number
  totalCandidates: number
  passRate: number
  avgScore: number
  attemptTrend: DailyCount[]
  attemptStatusCounts: { submitted: number; inProgress: number; timedOut: number; abandoned: number }
  passRateByCampaign: { campaignName: string; passRate: number; invitedCount: number; status: string }[]
  passFailRatio: { passed: number; failed: number }
  onlineUsersCount: number
}

export interface CompanyStats {
  totalMembers: number
  totalFormateurs: number
  activeCampaigns: number
  passRate: number
  avgScore: number
  attemptTrend: DailyCount[]
  attemptStatusCounts: { submitted: number; inProgress: number; timedOut: number; abandoned: number }
  passRateByCampaign: { campaignName: string; passRate: number; invitedCount: number; status: string }[]
  roleDistribution: Record<string, number>
  passFailRatio: { passed: number; failed: number }
  onlineUsersCount: number
}

export type CreatorActivityPeriod = 'all' | '30d' | '90d' | '12m'

export interface CreatorActivityItem {
  userId: number
  userName: string
  role: 'CompanyAdmin' | 'Formateur' | string
  questionsCount: number
  campaignsCount: number
  totalCount: number
}

export interface CompanyCreatorActivityStats {
  period: CreatorActivityPeriod
  items: CreatorActivityItem[]
}

function extractError(err: unknown, fallback: string): string {
  const e = err as { response?: { data?: unknown } }
  const raw = e.response?.data
  if (typeof raw === 'string') return raw
  return (raw as { message?: string })?.message ?? fallback
}

export async function getCandidatStatsApi() {
  try {
    const res = await http.get<CandidatStats>('/stats/candidat')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to load stats') }
  }
}

export async function getFormateurStatsApi() {
  try {
    const res = await http.get<FormateurStats>('/stats/formateur')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to load stats') }
  }
}

export async function getAdminStatsApi() {
  try {
    const res = await http.get<AdminStats>('/stats/admin')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to load stats') }
  }
}

export async function getCompanyStatsApi() {
  try {
    const res = await http.get<CompanyStats>('/stats/company')
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to load stats') }
  }
}

export async function getCompanyCreatorActivityStatsApi(period: CreatorActivityPeriod = 'all') {
  try {
    const res = await http.get<CompanyCreatorActivityStats>('/stats/company/creator-activity', {
      params: { period },
    })
    return { data: res.data, error: undefined }
  } catch (err) {
    return { data: undefined, error: extractError(err, 'Failed to load creator activity stats') }
  }
}
