export interface ThemeItem {
  id: number
  name: string
  description?: string | null
  icon?: string | null
  order: number
  parentId?: number | null
  companyId: number
  createdById: number
  children: ThemeItem[]
  questions?: { id: number }[]
  createdAt: string
  updatedAt?: string | null
  enable: boolean
}
