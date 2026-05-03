export type CompanyStatus = 'Pending' | 'Approved' | 'Rejected'

export interface Company {
    id: number
    name: string
    logo?: string | null
    description?: string | null
    email?: string | null
    phone?: string | null
    website?: string | null
    address?: string | null
    requesterName?: string | null
    requesterEmail?: string | null
    status: CompanyStatus
    rejectionReason?: string | null
    reviewedAt?: string | null
    reviewedByUserId?: number | null
    isActive: boolean
    createdAt: string
    updatedAt?: string | null
}
