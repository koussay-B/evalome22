// ── Attempt / Test-taking models ─────────────────────────────────────────────

export interface AttemptChoice {
  id:   number
  text: string
}

export interface AttemptQuestion {
  questionId:        number
  title:             string
  hint?:             string | null
  type:              'QCU' | 'QCM' | 'TrueFalse'
  complexity?:       string
  points?:           number
  weight?:           number
  order:             number
  choices:           AttemptChoice[]
  timeLimitSeconds?: number | null
}

export interface StartAttemptResponse {
  attemptId:            number
  questions:            AttemptQuestion[]   // parsed from JSON snapshot
  questionnaireName?:   string
  durationMinutes?:     number | null
  allowNavigationBack:  boolean
  showTimer:            boolean
  tabSwitchMaxCount:    number
  maxAttempts?:         number
  campaignCandidateId?: number
}

// ── Termination reason (why test was auto-submitted) ──────────────────────────
export type TerminationReason = 'TimerExpired' | 'TabSwitchLimit' | 'UserQuit'

export interface SubmitAttemptResult {
  rawScore:         number
  maxPossibleScore: number
  percentage:       number
  passed:           boolean
  campaignPassed:   boolean
  campaignId:       number
}

export interface AttemptReport {
  themeScores?:       Record<string, number>
  aiRecommendations?: string | null
  aiStrengths?:       string | null
  campaignPercentile?: number | null
  groupAverageScore?:  number | null
}

export interface AttemptResultItem {
  id:                   number
  campaignId:           number
  questionnaireId?:     number | null
  questionnaireName?:   string | null
  campaignCandidateId?: number | null
  rawScore:             number
  maxPossibleScore:     number
  percentage:           number
  passed:               boolean
  startedAt:            string
  submittedAt?:         string | null
  status:               string
  tabSwitchCount?:      number
  report?:              AttemptReport | null
  remainingAttempts?:   number    // -1 = unlimited, 0 = no more, N = N left
  maxAttempts?:         number
}

// ── Candidate campaign list ───────────────────────────────────────────────────

export interface MyCampaignItem {
  campaignId:          number
  name:                string
  startDate:           string
  endDate:             string
  status:              string   // CampaignStatus
  candidateStatus:     string   // CampaignCandidateStatus
  testStatus:          string
  lastAttemptStatus?:  string | null
  passed:              boolean
  questionnairesCount: number
}

export interface CampaignQuestionnaireForCandidate {
  questionnaireId:   number
  title:             string
  label?:            string | null
  isRequired:        boolean
  order:             number
  questionCount:     number
  durationMinutes?:  number | null
  passingScore:      number
  attemptStatus:     string   // NotStarted | InProgress | Submitted | TimedOut | Abandoned
  attemptId?:        number | null
  score?:            number | null
  passed?:           boolean | null
  maxAttempts:       number   // -1 = unlimited
  attemptsUsed:      number
  remainingAttempts: number   // -1 = unlimited, 0 = none left
}

export interface MyCampaignDetail {
  campaignId:      number
  name:            string
  description:     string
  startDate:       string
  endDate:         string
  status:          string
  candidateStatus: string
  questionnaires:  CampaignQuestionnaireForCandidate[]
}

// ── Certificates ──────────────────────────────────────────────────────────────

export interface CertificateItem {
  id:              number
  campaignId:      number
  campaignName:    string
  companyName:     string
  percentage:      number
  issuedAt:        string
  certificateCode: string
  candidateName:   string
}
