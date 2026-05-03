<template>
  <Sidebar collapsible="icon" v-bind="$attrs">
    <SidebarHeader>
      <SidebarMenu>
        <SidebarMenuItem>
          <SidebarMenuButton size="lg" as-child>
            <router-link :to="brandUrl">
              <div class="flex aspect-square size-16 items-center justify-center rounded-2xl border border-brand/15 bg-white shadow-sm dark:bg-background/70">
                <EvaloMark class="size-14 text-brand" />
              </div>
              <div class="grid flex-1 text-left text-xl leading-tight group-data-[collapsible=icon]:hidden">
                <span class="truncate font-black tracking-tighter text-slate-800 ">
                  <span class="uppercase">EVALO</span><span class="text-brand">.</span><span>me</span>
                </span>
                <span class="truncate text-xs font-medium text-muted-foreground">
                  {{ brandSubtitle || "Unlock your team's potential" }}
                </span>
              </div>
            </router-link>
          </SidebarMenuButton>
        </SidebarMenuItem>
      </SidebarMenu>
    </SidebarHeader>

    <SidebarContent>
      <NavMain :items="navMain" :label="platformLabel" />
      <NavProjects v-if="navProjects?.length" :projects="navProjects" />
    </SidebarContent>

    <SidebarFooter>
      <NavUser />
    </SidebarFooter>

    <SidebarRail />
  </Sidebar>
</template>

<script setup lang="ts">
import {
  Sidebar, SidebarContent, SidebarFooter,
  SidebarHeader, SidebarMenu, SidebarMenuButton,
  SidebarMenuItem, SidebarRail,
} from '@/components/ui/sidebar';
import type { SidebarData } from './types';
import NavMain from './NavMain.vue';
import NavProjects from './NavProjects.vue';
import NavSecondary from './NavSecondary.vue';
import NavUser from './NavUser.vue';
import EvaloMark from '@/components/branding/EvaloMark.vue';

const props = withDefaults(defineProps<SidebarData & {
  brandSubtitle?: string
  brandUrl?: string
}>(), {
  brandUrl: '/',
  brandSubtitle: "Unlock your team's potential",
  platformLabel: 'Platform',
})
</script>
