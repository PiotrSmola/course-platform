import { useMutation } from '@tanstack/vue-query'
import { subscribeNewsletter } from '@/features/newsletter/api/newsletter.api'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { toast } from '@/shared/toast/toast'

export function useNewsletterSubscribe() {
  return useMutation({
    mutationFn: subscribeNewsletter,
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się zapisać do newslettera')
    }
  })
}
