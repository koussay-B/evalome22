export type NotificationType = 'Info' | 'Success' | 'Warning' | 'Danger'

export interface Notification {
    id: number
    title: string
    body: string
    date: string
    received: boolean
    type: NotificationType
    actionUrl?: string
}
