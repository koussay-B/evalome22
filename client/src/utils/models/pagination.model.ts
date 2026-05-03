// Pages keyed by page number
export type PageCache<T> = Record<number, T[]>

export interface PaginationMeta {
  currentPage: number
  totalPages:  number
  pageSize:    number
  totalCount:  number
}

// Generic discriminated union for any paginated resource
// T = item type  |  P = search/filter params type
export type PaginatedState<T, P = Record<string, unknown>> =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'error';  message: string }
  | { status: 'data';   pages: PageCache<T>; meta: PaginationMeta; params: P }
