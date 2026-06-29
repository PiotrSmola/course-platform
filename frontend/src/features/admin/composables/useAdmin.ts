import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import {
  getAdminUsers,
  assignUserRole,
  getAdminCourses,
  updateCourseStatus,
  deleteReview
} from '@/features/admin/api/admin.api'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import type { CourseStatus } from '@/features/courses/types/course.types'

export function useAdminUsers() {
  return useQuery({
    queryKey: queryKeys.adminUsers(),
    queryFn: getAdminUsers
  })
}

export function useAdminCourses() {
  return useQuery({
    queryKey: queryKeys.adminCourses(),
    queryFn: () => getAdminCourses()
  })
}

export function useAdminMutations() {
  const queryClient = useQueryClient()

  const assignRole = useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: string }) => assignUserRole(userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.adminUsers() })
      toast.success('Rola została przypisana')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się przypisać roli')
  })

  const setCourseStatus = useMutation({
    mutationFn: ({ courseId, status }: { courseId: string; status: CourseStatus }) =>
      updateCourseStatus(courseId, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.adminCourses() })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
      toast.success('Status kursu został zmieniony')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się zmienić statusu')
  })

  const removeReview = useMutation({
    mutationFn: (reviewId: string) => deleteReview(reviewId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
      toast.success('Recenzja została usunięta')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć recenzji')
  })

  return { assignRole, setCourseStatus, removeReview }
}
