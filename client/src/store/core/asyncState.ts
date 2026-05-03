// ─── Async State ─────────────────────────────────────────────────────────────
// A discriminated union representing the four lifecycle states of any async op.

export type AsyncState<T> =
    | { status: 'idle' }
    | { status: 'loading' }
    | { status: 'data'; data: T }
    | { status: 'error'; message: string }

// ─── Pagination ───────────────────────────────────────────────────────────────

export interface PaginationMeta {
    currentPage: number
    totalPages: number
    totalCount: number
    pageSize: number
}

export interface PaginatedData<T> {
    /** Pages fetched so far, keyed by page number */
    pages: Record<number, T[]>
    meta: PaginationMeta
}

export type PaginatedState<T, P> = AsyncState<PaginatedData<T>> & { params?: P }