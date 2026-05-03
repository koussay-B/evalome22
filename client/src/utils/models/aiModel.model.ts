export type AiProvider = 'OpenAI' | 'Anthropic' | 'Google' | 'OpenRouter' | 'Other'

export interface AiModelItem {
  id: number
  name: string
  provider: AiProvider
  modelIdentifier: string
  apiKeyMasked: string
  apiBaseUrl?: string | null
  isDefault: boolean
  isEnabled: boolean
  createdAt: string
  updatedAt?: string | null
}

export interface AiModelPayload {
  name: string
  provider: AiProvider
  modelIdentifier: string
  apiKey: string
  apiBaseUrl?: string | null
  isDefault: boolean
  isEnabled: boolean
}

export interface TestAiModelPayload {
  provider: string
  modelIdentifier: string
  apiKey: string
  apiBaseUrl?: string | null
  modelId?: number
}

export interface TestAiModelResult {
  success: boolean
  message: string
}

export interface GenerateQuestionsPayload {
  modelId?: number | null
  count: number
  context: {
    questionnaireTitle: string
    questionnaireDescription?: string | null
    themeId?: number | null
    themeName?: string | null
    language?: string | null
    existingQuestions?: string[]
    difficulty?: string | null
    topics?: string[] | null
    customContext?: string | null
  }
}

export interface GeneratedChoiceDto {
  text: string
  isCorrect: boolean
  order: number
}

export interface GeneratedQuestionDto {
  title: string
  type: 'QCU' | 'QCM' | 'TrueFalse'
  complexity: 'Beginner' | 'Intermediate' | 'Advanced' | 'Expert'
  points: number
  timeLimitSeconds?: number | null
  explanation?: string | null
  choices: GeneratedChoiceDto[]
}
