export interface User {
    id?: number
    userName: string
    email: string
    role: string
    imageUrl?: string | null
    phoneNumber?: string | null
    companyId?: number | null
}

export interface UserListItem {
    id: number
    userName: string
    email: string
    phoneNumber?: string | null
    imageUrl?: string | null
    role: string
    isActive: boolean
    companyId?: number | null
    companyName?: string | null
    companyLogo?: string | null
}

export interface UpdateManagedUserPayload {
    userName: string
    email: string
    role: string
    isActive: boolean
}

export interface ManagedUserStats {
    totalAttempts?: number
    passedAttempts?: number
    activeCampaigns?: number
    certificatesEarned?: number
    campaignsCreated?: number
    questionnairesCreated?: number
    totalCandidatesManaged?: number
    employeesCount?: number
    activeUsers?: number
    questionnairesCount?: number
    formateursCount?: number
    totalUsers?: number
    companiesCount?: number
    managementAccounts?: number
}
