export const UserRole = {
    Admin: 'Admin',
    CompanyAdmin: 'CompanyAdmin',
    Formateur: 'Formateur',
    Condidat: 'Condidat',
} as const

export type UserRole = typeof UserRole[keyof typeof UserRole]

/**
 * Returns the background and text color classes for a role avatar.
 */
export function getRoleAvatarStyles(role: string | UserRole) {
    switch (role) {
        case UserRole.Admin:
            return 'bg-red-100 text-red-700 dark:bg-red-900/50 dark:text-red-300'
        case UserRole.CompanyAdmin:
            return 'bg-sky-100 text-sky-700 dark:bg-sky-900/50 dark:text-sky-300'
        case UserRole.Formateur:
            return 'bg-violet-100 text-violet-700 dark:bg-violet-900/50 dark:text-violet-300'
        case UserRole.Condidat:
            return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/50 dark:text-emerald-300'
        default:
            return 'bg-muted text-muted-foreground'
    }
}

/**
 * Returns the color classes for a role chip/badge.
 */
export function getRoleChipStyles(role: string | UserRole) {
    switch (role) {
        case UserRole.Admin:
            return 'bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-400'
        case UserRole.CompanyAdmin:
            return 'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-400'
        case UserRole.Formateur:
            return 'bg-violet-100 text-violet-700 dark:bg-violet-900/40 dark:text-violet-400'
        case UserRole.Condidat:
            return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-400'
        default:
            return 'bg-muted text-muted-foreground'
    }
}

/**
 * Returns the dot color class for a role indicator.
 */
export function getRoleDotClass(role: string | UserRole) {
    switch (role) {
        case UserRole.Admin:
            return 'bg-red-500'
        case UserRole.CompanyAdmin:
            return 'bg-sky-500'
        case UserRole.Formateur:
            return 'bg-violet-500'
        case UserRole.Condidat:
            return 'bg-emerald-500'
        default:
            return 'bg-muted-foreground'
    }
}
