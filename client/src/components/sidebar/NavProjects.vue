<template>
  <SidebarGroup class="group-data-[collapsible=icon]:hidden">
    <SidebarGroupLabel>Projects</SidebarGroupLabel>
    <SidebarMenu>
      <SidebarMenuItem v-for="item in projects" :key="item.name">
        <SidebarMenuButton 
          as-child
          class="data-[active=true]:bg-brand data-[active=true]:text-white font-semibold hover:bg-brand/10 transition-colors"
        >
          <router-link :to="item.url">
            <component :is="item.icon" class="stroke-[2.5px]" />
            <span class="font-semibold">{{ item.name }}</span>
          </router-link>
        </SidebarMenuButton>

        <!-- "More" dropdown per project -->
        <DropdownMenu>
          <DropdownMenuTrigger as-child>
            <SidebarMenuAction :show-on-hover="true">
              <MoreHorizontal class="size-4" />
              <span class="sr-only">More</span>
            </SidebarMenuAction>
          </DropdownMenuTrigger>
          <DropdownMenuContent
            class="w-48 rounded-lg"
            :side="isMobile ? 'bottom' : 'right'"
            :align="isMobile ? 'end' : 'start'"
          >
            <DropdownMenuItem>
              <Folder class="mr-2 size-4 text-muted-foreground" />
              <span>View Project</span>
            </DropdownMenuItem>
            <DropdownMenuItem>
              <Share class="mr-2 size-4 text-muted-foreground" />
              <span>Share Project</span>
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem class="text-destructive focus:text-destructive">
              <Trash2 class="mr-2 size-4" />
              <span>Delete Project</span>
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </SidebarMenuItem>

      <SidebarMenuItem>
        <SidebarMenuButton class="text-sidebar-foreground/70">
          <MoreHorizontal class="size-4 text-sidebar-foreground/70" />
          <span>More</span>
        </SidebarMenuButton>
      </SidebarMenuItem>
    </SidebarMenu>
  </SidebarGroup>
</template>

<script setup lang="ts">
import { Folder, MoreHorizontal, Share, Trash2 } from 'lucide-vue-next';
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem,
  DropdownMenuSeparator, DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  SidebarGroup, SidebarGroupLabel, SidebarMenu,
  SidebarMenuAction, SidebarMenuButton, SidebarMenuItem,
  useSidebar,
} from '@/components/ui/sidebar';
import type { NavProject } from './types';

defineProps<{ projects: NavProject[] }>()

const { isMobile } = useSidebar()
</script>
