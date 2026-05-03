export interface CampaignReportAttempt {
  id: number
  candidateId: number
  candidateName: string
  candidateEmail?: string | null
  questionnaireId?: number | null
  questionnaireTitle?: string | null
  attemptNumber: number
  percentage: number
  passed: boolean
  status: string
  startedAt: string
  submittedAt?: string | null
}

export interface CampaignReportData {
  attempts: CampaignReportAttempt[]
  groupAverageScore: number
  passRate: number
}
