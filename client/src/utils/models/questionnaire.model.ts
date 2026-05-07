import type { QuestionItem } from './question.model'

export type QuestionnaireStatus = 'Draft' | 'Published' | 'Archived'

export interface QuestionnaireQuestionItem {
  id:              number
  questionnaireId: number
  questionId:      number
  order:           number
  weight:          number
  isMandatory:     boolean
  poolGroup?:      number | null
  question?:       QuestionItem
}

export interface QuestionnaireItem {
  id:                     number
  title:                  string
  description?:           string | null
  instructions?:          string | null
  coverImageUrl?:         string | null
  status:                 QuestionnaireStatus
  companyId:              number
  createdById:            number
  createdAt:              string
  updatedAt?:             string | null
  questionnaireQuestions: QuestionnaireQuestionItem[]
}

// ── Create / Update payloads ──────────────────────────────────────────────

export interface CreateQuestionnairePayload {
  title:         string
  description?:  string | null
  instructions?: string | null
  coverImageUrl?: string | null
}

export type UpdateQuestionnairePayload = Pick<CreateQuestionnairePayload, 'title' | 'description'>

export interface AddQuestionToQuestionnairePayload {
  questionId:   number
  order:        number
  weight?:      number
  isMandatory?: boolean
  poolGroup?:   number | null
}
