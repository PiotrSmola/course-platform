import { computed } from 'vue'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { getMyEnrollments, enroll } from '@/features/enrollment/api/enrollment.api'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { queryKeys } from '@/shared/queryKeys'

export function useEnrollments() {
  const authStore = useAuthStore()
  return useQuery({
    queryKey: queryKeys.enrollments(),
    queryFn: getMyEnrollments,
    enabled: computed(() => authStore.isAuthenticated)
  })
}

export function useEnroll() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: enroll,
    onSuccess: (_data, courseId) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.enrollments() })
      queryClient.invalidateQueries({ queryKey: queryKeys.course(courseId) })
      toast.success('Zapisano na kurs')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się zapisać na kurs')
    }
  })
}
