import { useQuery } from '@tanstack/vue-query'
import { getPopularCourses } from '@/features/courses/api/popularCourses.api'

export const popularCoursesQueryKey = ['courses', 'popular'] as const

export function usePopularCourses() {
  return useQuery({
    queryKey: popularCoursesQueryKey,
    queryFn: getPopularCourses
  })
}
