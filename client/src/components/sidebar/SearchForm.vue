<template>
  <form v-bind="$attrs" @submit.prevent>
    <div class="relative">
      <label for="sidebar-search" class="sr-only">{{ t('layout.header.search') }}</label>
      <SidebarInput
        id="sidebar-search"
        v-model="query"
        :placeholder="t('layout.header.typeToSearch')"
        class="h-8 pl-7"
      />
      <Search class="pointer-events-none absolute top-1/2 left-2 size-4 -translate-y-1/2 opacity-50 select-none" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { Search } from 'lucide-vue-next';
import { SidebarInput } from '@/components/ui/sidebar';
import { useLocale } from '@/composables/useLocale';

const { t } = useLocale()
const query = ref('')
</script>
