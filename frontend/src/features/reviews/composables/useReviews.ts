import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { toValue, type MaybeRefOrGetter } from 'vue'
import {
  createReview,
  updateReview,
  deleteReview,
  type CreateReviewRequest,
  type UpdateReviewRequest
} from '@/features/reviews/api/reviews.api'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'

function invalidateCourseQueries(queryClient: ReturnType<typeof useQueryClient>, courseId: string) {
  queryClient.invalidateQueries({ queryKey: queryKeys.course(courseId) })
  queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
  queryClient.invalidateQueries({ queryKey: queryKeys.coursesBrowseAll() })
}

export function useCreateReview(courseId: MaybeRefOrGetter<string>) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: Omit<CreateReviewRequest, 'courseId'>) =>
      createReview({ ...data, courseId: toValue(courseId) }),
    onSuccess: () => {
      invalidateCourseQueries(queryClient, toValue(courseId))
      toast.success('Opinia została dodana')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się dodać opinii')
  })
}

export function useUpdateReview(courseId: MaybeRefOrGetter<string>) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: Omit<UpdateReviewRequest, 'courseId'>) =>
      updateReview({ ...data, courseId: toValue(courseId) }),
    onSuccess: () => {
      invalidateCourseQueries(queryClient, toValue(courseId))
      toast.success('Opinia została zaktualizowana')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się zaktualizować opinii')
  })
}

export function useDeleteReview(courseId: MaybeRefOrGetter<string>) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (reviewId: string) => deleteReview(toValue(courseId), reviewId),
    onSuccess: () => {
      invalidateCourseQueries(queryClient, toValue(courseId))
      toast.success('Opinia została usunięta')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć opinii')
  })
}
