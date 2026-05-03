import type { Component } from 'vue'

// A single navigation item (can have sub-items)
export interface NavItem {
    title: string
    url: string
    icon: Component
    isActive?: boolean
    items?: NavSubItem[]
}

// A sub-item inside a collapsible NavItem
export interface NavSubItem {
    title: string
    url: string
}

// A project / quick-link item
export interface NavProject {
    name: string
    url: string
    icon: Component
}

// A secondary (bottom) nav item
export interface NavSecondaryItem {
    title: string
    url: string
    icon: Component
}

// Full sidebar data shape — passed as props to AppSidebar
export interface SidebarData {
    /** Label shown at the top of the Platform group */
    platformLabel?: string
    navMain: NavItem[]
    navProjects?: NavProject[]
    navSecondary?: NavSecondaryItem[]
}
