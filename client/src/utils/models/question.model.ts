export type QuestionType     = 'QCU' | 'QCM' | 'TrueFalse'
export type ComplexityLevel  = 'Beginner' | 'Intermediate' | 'Advanced' | 'Expert'

export interface QuestionChoice {
  id:           number
  text:         string
  isCorrect:    boolean
  order:        number
  explanation?: string | null
}

export interface QuestionItem {
  id:               number
  title:            string
  explanation?:     string | null
  hint?:            string | null
  type:             QuestionType
  complexity:       ComplexityLevel
  timeLimitSeconds?: number | null
  points:           number
  themeId:          number
  companyId:        number
  createdById:      number
  createdAt:        string
  updatedAt?:       string | null
  enable:           boolean
  choices:          QuestionChoice[]
}

// ── Create / Update payloads ──────────────────────────────────────────────

export interface ChoicePayload {
  text:         string
  isCorrect:    boolean
  order:        number
  explanation?: string | null
}

export interface CreateQuestionPayload {
  title:            string
  explanation?:     string | null
  hint?:            string | null
  type:             QuestionType
  complexity:       ComplexityLevel
  timeLimitSeconds?: number | null
  points:           number
  themeId:          number
  choices?:         ChoicePayload[]
}

export type UpdateQuestionPayload = CreateQuestionPayload
