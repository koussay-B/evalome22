<template>
  <SidebarGroup>
    <SidebarGroupLabel>{{ label }}</SidebarGroupLabel>
    <SidebarMenu>
      <template v-for="item in items" :key="item.title">
        <!-- Item with sub-items → Collapsible -->
        <Collapsible
          v-if="item.items?.length"
          as-child
          :default-open="item.isActive || isParentActive(item)"
        >
          <SidebarMenuItem>
            <SidebarMenuButton 
              :tooltip="item.title" 
              as-child
              class="data-[active=true]:bg-brand data-[active=true]:text-white hover:bg-brand/10 transition-all duration-200"
            >
              <router-link :to="item.url">
                <!-- Adjust stroke-width for icon thickness -->
                <component :is="item.icon" class="size-4 stroke-[2.5px]" />
                <span class="">{{ item.title }}</span>
              </router-link>
            </SidebarMenuButton>

            <CollapsibleTrigger as-child>
              <SidebarMenuAction class="data-[state=open]:rotate-90 transition-transform duration-200">
                <ChevronRight class="size-4" />
                <span class="sr-only">Toggle</span>
              </SidebarMenuAction>
            </CollapsibleTrigger>

            <CollapsibleContent>
              <SidebarMenuSub>
                <SidebarMenuSubItem v-for="sub in item.items" :key="sub.title">
                  <SidebarMenuSubButton 
                    as-child 
                    :is-active="isActive(sub.url)"
                    class="data-[active=true]:text-brand "
                  >
                    <router-link :to="sub.url">
                      <span>{{ sub.title }}</span>
                    </router-link>
                  </SidebarMenuSubButton>
                </SidebarMenuSubItem>
              </SidebarMenuSub>
            </CollapsibleContent>
          </SidebarMenuItem>
        </Collapsible>

        <!-- Item without sub-items → plain button -->
        <SidebarMenuItem v-else>
          <SidebarMenuButton 
            as-child 
            :tooltip="item.title" 
            :is-active="isActive(item.url)"
            class="data-[active=true]:bg-brand data-[active=true]:text-white data-[active=true]:font-bold hover:bg-brand/10 transition-all duration-200"
          >
            <router-link :to="item.url">
              <component :is="item.icon" class="size-4 stroke-[2.5px]" />
              <span class="font-semibold">{{ item.title }}</span>
            </router-link>
          </SidebarMenuButton>
        </SidebarMenuItem>
      </template>
    </SidebarMenu>
  </SidebarGroup>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';
import { ChevronRight } from 'lucide-vue-next';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import {
  SidebarGroup, SidebarGroupLabel, SidebarMenu, SidebarMenuAction,
  SidebarMenuButton, SidebarMenuItem, SidebarMenuSub,
  SidebarMenuSubButton, SidebarMenuSubItem,
} from '@/components/ui/sidebar';
import type { NavItem } from './types';

const props = withDefaults(defineProps<{
  items: NavItem[]
  label?: string
}>(), {
  label: 'Platform',
})

const route = useRoute()

const isActive = (url: string) => route.path === url
const isParentActive = (item: NavItem) =>
  item.items?.some((sub) => route.path === sub.url) ?? false
</script>
