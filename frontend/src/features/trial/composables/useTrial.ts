import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { useRouter } from 'vue-router'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import { activateTrial, getTrialEligibleCourses } from '@/features/trial/api/trial.api'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'

export function useTrialEligibleCourses(enabled: MaybeRefOrGetter<boolean> = true) {
  return useQuery({
    queryKey: queryKeys.trialEligibleCourses(),
    queryFn: getTrialEligibleCourses,
    enabled: computed(() => toValue(enabled))
  })
}

export function useActivateTrial() {
  const queryClient = useQueryClient()
  const router = useRouter()

  return useMutation({
    mutationFn: activateTrial,
    onSuccess: async (_data, variables) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.trialEligibleCourses() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.enrollments() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.course(variables.courseId) })
      ])
      toast.success('Trial został aktywowany. Możesz obejrzeć dwie pierwsze lekcje.')
      await router.push({ name: 'CourseDetails', params: { id: variables.courseId } })
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się aktywować triala')
    }
  })
}
