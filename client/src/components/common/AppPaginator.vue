<template>
  <Pagination :class="props.class">
    <PaginationContent>

      <!-- Previous -->
      <PaginationItem>
        <PaginationPrevious
          :class="page === 1 ? 'pointer-events-none opacity-40' : ''"
          :aria-disabled="page === 1"
          @click.prevent="handleChange(page - 1)"
        />
      </PaginationItem>

      <!-- First page -->
      <PaginationItem v-if="page > 2">
        <PaginationLink @click.prevent="handleChange(1)">1</PaginationLink>
      </PaginationItem>

      <!-- Left ellipsis -->
      <PaginationItem v-if="page > 3">
        <PaginationEllipsis />
      </PaginationItem>

      <!-- Page before current -->
      <PaginationItem v-if="page > 1">
        <PaginationLink @click.prevent="handleChange(page - 1)">{{ page - 1 }}</PaginationLink>
      </PaginationItem>

      <!-- Current page -->
      <PaginationItem>
        <PaginationLink :is-active="true">{{ page }}</PaginationLink>
      </PaginationItem>

      <!-- Page after current -->
      <PaginationItem v-if="page < pages">
        <PaginationLink @click.prevent="handleChange(page + 1)">{{ page + 1 }}</PaginationLink>
      </PaginationItem>

      <!-- Right ellipsis -->
      <PaginationItem v-if="page < pages - 2">
        <PaginationEllipsis />
      </PaginationItem>

      <!-- Last page -->
      <PaginationItem v-if="page < pages - 1">
        <PaginationLink @click.prevent="handleChange(pages)">{{ pages }}</PaginationLink>
      </PaginationItem>

      <!-- Next -->
      <PaginationItem>
        <PaginationNext
          :class="page === pages ? 'pointer-events-none opacity-40' : ''"
          :aria-disabled="page === pages"
          @click.prevent="handleChange(page + 1)"
        />
      </PaginationItem>

    </PaginationContent>
  </Pagination>
</template>

<script setup lang="ts">
import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from '@/components/ui/pagination'
import { useLocale } from '@/composables/useLocale'

const props = defineProps<{
  page: number
  pages: number
  class?: string
}>()

const emit = defineEmits<{ pageChange: [page: number] }>()
useLocale()

function handleChange(newPage: number) {
  if (newPage !== props.page && newPage >= 1 && newPage <= props.pages) {
    emit('pageChange', newPage)
  }
}
</script>
