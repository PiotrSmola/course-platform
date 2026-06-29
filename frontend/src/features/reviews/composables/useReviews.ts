import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { createReview, type CreateReviewRequest } from '@/features/reviews/api/reviews.api'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'

export function useCreateReview(courseId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: Omit<CreateReviewRequest, 'courseId'>) =>
      createReview({ ...data, courseId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.course(courseId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
      toast.success('Opinia została dodana')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się dodać opinii')
  })
}
