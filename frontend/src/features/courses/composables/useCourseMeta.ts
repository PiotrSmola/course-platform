import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { getCategories, getTechnologies, getCourses } from '@/features/courses/api/courses.api'
import type { CategoryDto, TechnologyDto } from '@/features/courses/api/courses.api'
import { queryKeys } from '@/shared/queryKeys'

export function useCategories() {
  return useQuery<CategoryDto[]>({
    queryKey: queryKeys.categories(),
    queryFn: getCategories,
    initialData: []
  })
}

export function useTechnologies() {
  return useQuery<TechnologyDto[]>({
    queryKey: queryKeys.technologies(),
    queryFn: getTechnologies,
    initialData: []
  })
}

export function useCourseSearch(searchTerm: Ref<string>) {
  const term = computed(() => searchTerm.value.trim())

  return useQuery({
    queryKey: computed(() => queryKeys.courseSearch(term.value)),
    queryFn: () => getCourses({ searchTerm: term.value, pageSize: 6 }),
    enabled: computed(() => term.value.length > 0)
  })
}

