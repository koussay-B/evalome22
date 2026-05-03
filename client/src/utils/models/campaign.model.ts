export type CampaignStatus = 'Draft' | 'Scheduled' | 'Active' | 'Closed' | 'Archived'
export type ScoringMode    = 'SumWeighted' | 'Average' | 'Percentage'

export interface CampaignQuestionnaireItem {
  id:                 number
  questionnaireId:    number
  questionnaireTitle: string
  order:              number
  label?:             string | null
  isRequired:         boolean
}

export interface CampaignItem {
  id:                      number
  name:                    string
  description?:            string | null
  status:                  CampaignStatus
  availableFrom:           string
  availableUntil:          string
  allowedTimeSlots?:       string | null
  sendInviteEmail:         boolean
  reminderHoursBefore?:    number | null
  companyId:               number
  createdById:             number
  themeId?:                number | null
  questionnairesCount:     number
  createdAt:               string
  updatedAt?:              string | null
  // ── Test execution settings ──────────────────────────────────────────────
  passingScorePercent:         number
  scoringMode:                 ScoringMode
  allowPartialScore:           boolean
  globalDurationMinutes?:      number | null
  showTimer:                   boolean
  allowNavigationBack:         boolean
  randomizeQuestions:          boolean
  randomizeChoices:            boolean
  showQuestionScore:           boolean
  showCorrectAfterEach:        boolean
  showResultsImmediately:      boolean
  allowTabSwitch:              boolean
  tabSwitchMaxCount?:          number | null
  maxAttempts:                 number
  cooldownBetweenAttemptsMinutes?: number | null
  // Denormalized counters
  invitedCount:   number
  completedCount: number
  passedCount:    number
}

export interface CreateCampaignPayload {
  name:                           string
  description?:                   string | null
  status?:                        CampaignStatus
  availableFrom:                  string
  availableUntil:                 string
  themeId?:                       number | null
  sendInviteEmail?:               boolean
  reminderHoursBefore?:           number | null
  // ── Test execution settings ──────────────────────────────────────────────
  passingScorePercent?:               number
  scoringMode?:                       ScoringMode
  allowPartialScore?:                 boolean
  globalDurationMinutes?:             number | null
  showTimer?:                         boolean
  allowNavigationBack?:               boolean
  randomizeQuestions?:                boolean
  randomizeChoices?:                  boolean
  showQuestionScore?:                 boolean
  showCorrectAfterEach?:              boolean
  showResultsImmediately?:            boolean
  allowTabSwitch?:                    boolean
  tabSwitchMaxCount?:                 number | null
  maxAttempts?:                       number
  cooldownBetweenAttemptsMinutes?:    number | null
}

export type UpdateCampaignPayload = Partial<CreateCampaignPayload>

export interface AddCampaignQuestionnairePayload {
  questionnaireId: number
  label?:          string | null
  isRequired?:     boolean
}

export interface ReorderCampaignQuestionnairesPayload {
  items: { questionnaireId: number; order: number }[]
}

export type CampaignCandidateStatus = 'Invited' | 'InProgress' | 'Completed' | 'Expired' | 'Withdrawn'

export interface CampaignCandidateItem {
  campaignCandidateId: number
  candidateId:         number
  candidateName:       string
  candidateEmail:      string
  candidateImageUrl?:  string | null
  status:              CampaignCandidateStatus
  invitedAt:           string
}

export interface AvailableCandidateItem {
  id:         number
  userName:   string
  email:      string
  imageUrl?:  string | null
}
