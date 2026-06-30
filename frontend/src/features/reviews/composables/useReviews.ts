import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { toValue, type MaybeRefOrGetter } from 'vue'
import { createReview, type CreateReviewRequest } from '@/features/reviews/api/reviews.api'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'

export function useCreateReview(courseId: MaybeRefOrGetter<string>) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: Omit<CreateReviewRequest, 'courseId'>) =>
      createReview({ ...data, courseId: toValue(courseId) }),
    onSuccess: () => {
      const id = toValue(courseId)
      queryClient.invalidateQueries({ queryKey: queryKeys.course(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
      queryClient.invalidateQueries({ queryKey: queryKeys.coursesBrowseAll() })
      toast.success('Opinia została dodana')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się dodać opinii')
  })
}
