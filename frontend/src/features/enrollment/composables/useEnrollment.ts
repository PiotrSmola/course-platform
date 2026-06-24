import { useQuery, useMutation } from '@tanstack/vue-query'
import { getMyEnrollments, enroll } from '@/features/enrollment/api/enrollment.api'
import { toast } from 'vue3-toastify'

export function useEnrollments() {
  return useQuery({
    queryKey: ['enrollments'],
    queryFn: getMyEnrollments
  })
}

export function useEnroll() {
  return useMutation({
    mutationFn: enroll,
    onSuccess: () => {
      toast.success('Enrolled successfully')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.error || 'Enrollment failed')
    }
  })
}
