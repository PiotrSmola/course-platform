import { useQuery } from '@tanstack/vue-query'
import { computed, reactive, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getCourses } from '@/features/courses/api/courses.api'
import type { CoursesFilter } from '@/features/courses/api/courses.api'
import type { CourseListDto } from '@/features/courses/types/course.types'
import { CourseLevel, CourseStatus } from '@/features/courses/types/course.types'

export type SortOption = 'newest' | 'price-asc' | 'price-desc' | 'rating' | 'popular'

export interface BrowseState {
  searchTerm: string
  level: CourseLevel | null
  status: CourseStatus | null
  minPrice: number | null
  maxPrice: number | null
  language: string | null
  categoryIds: string[]
  technologyIds: string[]
  minRating: number | null
  sortBy: SortOption
  pageNumber: number
  pageSize: number
}

function parseNumber(val: string | null | undefined): number | null {
  if (!val) return null
  const n = Number(val)
  return Number.isNaN(n) ? null : n
}

function parseLevel(val: string | null | undefined): CourseLevel | null {
  if (val === '0') return CourseLevel.Beginner
  if (val === '1') return CourseLevel.Intermediate
  if (val === '2') return CourseLevel.Advanced
  return null
}

function parseStatus(val: string | null | undefined): CourseStatus | null {
  if (val === '1') return CourseStatus.Published
  return null
}

export function useCourseBrowse() {
  const route = useRoute()
  const router = useRouter()

  const state = reactive<BrowseState>({
    searchTerm: '',
    level: null,
    status: CourseStatus.Published,
    minPrice: null,
    maxPrice: null,
    language: null,
    categoryIds: [],
    technologyIds: [],
    minRating: null,
    sortBy: 'newest',
    pageNumber: 1,
    pageSize: 12
  })

  function syncFromRoute() {
    const q = route.query
    state.searchTerm = (q.searchTerm as string) || ''
    state.level = parseLevel(q.level as string | undefined)
    state.status = parseStatus(q.status as string | undefined) ?? CourseStatus.Published
    state.minPrice = parseNumber(q.minPrice as string | undefined)
    state.maxPrice = parseNumber(q.maxPrice as string | undefined)
    state.language = (q.language as string) || null
    state.categoryIds = q.categoryIds ? (Array.isArray(q.categoryIds) ? q.categoryIds as string[] : [q.categoryIds as string]) : []
    state.technologyIds = q.technologyIds ? (Array.isArray(q.technologyIds) ? q.technologyIds as string[] : [q.technologyIds as string]) : []
    state.minRating = parseNumber(q.minRating as string | undefined)
    state.sortBy = (q.sortBy as SortOption) || 'newest'
    state.pageNumber = parseNumber(q.pageNumber as string | undefined) ?? 1
  }

  syncFromRoute()

  watch(
    () => route.query,
    () => syncFromRoute(),
    { deep: true }
  )

  function updateUrl() {
    const q: Record<string, string | string[]> = {}
    if (state.searchTerm) q.searchTerm = state.searchTerm
    if (state.level !== null) q.level = state.level.toString()
    if (state.status !== null && state.status !== CourseStatus.Published) q.status = state.status.toString()
    if (state.minPrice !== null) q.minPrice = state.minPrice.toString()
    if (state.maxPrice !== null) q.maxPrice = state.maxPrice.toString()
    if (state.language) q.language = state.language
    if (state.categoryIds.length) q.categoryIds = state.categoryIds
    if (state.technologyIds.length) q.technologyIds = state.technologyIds
    if (state.minRating !== null) q.minRating = state.minRating.toString()
    if (state.sortBy !== 'newest') q.sortBy = state.sortBy
    if (state.pageNumber > 1) q.pageNumber = state.pageNumber.toString()

    router.replace({ query: q })
  }

  const filter = computed<CoursesFilter>(() => ({
    searchTerm: state.searchTerm || undefined,
    level: state.level ?? undefined,
    status: state.status ?? undefined,
    sortBy: state.sortBy,
    minPrice: state.minPrice ?? undefined,
    maxPrice: state.maxPrice ?? undefined,
    language: state.language ?? undefined,
    categoryIds: state.categoryIds.length ? state.categoryIds : undefined,
    technologyIds: state.technologyIds.length ? state.technologyIds : undefined,
    minRating: state.minRating ?? undefined,
    pageNumber: state.pageNumber,
    pageSize: state.pageSize
  }))

  const { isLoading, isError, error, data } = useQuery({
    queryKey: computed(() => ['courses-browse', { ...filter.value }]),
    queryFn: () => getCourses(filter.value)
  })

  const filteredItems = computed<CourseListDto[]>(() => {
    return data.value?.items ?? []
  })

  const totalCount = computed(() => data.value?.totalCount ?? 0)
  const totalPages = computed(() => Math.ceil(totalCount.value / state.pageSize))

  function setSearchTerm(term: string) {
    state.searchTerm = term
    state.pageNumber = 1
    updateUrl()
  }

  function setLevel(level: CourseLevel | null) {
    state.level = level
    state.pageNumber = 1
    updateUrl()
  }

  function setSortBy(sortBy: SortOption) {
    state.sortBy = sortBy
    updateUrl()
  }

  function setPage(page: number) {
    state.pageNumber = page
    updateUrl()
  }

  function setPriceRange(min: number | null, max: number | null) {
    state.minPrice = min
    state.maxPrice = max
    state.pageNumber = 1
    updateUrl()
  }

  function setLanguage(language: string | null) {
    state.language = language
    state.pageNumber = 1
    updateUrl()
  }

  function toggleCategoryId(id: string) {
    const idx = state.categoryIds.indexOf(id)
    if (idx >= 0) {
      state.categoryIds.splice(idx, 1)
    } else {
      state.categoryIds.push(id)
    }
    state.pageNumber = 1
    updateUrl()
  }

  function toggleTechnologyId(id: string) {
    const idx = state.technologyIds.indexOf(id)
    if (idx >= 0) {
      state.technologyIds.splice(idx, 1)
    } else {
      state.technologyIds.push(id)
    }
    state.pageNumber = 1
    updateUrl()
  }

  function setMinRating(rating: number | null) {
    state.minRating = rating
    state.pageNumber = 1
    updateUrl()
  }

  function resetFilters() {
    state.searchTerm = ''
    state.level = null
    state.minPrice = null
    state.maxPrice = null
    state.language = null
    state.categoryIds = []
    state.technologyIds = []
    state.minRating = null
    state.sortBy = 'newest'
    state.pageNumber = 1
    updateUrl()
  }

  return {
    state,
    isLoading,
    isError,
    error,
    filteredItems,
    totalCount,
    totalPages,
    setSearchTerm,
    setLevel,
    setSortBy,
    setPage,
    setPriceRange,
    setLanguage,
    toggleCategoryId,
    toggleTechnologyId,
    setMinRating,
    resetFilters
  }
}
