import { useQuery } from '@tanstack/vue-query'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import {
  getAdminRevenueOverview,
  getInstructorAnalytics
} from '@/features/analytics/api/analytics.api'
import { queryKeys } from '@/shared/queryKeys'

export function useInstructorAnalytics(courseId: MaybeRefOrGetter<string | undefined> = undefined) {
  return useQuery({
    queryKey: computed(() => queryKeys.instructorAnalytics(toValue(courseId))),
    queryFn: () => getInstructorAnalytics(toValue(courseId))
  })
}

export function useAdminRevenueOverview(enabled: MaybeRefOrGetter<boolean> = true) {
  return useQuery({
    queryKey: queryKeys.adminRevenue(),
    queryFn: getAdminRevenueOverview,
    enabled: computed(() => toValue(enabled))
  })
}
